using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HexingChaos.Modules
{
    internal static class Materials
    {
        internal static void GetMaterial(GameObject model, string childObject, Color color, ref Material material, float scaleMultiplier = 1, bool replaceAll = false)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Renderer smr = renderer;

                if (string.Equals(renderer.name, childObject))
                {
                    if (color == Color.clear)
                    {
                        UnityEngine.GameObject.Destroy(renderer);
                        return;
                    }

                    if (material == null)
                    {
                        material = new Material(renderer.material);
                        material.mainTexture = renderer.material.mainTexture;
                        material.shader = renderer.material.shader;
                        material.color = color;
                    }
                    renderer.material = material;
                    renderer.transform.localScale *= scaleMultiplier;
                    if (!replaceAll)
                        break;
                }
            }
        }
        internal static void DebugMaterial(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Renderer smr = renderer;
                Debug.Log("Material: " + smr.name.ToString());
            }
        }

        #region shaders lol

        public static void SwapShadersFromMaterialsInBundle(AssetBundle bundle) => SwapAllShaders(bundle);


        internal static void SwapAllShaders(AssetBundle assetBundle)
        {
            foreach (var material in assetBundle.LoadAllAssets<Material>())
            {
                Log.Debug("Trying to swap shader for " + material.name);
                TrySwapShader(material);
            }

        }

        internal static void TrySwapShader(Material material)
        {
            var shaderName = material.shader.name;
            if (shaderName.Contains("Stubbed"))
            {
                shaderName = shaderName.Replace("Stubbed", string.Empty) + ".shader";
                var replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();

                if (replacementShader != null)
                {
                    material.shader = replacementShader;
                    //RiskOfRamenMain.LogDebug("Swapped shader " + material.name + "!");
                }
                else
                {
                    Log.Error("Failed to load shader " + shaderName);
                }
            }
            else if (shaderName == "Standard")
            {
                var normalMap = material.GetTexture("_BumpMap");
                var normalStrength = material.GetFloat("_BumpScale");
                var emissionMap = material.GetTexture("_EmissionMap");

                material.shader = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

                material.SetTexture("_NormalMap", normalMap);
                material.SetFloat("_NormalStrength", normalStrength);
                material.SetTexture("_EmTex", emissionMap);

                material.SetColor("_EmColor", new Color(0.2f, 0.2f, 0.2f));
                material.SetFloat("_EmPower", 0.15f);

            }
        }
        private static void SwapShader(Material material)
        {
            var shaderName = material.shader.name.Substring("Stubbed".Length);
            var adressablePath = $"{shaderName}.shader";
            Shader shader = Addressables.LoadAssetAsync<Shader>(adressablePath).WaitForCompletion();
            material.shader = shader;
            MaterialsWithSwappedShaders.Add(material);
        }
        public static List<Material> MaterialsWithSwappedShaders { get; } = new List<Material>();
        #endregion

        private static List<Material> cachedMaterials = new List<Material>();

        internal static Shader hotpoo = RoR2.LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

        public static Material LoadMaterial(this AssetBundle assetBundle, string materialName) => CreateHopooMaterialFromBundle(assetBundle, materialName);
        public static Material CreateHopooMaterialFromBundle(this AssetBundle assetBundle, string materialName)
        {
            Material tempMat = cachedMaterials.Find(mat =>
            {
                materialName.Replace(" (Instance)", "");
                return mat.name.Contains(materialName);
            });
            if (tempMat)
            {
                Log.Debug($"{tempMat.name} has already been loaded. returning cached");
                return tempMat;
            }
            tempMat = assetBundle.LoadAsset<Material>(materialName);

            if (!tempMat)
            {
                Log.ErrorAssetBundle(materialName, assetBundle.name);
                return new Material(hotpoo);
            }

            return tempMat.ConvertDefaultShaderToHopoo();
        }

        public static Material SetHopooMaterial(this Material tempMat) => ConvertDefaultShaderToHopoo(tempMat);
        public static Material ConvertDefaultShaderToHopoo(this Material tempMat)
        {
            if (cachedMaterials.Contains(tempMat))
            {
                Log.Debug($"{tempMat.name} has already been loaded. returning cached");
                return tempMat;
            }

            float? bumpScale = null;
            Color? emissionColor = null;

            //grab values before the shader changes
            if (tempMat.IsKeywordEnabled("_NORMALMAP"))
            {
                bumpScale = tempMat.GetFloat("_BumpScale");
            }
            if (tempMat.IsKeywordEnabled("_EMISSION"))
            {
                emissionColor = tempMat.GetColor("_EmissionColor");
            }

            //set shader
            tempMat.shader = hotpoo;

            //apply values after shader is set
            tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            tempMat.EnableKeyword("DITHER");

            if (bumpScale != null)
            {
                tempMat.SetFloat("_NormalStrength", (float)bumpScale);
                tempMat.SetTexture("_NormalTex", tempMat.GetTexture("_BumpMap"));
            }
            if (emissionColor != null)
            {
                tempMat.SetColor("_EmColor", (Color)emissionColor);
                tempMat.SetFloat("_EmPower", 1);
            }

            //set this keyword in unity if you want your model to show backfaces
            //in unity, right click the inspector tab and choose Debug
            if (tempMat.IsKeywordEnabled("NOCULL"))
            {
                tempMat.SetInt("_Cull", 0);
            }
            //set this keyword in unity if you've set up your model for limb removal item displays (eg. goat hoof) by setting your model's vertex colors
            if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
            {
                tempMat.SetInt("_LimbRemovalOn", 1);
            }

            cachedMaterials.Add(tempMat);
            return tempMat;
        }

        /// <summary>
        /// Makes this a unique material if we already have this material cached (i.e. you want an altered version). New material will not be cached
        /// <para>If it was not cached in the first place, simply returns as it is already unique.</para>
        /// </summary>
        public static Material MakeUnique(this Material material)
        {

            if (cachedMaterials.Contains(material))
            {
                return new Material(material);
            }
            return material;
        }

        public static Material SetColor(this Material material, Color color)
        {
            material.SetColor("_Color", color);
            return material;
        }

        public static Material SetNormal(this Material material, float normalStrength = 1)
        {
            material.SetFloat("_NormalStrength", normalStrength);
            return material;
        }

        public static Material SetEmission(this Material material) => SetEmission(material, 1);
        public static Material SetEmission(this Material material, float emission) => SetEmission(material, emission, Color.white);
        public static Material SetEmission(this Material material, float emission, Color emissionColor)
        {
            material.SetFloat("_EmPower", emission);
            material.SetColor("_EmColor", emissionColor);
            return material;
        }
        public static Material SetCull(this Material material, bool cull = false)
        {
            material.SetInt("_Cull", cull ? 1 : 0);
            return material;
        }

        public static Material SetSpecular(this Material material, float strength)
        {
            material.SetFloat("_SpecularStrength", strength);
            return material;
        }
        public static Material SetSpecular(this Material material, float strength, float exponent)
        {
            material.SetFloat("_SpecularStrength", strength);
            material.SetFloat("SpecularExponent", exponent);
            return material;
        }
    }
    internal static class Particles
    {
        internal static void GetParticle(GameObject model, string childObject, Color color, float sizeMultiplier = 1, bool replaceAll = false)
        {
            ParticleSystem[] partSystems = model.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem partSys in partSystems)
            {
                ParticleSystem ps = partSys;
                var main = ps.main;
                var lifetime = ps.colorOverLifetime;
                var speed = ps.colorBySpeed;

                if (string.Equals(ps.name, childObject))
                {
                    main.startColor = color;
                    main.startSizeMultiplier *= sizeMultiplier;
                    lifetime.color = color;
                    speed.color = color;
                    if (!replaceAll)
                        break;
                }
            }
        }
        internal static void DebugParticleSystem(GameObject model)
        {
            ParticleSystem[] partSystems = model.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem partSys in partSystems)
            {
                ParticleSystem ps = partSys;
                Debug.Log("Particle: " + ps.name.ToString());
            }
        }
    }
}
