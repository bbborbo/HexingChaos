using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static RainrotSharedUtils.Styling;
using static R2API.RecalculateStatsAPI;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace HexingChaos.Items
{
    public class ImmortalGupHelper : ItemBase<ImmortalGupHelper>
    {
        public override int loadOrder => -1;
        public override bool forcePrerequisites => true;
        public override bool GetPrerequisites()
        {
            return true;
        }
        public override bool isEnabled => true;

        public override string ItemName => "You Are The Immortal Gup";

        public override string ItemLangTokenName => "IMMORTALGUPHELPER";

        public override string ItemPickupDesc => "You Are The Immortal Gup";

        public override string ItemFullDescription => "You Are The Immortal Gup";

        public override string ItemLore => "You Are The Immortal Gup";

        public override ItemTier Tier => ItemTier.NoTier;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            GetStatCoefficients += ImmortalGupHelper_GetStatCoefficients;
        }

        private void ImmortalGupHelper_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            int stack = GetCount(sender);
            if (stack <= 0)
                return;
            args.moveSpeedMultAdd += GetStackValue(VoidImmortalGup.gipSpeedBase - 1, VoidImmortalGup.gipSpeedStack, stack);
            args.baseRegenAdd += 50f + (10f * sender.level);
        }
    }
    public class ImmortalGupHelperBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => ImmortalGupHelper.instance?.ItemsDef ?? null;

        void OnEnable()
        {
            this.body.bodyFlags |= CharacterBody.BodyFlags.Buddha | CharacterBody.BodyFlags.ImmuneToExecutes;
            this.body.AddTimedBuff(RoR2Content.Buffs.Nullified, 5f);
            this.body.AddTimedBuff(DLC2Content.Buffs.DisableAllSkills, 5f);
        }

        void FixedUpdate()
        {

        }

        void OnDisable()
        {
            body.master.TrueKill();
        }
    }
}
