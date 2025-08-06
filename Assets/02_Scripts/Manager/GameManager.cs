using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [Header("외부 매니저 참조")]
    public TimeManager timeManager;
    public ProceduralWorldManager worldManager;
    
    //[SerializeField] private PlayerSpawner playerSpawner;

    [Header("로딩 화면 설정")]
    public CanvasGroup loadingCanvas;
    public TMP_Text loadingText;
    public Slider loadingBar;

    public bool IsLoading { get; private set; } = true;

    public bool GodMode=false;

    private float currentProgress = 0f;
    private float targetProgress = 0f;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
    
        // 점진적으로 로딩 바를 채움
        if (loadingBar != null && currentProgress < targetProgress)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);
            loadingBar.value = currentProgress;
            int displayPercent = Mathf.FloorToInt(currentProgress * 100f);
            if (displayPercent >= 100) displayPercent = 99;
            loadingText.text = $"Loading... {displayPercent}%";
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.N)) {
            GodMode = !GodMode;
            Logger.Log($"갓모드:{GodMode}");
        }
    }
    
    private IEnumerator Start()
    {  
        InputController.Instance.SetEnable(false);
        loadingCanvas.alpha = 1f;
        loadingCanvas.blocksRaycasts = true;
        loadingBar.value = 0f;
        currentProgress = 0f;
        targetProgress = 0f;

        int seed = GameSettings.seed != 0 ? GameSettings.seed : Random.Range(int.MinValue, int.MaxValue);
        GameSettings.seed = seed;
        Debug.Log($"[GameManager] 사용된 시드값: {seed}");
    
        // 1. 맵 생성
        yield return StartCoroutine(worldManager.GenerateWorldAsync(seed, (msg, prog) =>
        {
            loadingText.text = msg;
            targetProgress = prog * 0.8f; // 최대 80%
        }));
        
        // 2. 네비 베이크
        loadingText.text = "네비게이션 메쉬 생성 중...";
        targetProgress = 0.85f;
        NavMesh2DManager.Instance.BakeNavMesh();
        yield return new WaitForSeconds(0.2f);
    
        // 3. 풀 초기화
        loadingText.text = "오브젝트 풀 초기화 중...";
        targetProgress = 0.9f;
        ObjectPoolManager.Instance.InitializePools();
        yield return new WaitForSeconds(0.3f);
    
        // 4. 몬스터 스포너
        loadingText.text = "몬스터 스포너 배치 중...";
        targetProgress = 0.95f;
        worldManager.enemySpawnerPlacer.Place(worldManager.regionGenerator.TileToRegionMap, worldManager.biomeAssigner.RegionBiomes);
        yield return new WaitForSeconds(0.1f);
    
        // 5. 보스 스포너
        loadingText.text = "보스 스포너 배치 중...";
        targetProgress = 1.0f;
        worldManager.bossSpawnerPlacer.Place(worldManager.regionGenerator.TileToRegionMap, worldManager.biomeAssigner.RegionBiomes);
        yield return new WaitForSeconds(0.2f);
    
        // 완료 대기
        yield return new WaitUntil(() => currentProgress >= 0.999f);
    
        loadingText.text = "Loading... 100%";
        yield return new WaitForSeconds(0.4f);
    
        //playerSpawner.SpawnPlayer();
        // 로딩 종료
        InputController.Instance.SetEnable(true);
        PlayerStatusManager.Instance.StartDaySlimeGaugeRoutine();
        loadingCanvas.alpha = 0f;
        loadingCanvas.blocksRaycasts = false;
        IsLoading = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}