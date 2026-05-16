using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 설치 확인창 함정 미니게임
// 제한 시간 안에 예/아니요를 선택해야 하며,
// 오답 또는 시간 초과 시 실패, 모든 질문 통과 시 성공.
public class ConfirmTrapGame : MonoBehaviour
{
    [System.Serializable]
    public class QuestionData
    {
        public string question;             // 화면에 표시할 질문
        public bool correctAnswerIsYes;     // 정답이 '예'인지 여부
        public bool swapButtonPosition;     // 예/아니요 버튼 위치를 바꿀지 여부
        public bool reverseButtonColor;     // 예/아니요 버튼 색상을 반전할지 여부

        public QuestionData(string question, bool correctAnswerIsYes, bool swapButtonPosition, bool reverseButtonColor)
        {
            this.question = question;
            this.correctAnswerIsYes = correctAnswerIsYes;
            this.swapButtonPosition = swapButtonPosition;
            this.reverseButtonColor = reverseButtonColor;
        }
    }

    [Header("UI")]
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_Text progressText;
    public TMP_Text resultText;

    public Button yesButton;
    public Button noButton;
    public Button nextButton;
    public Button retryButton;

    [Header("Difficulty Settings")]
    // 난이도 조절 구간
    // timeLimitPerQuestion: 질문 하나당 제한 시간. 낮을수록 어려움
    // questionsPerRound: 한 판에 출제되는 질문 수. 많을수록 어려움
    public float timeLimitPerQuestion = 6f;
    public int questionsPerRound = 6;

    [Header("Button Trap Settings")]
    // 버튼 위치 기본값
    // 함정 질문에서는 예/아니요 버튼 위치가 서로 바뀔 수 있음
    public Vector2 leftButtonPosition = new Vector2(-250f, 20f);
    public Vector2 rightButtonPosition = new Vector2(250f, 20f);

    // 버튼 기본 색상
    // 기본: 예 = 초록, 아니요 = 빨강
    // 함정 질문에서는 색상이 반대로 적용될 수 있음
    public Color yesColor = new Color(0.25f, 0.8f, 0.35f);
    public Color noColor = new Color(0.9f, 0.25f, 0.25f);

    private List<QuestionData> selectedQuestions = new List<QuestionData>();
    private int currentQuestionIndex = 0;
    private float currentTimer = 0f;

    private bool isCleared = false;
    private bool isFailed = false;

    void Start()
    {
        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        resultText.text = "";

        yesButton.onClick.AddListener(() => SelectAnswer(true));
        noButton.onClick.AddListener(() => SelectAnswer(false));
        retryButton.onClick.AddListener(RetryGame);

        CreateQuestionList();
        LoadQuestion();
    }

    void Update()
    {
        if (isCleared || isFailed) return;

        currentTimer -= Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0f, timeLimitPerQuestion);

        timerText.text = "남은 시간: " + currentTimer.ToString("F1");

