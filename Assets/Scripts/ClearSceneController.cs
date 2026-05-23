using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClearSceneController : MonoBehaviour
{
    public Button restartButton;

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(GoToMainMenu);
        }
    }

    void GoToMainMenu()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ResetInstallProcess();
        }

        SceneManager.LoadScene("MainMenu");
    }
}