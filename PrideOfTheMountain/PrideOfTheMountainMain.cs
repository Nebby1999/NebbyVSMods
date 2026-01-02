using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PrideOfTheMountain
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PrideOfTheMountainMain : BaseUnityPlugin
    {
        private const string GAY_PRIDE = "5;#3D1A78;#7BADE2;#FFFFFF;#98E8C1;#078D70";
        private const string RAINBOW_PRIDE = "6;#732982;#004CFF;#008062;#FFED00;#FF8C00;#E40303";
        private const string LESBIAN_PRESET = "5;#A40062;#D462A6;#FFFFFF;#FF9B56;#D62800";
        private const string BISEXUAL_PRESET = "5;#0038A8;#0038A8;#9B4F96;#D60270;#D60270";
        private const string TRANSGENDER_PRESET = "4;#5BCFFB;#F5ABB9;#FFFFFF;#F5ABB9";
        private const string NON_BINARY_PRESET = "4;#282828;#9D59D2;#FCFCFC;#FCF431";
        private const string CUSTOM_DEFAULT_PRESET = "4;#810081;#FFFFFF;#A4A4A4;#000000";
        public enum Presets
        {
            Custom = -1,
            Gay,
            Rainbow,
            Lesbian,
            Bisexual,
            Transgender,
            NonBinary
        }

        private readonly struct FlagColorer
        {
            public readonly int colorCount;
            public readonly ReadOnlyArray<Color> flagColors;

            public static FlagColorer Parse(string input)
            {
                if(TryParse(input, out var result))
                {
                    return result;
                }
                return default;
            }
            public static bool TryParse(string input, out FlagColorer output)
            {
                output = new FlagColorer();
                try
                {
                    string[] split = input.Split(';');
                    if(split.Length == 0)
                    {
                        throw new FormatException("String has the incorrect format, each value must be separated using the char \";\"");
                    }

                    if(split.Length <= 1)
                    {
                        throw new FormatException("String does not have enough ; separated arguments. There's only one, which is the color amount, the rest of the arguments must be the colors, which there are none.");
                    }

                    if (!int.TryParse(split[0], out int colorCount))
                    {
                        throw new FormatException($"The first argument of the String (\"{split[0]}\") is not a number.");
                    }

                    //We take the split array's length, and subtract one, that will yield the amount of possible color strings in the array.
                    int splitArrayLengthMinusOne = split.Length - 1;
                    if(splitArrayLengthMinusOne != colorCount)
                    {
                        throw new FormatException($"The string should have a total of \"{colorCount}\" colors, however, only {splitArrayLengthMinusOne} entries are left in the split array. The amount of colors defined after the first argument must be the amount specified. (3 means 3 hexadecimal colors)");
                    }

                    Color[] colors = new Color[splitArrayLengthMinusOne];
                    //We start at index 1, since index 0 is the color count.

                    for(int i = 1; i < split.Length; i++)
                    {
                        string hexadecimalColorAsString = split[i];
                        if(!ColorUtility.TryParseHtmlString(hexadecimalColorAsString, out Color color))
                        {
                            throw new FormatException($"The hexadecimal color \"{hexadecimalColorAsString}\" has an invalid hexadecimal format.");
                        }

                        colors[i - 1] = color;
                    }

                    output = new FlagColorer(colorCount, colors);
                    return true;
                }
                catch(Exception e)
                {
                    logger.LogError($"Failed to parse FlagColorer with input \"{input}\".\n{e}");
                    return false;
                }
            }

            private FlagColorer(int colorCount, Color[] colors)
            {
                this.colorCount = colorCount;
                this.flagColors = new ReadOnlyArray<Color>(colors);
            }
        }
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "Nebby1999";

        public const string PluginName = "PrideOfTheMountain";

        public const string PluginVersion = "1.0.0";
        private int _TintColor = Shader.PropertyToID(nameof(_TintColor));
        private ConfigEntry<Presets> _chosenPreset;
        private ConfigEntry<string> _customPreset;
        private ConfigEntry<bool> _prideify;
        private Dictionary<Presets, FlagColorer> _flagColorers = new Dictionary<Presets, FlagColorer>();
        private bool _customFailedToParse;

        private static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;

            BindConfigs();


            PopulateColorers();

            if (_prideify.Value == false || _customFailedToParse == true)
                return;

            On.RoR2.BossShrineCounter.RebuildIndicators += SetupPrideIndicators;
        }

        private void BindConfigs()
        {
            _prideify = Config.Bind("Main Config", "Activate Recolors", true, "Wether the mod is active, set this to false to disable the mod's functionality.");
            _chosenPreset = Config.Bind("Main Config", "Preset", Presets.Transgender, "The current preset for shrine of the mountain icon patterns, each preset represents one of the built in pride flags. You can set this value to Custom if you wish to declare your own custom flag.");
            _customPreset = Config.Bind("Main Config", "Custom Preset", CUSTOM_DEFAULT_PRESET, "A custom color preset. a Color preset is a string value separated by \";\" The first value is the total amount of colors the preset uses, afterwards you must specify each color for each index in a Hexadecimal format (ex: #FFFFFF). The default value for this config is a way to declare the \"Asexual\" flag colors as a custom preset for the mod.");
        }

        private void PopulateColorers()
        {
            _flagColorers[Presets.Rainbow] = FlagColorer.Parse(RAINBOW_PRIDE);
            _flagColorers[Presets.NonBinary] = FlagColorer.Parse(NON_BINARY_PRESET);
            _flagColorers[Presets.Lesbian] = FlagColorer.Parse(LESBIAN_PRESET);
            _flagColorers[Presets.Gay] = FlagColorer.Parse(GAY_PRIDE);
            _flagColorers[Presets.Bisexual] = FlagColorer.Parse(BISEXUAL_PRESET);
            _flagColorers[Presets.Transgender] = FlagColorer.Parse(TRANSGENDER_PRESET);

            if(FlagColorer.TryParse(_customPreset.Value, out FlagColorer result))
            {
                _flagColorers[Presets.Custom] = result;
            }
            else
            {
                logger.LogError("Custom preset failed to parse, the mod will not modify anything this session.");
                _customFailedToParse = true;
            }
        }

        private void SetupPrideIndicators(On.RoR2.BossShrineCounter.orig_RebuildIndicators orig, RoR2.BossShrineCounter self)
        {
            DestroyMaterialsOnIndicators(self._indicators);
            orig(self);
            PrideifyCounters(self._indicators, self._indicatorCount);
        }

        private void DestroyMaterialsOnIndicators(List<GameObject> indicators)
        {
            for(int i = 0; i < indicators.Count; i++)
            {
                GameObject indicator = indicators[i];
                MeshRenderer meshRenderer = indicator.GetComponent<MeshRenderer>();

                Destroy(meshRenderer.material);
            }
        }

        private void PrideifyCounters(List<GameObject> indicators, int indicatorCount)
        {
            if (_flagColorers.TryGetValue(_chosenPreset.Value, out FlagColorer flagColorer))
            {
                int colorerIndex = 0;

                //This is the indicator index.
                for(int i = 0; i < indicatorCount; i++)
                {
                    GameObject indicator = indicators[i];
                    MeshRenderer meshRenderer = indicator.GetComponent<MeshRenderer>();

                    Material material = meshRenderer.material;
                    material.SetColor(_TintColor, flagColorer.flagColors[colorerIndex]);
                    meshRenderer.material = material;

                    colorerIndex++;
                    if(colorerIndex >= flagColorer.colorCount)
                    {
                        colorerIndex = 0;
                    }
                }
            }
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
