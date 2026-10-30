using RoR2.ExpansionManagement;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class SwanEliteSpread : ItemBase<SwanEliteSpread>
    {
        public override bool GetPrerequisites()
        {
            return MoreHexesPlugin.isSwanSongLoaded;
        }
        public static int spreadCtBase = 2;
        public static int spreadCtStack = 1;
        public static float spreadRangeBase = 30f;
        public static float spreadRangeStack = 10f;
        public static float spreadDurationBase = 8f;
        public static float spreadDurationStack = 5f;
        public static bool blacklistVoidtouched = true;
        public override string ItemName => "Hex of Power";

        public override string ItemLangTokenName => "CURSEELITESPREAD";

        public override string ItemPickupDesc => $"Slain elites {HexTierColor("empower")} other nearby enemies.";

        public override string ItemFullDescription => $"Slain Elites {HexTierColor("empower")} up to {UtilityColor(spreadCtBase.ToString())} {StackText("+" + spreadCtStack)} nearby enemies. " +
            $"Empowered enemies gain the {DamageColor("power")} of the killed elite monster for {DamageColor(spreadDurationBase.ToString())} {StackText("+" + spreadDurationStack)}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { };

        public override GameObject ItemModel => LoadDropPrefab("PickupCurseEliteSpread");//8

        public override Sprite ItemIcon => LoadItemIcon("texIconHexEliteSpread");
        public override ExpansionDef RequiredExpansion => SwanSongExtended.SwanSongPlugin.expansionDefSS2;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            GlobalEventManager.onCharacterDeathServer += SpreadElitePower;
        }

        private void SpreadElitePower(DamageReport damageReport)
        {
            if (damageReport.victimBody == null && damageReport.attackerBody == null)
                return;
            if (damageReport.victimIsElite == false)
                return;
            int count = GetCount(damageReport.attackerBody);
            if (count <= 0)
                return;
            float maxTransferCount = GetStackValue(spreadCtBase, spreadCtStack, count);
            float maxRange = GetStackValue(spreadRangeBase, spreadRangeStack, count);
            float maxDuration = GetStackValue(spreadDurationBase, spreadDurationStack, count);

            SphereSearch sphereSearch = new SphereSearch
            {
                mask = LayerIndex.entityPrecise.mask,
                origin = damageReport.victimBody.transform.position,
                queryTriggerInteraction = QueryTriggerInteraction.Collide,
                radius = maxRange
            };

            TeamMask teamMask = TeamMask.GetEnemyTeams(damageReport.attackerTeamIndex);
            List<HurtBox> hurtBoxesList = new List<HurtBox>();

            sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(teamMask).FilterCandidatesByDistinctHurtBoxEntities().OrderCandidatesByDistance().GetHurtBoxes(hurtBoxesList);
            int hurtBoxCount = hurtBoxesList.Count;
            if (hurtBoxCount <= 0)
                return;

            BuffDef[] victimEliteBuffs = damageReport.victimBody.activeBuffsList
                .Select(buffIndex => BuffCatalog.GetBuffDef(buffIndex))
                .Where(buffDef => buffDef.isElite == true 
                    && (blacklistVoidtouched == false || buffDef.buffIndex != DLC1Content.Buffs.EliteVoid.buffIndex))
                .ToArray();

            int transferCount = 0;
            for (int i = 0; i < hurtBoxCount; i++)
            {
                if (transferCount >= maxTransferCount)
                    break;

                HurtBox targetHurtBox = hurtBoxesList[i];
                if (targetHurtBox == null)
                    continue;

                HealthComponent healthComponent = targetHurtBox.healthComponent;
                CharacterBody enemyBody = healthComponent.body;
                if (enemyBody == null || enemyBody == damageReport.victimBody)
                    continue;

                bool didTransfer = false;
                foreach(BuffDef buffDef in victimEliteBuffs)
                {
                    if (enemyBody.HasBuff(buffDef))
                        continue;
                    enemyBody.AddTimedBuff(buffDef, maxDuration);
                    didTransfer = true;
                }
                if (didTransfer == false)
                    continue;
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("prefabs/effects/JellyfishNova"), new EffectData
                {
                    origin = enemyBody.corePosition,
                    scale = enemyBody.bestFitRadius + 4
                }, true);
                transferCount++;
            }
        }
    }
}
