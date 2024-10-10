using BepInEx;
using RoR2;
using RoR2.Stats;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LoadingScreenFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class LoadingScreenFix : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nebby1999";
        public const string PluginName = "LoadingScreenFix";
        public const string PluginVersion = "1.0.0";

        private GameObject _blackBackground;
        private void Awake()
        {
            On.RoR2.PickRandomObjectOnAwake.Awake += AddBlackBackground;
#if RELEASE
            SceneManager.sceneLoaded += ShouldDestroyObjectAndSuicide;
#endif

            Log("Awake Called");
        }

        private void ShouldDestroyObjectAndSuicide(Scene arg0, LoadSceneMode arg1)
        {
            Log("Checking if splash");
            if(arg0.name == "splash")
            {
                Log("Its splash, killing unhooking, destroying background and killing self.");
                On.RoR2.PickRandomObjectOnAwake.Awake -= AddBlackBackground;
                SceneManager.sceneLoaded -= ShouldDestroyObjectAndSuicide;
                if(_blackBackground)
                    Destroy(_blackBackground);

                Destroy(this);
                return;
            }
            Log("Not Splash");
        }

        private void AddBlackBackground(On.RoR2.PickRandomObjectOnAwake.orig_Awake orig, PickRandomObjectOnAwake self)
        {
            orig(self);

            if (self.gameObject.name != "MiniScene")
                return;

            foreach(var obj in self.ObjectsToSelect)
            {
                if (!obj.activeSelf)
                    continue;

                Log("adding Background to " + obj);
                _blackBackground = new GameObject("BlackBackground");
                var t = _blackBackground.AddComponent<RectTransform>();
                t.SetParent(self.transform.parent);
                t.SetAsFirstSibling();
                t.sizeDelta = new Vector2(256, 256);
                t.localPosition = new Vector3(64, 16, -16);
                var img = _blackBackground.AddComponent<Image>();
                img.color = Color.black;
            }
        }

        private void Log(object thing)
        {
#if !RELEASE
            Logger.LogDebug(thing);
#endif
        }
    }
}
