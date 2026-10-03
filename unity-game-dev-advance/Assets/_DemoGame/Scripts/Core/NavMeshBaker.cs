using Unity.AI.Navigation;
using UnityEngine;

namespace DemoGame
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class NavMeshBaker : MonoBehaviour
    {
        [SerializeField]
        private NavMeshSurface _surface;

        private void Awake()
        {
            if (_surface == null)
            {
                _surface = GetComponent<NavMeshSurface>();
            }

            _surface.BuildNavMesh();
        }
    }
}
