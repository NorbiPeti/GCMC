using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using HarmonyLib;
using Newtonsoft.Json;
using RobocraftX.Common.Input;
using RobocraftX.Common.Utilities;
using RobocraftX.StateSync;
using Svelto.ECS;
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

        private Dictionary<string, BlockType> mapping = new Dictionary<string, BlockType>(10);
        private Blocks[] blocksArray;
        private int C;
        private bool state;
        private string filename;

        private void ImportWorld(string name)
        {
            try
            {
                filename = name;
                Log.Output("Reading block mappings...");
                var parser = new IniParser.FileIniDataParser();
                var ini = parser.ReadFile("BlockTypes.ini");
                mapping.Clear();
                C = 0;
                blocksArray = null;
                state = false;
                foreach (var section in ini.Sections)
                {
                    var mcblocks = section.SectionName.Split(',');
                    BlockIDs type;
                    if (section.Keys["type"] == null)
                    {
                        if (section.Keys["ignore"] != "true")
                        {
                            Log.Warn("Block type not specified for " + section.SectionName);
                            continue;
                        }

                        type = BlockIDs.Invalid;
                    }
                    else if (!Enum.TryParse(section.Keys["type"], out type))
                    {
                        Log.Warn("Block type specified in ini not found: " + section.Keys["type"]);
                        continue;
                    }

                    BlockColors color;
                    if (section.Keys["color"] == null)
                        color = BlockColors.Default;
                    else if (!Enum.TryParse(section.Keys["color"], out color))
                    {
                        Log.Warn("Block color specified in ini not found: " + section.Keys["color"]);
                        continue;
                    }

                    byte darkness;
                    if (section.Keys["darkness"] == null)
                        darkness = 0;
                    else if (!byte.TryParse(section.Keys["darkness"], out darkness) || darkness > 9)
                    {
                        Log.Warn("Block darkness specified in ini isn't a number between 0 and 9: " +
                                 section.Keys["darkness"]);
                        continue;
                    }

                    foreach (var mcblock in mcblocks)
                    {
                        mapping.Add(mcblock.ToUpper(), new BlockType
                        {
                            Material = mcblock.ToUpper(),
                            Type = type,
                            Color = new BlockColor {Color = color, Darkness = darkness}
                        });
                    }
                }

                Log.Output("Starting...");
                Task.Run(() =>
                {
                    blocksArray = JsonSerializer.Create()
                        .Deserialize<Blocks[]>(new JsonTextReader(File.OpenText(filename)));
                });
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
            state = !state;
            if (blocksArray == null || C >= blocksArray.Length
                                    || state) return default;
            int i;
            for (i = C; i < C + 10000 && i < blocksArray.Length; i++)
            {
                var blocks = blocksArray[i];
                if (!mapping.TryGetValue(blocks.Material, out var type))
                {
                    Console.WriteLine("Unknown block: " + blocks.Material);
                    continue;
                }

                if (type.Type == BlockIDs.Invalid) continue;

                Block.PlaceNew(type.Type, (blocks.Start + blocks.End) / 10 * 3, color: type.Color.Color,
                    darkness: type.Color.Darkness, scale: (blocks.End - blocks.Start + 1) * 3,
                    rotation: float3.zero);
            }

            AccessTools.Method(typeof(Block), "Sync").Invoke(null, new object[0]);
            C = i;
            Log.Output(C + " blocks placed.");
            return new JobHandle();
        }

        public string name { get; } = "Cube placer engine";
    }
}