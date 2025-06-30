using UnityEngine;

[CreateAssetMenu(menuName = "TempItemSO")]
public class TempItemSO : ScriptableObject
{
    public int Idx;
    public string ItemName;
    public Sprite ItemIcon;
    public int MaxStack = 1;
}
