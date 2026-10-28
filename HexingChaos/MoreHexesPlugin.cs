using BepInEx;
using R2API.Utils;
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static HexingChaos.Modules.Bindings;
using HexingChaos.Modules;
using System.Reflection;
using HexingChaos.Items;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2.ContentManagement;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning disable 
namespace HexingChaos
{
    //languageapi
    //contentaddition
    //swansong soft dep
    //rrsul hard dep
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(guid, modName, version)]
    public class MoreHexesPlugin : BaseUnityPlugin
    {
        public static PluginInfo PInfo;
        public const string guid = "com." + teamName + "." + modName;
        public const string teamName = "RiskOfBrainrot";
        public const string modName = "HexingChaos";
        public const string version = "0.1.0";
        private static AssetBundle _mainAssetBundle;
        public static AssetBundle mainAssetBundle
        {
            get
            {
                if (_mainAssetBundle == null)
                    _mainAssetBundle = Modules.Assets.LoadAssetBundle("baditemrehab");
                return _mainAssetBundle;
            }
            set
            {
                _mainAssetBundle = value;
            }
        }
        internal static event Action onPluginLoaded;
        public static bool isSwanSongLoaded => Tools.isLoaded(SwanSongExtended.SwanSongPlugin.guid);
        void Awake()
        {
            PInfo = this.Info;
            Modules.Config.Init();
            CommonAssets.Init();

            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();
            BeginInitializing<ItemBase>(allTypes, "HexingChaos.txt");

            Modules.Config.Save();
        }

        #region content initialization
        private void BeginInitializing<T>(Type[] allTypes, string fileName = "") where T : SharedBase
        {
            Type baseType = typeof(T);
            //base types must be a base and not abstract
            if (!baseType.IsAbstract)
            {
                Log.Error(Log.Combine() + "Incorrect BaseType: " + baseType.Name);
                return;
            }


            IEnumerable<Type> objTypesOfBaseType = allTypes.Where(type => !type.IsAbstract && type.IsSubclassOf(baseType));

            if (objTypesOfBaseType.Count() <= 0)
                return;

            IEnumerable<SharedBase> objsOfBaseType =
                objTypesOfBaseType
                    .Select((objType) => (T)System.Activator.CreateInstance(objType))
                    .OrderBy((sharedBase) => sharedBase.loadOrder);

            Log.Debug(Log.Combine(baseType.Name) + "Initializing");

            foreach (SharedBase obj in objsOfBaseType)
            {
                string s = Log.Combine(baseType.Name, obj.ConfigName);
                if (ValidateBaseType(obj as SharedBase))
                {
                    Log.Debug(s + "Validated");
                    InitializeBaseType(obj as SharedBase);
                    Log.Debug(s + "Initialized");
                }
            }

            //if (!string.IsNullOrEmpty(fileName))
            //    Modules.Language.TryPrintOutput(fileName);
        }

        bool ValidateBaseType(SharedBase obj)
        {
            bool enabled = obj.isEnabled;
            if (obj.forcePrerequisites)
                return enabled && obj.GetPrerequisites();

            return obj.Bind(enabled, "Should This Content Be Enabled") && obj.GetPrerequisites();
        }
        void InitializeBaseType(SharedBase obj)
        {
            obj.Init();
        }
        #endregion

        public static bool GetConfigBool(bool defaultValue, string packetTitle, string desc = "")
        {
            return ConfigManager.DualBindToConfig<bool>(packetTitle, Modules.Config.MyConfig, "Should This Content Be Enabled", defaultValue, desc);
            //if (desc != "")
            //{
            //    return CustomConfigFile.Bind<bool>("Packets - See README For Details.",
            //        packetTitle + " Packet", defaultValue,
            //        $"The changes in this Packet will be enabled if set to true.").Value;
            //}
            //return CustomConfigFile.Bind<bool>("Packets",
            //    packetTitle + " Packet", defaultValue,
            //    "(The following changes will be enabled if set to true) " + desc).Value;
        }
        #region loading stuff
        public static Sprite TryLoadSpriteFromBundle(string path, AssetBundle assetBundle = null, bool fallBackOnWrench = false)
        {
            Sprite s = TryLoadFromBundle<Sprite>(path, assetBundle);
            if (s)
                return s;

            s = Addressables.LoadAssetAsync<Sprite>(
                    (fallBackOnWrench ? RoR2BepInExPack.GameAssetPaths.RoR2_Base_Core.texNullIcon_png
                    : RoR2BepInExPack.GameAssetPaths.RoR2_Base_Common_MiscIcons.texWIPIcon_png)
                    ).WaitForCompletion();//Resources.Load<Sprite>("textures/miscicons/texWIPIcon");
            return s;
        }
        public static T TryLoadFromBundle<T>(string path, AssetBundle assetBundle = null) where T : UnityEngine.Object
        {
            if (assetBundle == null)
                assetBundle = mainAssetBundle;

            if (assetBundle && !string.IsNullOrWhiteSpace(path))
            {
                if (assetBundle.Contains(path))
                    return assetBundle.LoadAsset<T>(path);
            }
            return null;
        }
        #endregion
    }
}
