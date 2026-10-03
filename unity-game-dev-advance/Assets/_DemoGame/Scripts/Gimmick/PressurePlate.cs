using System.Collections.Generic;
using UnityEngine;

namespace DemoGame
{
    // 床スイッチ。プレイヤーか箱（PressBox）が乗っている間だけ押されている
    // 乗った瞬間に Pressed、全部降りた瞬間に Released を流す。設計：doc/demo-game/10-level-design.md §4.4
    // この GameObject のコライダーはトリガーにしておく
    public class PressurePlate : SwitchBase
    {
        [Tooltip("沈ませる見た目。未設定なら沈まない")]
        [SerializeField]
        private Transform _visual;

        [SerializeField, Range(0f, 0.2f)]
        private float _pressDepth = 0.05f;

        [Tooltip("箱を載せても押されるか")]
        [SerializeField]
        private bool _pressedByBox = true;

        private Vector3 _visualDefaultPosition;

        // 乗っている物（プレイヤーのルート・箱）。同じ物の複数のコライダーを1つに数える
        private readonly Dictionary<Transform, int> _inside = new Dictionary<Transform, int>();

        private void Awake()
        {
            if (_visual != null)
            {
                _visualDefaultPosition = _visual.localPosition;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var presser = FindPresser(other);
            if (presser == null) return;

            _inside.TryGetValue(presser, out int count);
            _inside[presser] = count + 1;

            if (_inside.Count == 1 && count == 0)
            {
                SetPressedVisual(true);
                NotifyPressed();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var presser = FindPresser(other);
            if (presser == null || !_inside.TryGetValue(presser, out int count)) return;

            if (count > 1)
            {
                _inside[presser] = count - 1;
                return;
            }

            _inside.Remove(presser);
            if (_inside.Count > 0) return;

            SetPressedVisual(false);
            NotifyReleased();
        }

        private void Update()
        {
            // 乗っていた物が消えた・無効になったときは、トリガーから出たことにならないので自分で外す
            if (_inside.Count == 0) return;

            bool removed = false;
            foreach (var key in new List<Transform>(_inside.Keys))
            {
                if (key == null || !key.gameObject.activeInHierarchy)
                {
                    _inside.Remove(key);
                    removed = true;
                }
            }

            if (removed && _inside.Count == 0)
            {
                SetPressedVisual(false);
                NotifyReleased();
            }
        }

        private Transform FindPresser(Collider other)
        {
            if (other.CompareTag("Player")) return other.transform.root;

            if (_pressedByBox)
            {
                var box = other.GetComponentInParent<PressBox>();
                if (box != null) return box.transform;
            }

            return null;
        }

        private void SetPressedVisual(bool pressed)
        {
            if (_visual == null) return;

            _visual.localPosition = _visualDefaultPosition + (pressed ? Vector3.down * _pressDepth : Vector3.zero);
        }
    }
}
