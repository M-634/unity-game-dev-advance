using UnityEngine;

public class EmptyPointRenderer : MonoBehaviour
{
    [SerializeField] private Color _color = Color.green;
    [SerializeField] private float _radius = 0.2f;

    private void OnDrawGizmos()
    {
        Gizmos.color = _color;
        Gizmos.DrawSphere(transform.position, _radius);
    }
}