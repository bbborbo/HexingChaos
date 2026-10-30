using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class CollectiveEulogy : ItemBase<CollectiveEulogy>
    {
        public static float corruptChanceBase = 0.05f;
        public static float corruptChanceStack = 0.05f;
        public override string ItemName => "Hex of Corruption";

        public override string ItemLangTokenName => "CURSEEULOGY";

        public override string ItemPickupDesc => $"Items and equipment have a small chance to {HexTierColor("transform into a random Hex")} instead.";

        public override string ItemFullDescription => $"Items have a {UtilityColor(corruptChanceBase.AsPercent())} {StackText("+" + corruptChanceStack.AsPercent())} chance " +
            $"to become a random {HexTierColor("Hex")} instead.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.CannotMagnify, ItemTag.DeathAffixBlacklist, ItemTag.BrotherBlacklist, ItemTag.DevotionBlacklist, ItemTag.ExtractorUnitBlacklist };

        public override GameObject ItemModel => LoadDropPrefab("PickupCurseEulogy");//7

        public override Sprite ItemIcon => LoadItemIcon("texIconHexCorruption");
        public override RoR2.ExpansionManagement.ExpansionDef RequiredExpansion => RainrotSharedUtils.SharedUtilsPlugin.dlc3;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            On.RoR2.Items.RandomlyLunarUtils.CheckForLunarReplacement_UniquePickup_Xoroshiro128Plus += RandomlyHex;
        }

        private UniquePickup RandomlyHex(On.RoR2.Items.RandomlyLunarUtils.orig_CheckForLunarReplacement_UniquePickup_Xoroshiro128Plus orig, UniquePickup uniquePickup, Xoroshiro128Plus rng)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(uniquePickup.pickupIndex);
			if (RandomlyLunarUtils.CanReplace(pickupDef))
			{
				int totalItemCountFromTeam = Util.GetTotalItemCountFromTeam(new Util.GetTotalItemCountFromTeamArgs(ItemsDef.itemIndex)
				{
					requiresAlive = false,
					requiredTeam = null,
					requiresConnected = false,
					onlyPlayerControlledCharacters = true
				});
				if (totalItemCountFromTeam > 0)
				{
					List<PickupIndex> list = Run.instance.availableHexTierDropList;
					if (list != null && list.Count > 0 && rng.nextNormalizedFloat < GetStackValue(corruptChanceBase, corruptChanceStack, totalItemCountFromTeam))
					{
						int index = rng.RangeInt(0, list.Count);
						return uniquePickup.WithPickupIndex(list[index]);
					}
				}
			}
			return orig(uniquePickup, rng);
        }
    }
}
