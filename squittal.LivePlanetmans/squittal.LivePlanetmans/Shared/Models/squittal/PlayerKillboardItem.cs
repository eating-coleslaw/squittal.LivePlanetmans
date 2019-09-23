﻿using System;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerKillboardItem
    {
        public string VictimId { get; set; }
        public string VictimName { get; set; }
        public int? VictimFactionId { get; set; }
        public bool IsHeadshot { get; set; }
        public int? VictimLoadoutId { get; set; }
        public string AttackerId { get; set; }
        public string AttackerName { get; set; }
        public int? AttackerFactionId { get; set; }
        public int? AttackerLoadoutId { get; set; }
        public int? AttackerWeaponId { get; set; }
        public DateTime KillTimestamp { get; set; }
    }
}
