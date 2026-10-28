using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class CollectiveExodus : ItemBase<CollectiveExodus>
    {
        public override string ItemName => "Hex of Exodus";

        public override string ItemLangTokenName => "CURSEEXODUS";

        public override string ItemPickupDesc => $"Does {HexTierColor("nothing...?")}";

        public override string ItemFullDescription => $"Does {HexTierColor("nothing...?")}";

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
