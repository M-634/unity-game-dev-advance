using TMPro;
using UnityEngine;

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

            if (_timeText == null) return;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            _timeText.text = $"TIME  {minutes:00}:{seconds:00}";
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }
    }
}
