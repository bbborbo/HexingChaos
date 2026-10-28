using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class SwanSmallerTp : ItemBase<SwanSmallerTp>
    {
        /// <summary>
        /// hyperbolic plsss
        /// </summary>
        public static float shrinkAmt = 0.3f;
        public override string ItemName => "Hex of Convergence";

        public override string ItemLangTokenName => "CURSECONVERGENCE";

        public override string ItemPickupDesc => $"{HexTierColor("Reduce")} the size of the Teleporter's radius.";

        public override string ItemFullDescription => $"Reduce the size of the Teleporter and 'Holdout Zones' such as the Void Fields " +
            $"by {HexTierColor(shrinkAmt.AsPercent())} {StackText("-" + shrinkAmt.AsPercent())}.";

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

        }
    }
}
