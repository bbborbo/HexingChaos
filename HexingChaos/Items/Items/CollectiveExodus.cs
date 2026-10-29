using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;
using static RainrotSharedUtils.SharedUtilsPlugin;
using RoR2.Projectile;
using HexingChaos.Modules;
using UnityEngine.Networking;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace HexingChaos.Items
{
    public class CollectiveExodus : ItemBase<CollectiveExodus>
    {
        public static string[] exodusLeaveText = new string[]
        {
            "Wait, where'd it go!?",
            "Is your fridge running..?",
            "Something is off...",
            "Must've been the wind?"
        };
        public static GameObject exodusProjectilePrefab;
        public static float exodusInterval = 10f;
        public static float exodusChanceBase = 40f;
        public static float exodusChanceStack = 5f;
        public static float yawSpread = 270;
        private string pickupName => "ItemIndex.ITEM_" + this.ItemLangTokenName;
        public override string ItemName => "Hex of Exodus";

        public override string ItemLangTokenName => "CURSEEXODUS";

        public override string ItemPickupDesc => $"Does {HexTierColor("nothing..?")}";

        public override string ItemFullDescription => $"Does {HexTierColor("nothing..?")} {UtilityColor("Unaffected by luck")}.";

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
            LoadAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_Items_TransferDebuffOnHit.TransferDebuffOnHitProjectile_prefab, CreateExodusProjectile);
        }

        private void CreateExodusProjectile(GameObject transferDebuffOnHitProjectile)
        {
            exodusProjectilePrefab = transferDebuffOnHitProjectile.InstantiateClone("HexExodusProjectile", true);

            if(exodusProjectilePrefab.TryGetComponent(out ProjectileControllerTrigger pct))
            {
                LoadAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_Items_TransferDebuffOnHit.TransferDebuffOnHitProjectileGhost_prefab, (transferDebuffOnHitProjectileGhost) =>
                {
                    GameObject ghost = transferDebuffOnHitProjectileGhost.InstantiateClone("HexExodusProjectileGhost", false);
                    pct.ghostPrefab = ghost;
                    pct.ghostPrefabAddress = null;

                    /*
                     * ghost (parent)
                     * >Model Holder
                     * >>Spinner
                     * >>>PickupTransferDebuffOnHit
                     *    - disable this and replace with custom mesh
                     * >>>> .....
                     * >Aura
                     * >>Sphere
                     *    - MeshRenderer/Material/TintColor:0,160,34,255
                     * >>Rim
                     * >StartFX
                     * >>Rim
                     * >>Glow
                     * >>Distortion
                     * >>Flash
                     * >Trail
                     */
                });
            }

            if(exodusProjectilePrefab.TryGetComponent(out ProjectileTransferItem pti))
            {
                SerializablePickupIndex pickupName = new SerializablePickupIndex();
                pickupName.pickupName = this.pickupName;
                pti.pickupName = pickupName;
                pti.maxCapacity = 1; 
                pti.destroyOnGrant = true;
            }
            #region disabling stuff
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileDeployToOwner dto))
            {
                dto.enabled = false;
            }
            if (exodusProjectilePrefab.TryGetComponent(out Deployable deployable))
            {
                deployable.enabled = false;
            }
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileSphereTargetFinder pstf))
            {
                pstf.enabled = false;
            }
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileDirectionalTargetFinder pdtf))
            {
                pdtf.enabled = false;
            }
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileSteerTowardTarget steer))
            {
                steer.enabled = false;
            }
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileTargetComponent ptc))
            {
                ptc.enabled = false;
            }
            #endregion
            if (exodusProjectilePrefab.TryGetComponent(out ProjectileFuse fuse))
            {
                fuse.enabled = true;
                fuse.fuse = 1f;
                if (pti != null)
                {
                    fuse.onFuse.RemoveAllListeners();
                    fuse.onFuse.AddPersistentListener(pti.DropItem);
                }
            }
            CharacterController controller = exodusProjectilePrefab.AddComponent<CharacterController>();
            controller.contactOffset = 0.01f;
            controller.radius = 0.1f;
            controller.height = 0.5f;
            controller.slopeLimit = 80f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;
            controller.minMoveDistance = 0.001f;
            controller.detectCollisions = true;
            controller.enableOverlapRecovery = true;
            ProjectileCharacterController pcc = exodusProjectilePrefab.AddComponent<ProjectileCharacterController>();
            pcc.velocity = 10f;

            ContentAddition.AddProjectile(exodusProjectilePrefab);
        }

        public override void Hooks()
        {
            On.RoR2.Projectile.ProjectileTransferItem.DropItem += ProjectileTransferItem_DropItem;
        }

        private void ProjectileTransferItem_DropItem(On.RoR2.Projectile.ProjectileTransferItem.orig_DropItem orig, ProjectileTransferItem self)
        {
            orig(self);
            if (NetworkServer.active)
                self.hasGivenItem = true;
        }

        internal static void FireExodusProjectile(CharacterBody sourceBody, Vector3 forward)
        {
            Inventory inventory = sourceBody.inventory;
            if (!inventory)
            {
                return;
            }
            int count = inventory.GetItemCountEffective(CollectiveExodus.instance.ItemsDef);
            if (count <= 0)
            {
                return;
            }

            CharacterMasterNotificationQueue.CustomOverrideInfo overrideInfo = new CharacterMasterNotificationQueue.CustomOverrideInfo
            {
                descriptionText = exodusLeaveText[UnityEngine.Random.RandomRangeInt(0, exodusLeaveText.Length)]
            };
            CharacterMasterNotificationQueue.SendCustomNotification(sourceBody.master, CollectiveExodus.instance.ItemsDef.itemIndex, overrideInfo);

            forward.y = 0;
            forward = Vector3.Normalize(forward);
            for(int i = 0; i < count; i++)
            {
                float yawSpread = CollectiveExodus.yawSpread / count;
                Vector3 dir = Util.ApplySpread(
                    forward, 
                    minSpread: 0, 
                    maxSpread: 5, 
                    spreadYawScale: 1, 
                    spreadPitchScale: 0, 
                    bonusYaw: ((float)count / 2f - i) * yawSpread, 
                    bonusPitch: 0
                );

                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = exodusProjectilePrefab,
                    owner = sourceBody.gameObject,
                    target = null,
                    position = sourceBody.corePosition,
                    rotation = Util.QuaternionSafeLookRotation(dir)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
    }
    public class CollectiveExodusBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        private static ItemDef GetItemDef() => CollectiveExodus.instance?.ItemsDef ?? null;
        public bool canExodus = true;
        public float exodusCountdown = 0;

        void OnEnable()
        {
            canExodus = Stage.instance.sceneDef.sceneType == SceneType.Stage 
                || Stage.instance.sceneDef.sceneType == SceneType.UntimedStage
                || Stage.instance.sceneDef.sceneType == SceneType.TimedIntermission;
            exodusCountdown = CollectiveExodus.exodusInterval;
            TeleporterInteraction.onTeleporterFinishGlobal += DisableExodus;
        }

        void OnDisable()
        {
            TeleporterInteraction.onTeleporterFinishGlobal -= DisableExodus;
        }

        private void DisableExodus(TeleporterInteraction obj)
        {
            canExodus = false;
        }
        private void FixedUpdate()
        {
            if (canExodus == false)
                return;

            if(exodusCountdown > 0)
            {
                exodusCountdown -= Time.fixedDeltaTime;
                return;
            }
            exodusCountdown = CollectiveExodus.exodusInterval;
            float chance = ItemBase.GetStackValue(CollectiveExodus.exodusChanceBase, CollectiveExodus.exodusChanceStack, stack);
            if (Util.CheckRoll(chance))
            {
                Debug.LogError("Exodus!!");
                CollectiveExodus.FireExodusProjectile(body, body.inputBank?.aimDirection ?? body.transform.forward);
            }
        }
    }
}
