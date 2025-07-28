using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SlimeFormChangeEffect : MonoBehaviour
{
    private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");

    [Header("연출용 오브젝트")]
    [SerializeField] private InputController inputController;
    [SerializeField] private Camera playerOnlyCamera; // 플레이어만 찍는 카메라
    [SerializeField] private RawImage playerRawImage; // RenderTexture 출력용 RawImage
    [SerializeField] private SpriteRenderer playerRenderer; 
    [SerializeField] private Animator playerAnimator;      
    [SerializeField]private Material changeEffectMaterial;

    [Header("연출 파라미터")]
    [SerializeField] private string transformAnimTrigger = "Transform";
    [SerializeField] private float zoomInSize = 0.5f;
    [SerializeField] private float zoomTime = 0.5f;
    [SerializeField] private float slowTimeScale = 0.3f;
    [SerializeField] private float slowTimeDuration = 1.0f;

    private Camera mainCamera;
    private int playerLayer;
    private int mainCameraOriginalMask;
    private float mainCameraOriginalSize;
    private float blendAmountBackup;
    
    private Material originalMaterial;
    private void Awake()
    {
        mainCamera = Camera.main;
        playerLayer = LayerMask.NameToLayer("Player");

        if (mainCamera != null)
            mainCameraOriginalSize = mainCamera.orthographicSize;

        if (playerRenderer != null && playerRenderer.material.HasProperty(BlendAmount))
            blendAmountBackup = playerRenderer.material.GetFloat(BlendAmount);
        
        if(!playerRenderer.IsUnityNull())
            originalMaterial = playerRenderer.material;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartFormChangeEffect();
        }
    }

    public void StartFormChangeEffect()
    {
        StartCoroutine(AnimStart());
    }

    private IEnumerator AnimStart()
    {
        inputController.PlayerActions.Disable();  
        playerRenderer.material = changeEffectMaterial;
        
        mainCameraOriginalMask = mainCamera.cullingMask;
        mainCamera.cullingMask &= ~(1 << playerLayer);
        
        playerOnlyCamera.orthographicSize = mainCameraOriginalSize;
        playerOnlyCamera.gameObject.SetActive(true);
        playerRawImage.gameObject.SetActive(true);
       
        var originUpdateMode = playerAnimator.updateMode;
        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        // 시간 느리게
        Time.timeScale = slowTimeScale;

        // 카메라 줌인
        float t = 0f;
        
        while (t < zoomTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / zoomTime);
            playerOnlyCamera.orthographicSize = Mathf.Lerp(mainCameraOriginalSize, zoomInSize, lerp);
            yield return null;
        }

        // 플레이어 하얗게 (BlendAmount 1로)
        yield return StartCoroutine(BlendAmountLerp(blendAmountBackup, 1f, 0.3f));

        // 변신 애니메이션 
        playerAnimator.SetTrigger(transformAnimTrigger);
        
        yield return new WaitForSecondsRealtime(slowTimeDuration);

        // BlendAmount 복구
        yield return StartCoroutine(BlendAmountLerp(1f, blendAmountBackup, 0.3f));

        // 카메라줌아웃
        t = 0f;
        while (t < zoomTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / zoomTime);
            playerOnlyCamera.orthographicSize = Mathf.Lerp(zoomInSize, mainCameraOriginalSize, lerp);
            yield return null;
        }
        playerOnlyCamera.orthographicSize = mainCameraOriginalSize;

        // 시간원래대로
        Time.timeScale = 1f;

        playerAnimator.updateMode = originUpdateMode;
        
        inputController.PlayerActions.Enable();
        playerRenderer.material = originalMaterial;
        
        mainCamera.cullingMask = mainCameraOriginalMask;
        
        playerOnlyCamera.gameObject.SetActive(false);
        playerRawImage.gameObject.SetActive(false);
    }

    private IEnumerator BlendAmountLerp(float _from, float _to, float _duration)
    {
        float t = 0;
        while (t < _duration)
        {
            t += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(_from, _to, t / _duration);
            playerRenderer.material.SetFloat(BlendAmount, value);
            yield return null;
        }
        playerRenderer.material.SetFloat(BlendAmount, _to);
    }
}
