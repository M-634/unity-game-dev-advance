using System;
using R3;
using UnityEngine;

namespace DemoGame
{
    // 世界の明るさ。点けた灯火の数で、主光源・霧を変える。設計：doc/demo-game/11-lighting.md §5.1
    // ライトマップの物には実行時の Ambient の変更が効かないので、主光源（直接光は実行時）と霧で明るさを出す
    public class WorldLightingController : MonoBehaviour
    {
        [Serializable]
        public struct Stage
        {
            public Color LightColor;
            public float LightIntensity;

            [Tooltip("霧の色。0 灯は黒に近い寒色（ステージ全体の黒い霧）")]
            public Color FogColor;

            [Tooltip("霧の濃さ（Exponential Squared）")]
            public float FogDensity;

            public Stage(Color lightColor, float lightIntensity, Color fogColor, float fogDensity)
            {
                LightColor = lightColor;
                LightIntensity = lightIntensity;
                FogColor = fogColor;
                FogDensity = fogDensity;
            }
        }

        [SerializeField]
        private GameProgress _progress;

        [Tooltip("主光源（月の光）")]
        [SerializeField]
        private Light _mainLight;

        [Tooltip("点けた灯火の数（0〜3）ごとの値")]
        [SerializeField]
        private Stage[] _stages =
        {
            new Stage(new Color(0.55f, 0.65f, 0.9f), 0.3f, new Color(0.03f, 0.035f, 0.06f), 0.035f),
            new Stage(new Color(0.7f, 0.72f, 0.85f), 0.45f, new Color(0.08f, 0.08f, 0.1f), 0.025f),
            new Stage(new Color(0.9f, 0.8f, 0.75f), 0.6f, new Color(0.16f, 0.13f, 0.12f), 0.015f),
            new Stage(new Color(1f, 0.85f, 0.65f), 0.8f, new Color(0.3f, 0.22f, 0.16f), 0.008f),
        };

        [Tooltip("灯火が点いてから、次の段の値になるまでの秒数")]
        [SerializeField, Range(0f, 10f)]
        private float _transitionSeconds = 3f;

        private readonly CompositeDisposable _disposables = new();
        private Stage _from;
        private Stage _to;
        private float _elapsed;

        private void Start()
        {
            if (_progress == null)
            {
                _progress = FindFirstObjectByType<GameProgress>();
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            int start = _progress != null ? _progress.LitCount.CurrentValue : 0;
            _from = _to = GetStage(start);
            _elapsed = _transitionSeconds;
            Apply(_to);

            if (_progress != null)
            {
                _progress.LitCount.Skip(1).Subscribe(OnLitCountChanged).AddTo(_disposables);
            }
        }

        private void OnLitCountChanged(int count)
        {
            _from = Current();
            _to = GetStage(count);
            _elapsed = 0f;
        }

        private void Update()
        {
            if (_elapsed >= _transitionSeconds) return;

            _elapsed += Time.deltaTime;
            Apply(Current());
        }

        private Stage Current()
        {
            float t = _transitionSeconds <= 0f ? 1f : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_elapsed / _transitionSeconds));
            return new Stage(
                Color.Lerp(_from.LightColor, _to.LightColor, t),
                Mathf.Lerp(_from.LightIntensity, _to.LightIntensity, t),
                Color.Lerp(_from.FogColor, _to.FogColor, t),
                Mathf.Lerp(_from.FogDensity, _to.FogDensity, t));
        }

        private Stage GetStage(int litCount)
        {
            if (_stages.Length == 0) return new Stage(Color.white, 1f, Color.gray, 0f);

            return _stages[Mathf.Clamp(litCount, 0, _stages.Length - 1)];
        }

        private void Apply(Stage stage)
        {
            if (_mainLight != null)
            {
                _mainLight.color = stage.LightColor;
                _mainLight.intensity = stage.LightIntensity;
            }

            RenderSettings.fogColor = stage.FogColor;
            RenderSettings.fogDensity = stage.FogDensity;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
