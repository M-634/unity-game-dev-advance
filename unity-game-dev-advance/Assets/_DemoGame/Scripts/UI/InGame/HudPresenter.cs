using R3;
using UnityEngine;

namespace DemoGame
{
    public class HudPresenter : MonoBehaviour
    {
        [SerializeField]
        private HudView _view;

        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private PlayerHealth _playerHealth;

        [SerializeField]
        private PlayerInteractor _playerInteractor;

        [Tooltip("上部に HP バーを出すボス。起動されたら出し、倒されたら消す")]
        [SerializeField]
        private BossController _boss;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_view == null)
            {
                Debug.LogError($"{nameof(HudPresenter)}: HudView が設定されていません。", this);
                return;
            }

            if (_playerHealth != null)
            {
                _playerHealth.CurrentHp
                    .Subscribe(hp => _view.SetHp(hp, _playerHealth.MaxHp))
                    .AddTo(_disposables);
            }

            if (_progress != null)
            {
                _progress.LitCount
                    .Subscribe(count => _view.SetLitCount(count, GameProgress.RequiredLitCount))
                    .AddTo(_disposables);
            }

            if (_playerInteractor != null)
            {
                _playerInteractor.Current
                    .Subscribe(target => _view.SetInteractPrompt(target?.Prompt))
                    .AddTo(_disposables);
            }

            _view.SetBossHpVisible(false);

            if (_boss != null)
            {
                _boss.CurrentHp
                    .Subscribe(hp => _view.SetBossHp(hp, _boss.MaxHp))
                    .AddTo(_disposables);

                _boss.Activated
                    .Subscribe(_ => _view.SetBossHpVisible(true))
                    .AddTo(_disposables);

                _boss.Defeated
                    .Subscribe(_ => _view.SetBossHpVisible(false))
                    .AddTo(_disposables);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
