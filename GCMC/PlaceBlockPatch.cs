using System.Reflection;
using DataLoader;
using HarmonyLib;
using JetBrains.Annotations;
using RobocraftX.Common;
using RobocraftX.CR.MachineEditing;
using RobocraftX.StateSync;
using Svelto.ECS;
using Unity.Entities;
using UnityEngine;

namespace GCMC
{
    /*[HarmonyPatch]
    [UsedImplicitly]
    public class PlaceBlockPatch
    {
        static void Postfix(EnginesRoot enginesRoot, ref StateSyncRegistrationHelper stateSyncReg, bool isAuthoritative)
        {
            if (isAuthoritative)
            {
                stateSyncReg.AddDeterministicEngine(new CubePlacerEngine());
                Debug.Log($"Added Minecraft world import engine");
            }
            else
                Debug.Log("Not authoritative, not adding MC engine");
        }

        static MethodBase TargetMethod()
        {
            return typeof(MainEditingCompositionRoot).GetMethod("Compose",
                BindingFlags.Public | BindingFlags.Static);
        }
    }*/
}