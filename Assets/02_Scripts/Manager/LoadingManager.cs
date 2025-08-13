using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [Header("외부 참조")]
    [SerializeField] private ProceduralWorldManager worldManager;

    [Header("UI")]
    [SerializeField] private CanvasGroup loadingCanvas;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Slider loadingBar;

    private float currentProgress = 0f;
    private float targetProgress = 0f;

    public IEnumerator BeginLoading(Action onComplete)
    {
        InputController.Instance.SetEnable(false);
        ShowLoadingUI();

        int seed = GameSettings.seed != 0 ? GameSettings.seed : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
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
        worldManager.enemySpawnerPlacer.Place(
            worldManager.regionGenerator.TileToRegionMap,
            worldManager.biomeAssigner.RegionBiomes);
        yield return new WaitForSeconds(0.1f);

        // 5. 보스 스포너
        loadingText.text = "보스 스포너 배치 중...";
        targetProgress = 1.0f;
        worldManager.bossSpawnerPlacer.Place(
            worldManager.regionGenerator.TileToRegionMap,
            worldManager.biomeAssigner.RegionBiomes);
        yield return new WaitForSeconds(0.2f);

        // 게이지 100%까지 기다리기
        yield return new WaitUntil(() => currentProgress >= 0.999f);
        loadingText.text = "Loading... 100%";
        yield return new WaitForSeconds(0.4f);

        HideLoadingUI();

        onComplete?.Invoke();
    }

    private void ShowLoadingUI()
    {
        loadingCanvas.alpha = 1f;
        loadingCanvas.blocksRaycasts = true;
        loadingBar.value = 0f;
        currentProgress = 0f;
        targetProgress = 0f;
    }

    private void HideLoadingUI()
    {
        loadingCanvas.alpha = 0f;
        loadingCanvas.blocksRaycasts = false;
    }

    private void Update()
    {
        if (loadingBar != null && currentProgress < targetProgress)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 0.5f);
            loadingBar.value = currentProgress;
            int displayPercent = Mathf.FloorToInt(currentProgress * 100f);
            if (displayPercent >= 100) displayPercent = 99;
            loadingText.text = $"Loading... {displayPercent}%";
        }
    }
}
