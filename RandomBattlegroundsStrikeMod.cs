using MelonLoader;
using HarmonyLib;
using System.Linq;
using System;
using System.Reflection;
using Blizzard.T5.AssetManager;

namespace RandomBattlegroundsStrike
{
    public class RandomBattlegroundsStrikeMod : MelonMod
    {
        public static MelonLogger.Instance SharedLogger;

        public static int currentStrikeId = 0;

        public override void OnInitializeMelon()
        {
            RandomBattlegroundsStrikeMod.SharedLogger = LoggerInstance;
            var harmony = this.HarmonyInstance;
            harmony.PatchAll(typeof(FinisherGameplaySettingsPatcher));
            harmony.PatchAll(typeof(GameMgrPatcher));
        }
    }


    public static class GameMgrPatcher
    {
        [HarmonyPatch(typeof(GameMgr), "OnGameSetup")]
        [HarmonyPostfix]
        public static void OnGameSetupPostfix()
        {
            if (GameMgr.Get().IsBattlegrounds())
            {
                RandomBattlegroundsStrikeMod.currentStrikeId = Utils.GetRandomFinisher();
            }
        }
    }

    // Not super fan of rewriting the whole method, but simply patching GetTag() to return the correct 
    // strikeId didn't work, for some reason that I don't understand
    public static class FinisherGameplaySettingsPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FinisherGameplaySettings), "GetFinisherGameplaySettings")]
        public static bool PatchGetFinisherGameplaySettings(ref Entity hero, ref FinisherGameplaySettings __result)
        {
            if (hero.GetControllerSide() != Player.Side.FRIENDLY)
            {
                return true;
            }
            var strike = RandomBattlegroundsStrikeMod.currentStrikeId;

            // Below this is code from the original game (except the commented out code)
            int num = strike;
            //int num = hero.GetTag(GAME_TAG.BATTLEGROUNDS_FAVORITE_FINISHER);
            if (num <= 0)
            {
                Log.Spells.PrintError(hero.GetDebugName() + " has no tag BATTLEGROUNDS_FAVORITE_FINISHER. Using Default Finisher."); 
                RandomBattlegroundsStrikeMod.SharedLogger.Msg(hero.GetDebugName() + " has no tag BATTLEGROUNDS_FAVORITE_FINISHER. Using Default Finisher.");
                num = 1;
            }
            BattlegroundsFinisherDbfRecord record = GameDbf.BattlegroundsFinisher.GetRecord(num);
            if (record == null)
            {
                Log.Spells.PrintError($"No Finisher was found for Finisher ID {num}. Using default finisher.");
                RandomBattlegroundsStrikeMod.SharedLogger.Msg($"No Finisher was found for Finisher ID {num}. Using default finisher.");
                record = GameDbf.BattlegroundsFinisher.GetRecord(1);
            }
            AssetReference assetReference = AssetReference.CreateFromAssetString(record.GameplaySettings);
            AssetHandle<FinisherGameplaySettings> assetHandle = ((assetReference != null) ? AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(assetReference) : null);
            FinisherGameplaySettings finisherGameplaySettings = (assetHandle ? assetHandle.Asset : null);
            if (finisherGameplaySettings == null)
            {
                Log.Spells.PrintError($"Finisher ID {num} is missing its finisher settings entirely in HE2. Using default finisher.");
                RandomBattlegroundsStrikeMod.SharedLogger.Msg($"Finisher ID {num} is missing its finisher settings entirely in HE2. Using default finisher.");
                record = GameDbf.BattlegroundsFinisher.GetRecord(1);
                assetReference = AssetReference.CreateFromAssetString(record.GameplaySettings);
                assetHandle = AssetLoader.Get().LoadAsset<FinisherGameplaySettings>(assetReference);
                finisherGameplaySettings = assetHandle.Asset;
            }
            //return finisherGameplaySettings;
            __result = finisherGameplaySettings;
            return false;
        }
    }
}