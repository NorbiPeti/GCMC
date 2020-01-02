using System;
using DataLoader;
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
        internal static BlockEntityFactory _blockEntityFactory; //Injected from PlaceBlockEngine

        private void ImportWorld(string name)
        {
            PlaceBlock(0, BlockColors.Default, 0, new float3(0, 0, 0), 5, 1, 1, 1, 0);
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
                uint playerId = 0;
                PlaceBlock(block, (BlockColors) color, darkness, new float3(x, y, z), scale, scaleX, scaleY, scaleZ, playerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }

        /// <summary>
        /// Places a block at the given position
        /// </summary>
        /// <param name="block">The block's type</param>
        /// <param name="color">The block's color</param>
        /// <param name="darkness">The block color's darkness - 0 is default color</param>
        /// <param name="position">The block's position - default block size is 0.2</param>
        /// <param name="scale">The block's uniform scale - default scale is 1 (with 0.2 width)</param>
        /// <param name="scaleX">The block's non-uniform scale - less than 1 means <paramref name="scale"/> is used</param>
        /// <param name="scaleY">The block's non-uniform scale - less than 1 means <paramref name="scale"/> is used</param>
        /// <param name="scaleZ">The block's non-uniform scale - less than 1 means <paramref name="scale"/> is used</param>
        /// <param name="playerId">The player who placed the block</param>
        /// <exception cref="Exception"></exception>
        private void PlaceBlock(ushort block, BlockColors color, byte darkness, float3 position, int scale, float scaleX, float scaleY, float scaleZ, uint playerId)
        {
            try
            {
                if (darkness > 9) throw new Exception("That is too dark. Make sure to use 0-9 as darkness. (0 is default.)");
                BuildBlock(block, (byte)color, position, scale, scaleX, scaleY, scaleZ).Init(new BlockPlacementInfoStruct()
                {
                    loadedFromDisk = false,
                    placedBy = playerId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(e.Message);
            }
        }

        private EntityStructInitializer BuildBlock(ushort block, byte color, float3 position, int scale, float scaleX, float scaleY, float scaleZ)
        {
            if (_blockEntityFactory == null)
                throw new Exception("The factory is null.");
            if (scale == 0)
                throw new Exception("Scale needs to be at least 1");
            if (Math.Abs(scaleX) < 1) scaleX = scale;
            if (Math.Abs(scaleY) < 1) scaleY = scale;
            if (Math.Abs(scaleZ) < 1) scaleZ = scale;
            //RobocraftX.CR.MachineEditing.PlaceBlockEngine
            ScalingEntityStruct scaling = new ScalingEntityStruct {scale = new float3(scaleX, scaleY, scaleZ)};
            RotationEntityStruct rotation = new RotationEntityStruct {rotation = quaternion.identity};
            GridRotationStruct gridRotation = new GridRotationStruct
                {position = float3.zero, rotation = quaternion.identity};
            CubeCategoryStruct category = new CubeCategoryStruct
                {category = CubeCategory.General, type = CubeType.Block};
            uint dbid = block;
            DBEntityStruct dbEntity = new DBEntityStruct {DBID = dbid};
            uint num = PrefabsID.DBIDMAP[dbid];
            GFXPrefabEntityStructGO gfx = new GFXPrefabEntityStructGO {prefabID = num};
            BlockPlacementScaleEntityStruct placementScale = new BlockPlacementScaleEntityStruct
            {
                blockPlacementHeight = scale, blockPlacementWidth = scale, desiredScaleFactor = scale, snapGridScale = scale,
                unitSnapOffset = 0, isUsingUnitSize = true
            };
            EquippedColourStruct colour = new EquippedColourStruct {indexInPalette = color};
            EGID egid2;
            switch (category.category)
            {
                case CubeCategory.SpawnPoint:
                case CubeCategory.BuildingSpawnPoint:
                    egid2 = MachineEditingGroups.NewSpawnPointBlockID;
                    break;
                default:
                    egid2 = MachineEditingGroups.NewBlockID;
                    break;
            }

            int cubeId = PrefabsID.GenerateDBID((ushort) category.category, block);
            EntityStructInitializer structInitializer = _blockEntityFactory.Build(egid2, (uint) cubeId); //The ghost block index is only used for triggers
            if (colour.indexInPalette != byte.MaxValue)
                structInitializer.Init(new ColourParameterEntityStruct
                {
                    indexInPalette = colour.indexInPalette
                });
            structInitializer.Init(new GFXPrefabEntityStructGPUI(gfx.prefabID));
            structInitializer.Init(new PhysicsPrefabEntityStruct(gfx.prefabID));
            structInitializer.Init(dbEntity);
            structInitializer.Init(new PositionEntityStruct {position = position});
            structInitializer.Init(rotation);
            structInitializer.Init(scaling);
            structInitializer.Init(gridRotation);
            structInitializer.Init(new UniformBlockScaleEntityStruct
            {
                scaleFactor = placementScale.desiredScaleFactor
            });
            return structInitializer;
        }

        public JobHandle SimulatePhysicsStep(in float deltaTime, in PhysicsUtility utility, in PlayerInput[] playerInputs)
        {
            return new JobHandle();
        }

        public string name { get; } = "Cube placer engine";

        enum BlockColors
        {
            Default = byte.MaxValue,
            White = 0,
            Pink,
            Purple,
            Blue,
            Aqua,
            Green,
            Lime,
            Yellow,
            Orange,
            Red
        }
    }
}