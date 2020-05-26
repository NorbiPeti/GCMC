using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Engines;
using GamecraftModdingAPI.Utility;
using Newtonsoft.Json;
using RobocraftX.Common;
using Svelto.ECS;
using Unity.Mathematics;
using uREPL;

namespace GCMC
{
    public class CubePlacerEngine : IApiEngine
    {
        public void Ready()
        {
            RuntimeCommands.Register<string>("importWorld", ImportWorld, "Imports a Minecraft world.");
            _serializer.TraceWriter = _traceWriter;
        }

        public EntitiesDB entitiesDB { get; set; }

        private readonly Dictionary<string, BlockType> mapping = new Dictionary<string, BlockType>(10);
        private JsonSerializer _serializer = JsonSerializer.Create();
        private JsonTraceWriter _traceWriter = new JsonTraceWriter();

        private async void ImportWorld(string name)
        {
            try
            {
                Log.Output("Reading block mappings...");
                var parser = new IniParser.FileIniDataParser();
                var ini = parser.ReadFile("BlockTypes.ini");
                mapping.Clear();
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

                Log.Output("Reading file...");
                Blocks[] blocksArray = null;
                //Console.WriteLine("Outside thread: " + Thread.CurrentThread.ManagedThreadId);
                //Console.WriteLine("Outside context: " + SynchronizationContext.Current);
                await Task.Run(() =>
                {
                    //Console.WriteLine("Inside thread: " + Thread.CurrentThread.Name);
                    var fs = File.OpenText(name);
                    _traceWriter.FileLength = ((FileStream) fs.BaseStream).Length;
                    blocksArray = _serializer.Deserialize<Blocks[]>(new JsonTextReader(fs));
                });
                //Console.WriteLine("After thread: " + Thread.CurrentThread.ManagedThreadId);
                Log.Output("Placing blocks...");
                int i;
                uint start = BlockIdentifiers.LatestBlockID + 1;
                uint end = start;
                for (i = 0; i < blocksArray.Length; i++)
                {
                    //if (i % 5000 == 0) await AsyncUtils.WaitForSubmission();
                    var blocks = blocksArray[i];
                    if (!mapping.TryGetValue(blocks.Material, out var type))
                    {
                        Console.WriteLine("Unknown block: " + blocks.Material);
                        continue;
                    }

                    if (type.Type == BlockIDs.Invalid) continue;

                    end = Block.PlaceNew(type.Type, (blocks.Start + blocks.End) / 10 * 3, color: type.Color.Color,
                        darkness: type.Color.Darkness, scale: (blocks.End - blocks.Start + 1) * 3,
                        rotation: float3.zero).Id.entityID;
                }

                await AsyncUtils.WaitForSubmission();
                var conns = entitiesDB.QueryEntities<GridConnectionsEntityStruct>(CommonExclusiveGroups
                    .OWNED_BLOCKS_GROUP);
                Log.Output("Start: " + start + " - End: " + end);
                Log.Output("conn: " + conns[start].isIsolator + " " + conns[start].isProcessed + " " +
                           conns[start].areConnectionsAssigned + " " + conns[start].ID + " " +
                           conns[start].machineRigidBodyId);
                for (uint j = start; j <= end; j++)
                {
                    conns[j].isProcessed = false;
                    conns[j].areConnectionsAssigned = false;
                    conns[j].isIsolator = false;
                }

                Log.Output(i + " blocks placed.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }

        public void Dispose()
        {
        }

        public string Name { get; } = "GCMCCubePlacerEngine";
        public bool isRemovable { get; } = false;
    }
}