using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class CollectiveEulogy : ItemBase<CollectiveEulogy>
    {
        public static float corruptChanceBase = 5f;
        public static float corruptChanceStack = 5f;
        public override string ItemName => "Hex of Corruption";

        public override string ItemLangTokenName => "CURSEEULOGY";

        public override string ItemPickupDesc => $"Items and equipment have a small chance to {HexTierColor("transform into a random Hex")} instead.";

        public override string ItemFullDescription => $"Items have a {UtilityColor(corruptChanceBase.AsPercent())} {StackText("+" + corruptChanceStack.AsPercent())} chance " +
            $"to become a random {HexTierColor("Hex")} instead.";

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
