using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Playables;

namespace DemoGame
{
    // ボス撃破の演出。ボスが溶けて消えたら、いた場所に光が生まれ、光の束が天井へ昇る。設計：doc/demo-game/10-level-design.md §8
    // 演出のあと操作に戻り、光（ClearBeacon）に触れたらクリア（InGameController が見張る）
    public class BossDefeatDirector : MonoBehaviour
    {
        [SerializeField]
        private CutscenePlayer _cutscenePlayer;

        [Tooltip("BossDefeat の Timeline。未設定なら、光を出すだけ")]
        [SerializeField]
        private PlayableDirector _director;

        [SerializeField]
        private BossController _boss;

        [SerializeField]
        private ClearBeacon _beacon;

        [Tooltip("ボスが倒れてから光が生まれるまでの秒数（倒れる → 溶けて消える の長さに合わせる）")]
        [SerializeField, Range(0f, 10f)]
        private float _appearDelay = 2.7f;

        [Tooltip("演出用カメラが見る位置（光の生まれる場所に動かす）")]
        [SerializeField]
        private Transform _lookTarget;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_boss == null || _beacon == null)
            {
                Debug.LogError($"{nameof(BossDefeatDirector)}: ボスか ClearBeacon が設定されていません。", this);
                return;
            }

            _boss.Defeated.Subscribe(_ => PlayAsync(destroyCancellationToken).SuppressCancellationThrow().Forget()).AddTo(_disposables);
        }

        private async UniTask PlayAsync(CancellationToken ct)
        {
            // ボスは溶けて消えると Destroy されるので、先に位置を覚えておく
            Vector3 position = _boss.transform.position;

            await UniTask.Delay(System.TimeSpan.FromSeconds(_appearDelay), cancellationToken: ct);

            if (_lookTarget != null)
            {
                _lookTarget.position = position;
            }

            _beacon.Appear(position);

            if (_cutscenePlayer != null && _director != null)
            {
                await _cutscenePlayer.PlayAsync(_director, skippable: true, ct);
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
