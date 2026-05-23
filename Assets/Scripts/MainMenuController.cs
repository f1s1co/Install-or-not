using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MainMenu 씬의 화면 전환과 단계 목록 표시를 담당하는 스크립트.
// GameFlowManager가 만든 랜덤 미니게임 목록을 받아와서
// 1~5단계 목록으로 보여주고, 현재 단계 시작 버튼을 처리한다.
public class MainMenuController : MonoBehaviour
{
    [System.Serializable]
    public class StageSlotUI
    {
        public GameObject slotObject;
        public TMP_Text stageNumberText;
        public TMP_Text stageNameText;
        public TMP_Text stageDescriptionText;
        public TMP_Text stageStatusText;
    }

    [Header("Main Texts")]
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text infoText;

    [Header("Stage List")]
    public GameObject stageListPanel;
    public StageSlotUI[] stageSlots;

    [Header("Main Button")]
    public Button mainActionButton;
    public TMP_Text mainActionButtonText;

    [Header("Position Settings")]
    public Vector2 titleStartPosition = new Vector2(0f, 220f);
    public Vector2 titleStagePosition = new Vector2(0f, 520f);

    public Vector2 buttonStartPosition = new Vector2(0f, -40f);
    public Vector2 buttonStagePosition = new Vector2(0f, -335f);

    private RectTransform titleRect;
    private RectTransform buttonRect;

    private bool isStageListShown = false;

    void Start()
    {
        titleRect = titleText.GetComponent<RectTransform>();
        buttonRect = mainActionButton.GetComponent<RectTransform>();

        mainActionButton.onClick.RemoveAllListeners();
        mainActionButton.onClick.AddListener(OnMainActionButtonClicked);

        // 미니게임을 성공하고 MainMenu로 돌아온 경우에는
        // 기존 단계 목록을 그대로 다시 보여준다.
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.HasGeneratedStages())
        {
            ShowStageListScreen();
            UpdateStageListUI();
        }
        else
        {
            ShowStartScreen();
        }
    }

    void ShowStartScreen()
    {
        isStageListShown = false;

        titleRect.anchoredPosition = titleStartPosition;
        buttonRect.anchoredPosition = buttonStartPosition;

        subtitleText.gameObject.SetActive(true);
        infoText.gameObject.SetActive(true);
        stageListPanel.SetActive(false);

        mainActionButtonText.text = "설치 시작";

        mainActionButton.transform.SetAsLastSibling();
    }

    void ShowStageListScreen()
    {
        isStageListShown = true;

        titleRect.anchoredPosition = titleStagePosition;
        buttonRect.anchoredPosition = buttonStagePosition;

        subtitleText.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        stageListPanel.SetActive(true);

        mainActionButton.transform.SetAsLastSibling();
    }

    void OnMainActionButtonClicked()
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("GameFlowManager가 존재하지 않습니다.");
            return;
        }

        // 처음 설치 시작을 누른 경우
        if (!isStageListShown)
        {
            GameFlowManager.Instance.StartNewInstallProcess();

            ShowStageListScreen();
            UpdateStageListUI();

            return;
        }

        // 단계 목록이 이미 열린 상태에서 누르면 현재 단계 미니게임으로 이동
        GameFlowManager.Instance.LoadCurrentStage();
    }

    void UpdateStageListUI()
    {
        if (GameFlowManager.Instance == null) return;

        List<GameFlowManager.MiniGameInfo> selectedStages = GameFlowManager.Instance.GetSelectedStages();
        int currentStageIndex = GameFlowManager.Instance.GetCurrentStageIndex();

        for (int i = 0; i < stageSlots.Length; i++)
        {
            if (stageSlots[i] == null || stageSlots[i].slotObject == null)
            {
                continue;
            }

            if (i >= selectedStages.Count)
            {
                stageSlots[i].slotObject.SetActive(false);
                continue;
            }

            stageSlots[i].slotObject.SetActive(true);

            GameFlowManager.MiniGameInfo info = selectedStages[i];

            stageSlots[i].stageNumberText.text = (i + 1) + "단계";
            stageSlots[i].stageNameText.text = info.displayName;
            stageSlots[i].stageDescriptionText.text = info.description;

            if (i < currentStageIndex)
            {
                stageSlots[i].stageStatusText.text = "완료";
                SetSlotAlpha(stageSlots[i], 0.45f);
            }
            else if (i == currentStageIndex)
            {
                stageSlots[i].stageStatusText.text = "진행";
                SetSlotAlpha(stageSlots[i], 1f);
            }
            else
            {
                stageSlots[i].stageStatusText.text = "대기";
                SetSlotAlpha(stageSlots[i], 0.75f);
            }
        }

        if (currentStageIndex < selectedStages.Count)
        {
            mainActionButtonText.text = (currentStageIndex + 1) + "단계 시작";
        }
        else
        {
            mainActionButtonText.text = "설치 완료";
        }
    }

    void SetSlotAlpha(StageSlotUI slot, float alpha)
    {
        SetTextAlpha(slot.stageNumberText, alpha);
        SetTextAlpha(slot.stageNameText, alpha);
        SetTextAlpha(slot.stageDescriptionText, alpha);
        SetTextAlpha(slot.stageStatusText, alpha);
    }

    void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}