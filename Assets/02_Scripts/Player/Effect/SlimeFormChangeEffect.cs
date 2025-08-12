using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SlimeFormChangeEffect : MonoBehaviour
{
    private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");

    [Header("연출용 오브젝트")]
    [SerializeField] private InputController inputController;
    [SerializeField] private SpriteRenderer playerRenderer; 
    [SerializeField] private Animator playerAnimator;      
    [SerializeField] private Material changeEffectMaterial;

    [Header("연출 파라미터")]
    [SerializeField] private string transformAnimTrigger = "Transform";
    [SerializeField] private float zoomInSize = 0.5f;
    [SerializeField] private float zoomTime = 0.5f;
    [SerializeField] private float slowTimeScale = 0.3f;
    [SerializeField] private float slowTimeDuration = 1.0f;

    private float mainCameraOriginalSize;
    private float blendAmountBackup;
    private Camera mainCamera;
    private Material originalMaterial;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera != null)
            mainCameraOriginalSize = mainCamera.orthographicSize;

        if (playerRenderer != null && playerRenderer.material.HasProperty(BlendAmount))
            blendAmountBackup = playerRenderer.material.GetFloat(BlendAmount);

        if (!playerRenderer.IsUnityNull())
            originalMaterial = playerRenderer.material;
    }

    public void StartFormChangeEffect(Action _onChange)
    {
        StartCoroutine(AnimStart(_onChange));
    }

    private IEnumerator AnimStart(Action _onChange)
    {
        // 입력 비활성화
        inputController.PlayerActions.Disable();
        // 임시 연출 머테리얼 적용
        playerRenderer.material = changeEffectMaterial;

        // 애니메이터 시간 독립
        var originUpdateMode = playerAnimator.updateMode;
        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // 시간 느리게
        Time.timeScale = slowTimeScale;

        // 카메라 줌인
        float t = 0f;
        float startSize = mainCamera.orthographicSize;
        while (t < zoomTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / zoomTime);
            mainCamera.orthographicSize = Mathf.Lerp(mainCameraOriginalSize, zoomInSize, lerp);
            yield return null;
        }
        mainCamera.orthographicSize = zoomInSize;

        // 플레이어 하얗게 (BlendAmount 1로)
        yield return StartCoroutine(BlendAmountLerp(blendAmountBackup, 1f, 0.3f));

        // 변신 애니메이션 
        playerAnimator.SetTrigger(transformAnimTrigger);

        // 애니메이션/슬로우 지속시간
        yield return new WaitForSecondsRealtime(slowTimeDuration);

        _onChange?.Invoke();
        // BlendAmount 복구
        yield return StartCoroutine(BlendAmountLerp(1f, blendAmountBackup, 0.3f));

        // 카메라 줌아웃 복구
        t = 0f;
        while (t < zoomTime)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / zoomTime);
            mainCamera.orthographicSize = Mathf.Lerp(zoomInSize, mainCameraOriginalSize, lerp);
            yield return null;
        }
        mainCamera.orthographicSize = mainCameraOriginalSize;

        // 시간 원복
        Time.timeScale = 1f;
        playerAnimator.updateMode = originUpdateMode;

        // 입력/머테리얼 원복
        inputController.PlayerActions.Enable();
        playerRenderer.material = originalMaterial;
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
