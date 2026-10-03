using R3;
using UnityEngine;

namespace DemoGame
{
    // ボスを倒したあとに現れる光。触れたらクリア。設計：doc/demo-game/10-level-design.md §8
    // 光の柱（LightPillar）は _visual の下に置く。この GameObject のコライダーはトリガーにしておく
    public class ClearBeacon : MonoBehaviour
    {
        [Tooltip("現れるまで隠しておくもの（光の球・光の柱）")]
        [SerializeField]
        private GameObject _visual;

        [Tooltip("現れてから触れられるようになるまでの秒数（演出の途中で触れないように）")]
        [SerializeField, Range(0f, 10f)]
        private float _touchableDelay = 1f;

        private readonly Subject<Unit> _touched = new();
        private Collider _trigger;
        private float _touchableTime = float.PositiveInfinity;

        public Observable<Unit> Touched => _touched;
        public bool IsAppeared { get; private set; }
        public bool IsTouched { get; private set; }

        private void Awake()
        {
            _trigger = GetComponent<Collider>();
            SetVisible(false);
        }

        // 光が現れる。位置はボスがいた場所の足元
        public void Appear(Vector3 position)
        {
            if (IsAppeared) return;

            IsAppeared = true;
            transform.position = position;
            _touchableTime = Time.time + _touchableDelay;
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (_visual != null)
            {
                _visual.SetActive(visible);
            }

            if (_trigger != null)
            {
                _trigger.enabled = visible;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!IsAppeared || IsTouched) return;
            if (Time.time < _touchableTime) return;
            if (!other.CompareTag("Player")) return;

            IsTouched = true;
            _touched.OnNext(Unit.Default);
            _touched.OnCompleted();
        }

        private void OnDestroy()
        {
            _touched.Dispose();
        }
    }
}
