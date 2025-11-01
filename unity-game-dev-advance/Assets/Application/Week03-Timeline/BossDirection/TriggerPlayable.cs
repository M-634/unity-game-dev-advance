using UnityEngine;
using UnityEngine.Playables;

namespace Week03
{
    public class TriggerPlayable : MonoBehaviour
    {
        [SerializeField] PlayableDirector playableDirector;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playableDirector.Play();
            }
        }
    }
}