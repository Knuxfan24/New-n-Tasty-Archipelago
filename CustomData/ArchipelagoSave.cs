using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;

namespace NNT_Archipealgo.CustomData
{
    public class ArchipelagoSave
    {
        public bool HasLevers { get; set; }
        public bool CanPosses { get; set; }
        public bool HasGrenades { get; set; }
        public bool HasRocks { get; set; }
        public bool CanDefuseUXBs { get; set; }
        public bool CanUseLifts { get; set; }
        public bool CanUseSpiritRings { get; set; }
        public bool CanUseMeatSacks { get; set; }
        public bool CanUseShrykull { get; set; }

        public int RemainingLocations { get; set; }
        public int MudokonCount { get; set; }

        public bool[] UnlockedLocations { get; set; } = new bool[8];

        public Dictionary<long, ScoutedItemInfo> items;
    }
}
