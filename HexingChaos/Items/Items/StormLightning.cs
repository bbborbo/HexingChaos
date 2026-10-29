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
    public class StormLightning : ItemBase<StormLightning>
    {
        private static DamageTypeCombo lightningDamageTypeMeridian = new DamageTypeCombo(DamageType.LunarRuin, DamageTypeExtended.ApplyBuffPermanently, DamageSource.NoneSpecified);
        private static DamageTypeCombo lightningDamageTypeNonMeridian = new DamageTypeCombo(DamageType.LunarRuin, DamageTypeExtended.Generic, DamageSource.NoneSpecified);
        private static bool isMeridian = false;
        public static float strikeInterval = 7f;
        /// <summary>
        /// hyperbolic
        /// </summary>
        public static float strikeIntervalStack = 0.5f;
        public override string ItemName => "Hex of Flux";

        public override string ItemLangTokenName => "CURSELIGHTNING";

        public override string ItemPickupDesc => $"{HexTierColor("Ruinous lightning")} strikes on your position.";

        public override string ItemFullDescription => $"Every {UtilityColor(strikeInterval.ToString())} {StackText($"-{strikeIntervalStack.AsPercent()}")} seconds, {HexTierColor("ruinous lightning")} strikes on your position.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            Stage.onServerStageBegin += SwitchLightningDamageType;
            On.RoR2.LightningStormController.FireLightningBolt_Vector3 += LightningStormController_FireLightningBolt_Vector3;
        }

        private void SwitchLightningDamageType(Stage obj)
        {
            isMeridian = obj.sceneDef.baseSceneName == "meridian";
        }

        private void LightningStormController_FireLightningBolt_Vector3(On.RoR2.LightningStormController.orig_FireLightningBolt_Vector3 orig, Vector3 position)
        {
            if (LightningStormController.instance.stormActive == false)
            {
                SendLightningStrike(position, LightningStormController.lightningPrefab, null);
            }
            orig(position);
        }

        public static void SendLightningStrike(Vector3 position, GameObject lightningPrefab, BlastAttack _blastInfo = null)
        {
            if (!NetworkServer.active)
            {
                //LightningStormController.SendLightningRequest(position);
                return;
            }
            LightningStrikeInstance lightningStrike = null;
            if (LightningStormController.instance != null)
            {
                lightningStrike = LightningStormController.GetPooledLightningStrike(lightningPrefab);
            }
            if (lightningStrike == null)
            {
                GameObject strikePrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_meridian_DisableSkillsLightning.LightningStrikeInstance_prefab).WaitForCompletion();
                GameObject strikeInstance = UnityEngine.GameObject.Instantiate(strikePrefab, position, Quaternion.identity);
                if(strikeInstance.TryGetComponent(out lightningStrike) == false)
                    return;
            }
            lightningStrike.blastDamageType = isMeridian ? lightningDamageTypeMeridian : lightningDamageTypeNonMeridian;
            lightningStrike.Initialize(position, _blastInfo, 1f, true);
            LightningStormController.ReportLightningStrike(position);
        }
    }

    public class StormLightningBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = false, useOnClient = true)]
        private static ItemDef GetItemDef() => StormLightning.instance?.ItemsDef ?? null;
        public bool canStrike = true;
        public float strikeCountdown = 0;

        void OnEnable()
        {
            canStrike = true;
            strikeCountdown = 3f;
            TeleporterInteraction.onTeleporterFinishGlobal += DisableExodus;
        }

        void OnDisable()
        {
            TeleporterInteraction.onTeleporterFinishGlobal -= DisableExodus;
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
