using UnityEngine;
using UnityEngine.Playables;

namespace Section1.TimelineBasics
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
