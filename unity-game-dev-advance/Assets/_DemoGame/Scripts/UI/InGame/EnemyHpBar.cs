using R3;
using UnityEngine;

namespace DemoGame
{
    // 雑魚敵の頭上の HP バー。World Space の Canvas に付け、Billboard でカメラを向かせる
    // 中身の横幅（localScale.x）を残り HP の割合にする。中身の Pivot は左端（x = 0）にしておく
    // 高さはモデルの見た目（Renderer の範囲）のてっぺんに合わせる。モデルを差し替えても頭の上に出る
    public class EnemyHpBar : MonoBehaviour
    {
        [Tooltip("空なら親から探す")]
        [SerializeField]
        private EnemyController _enemy;

        [SerializeField]
        private RectTransform _fill;

        [Tooltip("モデルの頭のてっぺんから、どれだけ上に出すか（m）")]
        [SerializeField, Range(0f, 2f)]
        private float _heightAboveHead = 0.35f;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_enemy == null)
            {
                _enemy = GetComponentInParent<EnemyController>();
            }

            if (_enemy == null || _fill == null)
            {
                Debug.LogError($"{nameof(EnemyHpBar)}: 参照が設定されていません。", this);
                return;
            }

            PlaceAboveHead();

            _enemy.CurrentHp
                .Subscribe(hp => SetRatio((float)hp / _enemy.MaxHp))
                .AddTo(_disposables);

            _enemy.Died
                .Subscribe(_ => gameObject.SetActive(false))
                .AddTo(_disposables);
        }

        private void PlaceAboveHead()
        {
            if (_enemy.Visual == null) return;

            var renderers = _enemy.Visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var position = transform.position;
            position.y = bounds.max.y + _heightAboveHead;
            transform.position = position;
        }

        private void SetRatio(float ratio)
        {
            var scale = _fill.localScale;
            scale.x = Mathf.Clamp01(ratio);
            _fill.localScale = scale;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
