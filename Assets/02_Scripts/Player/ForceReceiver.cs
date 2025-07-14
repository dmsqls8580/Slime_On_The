using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    public Vector2 force;
    private float drag = 2f;

    public Vector2 Force => force;
    
    void Update()
    {
        force = Vector2.Lerp(force, Vector3.zero, drag * Time.deltaTime);
    }

    public void AddForce(Vector2 _force)
    {
        force += _force;
    }
}
