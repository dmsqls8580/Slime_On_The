using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    public Vector2 _force;
    private float _drag = 2f;

    public Vector2 Force => _force;
    
    void Update()
    {
        _force= Vector2.Lerp(_force, Vector3.zero, _drag*Time.deltaTime);
    }
}
