using System;
using System.IO;
using DataLoader;
using GamecraftModdingAPI.Blocks;
using Newtonsoft.Json;
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
            RuntimeCommands.Register("placedBy", GetPlacedBy, "Gets who placed a block.");
        }

        private void GetPlacedBy()
        {
            try
            {
                var placementInfo =
                    entitiesDB.QueryEntity<BlockPlacementInfoStruct>(new EGID(BlockIdentifiers.LatestBlockID,
                        BlockIdentifiers.OWNED_BLOCKS));
                Log.Output("Placed by: " + placementInfo.placedBy);
                Log.Output("Loaded from disk: " + placementInfo.loadedFromDisk);
            }
            catch (Exception e)
            {
                Log.Error("Failed to get who placed the block.");
                Console.WriteLine("Error getting who placed the block:\n" + e);
            }
        }

        public IEntitiesDB entitiesDB { get; set; }

        private void ImportWorld(string name)
        {
            try
            {
                Log.Output("Starting...");
                var blocksArray = JsonSerializer.Create()
                    .Deserialize<Blocks[]>(new JsonTextReader(File.OpenText(name)));
                int C = 0;
                foreach (var blocks in blocksArray)
                {
                    BlockIDs id;
                    BlockColors color;
                    byte darkness = 0;
                    switch (blocks.Material)
                    {
                        case "DIRT":
                            id = BlockIDs.DirtCube;
                            color = BlockColors.Default;
                            break;
                        case "GRASS":
                            id = BlockIDs.GrassCube;
                            color = BlockColors.Default;
                            break;
                        case "STONE":
                        case "COAL_ORE":
                        case "IRON_ORE":
                            id = BlockIDs.ConcreteCube;
                            color = BlockColors.White;
                            darkness = 5;
                            break;
                        case "LEAVES":
                            id = BlockIDs.AluminiumCube;
                            color = BlockColors.Green;
                            darkness = 5;
                            break;
                        case "AIR":
                            continue;
                        case "LOG":
                            id = BlockIDs.WoodCube;
                            color = BlockColors.Default;
                            break;
                        case "WATER":
                            id = BlockIDs.AluminiumCube;
                            color = BlockColors.Blue;
                            break;
                        case "SAND":
                            id = BlockIDs.AluminiumCube;
                            color = BlockColors.Yellow;
                            break;
                        default:
                            Console.WriteLine("Unknown block: " + blocks.Material);
                            continue;
                    }

                    Placement.PlaceBlock(id, (blocks.Start + blocks.End) / 10 * 3, color: color, darkness: darkness,
                        scale: (blocks.End - blocks.Start + 1) * 3, rotation: float3.zero);
                    C++;
                }

                Log.Output(C + " blocks placed.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
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
                float rotY = float.Parse(s[11]);
                float rotZ = float.Parse(s[12]);
                uint playerId = 0;
                Placement.PlaceBlock((BlockIDs) block, new float3(x, y, z), new float3(rotX, rotY, rotZ),
                    (BlockColors) color, darkness, scale, new float3(scaleX, scaleY, scaleZ), playerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }

        public JobHandle SimulatePhysicsStep(in float deltaTime, in PhysicsUtility utility,
            in PlayerInput[] playerInputs)
        {
            return new JobHandle();
        }

        public string name { get; } = "Cube placer engine";
    }
}