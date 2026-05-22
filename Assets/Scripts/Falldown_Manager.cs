using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Falldown_Manager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int maxLives = 2;
    [SerializeField] private float progressPerFile = 5f;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private float minSpawnX = -8f;
    [SerializeField] private float maxSpawnX = 8f;
    [SerializeField] private float spawnY = 6f;

    [Header("UI References")]
    [SerializeField] private Falldown_UI uiManager;

    // Game State
    private int currentLives;
    private float currentProgress;
    private bool isGameOver;
    private bool isGameWon;

    void Start()
    {
        StartNewGame();
        
    }

    public void StartNewGame()
    {
        StopAllCoroutines();

        currentLives = maxLives;
        currentProgress = 0;
        isGameOver = false;
        isGameWon = false;

        // UI 업데이트
        uiManager.UpdateLives(currentLives);
        uiManager.UpdateProgress(currentProgress);
        uiManager.HideGameOverScreen();

        
        // 기존 맵에 남아있던 아이템 청소
        ClearRemainingItems();
        // 아이템 생성 시작
        StartCoroutine(SpawnItems());
    }

    IEnumerator SpawnItems()
    {
        while (!isGameOver && !isGameWon)
        {
            // 진행률에 따라 생성 간격 감소 (1초 → 0.4초)
            float spawnInterval = Mathf.Max(0.4f, 1f - currentProgress * 0.006f);
            yield return new WaitForSeconds(spawnInterval);

            SpawnRandomItem();
        }
    }

    void SpawnRandomItem()
    {
        if (isGameOver || isGameWon) return;

        // 랜덤 위치
        float randomX = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0);

        // 60% 파일, 40% 쓰레기
        GameObject prefabToSpawn = Random.value > 0.4f ? filePrefab : trashPrefab;
        
        GameObject spawnedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // 진행률에 따라 낙하 속도 증가
        float speedMultiplier = 1f + (currentProgress / 100f) * 2f;
        spawnedItem.GetComponent<Falldown_item>().SetFallSpeed(2f * speedMultiplier);
    }

    public void OnItemCaught(bool isFile)
    {
        if (isGameOver || isGameWon) return;

        if (isFile)
        {
            // 파일 수집
            currentProgress = Mathf.Min(100f, currentProgress + progressPerFile);
            uiManager.UpdateProgress(currentProgress);

            // 승리 체크
            if (currentProgress >= 100f)
            {
                isGameWon = true;

                StopAllCoroutines();
                ClearRemainingItems();

                if (uiManager != null)
                {
                    uiManager.ShowWinScreen();
                }


            }
        }
        else
        {
            // 쓰레기 수집 (패널티)
            currentLives--;
            uiManager.UpdateLives(currentLives);

            // 게임 오버 체크
            if (currentLives <= 0)
            {
                isGameOver = true;

                // 스폰 중지 및 화면 안의 모든 아이템 즉시 삭제
                StopAllCoroutines();
                ClearRemainingItems();

                if (uiManager != null)
                {
                    uiManager.ShowGameOverScreen(currentProgress);
                }
            }
            
        }
    }
    private void ClearRemainingItems()
    {
        Falldown_item[] remainingItems = FindObjectsOfType<Falldown_item>();
        foreach (var item in remainingItems)
        {
            if (item != null)
            {
                // 1. 내려오는 속도를 0으로 만들어 멈추게 함
                item.SetFallSpeed(0f);

                // 2. 캐릭터와 더 이상 부딪히지 않도록 Collider2D를 즉시 비활성화
                Collider2D itemCollider = item.GetComponent<Collider2D>();
                if (itemCollider != null)
                {
                    itemCollider.enabled = false;
                }

                // 3. 오브젝트 파괴
                Destroy(item.gameObject);
            }
        }
    }

    public float GetCurrentProgress()
    {
        return currentProgress;
    }
}