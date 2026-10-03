using System.Collections.Generic;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 霧のエフェクト（地面を這う霧・ステージ全体の黒い霧）。点けた灯火の数で薄くする。設計：doc/demo-game/12-vfx.md #22・#23
    //   地面の霧：_groundFogPoints の位置に FX_GroundFog を出す（置き場所はステージに合わせて Transform を動かす）
    //   黒い霧  ：カメラについていく（Simulation Space が World なので、出た霧はその場に残る）
    public class FogEffectController : MonoBehaviour
    {
        [SerializeField]
        private GameProgress _progress;

        [SerializeField]
        private GameObject _groundFogPrefab;

        [Tooltip("地面の霧を置く場所")]
        [SerializeField]
        private Transform[] _groundFogPoints = new Transform[0];

        [SerializeField]
        private GameObject _blackFogPrefab;

        [Tooltip("黒い霧がついていく先。未設定なら Main Camera")]
        [SerializeField]
        private Transform _follow;

        [Tooltip("点けた灯火の数（0〜3）ごとの、霧の濃さ（数と色の濃さに掛ける）")]
        [SerializeField]
        private float[] _densityByLitCount = { 1f, 0.7f, 0.45f, 0.2f };

        [SerializeField, Range(0f, 10f)]
        private float _transitionSeconds = 3f;

        private readonly CompositeDisposable _disposables = new();
        private ParticleSystem[] _systems = new ParticleSystem[0];
        private float[] _baseRates = new float[0];
        private Color[] _baseColors = new Color[0];
        private Transform _blackFog;
        private float _density = 1f;
        private float _targetDensity = 1f;
        private float _velocity;

        private void Start()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            var effects = new List<GameObject>();
            foreach (var point in _groundFogPoints)
            {
                if (point == null || _groundFogPrefab == null) continue;
                effects.Add(Instantiate(_groundFogPrefab, point.position, point.rotation, point));
            }

            if (_blackFogPrefab != null)
            {
                var black = Instantiate(_blackFogPrefab, transform);
                _blackFog = black.transform;
                effects.Add(black);
            }

            var list = new List<ParticleSystem>();
            foreach (var effect in effects)
            {
                list.AddRange(effect.GetComponentsInChildren<ParticleSystem>(true));
            }

            _systems = list.ToArray();
            _baseRates = new float[_systems.Length];
            _baseColors = new Color[_systems.Length];
            for (int i = 0; i < _systems.Length; i++)
            {
                _baseRates[i] = _systems[i].emission.rateOverTimeMultiplier;
                _baseColors[i] = _systems[i].main.startColor.color;
            }

            if (_progress != null)
            {
                _progress.LitCount.Subscribe(count => _targetDensity = DensityFor(count)).AddTo(_disposables);
                _density = _targetDensity;
            }

            Apply();
        }

        private float DensityFor(int litCount)
        {
            if (_densityByLitCount.Length == 0) return 1f;

            return _densityByLitCount[Mathf.Clamp(litCount, 0, _densityByLitCount.Length - 1)];
        }

        private void LateUpdate()
        {
            if (_blackFog != null)
            {
                var follow = _follow;
                if (follow == null && Camera.main != null)
                {
                    follow = Camera.main.transform;
                }

                if (follow != null)
                {
                    _blackFog.position = follow.position;
                }
            }

            if (Mathf.Abs(_density - _targetDensity) < 0.001f) return;

            _density = Mathf.SmoothDamp(_density, _targetDensity, ref _velocity, Mathf.Max(0.01f, _transitionSeconds * 0.4f));
            Apply();
        }

        private void Apply()
        {
            for (int i = 0; i < _systems.Length; i++)
            {
                var emission = _systems[i].emission;
                emission.rateOverTimeMultiplier = _baseRates[i] * _density;

                var main = _systems[i].main;
                var color = _baseColors[i];
                color.a *= Mathf.Lerp(0.5f, 1f, _density);
                main.startColor = color;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
