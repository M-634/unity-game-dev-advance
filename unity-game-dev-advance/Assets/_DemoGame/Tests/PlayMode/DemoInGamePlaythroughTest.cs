using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DemoGame.Tests
{
    public class DemoInGamePlaythroughTest
    {
        private const float TimeScale = 4f;

        private PlayerController _player;
        private GameProgress _progress;
        private InGameController _inGame;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(SceneNames.InGame);
            yield return null;

            _player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            _progress = UnityEngine.Object.FindFirstObjectByType<GameProgress>();
            _inGame = UnityEngine.Object.FindFirstObjectByType<InGameController>();

            Assert.IsNotNull(_player, "PlayerController がシーンにない");
            Assert.IsNotNull(_progress, "GameProgress がシーンにない");
            Assert.IsNotNull(_inGame, "InGameController がシーンにない");

            Time.timeScale = TimeScale;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator PlaysFromIntroToClear()
        {
            var staff = _player.GetComponentInChildren<Staff>(true);
            Assert.IsNotNull(staff, "プレイヤーに Staff がない");

            var altar = FindByName<Bonfire>("Altar_Bonfire");
            Assert.IsNotNull(altar, "祭壇の篝火（Altar_Bonfire）がない");
            Assert.IsFalse(altar.IsBurning, "祭壇の火が最初から燃えている");

            yield return WaitUntil(() => staff.IsOwned.CurrentValue, 90f, "導入演出のあとで杖を拾えていない");
            yield return WaitUntil(() => altar.IsBurning, 30f, "導入演出のあとで祭壇に火が上がらない");
            yield return WaitUntil(() => !AnyCutscenePlaying(), 30f, "祭壇の演出が終わらない");

            Teleport(altar.transform.position + new Vector3(-2f, 0.2f, -2f));
            Assert.IsTrue(staff.Refill(staff.MaxGlow), "祭壇の火で杖を灯せない");
            Assert.IsTrue(staff.IsLit, "杖が灯っていない");

            var overlookTrigger = UnityEngine.Object.FindFirstObjectByType<SealOverlookTrigger>();
            Assert.IsNotNull(overlookTrigger, "SealOverlookTrigger がない");
            var overlook = FindByName<CameraShowcase>("OverlookShowcase");
            Assert.IsNotNull(overlook, "OverlookShowcase がない");
            Teleport(overlookTrigger.transform.position);
            yield return WaitUntil(() => overlook.IsPlaying, 5f, "封印の前で振り返りの演出が始まらない");
            yield return WaitUntil(() => !overlook.IsPlaying, 30f, "振り返りの演出が終わらない");

            var beacons = UnityEngine.Object.FindObjectsByType<LanternBeacon>(FindObjectsSortMode.None)
                .OrderBy(b => b.Order).ToArray();
            Assert.AreEqual(3, beacons.Length, "灯火の光（LanternBeacon）が 3 つない");
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, beacons.Select(b => b.Order).ToArray(), "灯火の順番が 1, 2, 3 になっていない");

            for (int i = 0; i < beacons.Length; i++)
            {
                var lantern = beacons[i].GetComponentInParent<Lantern>();
                Assert.IsNotNull(lantern, $"{i + 1} つ目の光に Lantern がない");

                Teleport(lantern.transform.position + new Vector3(0f, 0.5f, 3f));
                yield return null;
                lantern.LightUp();
                Assert.AreEqual(i + 1, _progress.LitCount.CurrentValue, $"{i + 1} つ目の灯火で LitCount が増えない");

                if (i == 0)
                {
                    yield return KillPlayerAndWaitRespawn();
                    yield return WaitUntil(() => CountEnemies() > 0, 20f, "灯火 1 のあとにフィールドの敵が出ない");
                }

                yield return WaitUntil(() => !AnyCutscenePlaying(), 40f, $"{i + 1} つ目の灯火のあとの演出が終わらない");
            }

            var door = UnityEngine.Object.FindFirstObjectByType<BossDoor>();
            Assert.IsNotNull(door, "BossDoor がない");
            yield return WaitUntil(() => door.IsOpen, 40f, "灯火 3 つで封印の扉が開かない");
            yield return WaitUntil(() => !AnyCutscenePlaying(), 40f, "封印解除の演出が終わらない");

            var boss = UnityEngine.Object.FindFirstObjectByType<BossController>();
            Assert.IsNotNull(boss, "ボスがいない");
            var bossTrigger = UnityEngine.Object.FindFirstObjectByType<BossRoomTrigger>();
            Assert.IsNotNull(bossTrigger, "BossRoomTrigger がない");
            Teleport(bossTrigger.transform.position);
            yield return WaitUntil(() => boss.IsActivated, 60f, "ボス部屋に入ってもボスが動き出さない");
            yield return WaitUntil(() => !AnyCutscenePlaying(), 40f, "ボス登場の演出が終わらない");

            yield return WaitUntil(() =>
            {
                if (!boss.IsDead) boss.TakeDamage(9999);
                return boss.IsDead;
            }, 30f, "ボスが倒れない");

            var beacon = UnityEngine.Object.FindFirstObjectByType<ClearBeacon>(FindObjectsInactive.Include);
            Assert.IsNotNull(beacon, "ClearBeacon がない");
            yield return WaitUntil(() => beacon.IsAppeared, 60f, "ボスを倒しても光が出ない");
            yield return WaitUntil(() => !AnyCutscenePlaying(), 40f, "撃破演出が終わらない");

            Teleport(beacon.transform.position);
            yield return WaitUntil(() => _inGame.IsFinished, 20f, "光に触れてもクリアにならない");

            var result = UnityEngine.Object.FindFirstObjectByType<ResultView>();
            Assert.IsNotNull(result, "ResultView がない");
            yield return WaitUntil(() => Cursor.visible, 5f, "クリア後にカーソルが戻らない");
        }

        private IEnumerator KillPlayerAndWaitRespawn()
        {
            var health = _player.GetComponent<PlayerHealth>();
            Assert.IsNotNull(health, "PlayerHealth がない");

            Vector3 before = _player.transform.position;
            health.TakeDamage(9999);
            Assert.IsTrue(health.IsDead, "プレイヤーが倒れない");

            yield return WaitUntil(() => !health.IsDead, 20f, "倒れたあとに再開しない");
            Assert.Greater(Vector3.Distance(before, _player.transform.position), 1f, "再開地点へ移っていない");
            Assert.AreEqual(health.MaxHp, health.CurrentHp.CurrentValue, "再開したのに HP が満タンでない");
        }

        private void Teleport(Vector3 position)
        {
            var controller = _player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            _player.transform.position = position;
            if (controller != null) controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static bool AnyCutscenePlaying()
        {
            if (UnityEngine.Object.FindObjectsByType<CameraShowcase>(FindObjectsSortMode.None).Any(s => s.IsPlaying)) return true;

            var player = UnityEngine.Object.FindFirstObjectByType<CutscenePlayer>();
            return player != null && player.IsPlaying;
        }

        private static int CountEnemies()
        {
            return UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
                .Count(e => !(e is BossController) && !e.IsDead);
        }

        private static T FindByName<T>(string name) where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c.gameObject.name == name);
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeoutSeconds, string message)
        {
            float end = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > end)
                {
                    Assert.Fail(message);
                }

                yield return null;
            }
        }
    }
}
