using System.Reflection;
using DataLoader;
using Harmony;
using JetBrains.Annotations;
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
    public class PlaceBlockPatch
    {
        static void Postfix(EnginesRoot enginesRoot, ref StateSyncRegistrationHelper stateSyncReg, bool isAuthoritative)
        {
            if (isAuthoritative)
            {
                stateSyncReg.AddEngine(new CubePlacerEngine());
                Debug.Log($"Added Minecraft world import engine");
            }
            else
                Debug.Log("Not authoritative, not adding MC engine");
        }

        static MethodBase TargetMethod(HarmonyInstance instance)
        {
            return _ComposeMethodInfo(MachineEditingCompositionRoot.StateSyncCompose);
        }

        private delegate void ComposeAction(EnginesRoot enginesRoot,
            IDataDB dataDB,
            RCXMode currentMode,
            World physicsWorld,
            ref StateSyncRegistrationHelper stateSyncReg,
            bool isAuthoritative,
            LabelResourceManager labelResourceManager,
            LabelResourceManager textBlockLabelResourceManager,
            MainGameOptions.Options mainGameOptions);
        private static MethodInfo _ComposeMethodInfo(ComposeAction a)
        {
            return a.Method;
        }
    }
}