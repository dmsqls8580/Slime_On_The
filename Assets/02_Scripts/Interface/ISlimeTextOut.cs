using UnityEngine;

public interface ISlimeTextOut
{
    void TryShowSlimeText(string _key, float _gauge, Vector3 _worldPos);
    void OnSlimeGaugeChanged(float _currentGauge, float _maxGauge, Vector3 _worldPos);
}