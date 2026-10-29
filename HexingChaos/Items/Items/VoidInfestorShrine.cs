using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public override ExpansionDef RequiredExpansion => RainrotSharedUtils.SharedUtilsPlugin.dlc1;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            GlobalEventManager.OnInteractionsPermittedForSpawnGlobal += ShrineInfestors;
        }

        private void ShrineInfestors(Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            if (interactableObject.TryGetComponent(out PurchaseInteraction purchaseInteraction) == false)
                return;
            if (purchaseInteraction.isShrine == false)
                return;
            if (interactor.gameObject.TryGetComponent(out CharacterBody body) == false)
                return;
            int count = GetCount(body);
            if (count <= 0)
                return;

            ModelLocator component4 = interactableObject.GetComponent<ModelLocator>();
            Transform transform2;
            if (component4 == null)
            {
                transform2 = null;
            }
            else
            {
                Transform modelTransform = component4.modelTransform;
                if (modelTransform == null)
                {
                    transform2 = null;
                }
                else
                {
                    ChildLocator component5 = modelTransform.GetComponent<ChildLocator>();
                    transform2 = ((component5 != null) ? component5.FindChild("FireworkOrigin") : null);
                }
            }
            if (transform2 == null)
                return;

            int infestorCount = (int)GetStackValue(Mathf.Round(Mathf.Lerp(infestorsMin, infestorsMax, Run.instance.spawnRng.nextNormalizedFloat)), infestorsStack, count);
            for (int i = 0; i < infestorCount; i++)
            {
                ScriptedCombatEncounter.SpawnInfo spawnInfo = new ScriptedCombatEncounter.SpawnInfo();
                spawnInfo.explicitSpawnPosition = transform2;
                spawnInfo.spawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/EliteVoid/cscVoidInfestor.asset").WaitForCompletion();
                Spawn(ref spawnInfo);
            }
        }
        private static void Spawn(ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
                minDistance = 0f,
                maxDistance = 1000f,
                position = spawnInfo.explicitSpawnPosition.position,
                spawnOnTarget = spawnInfo.explicitSpawnPosition
            };
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnInfo.spawnCard, directorPlacementRule, RoR2Application.rng);
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            directorSpawnRequest.teamIndexOverride = TeamIndex.Void;
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
    }
}
