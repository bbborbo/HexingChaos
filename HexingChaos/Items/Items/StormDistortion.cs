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

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace HexingChaos.Items
{
    public class StormDistortion : ItemBase<StormDistortion>
    {
        public static float corruptionInterval = 20f;
        public static float corruptionIntervalStack = 10f;
        public static float corruptionIntervalMin = 0f;
        public static float corruptionDuration = 10f;
        public override string ItemName => "Hex of Distortion";

        public override string ItemLangTokenName => "CURSEDISTORTION";

        public override string ItemPickupDesc => $"Lock a {HexTierColor("random skill")} for {corruptionDuration} seconds.";

        public override string ItemFullDescription => $"Lock a {HexTierColor("random skill")} every " +
            $"{UtilityColor(corruptionInterval.ToString())} {StackText("-" + corruptionIntervalStack)} seconds. " +
            $"Lasts for {UtilityColor(corruptionDuration.ToString())} seconds.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override GameObject ItemModel => LoadDropPrefab("PickupCurseDistortion");//9

        public override Sprite ItemIcon => LoadItemIcon("texIconHexDistortion");
        public override RoR2.ExpansionManagement.ExpansionDef RequiredExpansion => RainrotSharedUtils.SharedUtilsPlugin.dlc2;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {

        }
    }
    public class StormDistortionBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = true)]
        private static ItemDef GetItemDef() => StormDistortion.instance?.ItemsDef ?? null;
        SkillLocator skillLocator;
        GenericSkill targetSkill;
        bool isSkillLocked = false;
        float stateChangeCountdown;

        void OnEnable()
        {
            skillLocator = body.skillLocator;
            targetSkill = PickRandomSkill();
            SetSkillLocked(true);
            stateChangeCountdown = GetTimeUntilUnlock();
        }

        GenericSkill PickRandomSkill()
        {
            int random = UnityEngine.Random.RandomRangeInt(0, 4);
            if (Run.instance != null)
            {
                random = Mathf.FloorToInt(Run.instance.runRNG.nextNormalizedFloat * 4);
            }
            switch (random)
            {
                default:
                    return skillLocator.primary;
                case 1:
                    return skillLocator.secondary;
                case 2:
                    return skillLocator.utility;
                case 3:
                    return skillLocator.special;
            }
        }

        float GetTimeUntilLock()
        {
            return Mathf.Max(0, ItemBase.GetStackValue(StormDistortion.corruptionInterval, 0 - StormDistortion.corruptionIntervalStack, stack));
        }
        float GetTimeUntilUnlock()
        {
            return StormDistortion.corruptionDuration;
        }

        void OnDisable()
        {
            SetSkillLocked(false);
        }
        void SetSkillLocked(bool newState)
        {
            if (newState == isSkillLocked)
                return;
            isSkillLocked = newState;
            if (body.hasEffectiveAuthority == false)
                return;
            if(isSkillLocked == true)
            {
                targetSkill.SetSkillOverride(this, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
                Util.PlaySound("Play_env_meridian_primeDevestator_debuff", base.gameObject);
            }
            else
            {
                targetSkill.UnsetSkillOverride(this, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }
        private void FixedUpdate()
        {
            stateChangeCountdown -= Time.fixedDeltaTime;
            if(stateChangeCountdown <= 0 && isSkillLocked == true)
            {
                SetSkillLocked(false);
                stateChangeCountdown = GetTimeUntilLock();
            }
            if (stateChangeCountdown <= 0 && isSkillLocked == false)
            {
                targetSkill = PickRandomSkill();
                SetSkillLocked(true);
                stateChangeCountdown = GetTimeUntilUnlock();
            }
        }
    }
}
