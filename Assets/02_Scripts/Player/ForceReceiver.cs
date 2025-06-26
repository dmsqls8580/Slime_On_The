using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    public Vector3 _force;
    private float _drag = 2f;

    public Vector3 Force => _force;
    
    void Update()
    {
        _force= Vector3.Lerp(_force, Vector3.zero, _drag*Time.deltaTime);
    }
}
