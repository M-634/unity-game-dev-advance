using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace DemoGame
{
    public class TimedDoor : MonoBehaviour
    {
        public enum OpenMode
        {
            Hold,
            Timed,
        }

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField]
        private SwitchBase _switch;

        [SerializeField]
        private SlidingDoor _door;

        [SerializeField]
        private OpenMode _mode = OpenMode.Hold;

        [Tooltip("Hold：スイッチが離されてから閉じ始めるまでの秒数（点滅を含む）")]
        [SerializeField, Range(0f, 10f)]
        private float _closeDelay = 1f;

        [Tooltip("Timed：開ききってから閉じ始めるまでの秒数")]
        [SerializeField, Range(0.5f, 30f)]
        private float _openSeconds = 5f;

        [Header("残り時間の知らせ")]
        [Tooltip("閉じる前に点滅させる対象。未設定なら点滅しない")]
        [SerializeField]
        private Renderer _warningRenderer;

        [SerializeField, ColorUsage(false, true)]
        private Color _warningColor = new Color(2f, 0.6f, 0.2f);

        [SerializeField, Range(0f, 5f)]
        private float _warningSeconds = 1f;

        private readonly CompositeDisposable _disposables = new();
        private CancellationTokenSource _cycle;
        private Material _warningMaterial;

        private void Awake()
        {
            if (_door == null)
            {
                _door = GetComponent<SlidingDoor>();
            }

            if (_warningRenderer != null)
            {
                _warningMaterial = _warningRenderer.material;
            }
        }

        private void Start()
        {
            if (_switch == null || _door == null)
            {
                Debug.LogError($"{nameof(TimedDoor)}: スイッチか扉が設定されていません。", this);
                return;
            }

            _switch.Pressed.Subscribe(_ => Restart(RunPressedAsync)).AddTo(_disposables);

            if (_mode == OpenMode.Hold)
            {
                _switch.Released.Subscribe(_ => Restart(RunReleasedAsync)).AddTo(_disposables);
            }
        }

        private void Restart(Func<CancellationToken, UniTask> run)
        {
            _cycle?.Cancel();
            _cycle?.Dispose();
            _cycle = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            run(_cycle.Token).SuppressCancellationThrow().Forget();
        }

        private async UniTask RunPressedAsync(CancellationToken ct)
        {
            SetWarning(false);

            await _door.OpenAsync(ct);

            if (_mode == OpenMode.Hold) return;

            float waitSeconds = Mathf.Max(0f, _openSeconds - _warningSeconds);
            await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: ct);

            await WarnAndCloseAsync(_warningSeconds, ct);
        }

        private async UniTask RunReleasedAsync(CancellationToken ct)
        {
            float warning = Mathf.Min(_warningSeconds, _closeDelay);
            float waitSeconds = Mathf.Max(0f, _closeDelay - warning);
            if (waitSeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: ct);
            }

            await WarnAndCloseAsync(warning, ct);
        }

        private async UniTask WarnAndCloseAsync(float warningSeconds, CancellationToken ct)
        {
            float end = Time.time + warningSeconds;
            while (Time.time < end)
            {
                SetWarning(Mathf.Repeat(Time.time * 6f, 1f) < 0.5f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            SetWarning(false);
            await _door.CloseAsync(ct);
        }

        private void SetWarning(bool on)
        {
            if (_warningMaterial == null) return;

            _warningMaterial.SetColor(EmissionColorId, on ? _warningColor : Color.black);
        }

        private void OnDestroy()
        {
            _cycle?.Cancel();
            _cycle?.Dispose();
            _disposables.Dispose();

            if (_warningMaterial != null)
            {
                Destroy(_warningMaterial);
            }
        }
    }
}
