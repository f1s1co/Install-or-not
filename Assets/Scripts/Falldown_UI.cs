using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Button restartButton;

    [Header("Win Screen")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;

    private Falldown_Manager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<Falldown_Manager>();

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => { 
                SceneManager.LoadScene(0);
            });
        }
        HideGameOverScreen();


    }

    public void UpdateProgress(float progress)
    {
        progressSlider.value = progress;
        progressText.text = progress.ToString("F0") + "%";
    }

    public void UpdateLives(int lives)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].enabled = i < lives;
        }
    }

    public void ShowGameOverScreen(float finalProgress)
    {
        gameOverPanel.SetActive(true);
        winPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }

        gameOverProgressText.enableWordWrapping = false;
        gameOverProgressText.text = "Failed! Progress is " + finalProgress.ToString("F0") + "%";
    }

    public void ShowWinScreen()
    {
        winPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        winText.text = "Successfully installed!";
    }

    public void HideGameOverScreen()
    {
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }
    }
}
