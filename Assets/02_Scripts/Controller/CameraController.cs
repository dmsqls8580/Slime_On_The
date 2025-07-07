using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float minZoomSize;
    [SerializeField] private float maxZoomSize;
    
    private float zoomStep = 0.5f;

    private CameraShake  cameraShake;

    private void Awake()
    {
        cameraShake = FindObjectOfType<CameraShake>();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        float scroll= Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f)
        {
            CameraZoomOut();
        }
        else if (scroll > 0f)
        {
            CameraZoomIn();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            CameraShake();
        }
    }
    
    public void CameraShake()
    {
        cameraShake.Shake(1,1,1);
    }    
    public void StopCameraShake()
    {
        cameraShake.Shake(0,0,0);
    }

    private void CameraZoomIn()
    {
        float size = virtualCamera.m_Lens.OrthographicSize;
        size -= zoomStep;
        size = Mathf.Clamp(size, minZoomSize, maxZoomSize);
        virtualCamera.m_Lens.OrthographicSize = size;
    }

    private void CameraZoomOut()
    {
        float size = virtualCamera.m_Lens.OrthographicSize;
        size += zoomStep;
        size = Mathf.Clamp(size, minZoomSize, maxZoomSize);
        virtualCamera.m_Lens.OrthographicSize = size;
    }
}