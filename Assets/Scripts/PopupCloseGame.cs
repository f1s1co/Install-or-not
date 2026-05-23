using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 오류창 닫기 미니게임
// 광고/추적/제휴 팝업은 닫아야 하고,
// 중요 설치 팝업을 닫으면 실패하는 구조.
public class PopupCloseGame : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text countText;
    public TMP_Text resultText;

    public Button nextButton;
    public Button retryButton;

    [Header("Popup Prefab")]
    public PopupWindow popupPrefab;
    public Transform popupParent;

    [Header("Game Settings")]
    public float timeLimit = 15f;
    public int targetCloseCount = 15;
    public int maxActivePopups = 7;
    public int initialPopupCount = 2;
    public float spawnInterval = 0.7f;
    public float safeStartDuration = 4f;

    [Range(0f, 1f)]
    public float importantPopupChance = 0.25f;

    [Header("Spawn Area")]
    public float minX = -650f;
    public float maxX = 650f;
    public float minY = -230f;
    public float maxY = 220f;

    private float currentTimer;
    private float elapsedTime = 0f;
    private float spawnTimer = 0f;

    private int closedUsefulPopupCount = 0;

    private bool isCleared = false;
    private bool isFailed = false;

    private List<PopupWindow> activePopups = new List<PopupWindow>();

    private string[] closePopupMessages =
    {
        "광고 프로그램 설치 안내",
        "추천 툴바 설치 대기 중",
        "브라우저 시작 페이지 변경 준비",
        "사용 기록 전송 요청",
        "개인 맞춤 광고 모듈 활성화",
        "불필요한 제휴 프로그램 설치 중",
        "추적 쿠키 적용 대기 중",
        "광고 알림 서비스 실행 준비",
        "제휴 프로그램 다운로드 대기 중",
        "시작 페이지 변경 안내"
    };

    private string[] importantPopupMessages =
    {
        "필수 파일 복구 중",
        "설치 무결성 검사 중",
        "핵심 구성요소 설치 중",
        "보안 검증 진행 중",
        "설치 데이터 저장 중",
        "시스템 파일 확인 중"
    };

    void Start()
    {
        currentTimer = timeLimit;
        elapsedTime = 0f;
        spawnTimer = 0f;

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        resultText.text = "광고/추적 팝업만 닫고, 중요 설치 창은 닫지 마세요.";

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(GoToNextStage);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(GoToMainAfterFail);

        SetButtonText(nextButton, "다음 단계");
        SetButtonText(retryButton, "처음부터 다시");

        UpdateUI();

        // 시작할 때는 너무 갑자기 난장판이 되지 않도록 적은 수만 생성
        for (int i = 0; i < initialPopupCount; i++)
        {
            SpawnPopup();
        }
    }

    void Update()
    {
        if (isCleared || isFailed) return;

        elapsedTime += Time.deltaTime;

        currentTimer -= Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0f, timeLimit);

        spawnTimer += Time.deltaTime;

        // 일정 시간마다 팝업을 추가 생성
        // 단, 동시에 떠 있는 팝업 수가 maxActivePopups보다 적을 때만 생성
        if (spawnTimer >= spawnInterval && activePopups.Count < maxActivePopups)
        {
            spawnTimer = 0f;
            SpawnPopup();
        }

        UpdateUI();

        if (currentTimer <= 0f)
        {
            FailGame("시간 초과입니다. 설치가 취소됩니다.");
        }
    }

    void SpawnPopup()
    {
        if (isCleared || isFailed) return;

        int closeableCount = CountCloseablePopups();

        bool canSpawnImportant = elapsedTime >= safeStartDuration;
        bool spawnImportant = canSpawnImportant && Random.value < importantPopupChance;

        // 닫아야 하는 팝업이 하나도 없으면 무조건 닫아야 하는 팝업 생성.
        if (closeableCount <= 0)
        {
            spawnImportant = false;
        }

        string message;

        if (spawnImportant)
        {
            message = importantPopupMessages[Random.Range(0, importantPopupMessages.Length)];
        }
        else
        {
            message = closePopupMessages[Random.Range(0, closePopupMessages.Length)];
        }

        PopupWindow newPopup = Instantiate(popupPrefab, popupParent);

        RectTransform rect = newPopup.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );

        newPopup.gameObject.SetActive(true);
        newPopup.Init(message, !spawnImportant, this);

        // 새로 생성된 팝업은 맨 앞으로 올려서 새 창이 뜬 느낌을 줌
        newPopup.transform.SetAsLastSibling();

        activePopups.Add(newPopup);
    }

    public void OnPopupClosed(PopupWindow popup)
    {
        if (isCleared || isFailed) return;

        // shouldClose가 false면 중요한 설치 창을 닫은 것이므로 즉시 실패
        if (!popup.shouldClose)
        {
            FailGame("중요 설치 창을 닫았습니다. 설치가 취소됩니다.");
            return;
        }

        closedUsefulPopupCount++;

        activePopups.Remove(popup);
        Destroy(popup.gameObject);

        if (closedUsefulPopupCount >= targetCloseCount)
        {
            ClearGame();
            return;
        }

        // 팝업을 닫았는데 화면이 너무 비면 하나 생성해서 흐름 유지
        if (activePopups.Count < maxActivePopups)
        {
            SpawnPopup();
        }

        UpdateUI();
    }

    int CountCloseablePopups()
    {
        int count = 0;

        foreach (PopupWindow popup in activePopups)
        {
            if (popup != null && popup.shouldClose)
            {
                count++;
            }
        }

        return count;
    }

    void UpdateUI()
    {
        timerText.text = "남은 시간: " + currentTimer.ToString("F1");
        countText.text = "제거한 팝업: " + closedUsefulPopupCount + " / " + targetCloseCount;
    }

    void ClearGame()
    {
        isCleared = true;

        resultText.text = "불필요한 팝업을 모두 제거했습니다!";
        timerText.text = "";
        countText.text = "";

        ClearAllPopups();

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);
    }

    void FailGame(string message)
    {
        isFailed = true;

        resultText.text = message;
        timerText.text = "";
        countText.text = "";

        ClearAllPopups();

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
    }

    void ClearAllPopups()
    {
        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            if (activePopups[i] != null)
            {
                Destroy(activePopups[i].gameObject);
            }
        }

        activePopups.Clear();
    }

    void GoToNextStage()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnMiniGameClear();
        }
        else
        {
            Debug.LogWarning("GameFlowManager가 없어 현재 씬을 다시 시작합니다.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void GoToMainAfterFail()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnMiniGameFail();
        }
        else
        {
            Debug.LogWarning("GameFlowManager가 없어 현재 씬을 다시 시작합니다.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void SetButtonText(Button button, string text)
    {
        if (button == null) return;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
}