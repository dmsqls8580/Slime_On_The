using UnityEngine;

public class AfterEffectImage : MonoBehaviour, IPoolObject
{
    public GameObject GameObject=>gameObject;

    [SerializeField]private string poolID = "AfterEffect";
    public string PoolID => poolID;
    
    [SerializeField] private int poolSize = 16;
    public int PoolSize => poolSize;
    
    private SpriteRenderer spriteRenderer;
    private float duration;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Sprite _sprite, Color _color, float _duration, bool _flipX, bool _flipY)
    {
        spriteRenderer.sprite = _sprite;
        spriteRenderer.color = _color;
        spriteRenderer.flipX = _flipX;
        spriteRenderer.flipY = _flipY;
        startColor = _color;
        duration = _duration;
        timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b,
            Mathf.Lerp(startColor.a, 0f, t));

        if (timer >= duration)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }

    public void OnSpawnFromPool()
    {
        timer = 0f;
        gameObject.SetActive(true);
    }

    public void OnReturnToPool()
    {
        spriteRenderer.color= startColor;
        gameObject.SetActive(false);
    }
}
