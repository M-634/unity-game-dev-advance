using R3;
using UnityEngine;

namespace Section1.AnimationRigging
{
    // プレイヤーのロジック（移動・攻撃・HP）の状態を、見た目の Animator へ伝える
    // ロジック側はこのクラスの存在を知らない
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField]
        private CharacterVisual _visual;

        [SerializeField]
        private PlayerController _controller;

        [SerializeField]
        private PlayerAttack _attack;

        [SerializeField]
        private PlayerHealth _health;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _lastHp;

        private void Awake()
        {
            if (_visual == null)
            {
                _visual = GetComponentInChildren<CharacterVisual>();
            }

            if (_visual == null)
            {
                Debug.LogError($"{nameof(PlayerAnimation)}: CharacterVisual が見つかりません。", this);
            }
        }

        private void OnEnable()
        {
            if (_attack != null)
            {
                _attack.Swung += OnSwung;
            }

            if (_health != null)
            {
                _lastHp = _health.CurrentHp.CurrentValue;
                _health.CurrentHp.Subscribe(OnHpChanged).AddTo(_disposables);
            }
        }

        private void OnDisable()
        {
            if (_attack != null)
            {
                _attack.Swung -= OnSwung;
            }

            _disposables.Clear();
        }

        private void Update()
        {
            if (_visual == null || _controller == null) return;

            _visual.SetFloat(CharacterAnimatorParams.Speed, _controller.CurrentSpeed);
            _visual.SetBool(CharacterAnimatorParams.IsGrounded, _controller.IsGrounded);
        }

        private void OnSwung()
        {
            if (_visual == null) return;

            _visual.SetTrigger(CharacterAnimatorParams.Attack);
        }

        private void OnHpChanged(int hp)
        {
            if (_visual != null)
            {
                // 回復（リスポーン）でも HP は変わるので、減ったときだけ被弾とみなす
                if (hp < _lastHp && hp > 0)
                {
                    _visual.SetTrigger(CharacterAnimatorParams.Hit);
                }

                _visual.SetBool(CharacterAnimatorParams.IsDead, hp <= 0);
            }

            _lastHp = hp;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
