using R2API;
using RoR2;
using SwanSongExtended.Storms;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class SwanFasterStorm : ItemBase<SwanFasterStorm>
    {
        public static float stormTimeReductionSeconds = 40f;
        public static float stormTimeReductionSecondsStack = 20f;
        /// <summary>
        /// hyperbolic
        /// </summary>
        public static float strikeIntervalStack = 0.5f;
        public override string ItemName => "Hex of Stratus";

        public override string ItemLangTokenName => "CURSEFASTERSTORM";

        public override string ItemPickupDesc => $"The Storm will appear {HexTierColor(stormTimeReductionSeconds.ToString() + " seconds sooner")}.";

        public override string ItemFullDescription => $"The Storm will appear {HexTierColor(stormTimeReductionSeconds.ToString() + " seconds")} {StackText("-" + stormTimeReductionSecondsStack)} sooner.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.CannotMagnify, ItemTag.DeathAffixBlacklist, ItemTag.BrotherBlacklist, ItemTag.DevotionBlacklist, ItemTag.ExtractorUnitBlacklist };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            StormsCore.GetStormDelayModifierSeconds += StormDelayer;
        }

        private void StormDelayer(StormController sender, ref float delayModifier)
        {
            float reduction = GetStackValue(stormTimeReductionSeconds, stormTimeReductionSecondsStack, Util.GetItemCountForTeam(TeamIndex.Player, ItemsDef.itemIndex, true, true));

            delayModifier -= reduction;
        }
    }
}
