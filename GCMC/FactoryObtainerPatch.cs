using System.Reflection;
using DataLoader;
using Harmony;
using JetBrains.Annotations;
using RobocraftX.Blocks;
using RobocraftX.Blocks.GUI;
using RobocraftX.Common;
using RobocraftX.CR.MachineEditing;
using RobocraftX.StateSync;
using Svelto.ECS;
using Unity.Entities;
using UnityEngine;

namespace GCMC
{
    [HarmonyPatch]
    [UsedImplicitly]
    public class FactoryObtainerPatch
    {
        static void Postfix(BlockEntityFactory blockEntityFactory)
        {
            CubePlacerEngine._blockEntityFactory = blockEntityFactory;
            Debug.Log("Block entity factory injected.");
        }

        static MethodBase TargetMethod(HarmonyInstance instance)
        {
            return typeof(PlaceBlockEngine).GetConstructors()[0];
        }
    }
}