using System;
using UnityEngine;

namespace Week01
{
    // 敵の共通の数値。状態ごとの細かい調整値は、各状態の設定（EnemyStateSettings.cs）にある
    [Serializable]
    public class EnemyParameters
    {
        [Min(1)]
        public int MaxHp = 30;

        [Tooltip("相手に渡すダメージの元")]
        [Min(0)]
        public int AttackPower = 10;

        [Tooltip("受けた攻撃力から引く。受けるダメージ = max(1, 攻撃力 − 防御力)")]
        [Min(0)]
        public int DefencePower;

        [Tooltip("SearchState で周回するときの速さ（m/s）")]
        [Min(0f)]
        public float SearchWalkSpeed = 1.5f;

        [Tooltip("PursueState で追いかけるときの速さ（m/s）")]
        [Min(0f)]
        public float PursueSpeed = 3f;

        [Tooltip("攻撃を始めてから、次の攻撃を始められるまでの時間（秒）")]
        [Min(0.1f)]
        public float AttackIntervalTime = 1.5f;
    }
}
