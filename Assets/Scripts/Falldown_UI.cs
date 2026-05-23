using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Falldown_UI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image[] lifeImages;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverProgressText;
    [SerializeField] private Button failRestartButton;

    [Header("Win Screen")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private Button winNextButton;

    void Start()
    {
        if (winNextButton != null)
        {
            winNextButton.onClick.RemoveAllListeners();
            winNextButton.onClick.AddListener(GoToNextStage);
            SetButtonText(winNextButton, "다음 단계");
        }

        if (failRestartButton != null)
        {
            failRestartButton.onClick.RemoveAllListeners();
            failRestartButton.onClick.AddListener(GoToMainAfterFail);
            SetButtonText(failRestartButton, "처음부터 다시");
        }

        HideResultScreens();
    }

    public void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        if (progressText != null)
        {
            progressText.text = progress.ToString("F0") + "%";
        }
    }

    public void UpdateLives(int lives)
    {
        if (lifeImages == null) return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled = i < lives;
            }
        }
    }

    public void ShowGameOverScreen(float finalProgress)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (gameOverProgressText != null)
        {
            gameOverProgressText.enableWordWrapping = false;
            gameOverProgressText.text = "실패! 진행률: " + finalProgress.ToString("F0") + "%";
        }

        if (failRestartButton != null)
        {
            failRestartButton.gameObject.SetActive(true);
            failRestartButton.transform.SetAsLastSibling();
        }

        if (winNextButton != null)
        {
            winNextButton.gameObject.SetActive(false);
        }
    }

    public void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (winText != null)
        {
            winText.text = "다운로드 완료!";
        }

        if (winNextButton != null)
        {
            winNextButton.gameObject.SetActive(true);
            winNextButton.transform.SetAsLastSibling();
        }

        if (failRestartButton != null)
        {
            failRestartButton.gameObject.SetActive(false);
        }
    }

    public void HideResultScreens()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (winNextButton != null)
        {
            winNextButton.gameObject.SetActive(false);
        }

        if (failRestartButton != null)
        {
            failRestartButton.gameObject.SetActive(false);
        }
    }

    // 기존 Falldown_Manager.cs에서 이 함수명을 호출하고 있어서 유지해야 함.
    // 내부적으로는 새 결과 화면 숨김 함수로 연결한다.
    public void HideGameOverScreen()
    {
        HideResultScreens();
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

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
}