        if (currentTimer <= 0f)
        {
            FailGame("시간 초과입니다. 설치가 취소됩니다.");
        }
    }

    void CreateQuestionList()
    {
        selectedQuestions.Clear();

        // 첫 질문은 고정.
        // 플레이어에게 기본적으로 예/아니요 확인창을 넘기는 방식이라는 것을 학습시키는 역할.
        selectedQuestions.Add(new QuestionData(
            "설치를 계속하시겠습니까?",
            true,
            false,
            false
        ));

        // 질문 데이터 형식:
        // new QuestionData("질문 문장", 정답이 예인지, 버튼 위치 바꿀지, 버튼 색 반전할지)
        //
        // 예시:
        // true, false, false  = 정답은 예 / 버튼 위치 정상 / 색상 정상
        // false, true, false  = 정답은 아니요 / 버튼 위치 바꿈 / 색상 정상
        // true, true, true    = 정답은 예 / 버튼 위치 바꿈 / 색상 반전
        //
        // 함정 비율을 늘리면 난이도가 올라감.
        List<QuestionData> questionPool = new List<QuestionData>
        {
            // 누가 봐도 진행해야 하는 정상 설치 절차
            new QuestionData("필수 설치 파일을 복사하시겠습니까?", true, false, false),
            new QuestionData("손상된 설치 파일을 복구하시겠습니까?", true, false, false),
            new QuestionData("설치 무결성 검사를 진행하시겠습니까?", true, false, false),
            new QuestionData("필수 구성요소를 설치하시겠습니까?", true, false, false),
            new QuestionData("설치를 완료하시겠습니까?", true, false, false),
            new QuestionData("설치를 중단하지 않으시겠습니까?", true, false, false),

            // 누가 봐도 거부해야 하는 위험하거나 이상한 조항
            new QuestionData("추천 광고 프로그램을 함께 설치하시겠습니까?", false, false, false),
            new QuestionData("브라우저 시작 페이지를 임의로 변경하시겠습니까?", false, false, false),
            new QuestionData("사용자의 개인정보를 제3자에게 제공하시겠습니까?", false, false, false),
            new QuestionData("모든 파일 접근 권한을 허용하시겠습니까?", false, false, false),
            new QuestionData("PC 성능 저하에 동의하시겠습니까?", false, false, false),
            new QuestionData("원치 않는 프로그램을 추가로 설치하시겠습니까?", false, false, false),
            new QuestionData("설치 실패 시 모든 책임을 사용자에게 전가하시겠습니까?", false, false, false),
            new QuestionData("사용 기록을 무기한 저장하시겠습니까?", false, false, false),

            // 문장 구조, 버튼 위치, 색상으로 헷갈리게 만드는 함정 질문
            new QuestionData("제휴 프로그램 설치를 거부하시겠습니까?", true, true, false),
            new QuestionData("개인정보 제공에 동의하지 않으시겠습니까?", true, false, true),
            new QuestionData("보안 검사를 건너뛰지 않으시겠습니까?", true, true, true),
            new QuestionData("설치 완료를 중단하지 않으시겠습니까?", true, true, true)
        };

        Shuffle(questionPool);

        int randomQuestionCount = questionsPerRound - 1;

        for (int i = 0; i < randomQuestionCount && i < questionPool.Count; i++)
        {
            selectedQuestions.Add(questionPool[i]);
        }
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex >= selectedQuestions.Count)
        {
            ClearGame();
            return;
        }

        QuestionData currentQuestion = selectedQuestions[currentQuestionIndex];

        questionText.text = currentQuestion.question;
        progressText.text = "설치 확인 중...";
        resultText.text = "";

        currentTimer = timeLimitPerQuestion;

        ApplyButtonLayout(currentQuestion);
    }

    void SelectAnswer(bool selectedYes)
    {
        if (isCleared || isFailed) return;

        QuestionData currentQuestion = selectedQuestions[currentQuestionIndex];

        if (selectedYes == currentQuestion.correctAnswerIsYes)
        {
            currentQuestionIndex++;
            LoadQuestion();
        }
        else
        {
            FailGame("잘못된 선택입니다. 설치가 취소됩니다.");
        }
    }

    void ApplyButtonLayout(QuestionData question)
    {
        RectTransform yesRect = yesButton.GetComponent<RectTransform>();
        RectTransform noRect = noButton.GetComponent<RectTransform>();

        if (question.swapButtonPosition)
        {
            yesRect.anchoredPosition = rightButtonPosition;
            noRect.anchoredPosition = leftButtonPosition;
        }
        else
        {
            yesRect.anchoredPosition = leftButtonPosition;
            noRect.anchoredPosition = rightButtonPosition;
        }

        Image yesImage = yesButton.GetComponent<Image>();
        Image noImage = noButton.GetComponent<Image>();

        if (question.reverseButtonColor)
        {
            yesImage.color = noColor;
            noImage.color = yesColor;
        }
        else
        {
            yesImage.color = yesColor;
            noImage.color = noColor;
        }
    }

    void ClearGame()
    {
        isCleared = true;

        questionText.text = "확인 절차가 완료되었습니다.";
        timerText.text = "";
        progressText.text = "";
        resultText.text = "설치 확인 완료!";

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
    }

    void FailGame(string failMessage)
    {
        isFailed = true;

        resultText.text = failMessage;

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
    }

    void RetryGame()
    {
        // 현재는 테스트용으로 현재 미니게임 씬만 재시작.
        // 전체 구조 연결 후에는 메인 메뉴 또는 1단계 씬으로 이동하도록 수정 예정.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Shuffle(List<QuestionData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            QuestionData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}