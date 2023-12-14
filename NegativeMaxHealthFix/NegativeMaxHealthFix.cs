using BepInEx;
using MonoMod.RuntimeDetour;
using RoR2;
using System;

namespace ImprovedEndingRewards
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NegativeMaxHealthFix : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nebby1999";
        public const string PluginName = "NegativeMaxHealthFix";
        public const string PluginVersion = "1.0.0";

        private Hook _hook;

        private void Awake()
        {
            var method = typeof(CharacterBody).GetProperty("maxHealth").GetSetMethod(true);
            var onHookConfig = new HookConfig
            {
                ManualApply = true,
            };
            _hook = new Hook(method, NoNegativeMaxHealth, onHookConfig);
        }
        private void NoNegativeMaxHealth(Action<CharacterBody, float> orig, CharacterBody self, float maxHealth)
        {
            if(maxHealth < 0)
            {
                Logger.LogFatal("Negative Health Found! attempting fix and logging stack trace!");
                Logger.LogFatal(new System.Diagnostics.StackTrace().ToString());
                self.RecalculateStats();
                return;
            }
            orig(self, maxHealth);
        }
        private void OnEnable()
        {
            _hook.Apply();
        }

        private void OnDisable()
        {
            _hook.Undo();
        }

        private void OnDestroy()
        {
            _hook.Free();
        }

#if DEBUG
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Delete))
            {
                Logger.LogDebug("Setting max health to a negative value");
                var body = PlayerCharacterMasterController.instances[0].master.GetBody();
                body.maxHealth = UnityEngine.Random.Range(-42069, -1);
            }
        }
#endif
    }
}
