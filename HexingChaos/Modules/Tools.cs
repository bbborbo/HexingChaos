using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HG;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static HexingChaos.MoreHexesPlugin;

namespace HexingChaos.Modules
{
    public static class Extensions
    {
        public static void AddPersistentListener(this UnityEvent unityEvent, UnityAction action)
        {
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }
    }

    public static class Tools
    {
        internal static bool isLoaded(string modguid)
        {
            foreach (KeyValuePair<string, PluginInfo> keyValuePair in Chainloader.PluginInfos)
            {
                string key = keyValuePair.Key;
                PluginInfo value = keyValuePair.Value;
                bool flag = key == modguid;
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public static class Assets
    {
        /// <summary>
        /// Loads an embedded asset bundle
        /// </summary>
        /// <param name="resourceBytes">The bytes returned by Properties.Resources.ASSETNAME</param>
        /// <returns>The loaded bundle</returns>
        internal static Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

        internal static AssetBundle LoadAssetBundle(string bundleName)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
                return loadedBundles[bundleName];
            }

            AssetBundle assetBundle = null;
            assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MoreHexesPlugin.PInfo.Location), bundleName));

            loadedBundles[bundleName] = assetBundle;

            return assetBundle;
        }
    }
    public static class Bindings
    {
        internal static bool BypassExpansionRequirement;

        public static bool BindSection(string sectionName)
        {
            return ConfigManager.DualBindToConfig<bool>(": Full Section Config", Config.MyConfig,
                sectionName,
                true,
                "Vanilla is FALSE. Set to false if you wish to disable changes made to an entire item or group of items.");
        }

        public static void Init()
        {
            string section = "";

            BypassExpansionRequirement = ConfigManager.DualBindToConfig<bool>(": Hexing Chaos", Config.MyConfig, "Bypass Expansion Requirements", false,
                "If set to true, some Hexes that don't technically rely on DLC assets will be made available to the Hallowed Concepts Expansion without requiring other Expansions or Mods."
                );
        }
    }
}
