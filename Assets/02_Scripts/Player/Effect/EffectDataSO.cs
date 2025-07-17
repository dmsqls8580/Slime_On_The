using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "EffectData", menuName = "Effect/EffectData")]
public class EffectDataSO: ScriptableObject
{
    public string poolID;
    public GameObject effectPrefab;
    public int effectID; //Player는 0~20, Enemy는 21~40 으로 ID저장 해요.
    public float duration = 1f;
    public Color color;
}