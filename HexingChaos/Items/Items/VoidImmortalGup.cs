using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class VoidImmortalGup : ItemBase<VoidImmortalGup>
    {
        public static float gipSpeedStack = 0.20f;
        public override string ItemName => "Hex of Inevitability";

        public override string ItemLangTokenName => "CURSEIMMORTALGUP";

        public override string ItemPickupDesc => $"You are chased by an {HexTierColor("immortal gip")}.";

        public override string ItemFullDescription => $"You are chased by an {HexTierColor("immortal gip")}. " +
            $"The {HexTierColor("immortal gip")} moves {UtilityColor("0%")} faster {StackText("+" + gipSpeedStack.AsPercent())}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.CannotCopy };

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
