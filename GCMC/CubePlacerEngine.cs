using System;
using DataLoader;
using GamecraftModdingAPI.Blocks;
using RobocraftX.Blocks;
using RobocraftX.Blocks.Ghost;
using RobocraftX.Blocks.Scaling;
using RobocraftX.Character;
using RobocraftX.CommandLine.Custom;
using RobocraftX.Common;
using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.CR.MachineEditing;
using RobocraftX.StateSync;
using Svelto.ECS;
using Svelto.ECS.EntityStructs;
using Unity.Jobs;
using Unity.Mathematics;
using uREPL;

namespace GCMC
{
    public class CubePlacerEngine : IQueryingEntitiesEngine, IDeterministicSim
    {
        public void Ready()
        {
            RuntimeCommands.Register<string>("importWorld", ImportWorld, "Imports a Minecraft world.");
            RuntimeCommands.Register<string>("placeCube", PlaceBlock, "Places a cube.");
        }

        public IEntitiesDB entitiesDB { get; set; }

        private void ImportWorld(string name)
        {
        }

        private void PlaceBlock(string args)
        {
            try
            {
                var s = args.Split(' ');
                ushort block = ushort.Parse(s[0]);
                byte color = byte.Parse(s[1]);
                byte darkness = byte.Parse(s[2]);
                float x = float.Parse(s[3]), y = float.Parse(s[4]), z = float.Parse(s[5]);
                int scale = int.Parse(s[6]);
                float scaleX = float.Parse(s[7]);
                float scaleY = float.Parse(s[8]);
                float scaleZ = float.Parse(s[9]);
                float rotX = float.Parse(s[10]);
                uint playerId = 0;
                Placement.PlaceBlock((BlockIDs) block, new float3(x, y, z), new quaternion(rotX, 0, 0, 1),
                    (BlockColors) color, darkness, scale, new float3(scaleX, scaleY, scaleZ), playerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }

        public JobHandle SimulatePhysicsStep(in float deltaTime, in PhysicsUtility utility, in PlayerInput[] playerInputs)
        {
            return new JobHandle();
        }

        public string name { get; } = "Cube placer engine";
    }
}