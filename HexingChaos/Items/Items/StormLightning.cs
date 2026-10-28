using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class StormLightning : ItemBase<StormLightning>
    {
        public static float strikeInterval = 7f;
        /// <summary>
        /// hyperbolic
        /// </summary>
        public static float strikeIntervalStack = 0.5f;
        public override string ItemName => "Hex of Flux";

        public override string ItemLangTokenName => "CURSELIGHTNING";

        public override string ItemPickupDesc => $"{HexTierColor("Meridian lightning")} strikes on your position.";

        public override string ItemFullDescription => $"Every {UtilityColor(strikeInterval.ToString())} {StackText("-" + strikeIntervalStack.ToString())} seconds, {HexTierColor("Meridian lightning")} strikes on your position.";

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
