using System;
using UnityEngine;

namespace DemoGame
{
    [Serializable]
    public class SearchStateSettings
    {
        [Tooltip("プレイヤーがこの距離に入ったら追いかける（m）")]
        [Min(0f)]
        public float DetectRange = 10f;

        [Tooltip("周回ルート。順に回る。空なら出現位置のまわりをランダムに回る")]
        public Transform[] PatrolPoints;

        [Tooltip("周回ルートが空のとき、出現位置からどこまで離れた点を選ぶか（m）")]
        [Min(0.5f)]
        public float PatrolRadius = 6f;

        [Tooltip("地点に着いたら立ち止まる時間（秒）")]
        [Min(0f)]
        public float WaitTime = 1.5f;
    }

    [Serializable]
    public class PursueStateSettings
    {
        [Tooltip("この距離より離れたら見失って、周回に戻る（m）")]
        [Min(0f)]
        public float LoseRange = 15f;

        [Tooltip("この距離に入ったら攻撃を始める（m）")]
        [Min(0f)]
        public float AttackRange = 2f;

        [Tooltip("目的地を更新する間隔（秒）。毎フレーム経路を引き直さない")]
        [Min(0f)]
        public float RepathInterval = 0.2f;
    }

    [Serializable]
    public class AttackStateSettings
    {
        [Tooltip("攻撃を始めてから、当たり判定を出し始めるまでの時間（秒）。攻撃モーションの振り下ろしに合わせる")]
        [Min(0f)]
        public float HitStartTime = 0.15f;

        [Tooltip("当たり判定を消す時間（秒）。この間は毎フレーム判定し、一度当たったらその攻撃では終わり")]
        [Min(0f)]
        public float HitEndTime = 0.45f;

        [Tooltip("攻撃全体の長さ（秒）。終わったら追いかけに戻る。攻撃モーションが移動モーションに戻る時間に合わせる")]
        [Min(0f)]
        public float Duration = 0.6f;

        [Tooltip("当たり判定の球を付けるボーンの名前（Visual の子から探す）。モーションで動く部位（手・頭など）を指定する")]
        public string[] HitBoneNames = { "Head" };

        [Tooltip("ボーンに付ける当たり判定の球の半径（m）")]
        [Min(0f)]
        public float HitRadius = 0.6f;

        [Tooltip("ボーンが見つからないときの判定：相手がこの距離（m）と前方の角度（±度）の中にいれば当たる")]
        [Min(0f)]
        public float HitRange = 2.6f;

        [Range(0f, 180f)]
        public float HitAngle = 60f;
    }

    [Serializable]
    public class DamageStateSettings
    {
        [Tooltip("怯んで止まる時間（秒）")]
        [Min(0f)]
        public float StaggerTime = 0.5f;

        [Tooltip("一度怯んでから、次に怯めるまでの時間（秒）。0 なら毎回怯む")]
        [Min(0f)]
        public float StaggerCooldown;

        [Tooltip("被弾したときに光らせる色（DemoGame/Toon の _HitFlashColor）")]
        public Color FlashColor = new Color(1f, 0.3f, 0.3f);

        [Range(0f, 1f)]
        public float FlashDuration = 0.12f;
    }

    [Serializable]
    public class DieStateSettings
    {
        [Tooltip("倒れてから溶け始めるまでの時間（秒）。倒れるモーションを見せる")]
        [Min(0f)]
        public float DissolveDelay = 0.8f;

        [Tooltip("溶けて消えるまでの時間（秒）。消えたら GameObject を破棄する")]
        [Min(0f)]
        public float DissolveDuration = 0.6f;
    }
}
