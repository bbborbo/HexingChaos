using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class VoidInfestorShrine : ItemBase<VoidInfestorShrine>
    {
        public static int infestorsMin = 1;
        public static int infestorsMax = 3;
        public static int infestorsStack = 1;
        public override string ItemName => "Hex of Pests";

        public override string ItemLangTokenName => "CURSEINFESTORSHRINE";

        public override string ItemPickupDesc => $"Every shrine spawns {HexTierColor("void infestors")}.";

        public override string ItemFullDescription => $"Activating any shrine spawns {UtilityColor($"{infestorsMin}-{infestorsMax}")} {StackText("+" + infestorsStack.ToString())} {HexTierColor("void infestors")}.";

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
