using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class SwanEliteSpread : ItemBase<SwanEliteSpread>
    {
        public static int spreadCtBase = 3;
        public static int spreadCtStack = 1;
        public static float spreadRangeBase = 30f;
        public static float spreadRangeStack = 10f;
        public static float spreadDurationBase = 8f;
        public static float spreadDurationStack = 5f;
        public override string ItemName => "Hex of Carrion";

        public override string ItemLangTokenName => "CURSEELITESPREAD";

        public override string ItemPickupDesc => $"Slain elites {HexTierColor("empower")} other nearby enemies.";

        public override string ItemFullDescription => $"Slain Elites {HexTierColor("empower")} up to {UtilityColor(spreadCtBase.ToString())} {StackText("+" + spreadCtStack)} nearby enemies. " +
            $"Empowered enemies gain the {DamageColor("power")} of the killed elite monster for {DamageColor(spreadDurationBase.ToString())} {StackText("+" + spreadDurationStack)}.";

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
