using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTextUI : MonoBehaviour
{
    private Vector3 startScreenPos;

    private float moveMaxHeight = 80f;
    private float lifeTime = 1f;
    private float timer = 0f;

    private TextMeshProUGUI damageText;
    private RectTransform rectTransform;

    public void Init(float _damage, Color _textColor)
    {
        if (damageText.IsUnityNull())
        {
            damageText = GetComponent<TextMeshProUGUI>();
        }

        if (rectTransform.IsUnityNull())
        {
            rectTransform = GetComponent<RectTransform>();
        }

        damageText.text = _damage.ToString("N1");
        damageText.color = _textColor;
        timer = 0f;

        startScreenPos = rectTransform.position;

        StartCoroutine(DamageTextAnim());
    }

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }
    
    private IEnumerator DamageTextAnim()
    {
        timer = 0f;
        while (timer < lifeTime)
        {
            float t= timer/lifeTime;
            float textYOffset = -4 * moveMaxHeight * Mathf.Pow(t - 0.5f, 2) + moveMaxHeight;
            rectTransform.position= startScreenPos+Vector3.up*textYOffset;
            
            Color textColor= damageText.color;
            textColor.a = 1f - t;
            damageText.color = textColor;
            
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}