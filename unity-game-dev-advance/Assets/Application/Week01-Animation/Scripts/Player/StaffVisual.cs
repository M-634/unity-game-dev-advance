using UnityEngine;

namespace Week01
{
    // 杖の見た目。杖の Prefab（WeaponSocket の下）のルートに付ける
    // 状態（火の量・光の段・チャージ）は Staff / PlayerAttack から渡される。ここは見せ方だけを持つ
    // 炎のエフェクト（doc/demo-game/12-vfx.md #2）ができるまでは、先端の球の発光と Point Light で見せる
    public class StaffVisual : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Tooltip("火焔玉を出す位置（杖の先）")]
        [SerializeField]
        private Transform _muzzle;

        [Tooltip("杖を持っていないときに隠すもの（杖のモデル全体）")]
        [SerializeField]
        private GameObject _model;

        [Tooltip("火が灯ると光る先端")]
        [SerializeField]
        private Renderer _tipRenderer;

        [SerializeField]
        private Light _light;

        [Header("パーティクル（doc/demo-game/12-vfx.md #2・#5）")]
        [Tooltip("杖の先の炎（FX_StaffFlame）。火の量で数、光の段で大きさを変える")]
        [SerializeField]
        private GameObject _flameEffectPrefab;

        [Tooltip("チャージ中に杖の先へ集まる火の粉（FX_Charge）。1段以上で出る")]
        [SerializeField]
        private GameObject _chargeEffectPrefab;

        [Tooltip("チャージの段が上がった瞬間のリング")]
        [SerializeField]
        private GameObject _chargeLevelUpEffect;

        [Header("色（光の段 0〜3 の順）")]
        [SerializeField, ColorUsage(false, true)]
        private Color[] _glowColors =
        {
            new Color(1f, 0.45f, 0.1f),
            new Color(1f, 0.65f, 0.2f),
            new Color(1f, 0.85f, 0.4f),
            new Color(1f, 0.95f, 0.8f),
        };

        [SerializeField]
        private Color _unlitColor = new Color(0.15f, 0.12f, 0.12f);

        [Header("発光の強さ")]
        [SerializeField, Range(0f, 10f)]
        private float _emissionMin = 1.5f;

        [SerializeField, Range(0f, 10f)]
        private float _emissionPerGlow = 1.2f;

        [SerializeField, Range(0f, 10f)]
        private float _emissionPerCharge = 1.5f;

        [Header("ライト")]
        [SerializeField, Range(0f, 5f)]
        private float _lightIntensityMin = 0.6f;

        [SerializeField, Range(0f, 5f)]
        private float _lightIntensityMax = 1.5f;

        [SerializeField, Range(0f, 2f)]
        private float _lightIntensityPerGlow = 0.3f;

        [SerializeField, Range(1f, 20f)]
        private float _lightRange = 5f;

        [SerializeField, Range(0f, 5f)]
        private float _lightRangePerGlow = 1.5f;

        private ParticleSystem[] _flameSystems = new ParticleSystem[0];
        private float[] _flameRates = new float[0];
        private float[] _flameSizes = new float[0];
        private ParticleSystem[] _chargeSystems = new ParticleSystem[0];
        private float[] _chargeRates = new float[0];
        private Material _tipMaterial;
        private float _flame01;
        private int _glow;
        private int _charge = -1;

        public Transform Muzzle => _muzzle;

        private void Awake()
        {
            if (_tipRenderer != null)
            {
                // 杖は1本だけなので複製してよい（SRP Batcher のバッチは分かれるが1つだけ）
                _tipMaterial = _tipRenderer.material;
            }

            CacheParticles(FxSpawner.Attach(_flameEffectPrefab, _muzzle), out _flameSystems, out _flameRates, out _flameSizes);
            CacheParticles(FxSpawner.Attach(_chargeEffectPrefab, _muzzle), out _chargeSystems, out _chargeRates, out _);

            Apply();
        }

        public void SetOwned(bool isOwned)
        {
            if (_model != null)
            {
                _model.SetActive(isOwned);
            }
        }

        public void SetFlame(float flame01, int glow)
        {
            _flame01 = Mathf.Clamp01(flame01);
            _glow = glow;
            Apply();
        }

        // チャージ中の段。-1 はチャージしていない
        public void SetCharge(int level)
        {
            // 段が上がった瞬間（1段目から）にリングを出す
            if (level > _charge && level >= 1 && _chargeLevelUpEffect != null && _muzzle != null)
            {
                FxSpawner.Spawn(_chargeLevelUpEffect, _muzzle.position, _muzzle.rotation, 0.6f + 0.3f * level, _muzzle);
            }

            _charge = level;
            Apply();
        }

        private static void CacheParticles(GameObject root, out ParticleSystem[] systems, out float[] rates, out float[] sizes)
        {
            systems = root != null ? root.GetComponentsInChildren<ParticleSystem>(true) : new ParticleSystem[0];
            rates = new float[systems.Length];
            sizes = new float[systems.Length];
            for (int i = 0; i < systems.Length; i++)
            {
                rates[i] = systems[i].emission.rateOverTimeMultiplier;
                sizes[i] = systems[i].main.startSizeMultiplier;
            }
        }

        private void ApplyParticles(bool isLit, int charge)
        {
            // 炎：火の量で数、光の段で大きさ。消えていたら出さない
            float rateScale = isLit ? Mathf.Lerp(0.4f, 1f, _flame01) * (1f + 0.35f * _glow) : 0f;
            float sizeScale = 1f + 0.25f * _glow + 0.2f * charge;
            for (int i = 0; i < _flameSystems.Length; i++)
            {
                var emission = _flameSystems[i].emission;
                emission.rateOverTimeMultiplier = _flameRates[i] * rateScale;
                var main = _flameSystems[i].main;
                main.startSizeMultiplier = _flameSizes[i] * sizeScale;
            }

            // チャージ：1段以上で、段が上がるほど多く
            for (int i = 0; i < _chargeSystems.Length; i++)
            {
                var emission = _chargeSystems[i].emission;
                emission.rateOverTimeMultiplier = charge >= 1 ? _chargeRates[i] * charge : 0f;
            }
        }

        private void Apply()
        {
            bool isLit = _flame01 > 0f;
            int charge = Mathf.Max(0, _charge);
            int colorLevel = Mathf.Clamp(Mathf.Max(_glow, charge), 0, _glowColors.Length - 1);
            Color color = _glowColors.Length > 0 ? _glowColors[colorLevel] : Color.white;

            if (_tipMaterial != null)
            {
                float emission = _emissionMin * _flame01 + _emissionPerGlow * _glow + _emissionPerCharge * charge;
                _tipMaterial.SetColor("_BaseColor", isLit ? color : _unlitColor);
                _tipMaterial.SetColor(EmissionColorId, isLit ? color * emission : Color.black);
            }

            ApplyParticles(isLit, charge);

            if (_light != null)
            {
                _light.enabled = isLit;
                _light.color = color;
                _light.intensity = Mathf.Lerp(_lightIntensityMin, _lightIntensityMax, _flame01)
                    + _lightIntensityPerGlow * (_glow + charge);
                _light.range = _lightRange + _lightRangePerGlow * _glow;
            }
        }

        private void OnDestroy()
        {
            if (_tipMaterial != null)
            {
                Destroy(_tipMaterial);
            }
        }
    }
}
