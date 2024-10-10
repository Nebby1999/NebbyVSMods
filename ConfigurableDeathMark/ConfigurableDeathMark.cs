using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigurableDeathMark
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class ConfigurableDeathMark : BaseUnityPlugin
    {
        public static ConfigurableDeathMark Instance { get; private set; }
        public const string GUID = "com.Nebby.ConfigurableDeathMark";
        public const string VERSION = "1.0.0";
        public const string MODNAME = "ConfigurableDeathMark";

    }
}
