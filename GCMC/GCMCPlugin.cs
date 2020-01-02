using System.Reflection;
using Harmony;
using IllusionPlugin;
using UnityEngine;

namespace GCMC
{
    public class GCMCPlugin : IPlugin
    {
        public string Name { get; } = "GCMC";
        public string Version { get; } = "v0.0.1";
        public static HarmonyInstance harmony { get; protected set; }
        public const string HarmonyID = "io.github.norbipeti.GCMC";
        
        public void OnApplicationStart()
        {
            if (harmony == null)
            {
                harmony = HarmonyInstance.Create(HarmonyID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

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