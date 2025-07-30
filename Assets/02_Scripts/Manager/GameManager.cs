using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("외부 매니저 참조")]
    public TimeManager timeManager;
    public ProceduralWorldManager worldManager;

    public bool IsPaused { get; private set; } = false;

    private void Start()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);

        // 맵 생성 시작
        if (worldManager != null)
        {
            worldManager.GenerateWorld(seed);  // ProceduralWorldManager 내부 함수
        }
        else
        {
            Debug.LogWarning("WorldManager가 GameManager에 할당되지 않았습니다.");
        }
    }

    ///// <summary>
    ///// 시드 문자열을 받아 절차적 맵 생성
    ///// </summary>
    //public void StartGameWithSeed(string seedText)
    //{
    //    int seed;

    //    if (string.IsNullOrEmpty(seedText))
    //    {
    //        // 시드 미입력 시 랜덤 생성
    //        seed = Random.Range(int.MinValue, int.MaxValue);
    //        Debug.Log($"[랜덤 시드 생성] {seed}");
    //    }
    //    else if (!int.TryParse(seedText, out seed))
    //    {
    //        Debug.LogWarning("유효하지 않은 시드입니다. 숫자를 입력해주세요.");
    //        return;
    //    }

    //    if (worldManager != null)
    //    {
    //        worldManager.GenerateWorld(seed);
    //    }
    //    else
    //    {
    //        Debug.LogError("worldManager가 GameManager에 할당되지 않았습니다.");
    //    }
    //}

    /// <summary>
    /// 현재 씬을 다시 로드합니다.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 게임을 종료합니다.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
