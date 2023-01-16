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
                List<int> ownedBgStrikes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>()
                    .OwnedBattlegroundsFinishers.Select(s => s.ToValue()).ToList();
                var finisherRecords = GameDbf.BattlegroundsFinisher.GetRecords();
                //RandomBattlegroundsStrikeMod.SharedLogger.Msg(
                //    string.Join("\n", finisherRecords.Select(r => $"" +
                //    $"capseule={r.CapsuleType}, " +
                //    $"collectionName={r.CollectionName?.GetString()}, " +
                //    $"collectionShortName={r.CollectionShortName?.GetString()}, " +
                //    $"description={r.Description?.GetString()}, " +
                //    $"movie={r.DetailsMovie}, " +
                //    $"texture={r.DetailsTexture}, " +
                //    $"gameplaySettings={r.GameplaySettings}, " +
                //    $"ID={r.ID}, " +
                //    $"miniArtMaterial={r.MiniArtMaterial}, " +
                //    $"miniBodyMaterial={r.MiniBodyMaterial}, " +
                //    $"rarity={r.Rarity}" +
                //$"")));
                var recordsToKeep = new List<BattlegroundsFinisherDbfRecord>();
                foreach (var record in finisherRecords)
                {
                    if (!ownedBgStrikes.Contains(record.ID)) {
                        continue;
                    }
                    var shortName = record.CollectionShortName.GetString().Split('(')[0];
                    var rarity = record.Rarity;
                    var existing = recordsToKeep.Find(r => r.CollectionShortName.GetString().Split('(')[0] == shortName);
                    if (existing == null)
                    {
                        recordsToKeep.Add(record);
                        continue;
                    }
                    if (existing.Rarity > record.Rarity)
                    {
                        continue;
                    }
                    recordsToKeep.Remove(existing);
                    recordsToKeep.Add(record);
                }
                CacheBattlegroundsStrikes = recordsToKeep.Select(r => r.ID).ToList();
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
