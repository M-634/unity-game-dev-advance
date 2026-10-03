using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace DemoGame
{
    // SettingsModel と SettingsView を双方向につなぐ。
    // View → Model は「値を変えて」と頼むだけ。Model → View は変わった値を出すだけ。
    // タイトルからもポーズからも Open() で開き、閉じたら保存して Closed を流す。
    public class SettingsPresenter : MonoBehaviour
    {
        private static readonly string[] ScreenModeLabels = { "Fullscreen", "Windowed" };
        private static readonly string[] QualityLabels = { "Low", "Medium", "High", "Ultra" };
        private static readonly string[] OnOffLabels = { "Off", "On" };

        [SerializeField]
        private SettingsView _view;

        private readonly CompositeDisposable _disposables = new();
        private readonly Subject<Unit> _closed = new();
        private SettingsModel _model;
        private List<ScreenResolution> _resolutions;
        private bool _isBound;

        public Observable<Unit> Closed => _closed;

        public void Open()
        {
            if (!Bind()) return;

            _view.Show();
        }

        private void Close()
        {
            SettingsStore.Save();
            _view.Hide();
            _closed.OnNext(Unit.Default);
        }

        private void Start()
        {
            Bind();
        }

        // 他の Presenter の Start から先に Open() が呼ばれても動くように、最初に使われたときにつなぐ
        private bool Bind()
        {
            if (_isBound) return true;

            if (_view == null)
            {
                Debug.LogError($"{nameof(SettingsPresenter)}: SettingsView が設定されていません。", this);
                return false;
            }

            _isBound = true;
            _model = SettingsStore.Model;
            _resolutions = CollectResolutions();

            BindSelector(
                _view.Resolution,
                _resolutions.Select(r => r.ToString()).ToArray(),
                _model.Resolution.Select(IndexOfResolution),
                i => _model.SetResolution(_resolutions[i]));

            BindSelector(
                _view.ScreenMode,
                ScreenModeLabels,
                _model.ScreenMode.Select(mode => (int)mode),
                i => _model.SetScreenMode((ScreenMode)i));

            BindSelector(
                _view.Quality,
                QualityLabels,
                _model.Quality.Select(quality => (int)quality),
                i => _model.SetQuality((GraphicsQuality)i));

            BindSelector(
                _view.VSync,
                OnOffLabels,
                _model.VSync.Select(on => on ? 1 : 0),
                i => _model.SetVSync(i == 1));

            BindSelector(
                _view.FrameRate,
                SettingsModel.FrameRateChoices.Select(fps => fps > 0 ? fps.ToString() : "Unlimited").ToArray(),
                _model.FrameRateLimit.Select(fps => IndexOf(SettingsModel.FrameRateChoices, fps)),
                i => _model.SetFrameRateLimit(SettingsModel.FrameRateChoices[i]));

            // VSync が On のあいだはフレームレートの上限が効かないので、選べなくする
            _model.VSync
                .Subscribe(on => _view.FrameRate.SetInteractable(!on))
                .AddTo(_disposables);

            BindVolume(_view.MasterVolume, _model.MasterVolume, _model.SetMasterVolume);
            BindVolume(_view.BgmVolume, _model.BgmVolume, _model.SetBgmVolume);
            BindVolume(_view.SeVolume, _model.SeVolume, _model.SetSeVolume);

            _view.ResetClicked.Subscribe(_ => _model.ResetToDefault()).AddTo(_disposables);
            _view.BackRequested.Subscribe(_ => Close()).AddTo(_disposables);

            _view.Hide();
            return true;
        }

        private void BindSelector(OptionSelector selector, IReadOnlyList<string> labels, Observable<int> modelIndex, Action<int> onSelected)
        {
            if (selector == null) return;

            selector.SetOptions(labels);
            modelIndex.Subscribe(selector.SetIndexWithoutNotify).AddTo(_disposables);
            selector.IndexChanged.Subscribe(onSelected).AddTo(_disposables);
        }

        private void BindVolume(VolumeSlider slider, Observable<float> modelValue, Action<float> onChanged)
        {
            if (slider == null) return;

            modelValue.Subscribe(slider.SetValueWithoutNotify).AddTo(_disposables);
            slider.ValueChanged.Subscribe(onChanged).AddTo(_disposables);
        }

        // 幅×高さで重複を除き、小さい順に並べる
        private static List<ScreenResolution> CollectResolutions()
        {
            List<ScreenResolution> list = Screen.resolutions
                .Select(r => new ScreenResolution(r.width, r.height))
                .Distinct()
                .OrderBy(r => r.Width)
                .ThenBy(r => r.Height)
                .ToList();

            if (list.Count == 0)
            {
                Resolution current = Screen.currentResolution;
                list.Add(new ScreenResolution(current.width, current.height));
            }

            return list;
        }

        // 未設定（画面の解像度）や、このモニターに無い解像度は一番大きいものとして見せる
        private int IndexOfResolution(ScreenResolution resolution)
        {
            int index = _resolutions.IndexOf(resolution);
            return index >= 0 ? index : _resolutions.Count - 1;
        }

        private static int IndexOf(IReadOnlyList<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value) return i;
            }

            return 0;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _closed.Dispose();
        }
    }
}
