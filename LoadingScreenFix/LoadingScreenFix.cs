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
        public const string PluginVersion = "2.0.0";

        private GameObject _blackBackground;
        private GameObject _walkPrefab;
        private List<SimpleSpriteAnimation> _spriteAnimations = new List<SimpleSpriteAnimation>();
        private HashSet<AssetBundle> _assetBundles = new HashSet<AssetBundle>();
        private List<Image> _blackBackgroundInstances = new List<Image>();

        private AssetBundle _myBundle;
        private static bool _pastOrigSelfOnPickRandomObjectOnAwake;
        internal static LoadingScreenFix instance;
        internal static ManualLogSource logger;
        private void Awake()
        {
            instance = this;
            logger = Logger;
            LoadBundle();
            On.RoR2.PickRandomObjectOnAwake.Awake += AddBlackBackgroundAndSpriteAnimations;
#if RELEASE
            On.RoR2.UI.MainMenu.MainMenuController.Awake += FreeMemoryAndDestroySelf;
            SceneManager.sceneLoaded += DisableBlackBackgrounds;
#endif
            Log("Awake Called");
        }

        /// <summary>
        /// Adds the <see cref="SimpleSpriteAnimation"/> found within <paramref name="sourceBundle"/>.
        /// 
        /// <para>The API expects that this assetbundle only contains the sprite animations, as such, once we're past the loading screen, the loaded <see cref="SimpleSpriteAnimation"/>s and the <paramref name="sourceBundle"/> will be DESTROYED and UNLOADED respectively.</para>
        /// <br>If you made your <see cref="SimpleSpriteAnimation"/> from code, or want to avoid the unloading of the source bundle, look at <see cref="AddSpriteAnimation(SimpleSpriteAnimation)"/></br>
        /// </summary>
        /// <param name="sourceBundle">The AssetBundle from which we will load the simple sprite animations</param>
        public static void AddSpriteAnimations(AssetBundle sourceBundle)
        {
            if(_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            instance._assetBundles.Add(sourceBundle);
            foreach(var ssa in sourceBundle.LoadAllAssets<SimpleSpriteAnimation>())
            {
                AddSpriteAnimation(ssa);
            }
        }

        /// <summary>
        /// Adds the <paramref name="spriteAnimation"/>, which was loaded from <paramref name="sourceBundle"/>
        /// 
        /// <para>The API expects that this assetbundle only contains the sprite animations, as such, once we're past the loading screen, the <paramref name="spriteAnimation"/> and the <paramref name="sourceBundle"/> will be DESTROYED and UNLOADED respectively.</para>
        /// <br>If you made your <see cref="SimpleSpriteAnimation"/> from code, or want to avoid the unloading of the source bundle, look at <see cref="AddSpriteAnimation(SimpleSpriteAnimation)"/></br>
        /// </summary>
        /// <param name="spriteAnimation">The <see cref="SimpleSpriteAnimation"/> that contains the sprite animation</param>
        /// <param name="sourceBundle">The AssetBundle from which <paramref name="spriteAnimation"/> was loaded from</param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation spriteAnimation, AssetBundle sourceBundle)
        {
            if(_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            instance._assetBundles.Add(sourceBundle);
            AddSpriteAnimation(spriteAnimation);
        }

        /// <summary>
        /// Adds the <paramref name="spriteAnimation"/> to the loading screen sprite animations.
        /// 
        /// <para>The API will eventually DESTROY the instance of the <paramref name="spriteAnimation"/> once we're past the loading screen, if you want to keep your SpriteAnimation, consider giving the API a duplicate instance by calling UnityEngine.Object.Instantiate() and using your sprite animation as the argument.</para>
        /// <code>
        /// private static void AddAnimationButKeepObjectAlive(SimpleSpriteAnimation orig)
        /// {
        ///     LoadingScreenFix.AddSpriteAnimation(UnityEngine.Object.Instantiate(orig));
        /// }
        /// </code>
        /// </summary>
        /// <param name="spriteAnimation">The sprite animation which will be added.</param>
        public static void AddSpriteAnimation(SimpleSpriteAnimation spriteAnimation)
        {
            if (_pastOrigSelfOnPickRandomObjectOnAwake)
            {
                instance.Logger.LogInfo("Too late! we're already past the loading screen, consider calling the API's methods on your mod's awake.");
                return;
            }

            instance._spriteAnimations.Add(spriteAnimation);
        }

        private void LoadBundle()
        {
            var directoryName = Path.GetDirectoryName(Info.Location);
            var assetBundleFolder = Path.Combine(directoryName, "assetbundles");
            var bundlePath = Path.Combine(assetBundleFolder, "loadingscreenutility");
            _myBundle = AssetBundle.LoadFromFile(bundlePath);
            _walkPrefab = _myBundle.LoadAsset<GameObject>("CustomSpriteWalk");
        }

        private void FreeMemoryAndDestroySelf(On.RoR2.UI.MainMenu.MainMenuController.orig_Awake orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            Log("Freeing resources and killing self.");

            On.RoR2.PickRandomObjectOnAwake.Awake -= AddBlackBackgroundAndSpriteAnimations;
            On.RoR2.UI.MainMenu.MainMenuController.Awake -= FreeMemoryAndDestroySelf;
            SceneManager.sceneLoaded -= DisableBlackBackgrounds;

            if (_blackBackground)
            {
                Log("Destroying Black Background");
                Destroy(_blackBackground);
            }

            foreach(var anim in _spriteAnimations)
            {
                Log("Destroying " + anim);
                Destroy(anim);
            }

            foreach(var bundle in _assetBundles)
            {
                Log("Unloading " + bundle);
                bundle.Unload(true);
            }
            Log("Unloading own bundle");
            _myBundle.Unload(true);
        }

        private void DisableBlackBackgrounds(Scene arg0, LoadSceneMode arg1)
        {
            Log("Checking if splash");
            if(arg0.name == "splash")
            {
                Log("Its splash, disabling all black backgrounds.");

                foreach(var blackBackground in _blackBackgroundInstances)
                {
                    blackBackground.enabled = false;
                }
                return;
            }
            Log("Not Splash");
        }

        private void AddBlackBackgroundAndSpriteAnimations(On.RoR2.PickRandomObjectOnAwake.orig_Awake orig, PickRandomObjectOnAwake self)
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
                    Log($"Creating sprite for {anim}");

                    var instance = GameObject.Instantiate(_walkPrefab, self.transform);

                    instance.name = $"{anim}Animator";
                    var animator = instance.GetComponentInChildren<SimpleSpriteAnimator>();
                    animator.animation = anim;
                    animator.target.sprite = anim.frames.FirstOrDefault().sprite;

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
            _blackBackground = new GameObject("BlackBackground");
            var t = _blackBackground.AddComponent<RectTransform>();
            t.SetParent(self.transform.parent);
            t.SetAsFirstSibling();
            t.sizeDelta = new Vector2(256, 256);
            t.localPosition = new Vector3(64, 16, -16);
            var img = _blackBackground.AddComponent<Image>();
            _blackBackgroundInstances.Add(img);
            img.color = Color.black;
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
