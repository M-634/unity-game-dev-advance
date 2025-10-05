using UnityEngine;

namespace Week01
{
    public interface IInteractiveObject
    {
        int GetObjectID();
        void OnExecute(Transform player);
        void OnExit(Transform player);
    }
}