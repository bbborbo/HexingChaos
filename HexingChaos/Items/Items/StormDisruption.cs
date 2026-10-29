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
    public class StormDisruption : ItemBase<StormDisruption>
    {
        public static float corruptionInterval = 20f;
        public static float corruptionIntervalStack = 10f;
        public static float corruptionIntervalMin = 0f;
        public static float corruptionDuration = 10f;
        public override string ItemName => "Hex of Distortion";

        public override string ItemLangTokenName => "CURSEDISTORTION";

        public override string ItemPickupDesc => $"Lock a {HexTierColor("random skill")} every {corruptionInterval} seconds.";

        public override string ItemFullDescription => $"Lock a {HexTierColor("random skill")} every " +
            $"{UtilityColor(corruptionInterval.ToString())} {StackText("-" + corruptionIntervalStack)} seconds. " +
            $"Lasts for {UtilityColor(corruptionDuration.ToString())} seconds.";

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
    public class StormDistortionBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => StormDisruption.instance?.ItemsDef ?? null;
        SkillLocator skillLocator;
        bool isSkillLocked = false;

        void OnEnable()
        {
            skillLocator = body.skillLocator;
        }

        void OnDisable()
        {
            SetSkillLocked(false);
        }
        void SetSkillLocked(bool newState)
        {

        }

        private void DisableExodus(TeleporterInteraction obj)
        {
            canStrike = false;
        }

        private float GetInterval()
        {
            return StormLightning.strikeInterval * Util.ConvertAmplificationPercentageIntoReductionNormalizedInverted(stack * StormLightning.strikeIntervalStack);
        }
        private void FixedUpdate()
        {
            if (canStrike == false)
                return;

            if (strikeCountdown > 0)
            {
                strikeCountdown -= Time.fixedDeltaTime;
                return;
            }
            strikeCountdown = GetInterval();
            StormLightning.SendLightningStrike(body.footPosition, LightningStormController.lightningPrefab, null);
        }
    }
}
