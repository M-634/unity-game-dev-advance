using System;
using System.Collections.Generic;
using UnityEngine;

namespace Section1.AnimationRigging
{
    // キャラクターの「見た目」層。Prefab の Visual に付ける
    // モデルは Visual の子に置くだけでよい。Animator はモデル側のものを子から探して使う
    // ロジック側は Animator やマテリアルを直接触らず、ここを経由する
    public class CharacterVisual : MonoBehaviour
    {
        private static readonly int HitFlashId = Shader.PropertyToID("_HitFlash");
        private static readonly int HitFlashColorId = Shader.PropertyToID("_HitFlashColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [Tooltip("モデルの Animator に設定するコントローラー。モデル側で設定済みなら空でよい")]
        [SerializeField]
        private RuntimeAnimatorController _controller;

        [Tooltip("未設定なら子から探す。モデルを差し替えたら空に戻しておけば自動で拾い直す")]
        [SerializeField]
        private Animator _animator;

        // モデルごとに Animator Controller のパラメータが揃っているとは限らない。
        // 無いパラメータに Set すると毎フレーム警告が出るので、持っているものだけ送る
        private readonly HashSet<int> _parameters = new HashSet<int>();

        // renderer.materials はマテリアルを個体ごとに複製する。
        // 複製しても同じシェーダーなので SRP Batcher のバッチは崩れない（MaterialPropertyBlock は崩れる）
        // 使うときまで複製しない（点滅も消滅もしないキャラクターは共有のまま）
        private Material[] _materials;

        // 常にかけておく色（無敵の構えなど）。一瞬の点滅が終わったらこれに戻る
        private Color _overlayColor = Color.white;
        private float _overlayAmount;
        private float _flashEndTime;
        private bool _isFlashing;

        public Animator Animator => _animator;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator == null) return;

            if (_controller != null && _animator.runtimeAnimatorController == null)
            {
                _animator.runtimeAnimatorController = _controller;
            }

            // 移動は CharacterController / NavMeshAgent が持つ。モーションに位置を動かさせない
            _animator.applyRootMotion = false;

            CacheParameters();
        }

        private void CacheParameters()
        {
            _parameters.Clear();

            if (_animator.runtimeAnimatorController == null) return;

            foreach (var parameter in _animator.parameters)
            {
                _parameters.Add(parameter.nameHash);
            }
        }

        private bool CanSet(int id)
        {
            return _animator != null && _animator.isActiveAndEnabled && _parameters.Contains(id);
        }

        public void SetFloat(int id, float value)
        {
            if (!CanSet(id)) return;

            _animator.SetFloat(id, value);
        }

        public void SetBool(int id, bool value)
        {
            if (!CanSet(id)) return;

            _animator.SetBool(id, value);
        }

        public void SetTrigger(int id)
        {
            if (!CanSet(id)) return;

            _animator.SetTrigger(id);
        }

        // 一瞬だけ色をかける（被弾）
        public void Flash(Color color, float duration)
        {
            SetMaterialColor(HitFlashColorId, color);
            SetMaterialFloat(HitFlashId, 1f);
            _flashEndTime = Time.time + duration;
            _isFlashing = true;
        }

        // 色をかけ続ける。amount を 0 にすると外れる
        public void SetOverlay(Color color, float amount)
        {
            _overlayColor = color;
            _overlayAmount = amount;

            if (_isFlashing) return;

            ApplyOverlay();
        }

        public void SetEmission(Color color)
        {
            SetMaterialColor(EmissionColorId, color);
        }

        // 0 で見える、1 で溶けて消える
        public void SetDissolve(float value)
        {
            SetMaterialFloat(DissolveId, value);
        }

        private void Update()
        {
            if (!_isFlashing || Time.time < _flashEndTime) return;

            _isFlashing = false;
            ApplyOverlay();
        }

        private void ApplyOverlay()
        {
            SetMaterialColor(HitFlashColorId, _overlayColor);
            SetMaterialFloat(HitFlashId, _overlayAmount);
        }

        private Material[] GetMaterials()
        {
            if (_materials != null) return _materials;

            var materials = new List<Material>();

            foreach (var target in GetComponentsInChildren<Renderer>(true))
            {
                materials.AddRange(target.materials);
            }

            _materials = materials.ToArray();
            return _materials;
        }

        private void SetMaterialFloat(int id, float value)
        {
            foreach (var material in GetMaterials())
            {
                material.SetFloat(id, value);
            }
        }

        private void SetMaterialColor(int id, Color value)
        {
            foreach (var material in GetMaterials())
            {
                material.SetColor(id, value);
            }
        }

        private void OnDestroy()
        {
            if (_materials == null) return;

            // renderer.materials で複製したものは自動では破棄されない
            foreach (var material in _materials)
            {
                Destroy(material);
            }

            _materials = Array.Empty<Material>();
        }
    }
}
