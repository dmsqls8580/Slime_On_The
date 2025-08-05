using UnityEngine;

public class TooltipObject : MonoBehaviour
{
    
    
    public void SetPosition(Vector2 _localPos)
    {
        (transform as RectTransform).localPosition = _localPos;
    }
}
