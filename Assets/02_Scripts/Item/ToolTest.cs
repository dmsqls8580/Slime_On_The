using UnityEngine;

public class ToolTest : MonoBehaviour
{
    public ToolController toolController;
    
    void Start()
    {
        var testSO = ScriptableObject.CreateInstance<ItemSO>();
        testSO.itemName = "테스트 곡괭이";
        testSO.itemTypes = ItemType.Tool;
        testSO.toolData = new ToolData
        {
            toolType = ToolType.Pickaxe,
            power = 1f,
            actSpd = 1f
        };

        var testWeapon = new TestWeapon(testSO);

        toolController.EquipTool(testWeapon);
    }
}

public class TestWeapon : IWeapon
{
    public float AttackPow { get; }
    public float AttackSpd { get; }
    public GameObject ToolPrefab => null;

    public TestWeapon(ItemSO _so)
    {
        AttackPow = _so.toolData.power;
        AttackSpd = _so.toolData.actSpd;
    }
}