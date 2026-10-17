using R3;
using UnityEngine;

namespace DemoGame
{
    public class EncounterLanternTrigger : MonoBehaviour
    {
        [SerializeField]
        private EncounterArea _encounter;

        [SerializeField]
        private Lantern _lantern;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_lantern == null)
            {
                _lantern = GetComponentInParent<Lantern>();
            }

            if (_encounter == null || _lantern == null)
            {
                Debug.LogError($"{nameof(EncounterLanternTrigger)}: EncounterArea か Lantern が設定されていません。", this);
                return;
            }

            if (_encounter.IsCleared)
            {
                _lantern.LightUp();
                return;
            }

            _encounter.Cleared.Subscribe(_ => _lantern.LightUp()).AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
