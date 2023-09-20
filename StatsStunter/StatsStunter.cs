using BepInEx;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Stats;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace StatsStunter
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class StatsStunter : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nebby1999";
        public const string PluginName = "StatsStunter";
        public const string PluginVersion = "1.0.0";

        private void Awake()
        {
            On.RoR2.Stats.StatManager.Init += DoNot;
#if DEBUG
            On.RoR2.Stats.StatManager.ProcessEvents += StatManager_ProcessEvents;
#endif
        }

        private void DoNot(On.RoR2.Stats.StatManager.orig_Init orig)
        {
            orig();
            RoR2Application.onFixedUpdate -= StatManager.ProcessEvents;
        }

#if DEBUG
        private void StatManager_ProcessEvents(On.RoR2.Stats.StatManager.orig_ProcessEvents orig)
        {
            Logger.LogInfo("This should not appear every fixed update.");
        }
#endif

        private void OnEnable()
        {
            PauseManager.onPauseStartGlobal += ForceUpdate;
            RoR2Application.onShutDown += ForceUpdate;
            SceneManager.activeSceneChanged += ForceUpdateScene;
            RoR2Application.onFixedUpdate -= StatManager.ProcessEvents;
        }

        private void OnDisable()
        {
            PauseManager.onPauseStartGlobal -= ForceUpdate;
            RoR2Application.onShutDown -= ForceUpdate;
            SceneManager.activeSceneChanged -= ForceUpdateScene;
            RoR2Application.onFixedUpdate += StatManager.ProcessEvents;
        }

        private void ForceUpdate()
        {
            StatManager.ForceUpdate();
        }

        private void ForceUpdateScene(Scene _, Scene __)
        {
            ForceUpdate();
        }
    }
}
