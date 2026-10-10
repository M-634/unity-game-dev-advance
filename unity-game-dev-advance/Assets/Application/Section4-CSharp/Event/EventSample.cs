using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Section4.Events
{
    [Serializable]
    public class SampleUnityEvent : UnityEvent { }

    public class EventSample : MonoBehaviour
    {
        public event Action SampleAction;

        public event Func<int, int, int> SampleFunc;

        [SerializeField]
        private Button _sampleButton;

        [SerializeField]
        private SampleUnityEvent _sampleUnityEvent;

        private void Start()
        {
            // ① Button の onClick も、中身は同じ仕組み
            if (_sampleButton != null)
            {
                _sampleButton.onClick.AddListener(LogFirst);
                _sampleButton.onClick.AddListener(LogSecond);
            }

            // ② Inspector から登録できる UnityEvent
            _sampleUnityEvent?.Invoke();

            // ③ C# の素の event
            SampleAction += LogFirst;
            SampleAction += LogSecond;
            SampleFunc += Add;

            // 登録が 0 件のときに Invoke すると例外になるため ?. で守る
            SampleAction?.Invoke();

            // ⚠️ Func を複数登録しても、受け取れるのは最後に登録した1つの戻り値だけ
            SampleFunc?.Invoke(10, 10);
        }

        private void OnDestroy()
        {
            if (_sampleButton != null)
            {
                _sampleButton.onClick.RemoveListener(LogFirst);
                _sampleButton.onClick.RemoveListener(LogSecond);
            }

            // 登録したら必ず外す。外し忘れると、消えたはずのオブジェクトが呼ばれ続ける
            SampleAction -= LogFirst;
            SampleAction -= LogSecond;
            SampleFunc -= Add;
        }

        private void LogFirst()
        {
            Debug.Log("1つめの処理");
        }

        private void LogSecond()
        {
            Debug.Log("2つめの処理");
        }

        private int Add(int a, int b)
        {
            var result = a + b;
            Debug.Log($"{a} + {b} = {result}");
            return result;
        }
    }
}
