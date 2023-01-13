using Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomBattlegroundsStrike
{
    public class Utils
    {
        public static List<int> CacheBattlegroundsStrikes = new List<int>();

        public static class CacheInfo
        {
            public static void UpdateBattlegroundsStrike()
            {
                CacheBattlegroundsStrikes.Clear();
                HashSet<Hearthstone.BattlegroundsFinisherId> ownedBgStrikes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>().OwnedBattlegroundsFinishers;
                foreach (var strikeId in ownedBgStrikes)
                {
                    CacheBattlegroundsStrikes.Add(strikeId.ToValue());
                }
            }
        }

        public static int GetRandomFinisher()
        {
            if (CacheBattlegroundsStrikes.Count == 0)
            {
                CacheInfo.UpdateBattlegroundsStrike();
            }
            var strike = CacheBattlegroundsStrikes[UnityEngine.Random.Range(0, CacheBattlegroundsStrikes.Count)];
            var allStrikesInfo = CacheBattlegroundsStrikes.Select(id => $"(id={id},name={GameDbf.BattlegroundsFinisher.GetRecord(id)?.CollectionName.GetString()})");            
            RandomBattlegroundsStrikeMod.SharedLogger.Msg($"Loaded new strike {strike}. All options were {string.Join(", ", allStrikesInfo)}");
            return strike;
        }
    }
}
