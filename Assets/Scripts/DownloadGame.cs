using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Download minigame manager.
// The player must raise the download progress to 100%.
// During random verification phases, clicking the button causes failure.
public class DownloadGame : MonoBehaviour
{
    [Header("UI")]
    public Slider progressBar;
    public TMP_Text percentText;
    public TMP_Text statusText;
    public Button clickButton;
    public Button nextButton;
    public Button retryButton;

    [Header("Game Settings")]
    public float progress = 0f;

    // How much progress increases per click.
    public float increaseAmount = 7f;

    // How much progress decreases per second.
    public float decreaseAmount = 1f;

    [Header("Danger Zone Settings")]
    // Minimum time before the next danger zone starts.
    public float minSafeTime = 1f;

    // Maximum time before the next danger zone starts.
    public float maxSafeTime = 1.8f;

    // How long the danger zone lasts.
    public float dangerDuration = 1.6f;

    [Header("Button State Colors")]
    // 안전하게 누를 수 있는 상태의 버튼 색상
    public Color normalButtonColor = new Color(0.25f, 0.55f, 1f, 1f);

    // 누르면 실패하는 검증 상태의 버튼 색상
    public Color dangerButtonColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    private bool isCleared = false;
    private bool isFailed = false;
    private bool isDangerZone = false;

    private float dangerTimer = 0f;
    private float nextDangerTime = 0f;

    void Start()
    {
        // Hide result buttons at the start.
        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);

        // Register button events.
        clickButton.onClick.RemoveAllListeners();
        clickButton.onClick.AddListener(BoostDownload);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(GoToNextStage);

        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(GoToMainAfterFail);

        SetButtonText(nextButton, "다음 단계");
        SetButtonText(retryButton, "처음부터 다시");

        ScheduleNextDangerZone();

        statusText.text = "설치 파일을 다운로드하는 중입니다...";
        UpdateUI();
    }

    void Update()
    {
        if (isCleared || isFailed) return;

        // Progress slowly drops over time.
        progress -= decreaseAmount * Time.deltaTime;
        progress = Mathf.Clamp(progress, 0f, 100f);

        if (isDangerZone)
        {
            dangerTimer -= Time.deltaTime;

            if (dangerTimer <= 0f)
            {
                EndDangerZone();
            }
        }
        else
        {
            nextDangerTime -= Time.deltaTime;

            if (nextDangerTime <= 0f)
            {
                StartDangerZone();
            }
        }

        UpdateUI();
    }

    void BoostDownload()
    {
        if (isCleared || isFailed) return;

        // Clicking during file verification immediately fails the minigame.
        if (isDangerZone)
        {
            FailGame();
            return;
        }

        progress += increaseAmount;

        // If progress reaches 100 by clicking, clear immediately.
        if (progress >= 100f)
        {
            progress = 100f;
            UpdateUI();
            ClearGame();
            return;
        }

        progress = Mathf.Clamp(progress, 0f, 100f);
        UpdateUI();
    }

    void StartDangerZone()
    {
        isDangerZone = true;
        dangerTimer = dangerDuration;

        statusText.text = "파일 검증 중입니다. 누르지 마세요!";
        UpdateUI();
    }

    void EndDangerZone()
    {
        isDangerZone = false;

        statusText.text = "다운로드가 재개되었습니다. 버튼을 눌러 속도를 올리세요.";
        ScheduleNextDangerZone();
        UpdateUI();
    }

    void ScheduleNextDangerZone()
    {
        nextDangerTime = Random.Range(minSafeTime, maxSafeTime);
    }

    void ClearGame()
    {
        isCleared = true;
        isDangerZone = false;

        statusText.text = "다운로드 완료!";
        clickButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);

        UpdateUI();
    }

    void FailGame()
    {
        isFailed = true;
        isDangerZone = false;

        statusText.text = "설치가 취소되었습니다. 파일 검증 중 버튼을 눌렀습니다.";
        clickButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);

        UpdateUI();
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

    void UpdateUI()
    {
        progressBar.value = progress / 100f;
        percentText.text = Mathf.FloorToInt(progress) + "%";

        if (clickButton != null)
        {
            TMP_Text buttonText = clickButton.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = isDangerZone ? "누르지 마세요" : "다운로드 가속";
            }

            Image buttonImage = clickButton.GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.color = isDangerZone ? dangerButtonColor : normalButtonColor;
            }
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