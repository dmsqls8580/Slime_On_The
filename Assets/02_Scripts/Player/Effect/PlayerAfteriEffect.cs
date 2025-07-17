using System;
using UnityEngine;

public class PlayerAfteriEffect : MonoBehaviour
{
    [SerializeField] private float effectDelay;
    private float effectDelayTimer;
    [SerializeField] GameObject afteriEffectObject;
    private bool isEffectActive;

    private void Start()
    {
        effectDelayTimer = effectDelay;
    }

    private void FixedUpdate()
    {
        if (isEffectActive)
        {
            if (effectDelayTimer > 0)
            {
                effectDelayTimer -= Time.fixedDeltaTime;
            }
            else
            {
                GameObject curEffect = Instantiate(afteriEffectObject, transform.position, transform.rotation);
                Sprite curSprite = curEffect.GetComponent<SpriteRenderer>().sprite;
                curEffect.transform.localScale = transform.localScale;
                curEffect.GetComponent<SpriteRenderer>().sprite = curSprite;
                effectDelayTimer = effectDelay;
                Destroy(curEffect, 1f);
            }
        }
    }
}
