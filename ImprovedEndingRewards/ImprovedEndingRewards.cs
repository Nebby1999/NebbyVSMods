using BepInEx;
using BepInEx.Configuration;
using ImprovedEndingRewards.ImprovedEndings;
using MonoMod.RuntimeDetour;
using Moonstorm;
using Moonstorm.Config;
using RoR2;
using System;

namespace ImprovedEndingRewards
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ImprovedEndingRewards : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nebby1999";
        public const string PluginName = "ImprovedEndingRewards";
        public const string PluginVersion = "1.0.0";

        internal static ImprovedEndingRewards Instance { get; private set; }
        internal ImprovedObliteration ImprovedObliteration { get; private set; }
        public void Awake()
        {
            Instance = this;

            ImprovedObliteration = new ImprovedObliteration();
            new RewardsLog(Logger);
            new RewardsConfig();

            ImprovedObliteration.Config();

            ConfigSystem.AddMod(this);
        }
    }
}
