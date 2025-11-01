using UnityEngine;
using UnityEngine.Playables;

namespace Week03
{
    public class SampleTimeLine : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        
        private void Start()
        {
            _director.Play();
        }
    }
}
