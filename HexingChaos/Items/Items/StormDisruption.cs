using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class StormDisruption : ItemBase<StormDisruption>
    {
        public static float corruptionInterval = 25f;
        /// <summary>
        /// exponential
        /// </summary>
        public static float corruptionIntervalStack = 0.3f;
        public static float corruptionIntervalMin = 5f;
        public static float corruptionDuration = 10f;
        public override string ItemName => "Hex of Distortion";

        public override string ItemLangTokenName => "CURSEDISTORTION";

        public override string ItemPickupDesc => $"Lock a {HexTierColor("random skill")} every {corruptionInterval} seconds.";

        public override string ItemFullDescription => $"Lock a {HexTierColor("random skill")} every " +
            $"{UtilityColor(corruptionInterval.ToString())} {StackText("-" + corruptionIntervalStack.AsPercent())} seconds. " +
            $"Lasts for {UtilityColor(corruptionDuration.ToString())} seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {

        }
    }
}
