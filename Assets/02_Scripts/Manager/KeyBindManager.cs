using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class KeyBindManager : Singleton<KeyBindManager>
{
    private string waitingForInputTarget = null;
    public static event Action OnBindingChanged;

    // 금지 키
    private readonly HashSet<Key> bannedKeys = new()
    {
        Key.Escape, 
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0
    };

    // 지원 액션 리스트
    private readonly Dictionary<string, InputAction> rebindableActions = new();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        var inputActions = InputController.Instance.PlayerInputs.asset;
        
        // 허용된 액션 등록
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                switch (action.name)
                {
                    case "Move":
                    case "Dash":
                    case "Inventory":
                    case "Crafting":
                    case "Interaction":
                    case "Gathering":
                        rebindableActions[action.name] = action;
                        break;
                }
            }
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadRebinds();

        var input = InputController.Instance.PlayerInputs;

        string overrides = PlayerPrefs.GetString("rebinds");
        input.asset.LoadBindingOverridesFromJson(overrides);

        input.Enable();
    }

    // 외부에서 리바인딩 요청
    public void StartRebind(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) return;
        
        waitingForInputTarget = target;
        OnBindingChanged?.Invoke();

        // Move 복합 입력 (Up/Down/Left/Right)
        if (target is "up" or "down" or "left" or "right")
        {
            StartCompositeRebind("Move", target);
        }
        else if (rebindableActions.TryGetValue(target, out var action))
        {
            StartSimpleRebind(action);
        }
    }
    
    // 복합 입력 리바인딩
    private void StartCompositeRebind(string actionName, string partName)
    {
        if (!rebindableActions.TryGetValue(actionName, out var action)) return;

        int bindingIndex = action.bindings.ToList()
            .FindIndex(b => b.isPartOfComposite && b.name == partName);

        if (bindingIndex == -1) return;
        
        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape") // ESC 입력 시 리바인딩 취소
            .OnMatchWaitForAnother(0.1f)
            .OnPotentialMatch(op =>
            {
                if (!IsValidBinding(op.selectedControl))
                {
                    //SoundManager.Instance.PlaySFX(error);
                    op.Dispose();
                    StartCompositeRebind(actionName, partName);
                }
            })
            .OnComplete(op =>
            {
                RemoveDuplicateBindings(action, op.selectedControl); // 중복 제거
                SaveRebinds();
                waitingForInputTarget = null;
                OnBindingChanged?.Invoke();
                op.Dispose();
                action.Enable();
            })
            .Start();
    }

    // 단일 키 리바인딩
    private void StartSimpleRebind(InputAction action)
    {
        Debug.Log($"[Rebind] Action: {action.name}");

        // 바인딩 목록 디버깅
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            Debug.Log($"[{i}] name='{b.name}' path='{b.path}' isPartOfComposite={b.isPartOfComposite}");
        }

        // 키보드 바인딩 중 첫 번째 유효한 인덱스 찾기
        int bindingIndex = action.bindings.ToList()
            .FindIndex(b => !b.isPartOfComposite && b.path.StartsWith("<Keyboard>"));

        if (bindingIndex == -1)
        {
            Debug.LogWarning($"[Rebind] '{action.name}'에 유효한 키보드 바인딩이 없음");
            return;
        }

        action.Disable();

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnPotentialMatch(op =>
            {
                if (!IsValidBinding(op.selectedControl))
                {
                    op.Dispose();
                    StartSimpleRebind(action);
                }
            })
            .OnComplete(op =>
            {
                RemoveDuplicateBindings(action, op.selectedControl);
                SaveRebinds();
                waitingForInputTarget = null;
                OnBindingChanged?.Invoke();
                op.Dispose();
                action.Enable();
            })
            .Start();
    }

    // 유효한 키인지 검사
    private bool IsValidBinding(InputControl control)
    {
        if (control.device is not Keyboard kb) return false;
        if (control is KeyControl keyCtrl && bannedKeys.Contains(keyCtrl.keyCode)) return false;
        return true;
    }

    // 같은 키가 다른 액션에 바인딩되어 있을 경우 해당 액션의 바인딩 제거
    private void RemoveDuplicateBindings(InputAction targetAction, InputControl newControl)
    {
        foreach (var action in rebindableActions.Values)
        {
            if (action == targetAction) continue;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].effectivePath == newControl.path)
                {
                    action.ChangeBinding(i).Erase();
                }
            }
        }
    }

    // 액션 이름으로 현재 바인딩된 키를 <sprite name="xxx"> 형식으로 반환
    public string GetBindingText(string target)
    {
        if (waitingForInputTarget != null && 
            waitingForInputTarget.Equals(target, StringComparison.OrdinalIgnoreCase))
        {
            return "-";
        }
        
        // Move 액션의 조합 키(up/down/left/right)
        if (target is "Up" or "Down" or "Left" or "Right")
        {
            if (rebindableActions.TryGetValue("Move", out var move))
            {
                var binding = move.bindings.FirstOrDefault(b =>
                    b.isPartOfComposite &&
                    b.name.Equals(target, StringComparison.OrdinalIgnoreCase));

                if (binding != null)
                {
                    string readable = InputControlPath.ToHumanReadableString(binding.effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    return ConvertToSpriteText(readable);
                }
            }

            return "???";
        }

        // 일반 단일 키 (Inventory, Crafting 등)
        if (rebindableActions.TryGetValue(target, out var action))
        {
            var binding = action.bindings.FirstOrDefault(b => !b.isPartOfComposite);
            if (binding != null)
            {
                string readable = InputControlPath.ToHumanReadableString(binding.effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                return ConvertToSpriteText(readable);
            }
        }

        return "???";
    }

    // 키 이름을 <sprite name=""> 형식으로 변환
    private string ConvertToSpriteText(string readable)
    {
        return readable switch
        {
            "up" => "<sprite name=\"ku\">",
            "down" => "<sprite name=\"kd\">",
            "left" => "<sprite name=\"kl\">",
            "right" => "<sprite name=\"kr\">",
            "Control" => "<sprite name=\"ctrl\">",
            "Left Control" => "<sprite name=\"ctrl\">",
            "Right Control" => "<sprite name=\"ctrl\">",
            _ => $"<sprite name=\"{readable.ToLower()}\">"
        };
    }
    
    public void SaveRebinds()
    {
        var asset = InputController.Instance.PlayerInputs.asset;

        // 이건 OK (현재 바인딩 전체를 저장)
        string overrides = asset.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString("rebinds", overrides);
        PlayerPrefs.Save();

        Debug.Log("[KeyBindManager] Saved Full Rebind JSON:");
        Debug.Log(overrides);
    }
    
    public void LoadRebinds()
    {
        if (!PlayerPrefs.HasKey("rebinds"))
        {
            Debug.Log("[KeyBindManager] No saved rebinds found.");
            return;
        }

        var controller = InputController.Instance;
        var input = controller.PlayerInputs;

        if(input == null) return;
        
        input.Disable();
        string overrides = PlayerPrefs.GetString("rebinds");
        
        Debug.Log("[KeyBindManager] Loading Rebind JSON:");
        Debug.Log(overrides);
        
        input.asset.LoadBindingOverridesFromJson(overrides);

        controller.PlayerActions = input.Player;
        input.Enable();
        
        Debug.Log("[KeyBindManager] Applied Binding - Inventory: " +
                  controller.PlayerActions.Inventory.bindings[0].effectivePath);

        OnBindingChanged?.Invoke();
    }
    
    public void ResetToDefault()
    {
        var asset = InputController.Instance.PlayerInputs.asset;
        asset.RemoveAllBindingOverrides();
        OnBindingChanged?.Invoke();
        SaveRebinds();
    }
}
