using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RainrotSharedUtils.Styling;

namespace HexingChaos.Items
{
    public class SwanSmallerTp : ItemBase<SwanSmallerTp>
	{
		public override bool GetPrerequisites()
		{
			return MoreHexesPlugin.isSwanSongLoaded;
		}
		/// <summary>
		/// hyperbolic plsss
		/// </summary>
		public static float shrinkAmt = 0.3f;
        public override string ItemName => "Hex of Convergence";

        public override string ItemLangTokenName => "CURSECONVERGENCE";

        public override string ItemPickupDesc => $"{HexTierColor("Reduce")} the size of the Teleporter's radius.";

        public override string ItemFullDescription => $"Reduce the size of the Teleporter and 'Holdout Zones' such as the Void Fields " +
            $"by {HexTierColor(shrinkAmt.AsPercent())} {StackText("-" + shrinkAmt.AsPercent())}.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Hex;

        public override ItemTag[] ItemTags => new ItemTag[] { ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.CannotMagnify, ItemTag.DeathAffixBlacklist, ItemTag.BrotherBlacklist, ItemTag.DevotionBlacklist, ItemTag.ExtractorUnitBlacklist };

        public override GameObject ItemModel => LoadDropPrefab();

        public override Sprite ItemIcon => LoadItemIcon();
		public override ExpansionDef RequiredExpansion => SwanSongExtended.SwanSongPlugin.expansionDefSS2;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
        }

        private void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            orig(self);
            if (self.applyFocusConvergence)
            {
				self.gameObject.AddComponent<HexConvergenceController>();
            }
        }
    }

    public class HexConvergenceController : MonoBehaviour
	{
		private void Awake()
		{
			this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
		}
		private void OnEnable()
		{
			this.enabledTime = Run.FixedTimeStamp.now;
			this.holdoutZoneController.calcRadius += this.ApplyRadius;
			this.holdoutZoneController.calcColor += this.ApplyColor;
		}
		private void OnDisable()
		{
			this.holdoutZoneController.calcColor -= this.ApplyColor;
			this.holdoutZoneController.calcRadius -= this.ApplyRadius;
		}
		private void ApplyRadius(ref float radius)
		{
			if (this.currentFocusConvergenceCount > 0)
			{
				radius /= 1 + currentFocusConvergenceCount * SwanSmallerTp.shrinkAmt;
			}
		}
		private void ApplyColor(ref Color color)
		{
			color = Color.Lerp(color, HexConvergenceController.convergenceMaterialColor, HexConvergenceController.colorCurve.Evaluate(this.currentValue));
		}
		private void Update()
		{
			this.DoUpdate(Time.deltaTime);
		}
		private void DoUpdate(float deltaTime)
		{
			this.currentFocusConvergenceCount = Util.GetTotalItemCountFromTeam(new Util.GetTotalItemCountFromTeamArgs(SwanSmallerTp.instance.ItemsDef.itemIndex)
			{
				requiresAlive = true,
				requiredTeam = new TeamIndex?(this.holdoutZoneController.chargingTeam),
				requiresConnected = false
			});
			if (this.enabledTime.timeSince < HoldoutZoneController.FocusConvergenceController.startupDelay)
			{
				this.currentFocusConvergenceCount = 0;
			}
			this.currentFocusConvergenceCount = Mathf.Min(this.currentFocusConvergenceCount, HoldoutZoneController.FocusConvergenceController.cap);
			float target = ((float)this.currentFocusConvergenceCount > 0f) ? 1f : 0f;
			float num = Mathf.MoveTowards(this.currentValue, target, HoldoutZoneController.FocusConvergenceController.rampUpTime * deltaTime);
			if (this.currentValue <= 0f && num > 0f)
			{
				Util.PlaySound("Play_item_lunar_focusedConvergence", base.gameObject);
			}
			this.currentValue = num;
		}
		private static readonly float convergenceRadiusDivisor = 2f;
		private static readonly float convergenceChargeRateBonus = 0.3f;
		private static readonly Color convergenceMaterialColor = new Color32(0, 160, 34, 255);
		private static readonly float rampUpTime = 5f;
		private static readonly float startupDelay = 3f;
		private static readonly int cap = 3;
		private float currentValue;
		private HoldoutZoneController holdoutZoneController;
		private int currentFocusConvergenceCount;
		private Run.FixedTimeStamp enabledTime;
		private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	}
}
