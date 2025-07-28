using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SlimeTextUI : MonoBehaviour, IPoolObject
{
    [SerializeField]private TextMeshProUGUI slimeText;
    public GameObject GameObject => gameObject;
    public string PoolID => "SlimeText";
    public int PoolSize => 1;

    private Coroutine retrunCoroutine;

    public void OnSpawnFromPool()
    {
        slimeText.text = "";
        gameObject.SetActive(true);
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
    }

    public void ShowText(string _text, Vector3 _worldPos)
    {
        slimeText.text = _text;
        transform.position= Camera.main.WorldToScreenPoint(_worldPos); 
        gameObject.SetActive(true);

        if (retrunCoroutine != null)
        {
            StopCoroutine(retrunCoroutine);
        }

        retrunCoroutine = StartCoroutine(ReturnText());
    }

    private IEnumerator ReturnText()
    {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
