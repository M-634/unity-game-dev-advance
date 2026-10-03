using System;
using R3;
using UnityEngine;

namespace DemoGame
{
    // ボス扉の封印。紋様と、それに対応する灯を持つ
    // 灯が点いたら「この紋様が消える」と知らせ、全部消えたら Broken を流す。消す見せ方（演出）は BossSealDirector が持つ
    public class BossDoorSeal : MonoBehaviour
    {
        [Serializable]
        public class Emblem
        {
            [Tooltip("この紋様に対応する灯")]
            public Lantern Lantern;

            [Tooltip("消える紋様の見た目")]
            public Renderer Renderer;

            [NonSerialized]
            public bool IsBroken;
        }

        [SerializeField]
        private Emblem[] _emblems = new Emblem[0];

        private readonly Subject<Emblem> _breakRequested = new();
        private readonly Subject<Unit> _broken = new();
        private readonly CompositeDisposable _disposables = new();

        // 灯が点いて、この紋様を消してほしい
        public Observable<Emblem> BreakRequested => _breakRequested;

        // 紋様が全部消えた
        public Observable<Unit> Broken => _broken;

        public bool IsBroken { get; private set; }

        private void Start()
        {
            foreach (var emblem in _emblems)
            {
                if (emblem.Lantern == null) continue;

                var target = emblem;
                emblem.Lantern.LitUp.Subscribe(_ => _breakRequested.OnNext(target)).AddTo(_disposables);
            }
        }

        // 演出側が「消し終わった」と伝える
        public void MarkBroken(Emblem emblem)
        {
            if (emblem == null || emblem.IsBroken) return;

            emblem.IsBroken = true;

            foreach (var e in _emblems)
            {
                if (!e.IsBroken) return;
            }

            IsBroken = true;
            _broken.OnNext(Unit.Default);
            _broken.OnCompleted();
        }

        public bool IsLastRemaining(Emblem emblem)
        {
            foreach (var e in _emblems)
            {
                if (e != emblem && !e.IsBroken) return false;
            }

            return true;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _breakRequested.Dispose();
            _broken.Dispose();
        }
    }
}
