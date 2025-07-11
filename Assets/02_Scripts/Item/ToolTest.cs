using UnityEngine;

public class ToolTest : MonoBehaviour
{
    public ToolController toolController;
    [SerializeField] private ItemSO testToolItemSo;
    void Start()
    {
        var testWeapon = new TestWeapon(testToolItemSo);

        toolController.EquipTool(testWeapon);
    }
}

public class TestWeapon : IWeapon
{
    public float AttackPow { get; }
    public float AttackSpd { get; }
    public GameObject ToolPrefab => null;
    public ToolType ToolType { get; }

    public TestWeapon(ItemSO _so)
    {
        AttackPow = _so.toolData.power;
        AttackSpd = _so.toolData.actSpd;
        ToolType= _so.toolData.toolType;
    }
}