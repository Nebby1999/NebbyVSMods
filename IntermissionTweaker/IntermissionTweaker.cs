using BepInEx;
using MonoMod.RuntimeDetour;
using RoR2;
using System;

namespace IntermissionTweaker
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class IntermissionTweaker : BaseUnityPlugin
    {
        public const string GUID = AUTHOR + "." + NAME;
        public const string AUTHOR = "Nebby1999";
        public const string NAME = "IntermissionTweaker";
        public const string VERSION = "1.0.0";

        private void Awake()
        {
            SceneCatalog.availability.CallWhenAvailable(TweakIntermissions);
        }

        private void TweakIntermissions()
        {
            foreach(var sceneDef in SceneCatalog.allSceneDefs)
            {
                if (sceneDef.sceneType != SceneType.Intermission)
                    continue;

                if (sceneDef.sceneType != SceneType.TimedIntermission)
                    continue;

                if(IsTimed(sceneDef))
                {
                    sceneDef.sceneType = SceneType.TimedIntermission;
                }
                else
                {
                    sceneDef.sceneType = SceneType.Intermission;
                }
            }
        }

        private bool IsTimed(SceneDef sceneDef)
        {
            string section = "Scene Defs";
            string sceneCachedName = sceneDef.cachedName;
            return Config.Bind(section, $"{sceneCachedName} is Timed", sceneDef.sceneType == SceneType.TimedIntermission, $"Wether time passes in the SceneDef {sceneCachedName}").Value;
                
        }
    }
}
