using Section.Common;
using UnityEngine;
using UnityEngine.Playables;

namespace Section.One.Timeline
{
   [RequireComponent(typeof(PlayableDirector))]
   public class BossDirectionTrigger : MonoBehaviour
   {
      [SerializeField]
      PlayableDirector _director;

      [SerializeField]
      Collider _collider;

      [SerializeField]
      GameObject[] _cameraObjects;
      
      [SerializeField]
      private Cinemachine.CinemachineFreeLook _tpsCamera;


      private InputReader _input;

      private void Awake()
      {
         InitializeComponents();
      }

      private void Start()
      {
         SetActiveCameras(false);
      }

      private void OnTriggerEnter(Collider other)
      {
         if(!other.TryGetComponent(out PlayerController playerController)) return;
         if (!other.TryGetComponent(out _input)) return;
         
         PlayDirection(playerController);
      }

      private void InitializeComponents()
      {
         if (_collider == null)
         {
            _collider = GetComponentInChildren<Collider>();
            _collider.isTrigger = true;
         }

         if (_director == null)
         {
            _director = GetComponent<PlayableDirector>();
         }
      }

      private void SetActiveCameras(bool isActive)
      {
         foreach (var cameraObject in _cameraObjects)
         {
            cameraObject.SetActive(isActive);
         }
      }

      private void PlayDirection(PlayerController playerController)
      {
         _input.SetCutsceneMode(true);
         SetActiveCameras(true);
         _input.SkipPressed += Skip;
         _director.stopped += OnFinished;
         
         playerController.VisualRoot.rotation = Quaternion.identity;
         
         var target = _tpsCamera.Follow.position;
         var position = target + new Vector3(0f, 2.5f, -3f);

         _tpsCamera.m_YAxis.Value = 0.5f;
         _tpsCamera.ForceCameraPosition(
            position,
            Quaternion.LookRotation(_tpsCamera.LookAt.position - position)
         );
         
         _director.Play();
      }

      private void Skip()
      {
         _director.time = _director.duration;
         _director.Evaluate();
         _director.Stop();
      }

      private void OnFinished(PlayableDirector director)
      {
         _director.stopped -= OnFinished;
         _input.SkipPressed -= Skip;
         _input.SetCutsceneMode(false);
         SetActiveCameras(false);
      }


#if UNITY_EDITOR

      private void Reset()
      {
         InitializeComponents();
      }

#endif
   }
}
