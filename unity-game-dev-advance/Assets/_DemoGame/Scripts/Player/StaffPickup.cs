using UnityEngine;

namespace DemoGame
{
    // 導入演出で拾う杖（地面に置いてある方）。設計：doc/demo-game/10-level-design.md §4.1
    // 導入演出（InGameIntro）が終わったら（スキップでも）PickUp() が呼ばれ、地面の杖が消えてプレイヤーが杖を持つ
    public class StaffPickup : MonoBehaviour
    {
        [SerializeField]
        private Staff _staff;

        [Tooltip("地面に置いてある杖の見た目。拾ったら消す")]
        [SerializeField]
        private GameObject _groundModel;

        public bool IsPicked { get; private set; }

        private void Awake()
        {
            if (_staff == null)
            {
                _staff = FindFirstObjectByType<Staff>();
            }

            if (_groundModel == null)
            {
                _groundModel = gameObject;
            }
        }

        public void PickUp()
        {
            if (IsPicked) return;

            IsPicked = true;

            if (_staff != null)
            {
                _staff.Acquire();
            }

            _groundModel.SetActive(false);
        }
    }
}
