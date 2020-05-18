using System;
using System.IO;
using DataLoader;
using GamecraftModdingAPI;
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
    public class CubePlacerEngine : IQueryingEntitiesEngine, IDeterministicTimeStopped
    {
        public void Ready()
        {
            RuntimeCommands.Register<string>("importWorld", ImportWorld, "Imports a Minecraft world.");
        }

        public EntitiesDB entitiesDB { get; set; }

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
                        case "DOUBLE_PLANT":
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
                        case "LONG_GRASS":
                            id = BlockIDs.Flower1;
                            color = BlockColors.Default;
                            break;
                        case "YELLOW_FLOWER":
                            id = BlockIDs.Flower2;
                            color = BlockColors.Default;
                            break;
                        case "GRAVEL":
                            id = BlockIDs.ConcreteCube;
                            color = BlockColors.White;
                            darkness = 7;
                            break;
                        case "CLAY":
                            id = BlockIDs.ConcreteCube;
                            color = BlockColors.White;
                            darkness = 4;
                            break;
                        default:
                            Console.WriteLine("Unknown block: " + blocks.Material);
                            continue;
                    }

                    Block.PlaceNew(id, (blocks.Start + blocks.End) / 10 * 3, color: color, darkness: darkness,
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

        public JobHandle SimulatePhysicsStep(in float deltaTime, in PhysicsUtility utility, in PlayerInput[] playerInputs)
        {
            return new JobHandle();
        }

        public string name { get; } = "Cube placer engine";
    }
}