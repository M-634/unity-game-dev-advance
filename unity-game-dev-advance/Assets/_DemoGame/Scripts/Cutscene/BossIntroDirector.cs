using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DemoGame
{
    // ボス戦前の演出。ボスは部屋に入るまで隠しておき、演出の中で姿を現す。演出が終わったら（スキップでも）そのまま戦闘に入る
    public class BossIntroDirector : MonoBehaviour
    {
        [SerializeField]
        private CutscenePlayer _cutscenePlayer;

        [SerializeField]
        private PlayableDirector _director;

        [SerializeField]
        private BossController _boss;

        [Tooltip("演出まで、ボスを溶けた状態（見えない状態）にしておく")]
        [SerializeField]
        private bool _hideBossUntilIntro = true;

        private bool _isPlayed;
        private Renderer[] _bossRenderers = new Renderer[0];

        private void Start()
        {
            if (_boss == null) return;

            _bossRenderers = _boss.GetComponentsInChildren<Renderer>(true);

            if (_hideBossUntilIntro)
            {
                DissolveBehaviour.SetDissolve(_bossRenderers, 1f);
            }
        }

        public void Play()
        {
            if (_isPlayed) return;

            _isPlayed = true;
            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            if (_cutscenePlayer != null && _director != null)
            {
                BindBoss();
                bool canceled = await _cutscenePlayer.PlayAsync(_director, skippable: true, destroyCancellationToken).SuppressCancellationThrow();
                if (canceled) return;
            }

            // 現れる前にスキップしても、必ず見える状態で戦闘に入る
            DissolveBehaviour.SetDissolve(_bossRenderers, 0f);

            if (_boss != null)
            {
                _boss.Activate();
            }
        }

        // ボスのモデル（入れ子の Prefab）は再生直前に Binding を差す。モデルを替えても追従する
        private void BindBoss()
        {
            if (_boss == null || !(_director.playableAsset is TimelineAsset timeline)) return;

            var animator = _boss.GetComponentInChildren<Animator>();

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is AnimationTrack && animator != null)
                {
                    _director.SetGenericBinding(track, animator);
                }
                else if (track is DissolveTrack)
                {
                    _director.SetGenericBinding(track, _boss.gameObject);
                }
            }
        }
    }
}
