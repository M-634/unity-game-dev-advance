using UnityEngine;

namespace DemoGame
{
    // プレイヤーの攻撃が当たったら、当たった位置にダメージ数を出す
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField]
        private PlayerAttack _attack;

        [SerializeField]
        private DamageNumber _prefab;

        [Tooltip("当たった位置からどれだけ上に出すか（m）")]
        [SerializeField, Range(0f, 2f)]
        private float _heightOffset = 0.4f;

        [Tooltip("重なって読めなくならないように、横にずらす範囲（m）")]
        [SerializeField, Range(0f, 1f)]
        private float _randomRadius = 0.25f;

        private void OnEnable()
        {
            if (_attack == null || _prefab == null)
            {
                Debug.LogError($"{nameof(DamageNumberSpawner)}: 参照が設定されていません。", this);
                return;
            }

            _attack.Hit += OnHit;
        }

        private void OnDisable()
        {
            if (_attack == null) return;

            _attack.Hit -= OnHit;
        }

        private void OnHit(AttackHit hit)
        {
            Vector2 offset = Random.insideUnitCircle * _randomRadius;
            Vector3 position = hit.Point + new Vector3(offset.x, _heightOffset, offset.y);

            var number = Instantiate(_prefab, position, Quaternion.identity);
            number.Show(hit.Damage);
        }
    }
}
