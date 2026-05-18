using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 설치 방해 요소 피하기 미니게임
// 공식 설치 버튼은 눌러야 하지만,
// 그 위를 지나가는 가짜 광고 버튼을 누르면 실패하는 구조.
public class ButtonDodgeGame : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text countText;
    public TMP_Text resultText;

    public Button realInstallButton;
    public Button[] fakeButtons;

    public Button nextButton;
    public Button retryButton;

    [Header("Game Settings")]
    // timeLimit: 제한 시간
    // targetClickCount: 공식 설치 버튼을 눌러야 하는 횟수
    // fakeMoveSpeedMin/Max: 가짜 버튼 이동 속도 범위
    public float timeLimit = 12f;
    public int targetClickCount = 5;
    public float fakeMoveSpeedMin = 450f;
    public float fakeMoveSpeedMax = 800f;

    [Header("Random Direction Change")]
    // directionChangeIntervalMin/Max: 가짜 버튼이 방향을 바꾸는 시간 간격
    // directionChangeStrength: 기존 방향에 새 랜덤 방향을 얼마나 섞을지
    // 값이 높을수록 더 갑자기 꺾임
    public float directionChangeIntervalMin = 0.6f;
    public float directionChangeIntervalMax = 1.4f;
    public float directionChangeStrength = 0.45f;

    [Header("Move Area")]
    // 전체 이동 가능 범위
    public float minX = -650f;
    public float maxX = 650f;
    public float minY = -230f;
    public float maxY = 210f;

    [Header("Real Button Area")]
    // 공식 설치 버튼이 등장할 범위
    // 너무 가장자리로 가면 재미가 떨어져서 중앙 근처로 제한
    public float realMinX = -350f;
    public float realMaxX = 350f;
    public float realMinY = -120f;
    public float realMaxY = 130f;

    [Header("Fake Button Spawn")]
    // 가짜 버튼이 공식 버튼 주변을 지나가게 하기 위한 초기 배치 범위
    public float fakeSpawnRadiusX = 520f;
    public float fakeSpawnRadiusY = 260f;

    private float currentTimer;
    private int currentClickCount = 0;

    private bool isCleared = false;
    private bool isFailed = false;

    private RectTransform realButtonRect;
    private RectTransform[] fakeButtonRects;
    private Vector2[] fakeDirections;
    private float[] fakeSpeeds;

    private float[] directionChangeTimers;
    private float[] nextDirectionChangeTimes;

    private string[] fakeButtonTexts =
    {
        "초고속 다운로드",
        "PC 최적화 시작",
        "무료 쿠폰 받기",
        "광고 제거 다운로드",
        "지금 바로 실행",
        "필수 업데이트",
        "보너스 설치",
        "권장 프로그램 설치",
        "빠른 설치",
        "무료 백신 설치",
        "브라우저 업데이트",
        "광고 차단 다운로드",
        "제휴 설치 진행",
        "추가 구성요소 설치"
    };

    void Start()
    {
        currentTimer = timeLimit;

        realButtonRect = realInstallButton.GetComponent<RectTransform>();

        fakeButtonRects = new RectTransform[fakeButtons.Length];
        fakeDirections = new Vector2[fakeButtons.Length];
        fakeSpeeds = new float[fakeButtons.Length];

        directionChangeTimers = new float[fakeButtons.Length];
        nextDirectionChangeTimes = new float[fakeButtons.Length];

        for (int i = 0; i < fakeButtons.Length; i++)
        {
            fakeButtonRects[i] = fakeButtons[i].GetComponent<RectTransform>();
            ResetDirectionChangeTimer(i);
        }

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        resultText.text = "광고 버튼이 지나간 틈에 공식 설치 버튼을 누르세요.";

        realInstallButton.onClick.RemoveAllListeners();
        realInstallButton.onClick.AddListener(OnRealButtonClicked);

        for (int i = 0; i < fakeButtons.Length; i++)
        {
            int index = i;
            fakeButtons[index].onClick.RemoveAllListeners();
            fakeButtons[index].onClick.AddListener(() => OnFakeButtonClicked(index));
        }

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(RetryGame);

        SetupRound();
        UpdateUI();
    }

    void Update()
    {
        if (isCleared || isFailed) return;

        currentTimer -= Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0f, timeLimit);

        UpdateFakeDirectionTimers();
        MoveFakeButtons();
        KeepFakeButtonsAboveRealButton();

        UpdateUI();

        if (currentTimer <= 0f)
        {
            FailGame("시간 초과입니다. 설치가 취소됩니다.");
        }
    }

    void SetupRound()
    {
        RandomizeRealButton();
        RandomizeFakeButtonsAroundRealButton();
        KeepFakeButtonsAboveRealButton();
    }

    void RandomizeRealButton()
    {
        realButtonRect.anchoredPosition = new Vector2(
            Random.Range(realMinX, realMaxX),
            Random.Range(realMinY, realMaxY)
        );

        TMP_Text realText = realInstallButton.GetComponentInChildren<TMP_Text>();
        realText.text = "공식 설치";
    }

    void RandomizeFakeButtonsAroundRealButton()
    {
        Vector2 realPos = realButtonRect.anchoredPosition;

        for (int i = 0; i < fakeButtons.Length; i++)
        {
            // 공식 버튼 주변에서 시작하게 해서 실제로 버튼을 가리는 상황이 자주 나오게 함
            Vector2 spawnOffset = new Vector2(
                Random.Range(-fakeSpawnRadiusX, fakeSpawnRadiusX),
                Random.Range(-fakeSpawnRadiusY, fakeSpawnRadiusY)
            );

            Vector2 spawnPos = realPos + spawnOffset;
            spawnPos.x = Mathf.Clamp(spawnPos.x, minX, maxX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, minY, maxY);

            fakeButtonRects[i].anchoredPosition = spawnPos;

            TMP_Text fakeText = fakeButtons[i].GetComponentInChildren<TMP_Text>();
            fakeText.text = fakeButtonTexts[Random.Range(0, fakeButtonTexts.Length)];

            // 공식 버튼을 지나가도록 방향을 대략 공식 버튼 쪽으로 잡음
            Vector2 directionToReal = (realPos - spawnPos).normalized;

            // 완전히 직선으로만 가면 단조로우니 약간 랜덤성을 섞음
            Vector2 randomNoise = new Vector2(
                Random.Range(-0.6f, 0.6f),
                Random.Range(-0.6f, 0.6f)
            );

            Vector2 direction = directionToReal + randomNoise;

            if (direction.magnitude < 0.25f)
            {
                direction = GetRandomDirection();
            }

            fakeDirections[i] = direction.normalized;
            fakeSpeeds[i] = Random.Range(fakeMoveSpeedMin, fakeMoveSpeedMax);

            ResetDirectionChangeTimer(i);
        }
    }

    Vector2 GetRandomDirection()
    {
        Vector2 direction = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        );

        if (direction.magnitude < 0.25f)
        {
            direction = new Vector2(1f, 0.7f);
        }

        return direction.normalized;
    }

    void UpdateFakeDirectionTimers()
    {
        for (int i = 0; i < fakeButtons.Length; i++)
        {
            directionChangeTimers[i] += Time.deltaTime;

            if (directionChangeTimers[i] >= nextDirectionChangeTimes[i])
            {
                ChangeFakeDirectionSlightly(i);
                ResetDirectionChangeTimer(i);
            }
        }
    }

    void ChangeFakeDirectionSlightly(int index)
    {
        Vector2 randomDirection = GetRandomDirection();

        // 기존 방향에 랜덤 방향을 섞어서 너무 갑자기 꺾이지 않게 함
        fakeDirections[index] = Vector2.Lerp(
            fakeDirections[index],
            randomDirection,
            directionChangeStrength
        ).normalized;
    }

    void ResetDirectionChangeTimer(int index)
    {
        directionChangeTimers[index] = 0f;
        nextDirectionChangeTimes[index] = Random.Range(directionChangeIntervalMin, directionChangeIntervalMax);
    }

    void MoveFakeButtons()
    {
        for (int i = 0; i < fakeButtonRects.Length; i++)
        {
            RectTransform rect = fakeButtonRects[i];

            rect.anchoredPosition += fakeDirections[i] * fakeSpeeds[i] * Time.deltaTime;

            Vector2 pos = rect.anchoredPosition;

            // 화면 경계에 닿으면 튕김
            if (pos.x < minX || pos.x > maxX)
            {
                fakeDirections[i].x *= -1f;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
            }

            if (pos.y < minY || pos.y > maxY)
            {
                fakeDirections[i].y *= -1f;
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }

            rect.anchoredPosition = pos;
        }
    }

    void KeepFakeButtonsAboveRealButton()
    {
        // 공식 설치 버튼을 먼저 뒤쪽으로 보내고,
        // 가짜 버튼들을 위로 올려서 겹칠 때 가짜 버튼이 클릭을 먹게 함.
        realInstallButton.transform.SetAsLastSibling();

        for (int i = 0; i < fakeButtons.Length; i++)
        {
            fakeButtons[i].transform.SetAsLastSibling();
        }

        // 성공/실패 후 표시되는 버튼은 항상 맨 위에 위치
        nextButton.transform.SetAsLastSibling();
        retryButton.transform.SetAsLastSibling();
    }

    void OnRealButtonClicked()
    {
        if (isCleared || isFailed) return;

        currentClickCount++;

        if (currentClickCount >= targetClickCount)
        {
            ClearGame();
            return;
        }

        // 성공할 때마다 공식 버튼 위치와 광고 버튼 흐름을 다시 구성
        SetupRound();

        UpdateUI();
    }

    void OnFakeButtonClicked(int index)
    {
        if (isCleared || isFailed) return;

        FailGame("가짜 광고 버튼을 눌렀습니다. 설치가 취소됩니다.");
    }

    void UpdateUI()
    {
        timerText.text = "남은 시간: " + currentTimer.ToString("F1");
        countText.text = "설치 진행: " + currentClickCount + " / " + targetClickCount;
    }

    void ClearGame()
    {
        isCleared = true;

        resultText.text = "공식 설치 버튼을 모두 눌렀습니다!";
        timerText.text = "";
        countText.text = "";

        realInstallButton.gameObject.SetActive(false);

        foreach (Button fakeButton in fakeButtons)
        {
            fakeButton.gameObject.SetActive(false);
        }

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
    }

    void FailGame(string message)
    {
        isFailed = true;

        resultText.text = message;

        realInstallButton.gameObject.SetActive(false);

        foreach (Button fakeButton in fakeButtons)
        {
            fakeButton.gameObject.SetActive(false);
        }

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
    }

    void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}