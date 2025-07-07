using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;

    private Transform playerTransform;
    private ItemSO itemSo;
    private int amount;

    private bool canItemToPlayer = false;
    public float attractSpeed = 0.2f;
    public float attractDistance = 2f;

    public void Init(ItemSO _itemSo, int _amount, Transform _player)
    {
        itemSo = _itemSo;
        amount = _amount;
        playerTransform = _player;
        if (iconRenderer != null && itemSo != null)
        {
            iconRenderer.sprite = itemSo.icon;
        }
    }

    private void Update()
    {
        if (playerTransform.IsUnityNull() || !canItemToPlayer) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        float itemMoveSpeed= Mathf.Lerp(1f, attractSpeed, distance/attractDistance);
        if (distance < attractDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position,
                itemMoveSpeed * Time.deltaTime);
        }
    }

    public void DropAnimation(Rigidbody2D _rigid, float _dropAngleRange, float _dropUpForce, float _dropSideForce)
    {
        if (_rigid != null)
        {
            float angle = Random.Range(-_dropAngleRange * 0.5f, _dropAngleRange * 0.5f);
            float radius = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(radius), Mathf.Cos(radius)).normalized;

            float randForce = Random.Range(_dropUpForce, _dropUpForce + 2);
            Vector2 force = dir * randForce + Vector2.right * Random.Range(-_dropSideForce, _dropSideForce);
            _rigid.AddForce(force, ForceMode2D.Impulse);
            _rigid.AddTorque(Random.Range(-3f, 3f), ForceMode2D.Impulse);

            StartCoroutine(StopDropAnim(_rigid));
        }
    }

    private IEnumerator StopDropAnim(Rigidbody2D _rigid)
    {
        yield return new WaitForSeconds(0.6f);
        if (_rigid != null)
        {
            _rigid.gravityScale = 0f;
            _rigid.velocity = Vector2.zero;
            _rigid.angularVelocity = 0f;
            _rigid.isKinematic = true;
        }

        yield return new WaitForSeconds(3f);
        canItemToPlayer = true;
    }
}