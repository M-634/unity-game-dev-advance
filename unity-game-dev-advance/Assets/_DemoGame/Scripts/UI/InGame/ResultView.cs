using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DemoGame
{
    public class ResultView : MonoBehaviour
    {
        [Tooltip("パネル全体。表示・非表示を切り替える対象")]
        [SerializeField]
        private GameObject _root;

        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private TMP_Text _timeText;

        [SerializeField]
        private string _clearMessage = "CLEAR";

        [SerializeField]
        private string _gameOverMessage = "GAME OVER";

        [Tooltip("もう一度インゲームを遊ぶ。表示したとき最初に選ばれる")]
        [SerializeField]
        private Button _retryButton;

        [SerializeField]
        private Button _titleButton;

        private readonly Subject<Unit> _retryClicked = new();
        private readonly Subject<Unit> _titleClicked = new();

        public Observable<Unit> RetryClicked => _retryClicked;
        public Observable<Unit> TitleClicked => _titleClicked;

        public void Show(bool isCleared, float elapsedTime)
        {
            if (_root != null)
            {
                _root.SetActive(true);
            }

            if (_titleText != null)
            {
                _titleText.text = isCleared ? _clearMessage : _gameOverMessage;
            }

            if (_timeText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);
                _timeText.text = $"TIME  {minutes:00}:{seconds:00}";
            }

            SetInteractable(true);

            if (EventSystem.current != null && _retryButton != null)
            {
                EventSystem.current.SetSelectedGameObject(_retryButton.gameObject);
            }
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (_retryButton != null) _retryButton.interactable = interactable;
            if (_titleButton != null) _titleButton.interactable = interactable;
        }

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_titleButton != null) _titleButton.onClick.AddListener(OnTitleClicked);
        }

        private void OnRetryClicked()
        {
            _retryClicked.OnNext(Unit.Default);
        }

        private void OnTitleClicked()
        {
            _titleClicked.OnNext(Unit.Default);
        }

        private void OnDestroy()
        {
            if (_retryButton != null) _retryButton.onClick.RemoveListener(OnRetryClicked);
            if (_titleButton != null) _titleButton.onClick.RemoveListener(OnTitleClicked);

            _retryClicked.Dispose();
            _titleClicked.Dispose();
        }
    }
}
