using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ExpansionManagement;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RainrotSharedUtils.Styling;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace HexingChaos.Items
{
    public class VoidImmortalGup : ItemBase<VoidImmortalGup>
    {
        public static float gipSpeedBase = 0.6f;
        public static float gipSpeedStack = 0.20f;
        public override string ItemName => "Hex of Inevitability";

        public override string ItemLangTokenName => "CURSEIMMORTALGUP";

        public override string ItemPickupDesc => $"You are chased by an {HexTierColor("immortal gip")}.";

        public override string ItemFullDescription => $"You are chased by an {HexTierColor("immortal gip")} that will {HexTierColor("instantly kill you")}. " +
            $"The {HexTierColor("immortal gip")} moves {UtilityColor("0%")} faster {StackText("+" + gipSpeedStack.AsPercent())}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.CannotCopy };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();
        public override ExpansionDef RequiredExpansion => RainrotSharedUtils.SharedUtilsPlugin.dlc1;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.gameObject != damageInfo.attacker && damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody))
            {
                if(attackerBody.inventory != null && self.body.inventory != null)
                {
                    int victimHexStack = self.body.inventory.GetItemCountPermanent(VoidImmortalGup.instance.ItemsDef);
                    int attackerGupStack = attackerBody.inventory.GetItemCountPermanent(ImmortalGupHelper.instance.ItemsDef);
                    Debug.Log($"victim {self.name} hex stack {victimHexStack} attacker {attackerBody.name} gup stack {attackerGupStack}");
                    if (attackerGupStack > 0 && victimHexStack > 0)
                    {
                        damageInfo.damage = 999999f;
                        damageInfo.procCoefficient = 999999f;
                        damageInfo.damageType = new DamageTypeCombo(
                            DamageType.BleedOnHit | DamageType.BlightOnHit | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection | DamageType.ClayGoo | DamageType.CrippleOnHit | DamageType.Freeze2s | DamageType.IgniteOnHit | DamageType.LunarRuin | DamageType.LunarSecondaryRootOnHit | DamageType.PoisonOnHit | DamageType.SlowOnHit | DamageType.WeakOnHit,
                            DamageTypeExtended.Accelerant | DamageTypeExtended.DisableAllSkills | DamageTypeExtended.Frost | DamageTypeExtended.NanoBug | DamageTypeExtended.NoGravity,
                            damageInfo.damageType.damageSource);
                        damageInfo.crit = true;
                        damageInfo.procChainMask.AddProc(ProcType.SureProc);
                        damageInfo.force *= 10;
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
    public class ImmortalGupBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => VoidImmortalGup.instance?.ItemsDef ?? null;
        private static SpawnCard gupSpawnCard => Addressables.LoadAssetAsync<SpawnCard>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_Gup.cscGipBody_asset).WaitForCompletion();
        private CharacterMaster gupFriend;
        private BaseAI gupFriendAI;
        float verifyCountdown = 10;

        void OnEnable()
        {
        }

        void SpawnGup()
        {
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(gupSpawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                minDistance = 10f,
                maxDistance = 40f,
                spawnOnTarget = base.transform
            }, RoR2Application.rng);
            directorSpawnRequest.onSpawnedServer = new Action<SpawnCard.SpawnResult>(OnGupMasterSpawned);
            directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

            void OnGupMasterSpawned(SpawnCard.SpawnResult spawnResult)
            {
                GameObject spawnedInstance = spawnResult.spawnedInstance;
                if ((bool)spawnedInstance)
                {
                    gupFriend = spawnedInstance.GetComponent<CharacterMaster>();
                    if ((bool)gupFriend)
                    {
                        gupFriend.inventory.GiveItemPermanent(ImmortalGupHelper.instance.ItemsDef, stack);
                        gupFriend.inventory.GiveItemPermanent(RoR2Content.Items.TeleportWhenOob);
                    }
                    gupFriendAI = spawnedInstance.GetComponent<BaseAI>();
                }
            }
        }

        public override void OnInventoryRefresh()
        {
            base.OnInventoryRefresh();
            if(gupFriend == null)
            {
                SpawnGup();
                return;
            }
            VerifyGupStacks();
        }
        void VerifyGupStacks()
        {
            if (gupFriend != null)
            {
                int count = ImmortalGupHelper.instance.GetCount(gupFriend, true);
                int diff = count - stack;
                if(diff != 0)
                    gupFriend.inventory.GiveItemPermanent(ImmortalGupHelper.instance.ItemsDef, diff);
            }
        }
        void FixedUpdate()
        {
            if (gupFriend == null)
                SpawnGup();
            if (gupFriendAI == null)
                return;
            gupFriendAI.currentEnemy.gameObject = body.gameObject;

            verifyCountdown -= Time.fixedDeltaTime;
            if (verifyCountdown > 0)
                return;
            verifyCountdown = 10;
            VerifyGupStacks();
        }

        void OnDisable()
        {
            gupFriend.TrueKill();
        }
    }
}
