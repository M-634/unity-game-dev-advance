using R3;
using UnityEngine;

namespace DemoGame
{
    public class InteractLanternTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private Lantern _lantern;

        [Tooltip("火をつけたら始まる戦闘（灯火 2）。未設定ならすぐ点く")]
        [SerializeField]
        private EncounterRoom _encounterRoom;

        [Tooltip("杖を伸ばす先。未設定なら灯の見た目の中心")]
        [SerializeField]
        private Transform _reachPoint;

        [Tooltip("残っている（有効な）間は点けられない物")]
        [SerializeField]
        private GameObject[] _blockers = new GameObject[0];

        private readonly CompositeDisposable _disposables = new();
        private bool _isReaching;

        public string Prompt => "Light";

        private void Awake()
        {
            if (_lantern == null)
            {
                _lantern = GetComponentInParent<Lantern>();
            }
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            if (_lantern == null || _lantern.IsLit || _lantern.IsKindled) return false;
            if (_isReaching || PlayerStaffReach.IsBusy(interactor)) return false;

            foreach (var blocker in _blockers)
            {
                if (blocker != null && blocker.activeInHierarchy) return false;
            }

            var staff = interactor.GetComponentInParent<Staff>();
            return staff != null && staff.IsLit;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;

            _isReaching = true;
            PlayerStaffReach.ReachOrRun(interactor, ReachPosition(), Ignite);
        }

        private void Ignite()
        {
            _isReaching = false;

            if (_encounterRoom == null)
            {
                _lantern.LightUp();
                return;
            }

            _lantern.Kindle();
            _encounterRoom.Cleared.Subscribe(_ => _lantern.LightUp()).AddTo(_disposables);
            _encounterRoom.Begin();
        }

        private Vector3 ReachPosition()
        {
            if (_reachPoint != null) return _reachPoint.position;

            var renderer = _lantern.GetComponentInChildren<Renderer>();
            return renderer != null ? renderer.bounds.center : _lantern.transform.position + Vector3.up;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
