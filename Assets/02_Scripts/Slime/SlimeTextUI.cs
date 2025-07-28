using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SlimeTextUI : MonoBehaviour, IPoolObject
{
    [SerializeField]private TextMeshProUGUI slimeText;
    [SerializeField] private float paddingX = 30f; // 텍스트 양쪽 여백
    [SerializeField] private float minWidth = 100f;
    [SerializeField] private float maxWidth = 400f;
    
    [SerializeField]private RectTransform rectTransform;
    public GameObject GameObject => gameObject;
    public string PoolID => "SlimeText";
    public int PoolSize => 1;

    private Coroutine retrunCoroutine;
    private Action onReturnCallback;


    public void OnSpawnFromPool()
    {
        slimeText.text = "";
        gameObject.SetActive(true);
        onReturnCallback = null;
    }

    public void OnReturnToPool()
    {
        // 연출 종료, 상태 초기화
        if (retrunCoroutine != null)
        {
            StopCoroutine(retrunCoroutine);
            retrunCoroutine = null;
        }
        slimeText.text = "";
        gameObject.SetActive(false);
        onReturnCallback = null;
    }

    public void ShowText(string _text, Vector3 _worldPos, Action _onReturn = null)
    {
        slimeText.text = _text;
        SetTextObjectSize();
        
        onReturnCallback = _onReturn; 
        transform.position= Camera.main.WorldToScreenPoint(_worldPos); 

        if (retrunCoroutine != null)
        {
            StopCoroutine(retrunCoroutine);
        }

        retrunCoroutine = StartCoroutine(ReturnText());
    }

    private void SetTextObjectSize()
    {
        slimeText.ForceMeshUpdate(); // ← 중요! 텍스트가 갱신되지 않으면 크기 못 가져옴

        float textWidth = slimeText.preferredWidth;
        float textHeight = slimeText.preferredHeight;

        float paddingX = 0.25f;
        float paddingY = 0.3f;

        Vector2 newSize = new Vector2(
            textWidth + paddingX,
            textHeight + paddingY
        );

        rectTransform.sizeDelta = newSize;
    }

    public void OverrideText(string _text)
    {
        slimeText.text = _text;
        SetTextObjectSize();
    }
    
    private IEnumerator ReturnText()
    {
        yield return new WaitForSeconds(1f);
        
        onReturnCallback?.Invoke();
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
