using UnityEngine;
using UnityEngine.Playables;

namespace Section1.TimelineBasics
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