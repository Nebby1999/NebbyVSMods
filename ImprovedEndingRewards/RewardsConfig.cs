using BepInEx;
using BepInEx.Configuration;
using Moonstorm;
using Moonstorm.Config;
using Moonstorm.Loaders;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImprovedEndingRewards
{
    internal class RewardsConfig : ConfigLoader<RewardsConfig>
    {
        public const string MITHRIX = "MithrixEnding";
        public const string OBLITERATION = "ObliterationEnding";
        public const string SCAVENGER = "TwistedScavengerEnding";
        public const string VOIDLING = "VoidlingEnding";
        public override BaseUnityPlugin MainClass => ImprovedEndingRewards.Instance;
        public override bool CreateSubFolder => true;

        public static ConfigurableBool _enableMithrix;
        internal static ConfigFile _mithrixConfig;
        public static ConfigurableBool _enableObliteration;
        internal static ConfigFile _obliterationConfigFile;
        public static ConfigurableBool _enableScav;
        internal static ConfigFile _scavConfigFile;
        public static ConfigurableBool _enableVoidling;
        internal static ConfigFile _voidlingConfigFile;
        private void CreateConfigFiles()
        {
            _mithrixConfig = CreateConfigFile(MITHRIX, false);
            _obliterationConfigFile = CreateConfigFile(MITHRIX, false);
            _scavConfigFile = CreateConfigFile(MITHRIX, false);
            _voidlingConfigFile = CreateConfigFile(MITHRIX, false);
        }

        private void ConfigBooleans()
        {
            _enableMithrix = MakeConfigurableBool(true, (b) =>
            {
                b.Section = MSUtil.NicifyString(MITHRIX);
                b.Key = "Enable Mithrix Improvements";
                b.Description = "Whether the improvements of Mithrix's rewards are active";
                b.ConfigFile = _mithrixConfig;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () =>
                    {
                        return Run.instance;
                    }
                };
            }).DoConfigure();

            _enableObliteration = MakeConfigurableBool(true, (b) =>
            {
                b.Section = MSUtil.NicifyString(OBLITERATION);
                b.Key = "Enable Obliteration Improvements";
                b.Description = "Whether the improvements of Obliteration's rewards are active";
                b.ConfigFile = _mithrixConfig;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () =>
                    {
                        return Run.instance;
                    }
                };
                b.OnConfigChanged += var =>
                {
                    if (var)
                    {
                        ImprovedEndingRewards.Instance.ImprovedObliteration.UnsetHooks();
                        ImprovedEndingRewards.Instance.ImprovedObliteration.SetHooks();
                    }
                    else
                    {
                        ImprovedEndingRewards.Instance.ImprovedObliteration.UnsetHooks();
                    }
                };
            }).DoConfigure();

            _enableScav = MakeConfigurableBool(true, (b) =>
            {
                b.Section = MSUtil.NicifyString(SCAVENGER);
                b.Key = "Enable Twisted Scavenger Sack Improvements";
                b.Description = "Whether the improvements of TwistedScavenger's rewards are active";
                b.ConfigFile = _mithrixConfig;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () =>
                    {
                        return Run.instance;
                    }
                };
            }).DoConfigure();

            _enableVoidling = MakeConfigurableBool(true, (b) =>
            {
                b.Section = MSUtil.NicifyString(VOIDLING);
                b.Key = "Enable Voidling Improvements";
                b.Description = "Whether the improvements of Voidling's rewards are active";
                b.ConfigFile = _mithrixConfig;
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () =>
                    {
                        return Run.instance;
                    }
                };
            }).DoConfigure();
        }
        internal RewardsConfig()
        {
            CreateConfigFiles();
            ConfigBooleans();
        }
    }
}
