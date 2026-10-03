using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace DemoGame
{
    // 演出（Timeline）を再生して、終わるまで待てるようにする
    // 再生中はプレイヤーの操作を止め、HUD を隠す。スキップは「最後まで飛ばす」（途中で止めると出したものが残るため）
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField]
        private InputReader _input;

        [Tooltip("演出中に隠す HUD")]
        [SerializeField]
        private CanvasGroup _hud;

        [Tooltip("スキップできる演出のときだけ出す表示（CLICK TO SKIP）")]
        [SerializeField]
        private GameObject _skipHint;

        private int _lockCount;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (_skipHint != null)
            {
                _skipHint.SetActive(false);
            }
        }

        // 演出を再生し、終わるまで（スキップ・キャンセルを含む）待つ
        public async UniTask PlayAsync(PlayableDirector director, bool skippable, CancellationToken token)
        {
            if (director == null) return;

            // 同時には再生しない。前の演出が終わるのを待つ
            await UniTask.WaitWhile(() => _isPlaying, cancellationToken: token);

            _isPlaying = true;
            using var scope = Lock();

            bool skipRequested = false;
            void OnSkip() => skipRequested = true;

            if (skippable && _input != null)
            {
                _input.SkipPressed += OnSkip;
            }

            SetSkipHint(skippable);

            try
            {
                director.extrapolationMode = DirectorWrapMode.None;
                director.time = 0;
                director.Play();

                // Play した直後のフレームはまだ Playing にならないことがあるので、1 フレーム待つ
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                while (director.state == PlayState.Playing && !skipRequested)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            finally
            {
                if (_input != null)
                {
                    _input.SkipPressed -= OnSkip;
                }

                JumpToEnd(director);
                SetSkipHint(false);
                _isPlaying = false;
            }
        }

        // 演出の外でも操作を止めておきたいとき（扉が開くのを見せている間など）に使う。Dispose で戻る
        public IDisposable Lock()
        {
            _lockCount++;

            if (_lockCount == 1)
            {
                if (_input != null) _input.SetCutsceneMode(true);
                if (_hud != null) _hud.alpha = 0f;
            }

            return new Releaser(this);
        }

        private void Unlock()
        {
            _lockCount = Mathf.Max(0, _lockCount - 1);
            if (_lockCount > 0) return;

            if (_hud != null) _hud.alpha = 1f;
            RestoreInputAsync(destroyCancellationToken).SuppressCancellationThrow().Forget();
        }

        // スキップに使ったクリックを離してから操作を戻す（同じクリックで攻撃しないように）
        private async UniTask RestoreInputAsync(CancellationToken token)
        {
            if (_input == null) return;

            await UniTask.WaitWhile(() => _input.IsSkipHeld, cancellationToken: token);

            if (_lockCount > 0) return;

            _input.SetCutsceneMode(false);
        }

        private static void JumpToEnd(PlayableDirector director)
        {
            if (director == null) return;

            if (director.playableGraph.IsValid())
            {
                director.time = director.duration;
                director.Evaluate();
            }

            director.Stop();
        }

        private void SetSkipHint(bool visible)
        {
            if (_skipHint != null)
            {
                _skipHint.SetActive(visible);
            }
        }

        private sealed class Releaser : IDisposable
        {
            private CutscenePlayer _owner;

            public Releaser(CutscenePlayer owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null) return;

                _owner.Unlock();
                _owner = null;
            }
        }
    }
}
