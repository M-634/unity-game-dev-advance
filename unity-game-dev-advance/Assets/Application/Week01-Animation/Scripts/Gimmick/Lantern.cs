using R3;
using UnityEngine;

namespace Week01
{
    // 灯は「自分が点いた」としか言わない。
    // 何をしたら点くかは XxxLanternTrigger が持ち、点いたあと何が起きるかは受け取った側が決める
    public class Lantern : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        
        [Header("見た目")]
        [SerializeField]
        private Renderer _renderer;

        [SerializeField]
        private Light _light;

        [SerializeField]
        private Color _unlitColor = new Color(0.25f, 0.3f, 0.4f);

        [SerializeField]
        private Color _litColor = new Color(1f, 0.8f, 0.4f);

        [Tooltip("点灯時の発光の強さ。1 を超えると Bloom で光る")]
        [SerializeField, Range(0f, 10f)]
        private float _litEmissionIntensity = 3f;

        [Tooltip("火だけついた状態（灯火 2：敵を倒すまで）の明るさの割合")]
        [SerializeField, Range(0f, 1f)]
        private float _kindledRatio = 0.35f;

        [Tooltip("点いたら天へ昇る光の柱（LightPillar）。最初は非表示にしておく")]
        [SerializeField]
        private GameObject _pillar;

        [Tooltip("灯火の炎（FX_Fire。火だけついたときから出す）")]
        [SerializeField]
        private GameObject _fireEffectPrefab;

        [Tooltip("点いた瞬間に出すエフェクト（doc/demo-game/12-vfx.md #8）")]
        [SerializeField]
        private GameObject _lightUpEffect;

        private readonly Subject<Unit> _litUp = new();

        public Observable<Unit> LitUp => _litUp;
        public bool IsLit { get; private set; }

        // 火はついたが、まだ点いていない（灯火 2 で敵が出ている間）
        public bool IsKindled { get; private set; }

        private Material _material;
        private float _lightIntensity;
        private GameObject _fireEffect;

        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }
            
            if (_renderer != null)
            {
                _material = _renderer.material;
                _material.color = _unlitColor;
                _material.SetColor(EmissionColorId, Color.black);
            }

            if (_light != null)
            {
                _lightIntensity = _light.intensity;
                _light.enabled = false;
            }

            if (_pillar != null)
            {
                _pillar.SetActive(false);
            }

            // 炎は灯の見た目の中心に付けておき、点くまで隠す
            if (_fireEffectPrefab != null)
            {
                _fireEffect = FxSpawner.Attach(_fireEffectPrefab, transform);
                if (_renderer != null)
                {
                    _fireEffect.transform.position = _renderer.bounds.center;
                }

                _fireEffect.SetActive(false);
            }
        }

        // 火だけつける。LitCount は増やさない。LightUp() で本当に点く
        public void Kindle()
        {
            if (IsLit || IsKindled) return;

            IsKindled = true;
            SetFireVisible(0.5f);

            if (_material != null)
            {
                _material.color = Color.Lerp(_unlitColor, _litColor, _kindledRatio);
                _material.SetColor(EmissionColorId, _litColor * (_litEmissionIntensity * _kindledRatio));
            }

            if (_light != null)
            {
                _light.enabled = true;
                _light.intensity = _lightIntensity * _kindledRatio;
            }
        }

        public void LightUp()
        {
            if (IsLit) return;

            IsLit = true;

            if (_material != null)
            {
                _material.color = _litColor;
                _material.SetColor(EmissionColorId, _litColor * _litEmissionIntensity);
            }

            if (_light != null)
            {
                _light.enabled = true;
                _light.intensity = _lightIntensity;
            }

            if (_pillar != null)
            {
                _pillar.SetActive(true);
            }

            SetFireVisible(1f);
            FxSpawner.Spawn(_lightUpEffect, _renderer != null ? _renderer.bounds.center : transform.position + Vector3.up);

            _litUp.OnNext(Unit.Default);
            _litUp.OnCompleted();
        }

        // 炎の大きさ（火だけ：小さく ／ 点いた：そのまま）
        private void SetFireVisible(float scale)
        {
            if (_fireEffect == null) return;

            _fireEffect.SetActive(true);

            Vector3 s = transform.lossyScale;
            _fireEffect.transform.localScale = new Vector3(scale / s.x, scale / s.y, scale / s.z);
        }

        private void OnDestroy()
        {
            _litUp.Dispose();

            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
