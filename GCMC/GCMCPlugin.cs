using System.Reflection;
using GamecraftModdingAPI.Utility;
using HarmonyLib;
using IllusionPlugin;
using UnityEngine;

namespace GCMC
{
    public class GCMCPlugin : IPlugin
    {
        public string Name { get; } = "GCMC";
        public string Version { get; } = "v0.0.1";
        public static Harmony harmony { get; protected set; }
        public const string HarmonyID = "io.github.norbipeti.GCMC";
        
        public void OnApplicationStart()
        {
            /*if (harmony == null)
            {
                harmony = new Harmony(HarmonyID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }*/
            GameEngineManager.AddGameEngine(new CubePlacerEngine());

            Debug.Log("GCMC loaded");
        }

        public void OnApplicationQuit()
        {
            harmony?.UnpatchAll(HarmonyID);
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }
    }
}