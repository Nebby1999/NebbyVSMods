using BepInEx.Configuration;
using Moonstorm;
using Moonstorm.Config;
using R2API;
using RiskOfOptions.OptionConfigs;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ImprovedEndingRewards.ImprovedEndings
{
    internal class ImprovedObliteration : IImprovedEnding
    {
        public bool IsEnabled => RewardsConfig._enableObliteration;
        public static ConfigurableFloat _coinsPerStageBeaten;
        public static ConfigurableBool _enableStagePenalty;
        public static ConfigurableInt _stagePenalty;
        public Dictionary<DifficultyIndex, ConfigurableFloat> difficultyToMultiplier = new Dictionary<DifficultyIndex, ConfigurableFloat>();
        public void Config()
        {
            _coinsPerStageBeaten = RewardsConfig.MakeConfigurableFloat(0.25f, f =>
            {
                f.ConfigFile = RewardsConfig._obliterationConfigFile;
                f.Section = MSUtil.NicifyString(RewardsConfig.OBLITERATION);
                f.Key = "Coins per Stage Beaten";
                f.Description = "The number of coins gained per stage beaten\n" +
                "This number is the base number of the mathematical equation which dictates how many extra coins you get when obliterating";
                f.UseStepSlider = false;
                f.SliderConfig = new SliderConfig
                {
                    min = 0,
                    max = 1,
                    checkIfDisabled = () => !IsEnabled
                };
            });
            _enableStagePenalty = RewardsConfig.MakeConfigurableBool(true, b =>
            {
                b.ConfigFile = RewardsConfig._obliterationConfigFile;
                b.Section = MSUtil.NicifyString(RewardsConfig.OBLITERATION);
                b.Key = "Enable Stage Penalty System";
                b.Description = "By default, the first 5 stages do not count for the purposes of extra coins per stage beaten. Setting this to false disables this feature";
                b.CheckBoxConfig = new CheckBoxConfig
                {
                    checkIfDisabled = () => !IsEnabled
                };
            });

            _stagePenalty = RewardsConfig.MakeConfigurableInt(5, i =>
            {
                i.ConfigFile = RewardsConfig._obliterationConfigFile;
                i.Section = MSUtil.NicifyString(RewardsConfig.OBLITERATION);
                i.Key = "Stage Penalty Amount";
                i.Description = "By default, the first 5 stages do not give you any extra coins.";
                i.SliderConfig = new IntSliderConfig
                {
                    min = 0,
                    max = 100,
                    checkIfDisabled = () => !IsEnabled || !_enableStagePenalty
                };
            });
            RoR2Application.onLoad += ConfigDifficulties;
        }

        private void ConfigDifficulties()
        {
            foreach(var (index, def) in DifficultyAPI.difficultyDefinitions)
            {
                if (def == null)
                    continue;

                difficultyToMultiplier.Add(index, RewardsConfig.MakeConfigurableFloat(def.scalingValue, f =>
                {
                    f.ConfigFile = RewardsConfig._obliterationConfigFile;
                    f.Section = MSUtil.NicifyString(RewardsConfig.OBLITERATION) + " Difficulty Reward Scaling";
                    f.Key = Language.GetString(def.nameToken);
                    f.Description = $"The scaling value of {f.Key}, does not actually modify the difficulty's scaling value but instead modifies the multiplied that the difficulty has on coin reward";
                    f.UseStepSlider = false;
                    f.SliderConfig = new SliderConfig()
                    {
                        checkIfDisabled = () => !IsEnabled,
                        min = 0,
                        max = 10,
                    };
                }));
            }
        }

        public void LogMessage(object data)
        {
            RewardsLog.Message(data);
        }

        public void SetHooks()
        {
            Run.onServerGameOver += OnObliteration;
        }

        private void OnObliteration(Run run, GameEndingDef def)
        {
            if(def != RoR2Content.GameEndings.ObliterationEnding)
            {
                return;
            }

            LogMessage("Run Over, Obliteration Reward Calculation...");
            float baseReward = _coinsPerStageBeaten * ((run.stageClearCount - _stagePenalty) * run.loopClearCount);
            float multiplier;
            uint reward;
            if (difficultyToMultiplier.TryGetValue(run.selectedDifficulty, out var mult))
            {
                multiplier = mult;
                reward = (uint)Mathf.CeilToInt(baseReward * multiplier);
            }
            else
            {
                multiplier = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue;
                reward = (uint)Mathf.CeilToInt(baseReward * multiplier);
            }

            if (reward <= 0)
            {
                return;
            }

            LogMessage($"Coins to award to each player: {reward}\n" +
                $"({_coinsPerStageBeaten} * (({run.stageClearCount} - {_stagePenalty}) * {run.loopClearCount}) * {multiplier})");

            foreach(NetworkUser user in NetworkUser.readOnlyInstancesList)
            {
                if (!user)
                    continue;
                if (!user.isParticipating)
                    continue;

                LogMessage($"Awarding coins to {user.userName}");
                user.AwardLunarCoins(reward);
            }
        }

        public void UnsetHooks()
        {
            Run.onServerGameOver -= OnObliteration;
        }
    }
}
