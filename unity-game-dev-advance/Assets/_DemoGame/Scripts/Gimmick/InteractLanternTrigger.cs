using R3;
using UnityEngine;

namespace DemoGame
{
    // 点灯条件：杖が灯っている状態で、灯の近くで E キー。杖を灯へ伸ばして（PlayerStaffReach）、届いたら点く
    // 設計：doc/demo-game/10-level-design.md §4.3・§4.4
    //   ふつう            ：その場で点く
    //   _encounterRoom あり：火だけついて敵が出る（灯火 2）。全滅したら点く
    //   _blockers         ：どれかが残っている間は点けられない（灯火 3 の蔦・まだ開いていない魔法の壁）
    // 探されるのはコライダーなので、この GameObject か子にコライダー（トリガー可）を付けておく
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

            // 杖の火で点ける。杖が消えていたら働きかけられない（HUD にも出ない）
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
