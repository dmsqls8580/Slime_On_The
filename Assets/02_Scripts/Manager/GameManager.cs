using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("외부 매니저 참조")]
    [SerializeField] private LoadingManager loadingManager;
    public TimeManager timeManager;

    public bool GodMode = false;
    public bool IsLoading { get; private set; } = true;

    private void Start()
    {
        StartCoroutine(loadingManager.BeginLoading(() =>
        {
            IsLoading = false;
            InputController.Instance.SetEnable(true);
            PlayerStatusManager.Instance.StartDaySlimeGaugeRoutine();
        }));
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.N))
        {
            GodMode = !GodMode;
        }
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
