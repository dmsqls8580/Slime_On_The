using _02_Scripts.Manager;
using PlayerStates;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public enum CookingState
{
    Idle,
    Cooking,
    Finished
}

public class CookPotObject : MonoBehaviour
{
    [Header("저장 데이터")]
    private int cookIndex = -1;
    private CookingState currentState = CookingState.Idle;
    private float currentTime = 0f;
    private float cookingTime = 0f;
    private ItemSO finishedItem = null;

    private InventoryManager inventoryManager;
    private UIManager uiManager;

    public int CookIndex => cookIndex;

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

    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                var ui = uiManager.GetUIComponent<UICookPot>();
                ui.Initialize(this);
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
            currentState = CookingState.Cooking;
            currentTime = 0;
            cookingTime = _cookingTime;
            finishedItem = _item;
            StartCoroutine(CookRoutine());
        }
    }

    private IEnumerator CookRoutine()
    {
        while (true)
        {
            while (currentTime < cookingTime)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }

            // 완료슬롯에 finishedItem추가.

            // 다음 요리 가능한지 확인
            if (currentTime == 3)
            {
                currentTime = 0;
            }
            else
            {
                break;
            }
        }

        currentState = CookingState.Finished;
    }
}
