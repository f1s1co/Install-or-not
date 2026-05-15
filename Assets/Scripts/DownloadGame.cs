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
    public float increaseAmount = 5f;

    // How much progress decreases per second.
    public float decreaseAmount = 4f;

    [Header("Danger Zone Settings")]
    // Minimum time before the next danger zone starts.
    public float minSafeTime = 2f;

    // Maximum time before the next danger zone starts.
    public float maxSafeTime = 4f;

    // How long the danger zone lasts.
    public float dangerDuration = 2.5f;

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
        clickButton.onClick.AddListener(BoostDownload);
        retryButton.onClick.AddListener(RetryGame);

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

    void RetryGame()
    {
        // Reload current scene to reset the minigame completely.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        }
    }
}