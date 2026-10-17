using System;
using R3;
using UnityEngine;

namespace DemoGame
{
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

        public Observable<Emblem> BreakRequested => _breakRequested;

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
