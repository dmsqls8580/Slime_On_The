using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System;
using UnityEngine;

public enum CookingState
{
    Idle,
    Cooking,
    Finished
}

public class CookPotObject : MonoBehaviour
{
    [SerializeField] private int cookIndex = -1;

    private InventoryManager inventoryManager;
    private UIManager uiManager;
    public int GetCookIndex() => cookIndex;
    private bool hasCooked = false;

    private CookingState currentState = CookingState.Idle;
    private ItemSO finishedItem;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        uiManager = UIManager.Instance;
    }

    private void OnDisable()
    {
        inventoryManager.ReleaseCookIndex(cookIndex);
    }

    private void Start()
    {
        if (cookIndex < 0)
        {
            cookIndex = inventoryManager.GetNextAvailableCookIndex();
        }
    }

    // TODO: 요리하는거 동작 추가

    private void OnDisable()
    {
        InventoryManager.Instance.ReleaseCookIndex(cookIndex);
    }

    public void TryCook()
    {
        if (hasCooked) return;
        
        hasCooked = true;
    }

    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                var ui = uiManager.GetUIComponent<UICookPot>();
                ui.Initialize(cookIndex);
                uiManager.Toggle<UICookPot>();
                break;
            case InteractionCommandType.Space:
                break;
        }
    }

    public void StartCook(ItemSO _item, float _cookingTime)
    {
        if (currentState == CookingState.Idle)
        {
            StartCoroutine(CookRoutine(_item, _cookingTime));
        }
    }

    private IEnumerator CookRoutine(ItemSO _item, float _cookingTime)
    {
        // 상태 변경 및 변수 설정.
        currentState = CookingState.Cooking;
        finishedItem = _item;

        // TODO: 애니메이션 재생(?).

        // 요리 시간만큼 대기.
        yield return new WaitForSeconds(_cookingTime);

        // 요리 완료 처리.
        currentState = CookingState.Finished;

        // TODO: 완료 상태 스프라이트로 변경.
    }

    public int CookIndex => cookIndex;
}
