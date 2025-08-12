using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _02_Scripts.Manager
{
    public class UIManager : Singleton<UIManager>
    {
        private readonly Dictionary<Type, UIBase> UIDict = new();
        private readonly List<UIBase> openedUIList = new();
        
        public bool IsAnyUIOpen => openedUIList.Count > 0;
    
        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate)
                return;

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    
        private void Start()
        {
            InitializeUIRoot();
        }
    
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeUIRoot();
        }

        private void InitializeUIRoot()
        {
            UIDict.Clear();

            Transform uiRoot = GameObject.Find("UIRoot")?.transform;
            if (uiRoot == null)
            {
                Debug.LogWarning("[UIManager] UIRoot를 찾을 수 없습니다.");
                return;
            }

            UIBase[] uiComponents = uiRoot.GetComponentsInChildren<UIBase>(true);
            foreach (UIBase uiComponent in uiComponents)
            {
                UIDict[uiComponent.GetType()] = uiComponent;
                uiComponent.Close();
            }
        }
    
        public void Toggle<T>() where T : UIBase
        {
            if (typeof(T) == typeof(UISettings) &&
                UIDict.TryGetValue(typeof(UIDead), out var deadUI) &&
                deadUI.IsOpen)
            {
                return;
            }
            
            if (UIDict.TryGetValue(typeof(T), out var ui))
            {
                if (ui.IsOpen)
                    Close<T>();
                else
                    Open<T>();
            }
        }
    
        public void Open<T>() where T : UIBase
        {
            if (UIDict.TryGetValue(typeof(T), out UIBase ui))
            {
                if (!ui.IsOpen)
                {
                    openedUIList.Add(ui);
                    ui.Open();
                }
                else
                {
                    // 순서갱신(최근에 사용한 UI를 리스트 가장 마지막으로)
                    openedUIList.Remove(ui);
                    openedUIList.Add(ui);
                }
            }
        }

        public void Close<T>() where T : UIBase
        {
            if (UIDict.TryGetValue(typeof(T), out UIBase ui))
            {
                openedUIList.Remove(ui);
                ui.Close();
            }
        }
    
        // ESC키 할당
        public bool CloseTop()
        {
            if (openedUIList.Count == 0) return false;

            UIBase topUI = openedUIList[^1]; // 리스트의 마지막 요소
            openedUIList.RemoveAt(openedUIList.Count - 1);
            topUI.Close();
            return true;
        }
        
        public void CloseAll()
        {
            while (openedUIList.Count > 0)
            {
                CloseTop();
            }
        }

        public T GetUIComponent<T>() where T : UIBase
        {
            return UIDict.TryGetValue(typeof(T), out var ui) ? ui as T : null;
        }
    
#if UNITY_EDITOR
        public Dictionary<Type, UIBase> DebugUIDict => UIDict;
        public List<UIBase> DebugOpenedUIList => openedUIList;
#endif
    }
}
