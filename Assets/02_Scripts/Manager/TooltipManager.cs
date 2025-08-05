using TMPro;
using UnityEngine;

public class TooltipManager : SceneOnlySingleton<TooltipManager>
{
    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private TooltipObject tooltipObject;
    [SerializeField] private TMP_Text itemNameText;
    private bool isHovered = false;
    
    private void Awake()
    {
        itemNameText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!isHovered) return;

        Vector2 pos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipCanvas.transform as RectTransform,
            pos,
            tooltipCanvas.worldCamera,
            out Vector2 localPoint);

        tooltipObject.SetPosition(localPoint);
    }

    public void Show(string itemName)
    {
        isHovered = true;
        itemNameText.text = itemName;
        itemNameText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        isHovered = false;
        itemNameText.gameObject.SetActive(false);
    }
}