using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 설치 항목 검사 미니게임
// 항목 이름과 설명을 읽고 [설치 허용] 또는 [차단]을 선택하는 게임.
// 필수 설치 항목은 허용해야 하고, 위험하거나 말장난성 독소 항목은 차단해야 함.
public class InstallItemCheckGame : MonoBehaviour
{
    [System.Serializable]
    public class InstallItemData
    {
        public string itemName;
        public string itemDescription;
        public bool shouldAllow; // true면 설치 허용, false면 차단

        public InstallItemData(string itemName, string itemDescription, bool shouldAllow)
        {
            this.itemName = itemName;
            this.itemDescription = itemDescription;
            this.shouldAllow = shouldAllow;
        }
    }

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text countText;
    public TMP_Text resultText;

    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    public Button allowButton;
    public Button blockButton;
    public Button nextButton;
    public Button retryButton;

    [Header("Difficulty Settings")]
    // 난이도 조절 구간
    // timeLimit: 전체 제한 시간. 낮을수록 어려움
    // targetCheckCount: 처리해야 하는 항목 수. 많을수록 어려움
    public float timeLimit = 20f;
    public int targetCheckCount = 10;

    private float currentTimer;
    private int checkedItemCount = 0;

    private bool isCleared = false;
    private bool isFailed = false;

    private List<InstallItemData> selectedItems = new List<InstallItemData>();
    private int currentItemIndex = 0;

    void Start()
    {
        currentTimer = timeLimit;

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        resultText.text = "항목 설명을 읽고 설치 허용 또는 차단을 선택하세요.";

        allowButton.onClick.AddListener(() => SelectAnswer(true));
        blockButton.onClick.AddListener(() => SelectAnswer(false));
        retryButton.onClick.AddListener(RetryGame);

        CreateItemList();
        LoadCurrentItem();
        UpdateUI();
    }

    void Update()
    {
        if (isCleared || isFailed) return;

        currentTimer -= Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0f, timeLimit);

        UpdateUI();

