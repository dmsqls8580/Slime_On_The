using Unity.VisualScripting;
using UnityEngine;

public class PlayerAfterEffect : MonoBehaviour
{
    [SerializeField] private float effectDelay=0.04f;
    [SerializeField] private float effectDuration=0.2f;
    [SerializeField] private Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);
    
    private float effectDelayTimer;
    public bool isEffectActive;
    public bool IsEffectActive => isEffectActive;
    private SpriteRenderer playerSprite;

    private EffectTable effectTable;

    private void Awake()
    {
        playerSprite= GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        effectTable = TableManager.Instance.GetTable<EffectTable>();
        effectDelayTimer = effectDelay;
        effectTable.CreateTable();
    }

    private void FixedUpdate()
    {
        if (!isEffectActive) return;

        if (effectDelayTimer > 0f)
        {
            effectDelayTimer -= Time.fixedDeltaTime;
        }
        else
        {
            var effectData = effectTable.GetDataByID(0);
            if (effectData.IsUnityNull())
            {
                return;
            }
            var effectObj= ObjectPoolManager.Instance.GetObject(effectData.poolID);
            if (effectObj.IsUnityNull())
            {
                return;
            }
            if (effectObj.TryGetComponent<AfterEffectImage>(out var afterEffectImage))
            {
                afterEffectImage.transform.position = transform.position;
                afterEffectImage.transform.rotation = transform.rotation;
                afterEffectImage.transform.localScale = transform.localScale;
                afterEffectImage.Init(playerSprite.sprite, afterImageColor, effectDuration, playerSprite.flipX,
                    playerSprite.flipY);
            }
            
            effectDelayTimer = effectDelay;
        }
    }

    public void SetEffectActive(bool _active)
    {
        isEffectActive = _active;
    }
}
