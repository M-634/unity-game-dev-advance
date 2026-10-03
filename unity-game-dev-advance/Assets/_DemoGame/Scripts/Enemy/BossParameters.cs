using System;
using UnityEngine;

namespace DemoGame
{
    // ボスだけが持つ数値
    [Serializable]
    public class BossParameters
    {
        [Tooltip("HP がこの値以下になったら怒る（一度だけ）")]
        [Min(0)]
        public int AngryStateTargetHp = 150;

        [Tooltip("無敵（構え）の長さ（秒）")]
        [Min(0f)]
        public float InvincibleTime = 4f;
    }

    [Serializable]
    public class AngryStateSettings
    {
        [Tooltip("怒ったあと AttackPower / DefencePower に足す値。倒されるまで続く")]
        [Min(0)]
        public int AttackPowerBonus = 10;

        [Min(0)]
        public int DefencePowerBonus = 4;

        [Tooltip("咆哮して止まっている時間（秒）")]
        [Min(0f)]
        public float RoarDuration = 2.4f;

        [Tooltip("怒っている間の体の発光（DemoGame/Toon の _EmissionColor）")]
        [ColorUsage(false, true)]
        public Color EmissionColor = new Color(0.6f, 0.05f, 0f);
    }

    [Serializable]
    public class InvincibleStateSettings
    {
        [Tooltip("この秒数の間に、この回数当てられたら構えて無敵になる")]
        [Min(1)]
        public int HitCount = 5;

        [Min(0.1f)]
        public float HitWindow = 3f;

        [Tooltip("構えている間、体にかけ続ける色（_HitFlash）")]
        public Color GuardColor = new Color(0.85f, 0.95f, 1f);

        [Range(0f, 1f)]
        public float GuardAmount = 0.45f;
    }
}
