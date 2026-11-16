using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.Stats;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Path = System.IO.Path;

namespace LoadingScreenFix
{
    /// <summary>
    /// Main plugin, also works as the main API for adding new sprites
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class LoadingScreenFix : BaseUnityPlugin
    {
        private struct SimpleSpriteAnimationData
        {
            public SimpleSpriteAnimation animation;
            public Vector2 pivot;
            public Vector2 sizeDelta;
        }

        public const string MINI_SCENE_ADDRESS = "3a63b5675ffabd247aa47f7eeadf946e";
        /// <summary>
        /// Plugin GUID
        /// </summary>
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        /// <summary>
        /// Plugin's Author
        /// </summary>
        public const string PluginAuthor = "Nebby1999";
        /// <summary>
        /// Plugin's Name
        /// </summary>
        public const string PluginName = "LoadingScreenFix";
        /// <summary>
        /// Plugin's Version
        /// </summary>
        public const string PluginVersion = "3.1.0";

        private GameObject _blackBackground;
        private GameObject _walkPrefab;
        private List<SimpleSpriteAnimationData> _spriteAnimations = new List<SimpleSpriteAnimationData>();

        private AssetBundle _myBundle;
        private static bool _pastOrigSelfOnPickRandomObjectOnAwake;
        private static bool _blackBackgroundsAdded;

        internal static LoadingScreenFix instance;
        internal static ManualLogSource logger;
        private static PickRandomObjectOnAwake _screenTintCanvasMiniScenePickRandomObjectOnAwake;
        private void Awake()
        {
            instance = this;
            logger = Logger;
            LoadBundle();
            On.RoR2.PickRandomObjectOnAwake.OnEnable += AddBlackBackgroundAndSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake += UnhookDestroyBackgroundAndAddToMiniScenePrefab;
            SceneManager.sceneLoaded += DisableBlackBackgrounds;

            Addressables.LoadAssetAsync<GameObject>(MINI_SCENE_ADDRESS).Completed += (obj) => _screenTintCanvasMiniScenePickRandomObjectOnAwake = obj.Result.GetComponent<PickRandomObjectOnAwake>();

            Log("Awake Called");
        }

        /// <summary>
        /// Adds the <see cref="SimpleSpriteAnimation"/> found within <paramref name="sourceBundle"/>.
        /// <para>The AssetBundle will not be unloaded, and the SimpleSpriteAnimations will be added to the ones that can be found when loading new stages and scenes.</para>
        /// <br>If you made your <see cref="SimpleSpriteAnimation"/> from code, or want to avoid adding all of the ones found in the bundle, look at <see cref="AddSpriteAnimation(SimpleSpriteAnimation)"/></br>
        /// <param name="sourceBundle">The AssetBundle from which we will load the simple sprite animations</param>
        public static void AddSpriteAnimations(AssetBundle sourceBundle)
        {
            if(_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            foreach(var ssa in sourceBundle.LoadAllAssets<SimpleSpriteAnimation>())
            {
                AddSpriteAnimation(ssa);
            }
        }

        [Obsolete("LoadingScrenFix no longer stores the source assetbundle, either call \"AddSpriteAnimations(AssetBundle)\" or \"AddSpriteAnimation(SimpleSpriteAnimation)\"")]
        public static void AddSpriteAnimation(SimpleSpriteAnimation spriteAnimation, AssetBundle sourceBundle)
        {
            if(_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            AddSpriteAnimation(spriteAnimation);
        }

        /// <summary>
        /// Adds the <paramref name="spriteAnimation"/> to the loading screen sprite animations.
        /// </summary>
        /// <param name="spriteAnimation">The sprite animation which will be added.</param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation spriteAnimation)
        {
            AddSpriteAnimation(spriteAnimation, new Vector2(1, 0), new Vector2(32, 32));
        }

        /// <summary>
        /// Adds the <paramref name="spriteAnimation"/> to the loading screen sprite animations
        /// </summary>
        /// <param name="spriteAnimation">The sprite animation which will be added.</param>
        /// <param name="pivotOverride">Override for the RectTransform's pivot, by default its <code>new Vector2(1, 0)</code></param>
        /// <param name="sizeDeltaOverride">Override for the RectTransform's sizeDelta, by default its <code>new Vector2(32, 32)</code></param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation spriteAnimation, Vector2? pivotOverride = null, Vector2? sizeDeltaOverride = null)
        {
            Vector2 pivot = pivotOverride ?? new Vector2(1, 0);
            Vector2 sizeDelta = sizeDeltaOverride ?? new Vector2(32, 32);

            if (_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            instance._spriteAnimations.Add(new SimpleSpriteAnimationData
            {
                animation = spriteAnimation,
                pivot = pivot,
                sizeDelta = sizeDelta,
            });
        }

        private void LoadBundle()
        {
            var directoryName = Path.GetDirectoryName(Info.Location);
            var assetBundleFolder = Path.Combine(directoryName, "assetbundles");
            var bundlePath = Path.Combine(assetBundleFolder, "loadingscreenutility");
            _myBundle = AssetBundle.LoadFromFile(bundlePath);
            _walkPrefab = _myBundle.LoadAsset<GameObject>("CustomSpriteWalk");
        }

        private void UnhookDestroyBackgroundAndAddToMiniScenePrefab(On.RoR2.UI.MainMenu.MainMenuController.orig_Awake orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            Log("Freeing resources and killing self.");

            On.RoR2.PickRandomObjectOnAwake.OnEnable -= AddBlackBackgroundAndSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake -= UnhookDestroyBackgroundAndAddToMiniScenePrefab;
            SceneManager.sceneLoaded -= DisableBlackBackgrounds;

            if (_blackBackground)
            {
                Log("Destroying Black Background");
                Destroy(_blackBackground);
            }

            if(_screenTintCanvasMiniScenePickRandomObjectOnAwake)
            {
                AddSprites(_screenTintCanvasMiniScenePickRandomObjectOnAwake);
            }
        }

        private void DisableBlackBackgrounds(Scene arg0, LoadSceneMode arg1)
        {
            Log("Checking if splash");
            if(arg0.name == "splash")
            {
                Log("Its splash, deleting all black backgrounds.");
                Destroy(_blackBackground);
                _blackBackground = null;
                return;
            }
            Log("Not Splash");
        }

        private void AddBlackBackgroundAndSpriteAnimations(On.RoR2.PickRandomObjectOnAwake.orig_OnEnable orig, PickRandomObjectOnAwake self)
        {
            if (self.gameObject.name != "MiniScene")
            {
                orig(self);
                return;
            }
            _pastOrigSelfOnPickRandomObjectOnAwake = true;
            AddSprites(self);
            AddBackgrounds(self);

            orig(self);
        }

        private void AddSprites(PickRandomObjectOnAwake self)
        {
            foreach(var anim in _spriteAnimations)
            {
                try
                {
                    Log($"Creating sprite for {anim.animation}");

                    var instance = GameObject.Instantiate(_walkPrefab, self.transform);

                    //If this is true it means it's the sprites for the inbetween scene loading. Which's local position needs to be zero.
                    if(_blackBackgroundsAdded)
                        instance.transform.localPosition = Vector3.zero;

                    instance.name = $"{anim.animation.name}Animator";
                    
                    var animator = instance.GetComponentInChildren<SimpleSpriteAnimator>();
                    
                    animator.animation = anim.animation;
                    animator.target.sprite = anim.animation.frames.FirstOrDefault().sprite;

                    if(animator.TryGetComponent<RectTransform>(out var rectTransform))
                    {
                        rectTransform.pivot = anim.pivot;
                        rectTransform.sizeDelta = anim.sizeDelta;
                    }

                    HG.ArrayUtils.ArrayAppend(ref self.ObjectsToSelect, instance);
                }
                catch(Exception e)
                {
                    Logger.LogError($"Failed to add sprite animation for {anim}.\n{e}");
                }
            }
        }

        private void AddBackgrounds(PickRandomObjectOnAwake self)
        {
            if (_blackBackgroundsAdded)
                return;

            _blackBackgroundsAdded = true;

            _blackBackground = new GameObject("BlackBackground");
            var t = _blackBackground.AddComponent<RectTransform>();
            t.SetParent(self.transform.parent.parent);
            t.SetAsFirstSibling();
            FitToChildrenOf(t, self.transform.parent as RectTransform);
            //t.localPosition = new Vector3(800, -477, 0); // magic numbers i got that looked good from unity editor :nikodurr: .,., //Thanks kina <3
            var img = _blackBackground.AddComponent<Image>();
            img.color = Color.black;
        }

        private void FitToChildrenOf(RectTransform target, RectTransform source)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(source);
            Canvas.ForceUpdateCanvases();

            var corners = new Vector3[4];
            bool found = false;
            Vector2 min = Vector2.zero, max = Vector2.zero;

            foreach (var rt in source.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt == source) 
                    continue;

                rt.GetWorldCorners(corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 p = target.InverseTransformPoint(corners[i]);
                    if (!found) 
                    { 
                        min = max = p; 
                        found = true; 
                    }
                    else 
                    { 
                        min = Vector2.Min(min, p); 
                        max = Vector2.Max(max, p); 
                    }
                }
            }

            if (!found) 
                return;

            Vector2 size = max - min;
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            Vector2 desiredCenter = (min + max) * 0.5f;
            Vector2 currentCenter = new Vector2((0.5f - target.pivot.x) * size.x, (0.5f - target.pivot.y) * size.y);
            target.position += target.TransformVector(desiredCenter - currentCenter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Log(object thing)
        {
#if !RELEASE
            Logger.LogDebug(thing);
#endif
        }
    }
}
