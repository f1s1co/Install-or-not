using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 전체 게임 흐름을 관리하는 매니저.
// 미니게임 목록, 랜덤 단계 생성, 현재 단계 이동, 성공/실패 처리를 담당한다.
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [System.Serializable]
    public class MiniGameInfo
    {
        public string sceneName;
        public string displayName;
        public string description;

        public MiniGameInfo(string sceneName, string displayName, string description)
        {
            this.sceneName = sceneName;
            this.displayName = displayName;
            this.description = description;
        }
    }

    [Header("Game Flow Settings")]
    public int stagesToPlay = 5;

    private List<MiniGameInfo> allMiniGames = new List<MiniGameInfo>();
    private List<MiniGameInfo> selectedStages = new List<MiniGameInfo>();

    private int currentStageIndex = 0;
    private bool hasGeneratedStages = false;

    void Awake()
    {
        // 씬이 바뀌어도 GameFlowManager가 유지되도록 함
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeMiniGameList();
    }

    void InitializeMiniGameList()
    {
        allMiniGames.Clear();

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_Download",
            "설치 파일 다운로드",
            "위험 구간을 피하며 다운로드를 완료하세요."
        ));

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_Confirm",
            "확인창 함정",
            "문구를 읽고 올바른 선택지를 고르세요."
        ));

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_Popup",
            "팝업 정리",
            "수상한 팝업을 구분해 닫으세요."
        ));

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_ItemCheck",
            "설치 항목 검사",
            "필수 항목은 허용하고 위험 항목은 차단하세요."
        ));

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_ButtonDodge",
            "광고 버튼 회피",
            "가짜 광고 버튼을 피해 공식 설치 버튼을 누르세요."
        ));

        allMiniGames.Add(new MiniGameInfo(
            "Minigame_falldown",
            "파일 수집",
            "정상 파일을 받고 악성코드는 피하세요."
        ));
    }

    public void StartNewInstallProcess()
    {
        GenerateRandomStages();
        currentStageIndex = 0;
        hasGeneratedStages = true;
    }

    void GenerateRandomStages()
    {
        selectedStages.Clear();

        List<MiniGameInfo> candidateList = new List<MiniGameInfo>(allMiniGames);

        for (int i = 0; i < stagesToPlay && candidateList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, candidateList.Count);
            selectedStages.Add(candidateList[randomIndex]);
            candidateList.RemoveAt(randomIndex);
        }
    }

    public List<MiniGameInfo> GetSelectedStages()
    {
        return selectedStages;
    }

    public int GetCurrentStageIndex()
    {
        return currentStageIndex;
    }

    public bool HasGeneratedStages()
    {
        return hasGeneratedStages;
    }

    public void LoadCurrentStage()
    {
        if (!hasGeneratedStages || selectedStages.Count == 0)
        {
            StartNewInstallProcess();
        }

        if (currentStageIndex >= selectedStages.Count)
        {
            SceneManager.LoadScene("ClearScene");
            return;
        }

        string sceneName = selectedStages[currentStageIndex].sceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void OnMiniGameClear()
    {
        currentStageIndex++;

        if (currentStageIndex >= selectedStages.Count)
        {
            SceneManager.LoadScene("ClearScene");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnMiniGameFail()
    {
        ResetInstallProcess();
        SceneManager.LoadScene("MainMenu");
    }

    public void ResetInstallProcess()
    {
        selectedStages.Clear();
        currentStageIndex = 0;
        hasGeneratedStages = false;
    }
}