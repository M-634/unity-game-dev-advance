using UnityEngine;

namespace DemoGame
{
    public class BossRoomTrigger : MonoBehaviour
    {
        [Tooltip("ボス戦前の演出。設定されていれば、演出のあとでボスが動き出す")]
        [SerializeField]
        private BossIntroDirector _intro;

        [Tooltip("演出が無いときに、すぐ動かすボス")]
        [SerializeField]
        private BossController _boss;

        [Tooltip("一度入ったら、このトリガーを無効にする")]
        [SerializeField]
        private bool _once = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (_intro != null)
            {
                _intro.Play();
            }
            else if (_boss != null)
            {
                _boss.Activate();
            }
            else
            {
                return;
            }

            if (_once)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
