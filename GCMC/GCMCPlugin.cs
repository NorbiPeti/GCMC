using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GamecraftModdingAPI;
using GamecraftModdingAPI.Blocks;
using GamecraftModdingAPI.Commands;
using IllusionPlugin;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using uREPL;

namespace GCMC
{
    public class GCMCPlugin : IPlugin
    {
        public string Name { get; } = "GCMC";
        public string Version { get; } = "v0.0.1";

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
                await Task.Run(() =>
                {
                    var fs = File.OpenText(name);
                    _traceWriter.FileLength = ((FileStream) fs.BaseStream).Length;
                    blocksArray = _serializer.Deserialize<Blocks[]>(new JsonTextReader(fs));
                });
                Log.Output("Placing blocks...");
                int i;
                for (i = 0; i < blocksArray.Length; i++)
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

                Log.Output(i + " blocks placed.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }
        
        public void OnApplicationStart()
        {
            GamecraftModdingAPI.Main.Init();
            CommandBuilder.Builder("importWorld", "Imports a Minecraft world.")
                .Action<string>(ImportWorld).Build();
            _serializer.TraceWriter = _traceWriter;

            Debug.Log("GCMC loaded");
        }

        public void OnApplicationQuit()
        {
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