        if (currentTimer <= 0f)
        {
            FailGame("시간 초과입니다. 설치가 취소됩니다.");
        }
    }

    void CreateItemList()
    {
        selectedItems.Clear();

        List<InstallItemData> itemPool = new List<InstallItemData>
        {
            // 정상적으로 설치 허용해야 하는 필수 항목
            new InstallItemData(
                "필수 실행 파일",
                "프로그램 실행에 반드시 필요한 핵심 파일입니다.",
                true
            ),
            new InstallItemData(
                "설치 무결성 검사",
                "설치 파일이 손상되거나 변조되지 않았는지 확인합니다.",
                true
            ),
            new InstallItemData(
                "손상 파일 복구 모듈",
                "누락되거나 깨진 설치 파일을 정상 상태로 복구합니다.",
                true
            ),
            new InstallItemData(
                "필수 구성요소 설치",
                "프로그램 실행에 필요한 기본 구성요소를 설치합니다.",
                true
            ),
            new InstallItemData(
                "보안 검증 모듈",
                "설치 중 위험 파일이 포함되어 있는지 검사합니다.",
                true
            ),
            new InstallItemData(
                "악성 파일 차단 기능",
                "설치 중 발견된 위험 파일을 차단합니다.",
                true
            ),
            new InstallItemData(
                "광고 모듈 제거 도구",
                "설치 과정에 포함된 불필요한 광고 모듈을 제거합니다.",
                true
            ),
            new InstallItemData(
                "오류 복구 지원",
                "설치 실패 시 원인을 확인하고 복구 절차를 실행합니다.",
                true
            ),
            new InstallItemData(
                "필수 스크립트 다운로드",
                "프로그램 실행에 필요한 필수 스크립트를 다운로드합니다.",
                true
            ),

            // 명확하게 차단해야 하는 독소 항목
            new InstallItemData(
                "개인정보 판매 모듈",
                "사용자의 이름, 연락처, 사용 기록을 외부 업체에 판매합니다.",
                false
            ),
            new InstallItemData(
                "시스템 파일 삭제 도구",
                "설치 속도 향상을 위해 Windows 핵심 파일 일부를 삭제합니다.",
                false
            ),
            new InstallItemData(
                "광고 강제 실행기",
                "프로그램 실행 중 광고 창을 항상 최상단에 표시합니다.",
                false
            ),
            new InstallItemData(
                "브라우저 강제 변경기",
                "사용자의 동의 없이 시작 페이지와 검색 엔진을 변경합니다.",
                false
            ),
            new InstallItemData(
                "키 입력 기록 전송기",
                "사용자의 키 입력 기록을 외부 서버로 전송합니다.",
                false
            ),
            new InstallItemData(
                "필수 스크립트 차단기",
                "프로그램 실행에 필요한 필수 스크립트 다운로드를 차단합니다.",
                false
            ),
            new InstallItemData(
                "보안 검사 건너뛰기",
                "설치 시간을 줄이기 위해 보안 검사를 생략합니다.",
                false
            ),
            new InstallItemData(
                "오류 보고 차단기",
                "설치 실패 원인을 확인할 수 없도록 오류 보고를 차단합니다.",
                false
            ),
            new InstallItemData(
                "무단 자동 결제 등록",
                "설치 완료 후 사용자의 동의 없이 유료 결제를 등록합니다.",
                false
            ),
            new InstallItemData(
                "개인 파일 전체 접근 허용",
                "프로그램과 무관한 사진, 문서, 다운로드 폴더까지 모두 접근합니다.",
                false
            ),
            new InstallItemData(
                "삭제 방지 잠금",
                "설치 후 사용자가 프로그램을 삭제하지 못하도록 제거 기능을 비활성화합니다.",
                false
            ),
            new InstallItemData(
                "백그라운드 상시 실행",
                "프로그램 종료 후에도 사용자의 동의 없이 계속 실행됩니다.",
                false
            )
        };

        Shuffle(itemPool);

        for (int i = 0; i < targetCheckCount && i < itemPool.Count; i++)
        {
            selectedItems.Add(itemPool[i]);
        }
    }

    void LoadCurrentItem()
    {
        if (currentItemIndex >= selectedItems.Count)
        {
            ClearGame();
            return;
        }

        InstallItemData currentItem = selectedItems[currentItemIndex];

        itemNameText.text = "이름: " + currentItem.itemName;
        itemDescriptionText.text = "설명: " + currentItem.itemDescription;
    }

    void SelectAnswer(bool selectedAllow)
    {
        if (isCleared || isFailed) return;

        InstallItemData currentItem = selectedItems[currentItemIndex];

        if (selectedAllow == currentItem.shouldAllow)
        {
            checkedItemCount++;
            currentItemIndex++;

            if (checkedItemCount >= targetCheckCount)
            {
                ClearGame();
                return;
            }

            LoadCurrentItem();
            UpdateUI();
        }
        else
        {
            if (currentItem.shouldAllow)
            {
                FailGame("필수 설치 항목을 차단했습니다. 설치가 취소됩니다.");
            }
            else
            {
                FailGame("위험 항목을 설치 허용했습니다. 설치가 취소됩니다.");
            }
        }
    }

    void UpdateUI()
    {
        timerText.text = "남은 시간: " + currentTimer.ToString("F1");
        countText.text = "검사 완료: " + checkedItemCount + " / " + targetCheckCount;
    }

    void ClearGame()
    {
        isCleared = true;

        itemNameText.text = "설치 항목 검사 완료";
        itemDescriptionText.text = "위험 항목을 차단하고 필수 항목을 확인했습니다.";
        resultText.text = "설치 항목 검사가 완료되었습니다!";

        timerText.text = "";
        countText.text = "";

        allowButton.gameObject.SetActive(false);
        blockButton.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
    }

    void FailGame(string message)
    {
        isFailed = true;

        resultText.text = message;

        allowButton.gameObject.SetActive(false);
        blockButton.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
    }

    void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Shuffle(List<InstallItemData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            InstallItemData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}