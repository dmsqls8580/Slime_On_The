using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemInfoPanel : MonoBehaviour
{
    public Image image;
    public new TextMeshProUGUI name;
    public Transform requiredIngredient;
    public GameObject requiredIngredientItemSlotPrefab;
    public Craft craft;
}
