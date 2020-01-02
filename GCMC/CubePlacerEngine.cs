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
        }

        public IEntitiesDB entitiesDB { get; set; }
        private readonly BlockEntityFactory _blockEntityFactory; //Injected from PlaceBlockEngine - TODO

        private void ImportWorld(string name)
        {
            PlaceBlock(1, 1, 0);
        }

        private void PlaceBlock(ushort block, byte color, uint playerId)
        {
            BuildBlock(block, color).Init(new BlockPlacementInfoStruct()
            {
                loadedFromDisk = false,
                placedBy = playerId
            });
        }

        private EntityStructInitializer BuildBlock(ushort block, byte color)
        {
            //RobocraftX.CR.MachineEditing.PlaceBlockEngine
            ScalingEntityStruct scaling = new ScalingEntityStruct {scale = new float3(1, 1, 1)};
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
                blockPlacementHeight = 1, blockPlacementWidth = 1, desiredScaleFactor = 1, snapGridScale = 1,
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
            structInitializer.Init(new PositionEntityStruct {position = 0});
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

        public string name { get; }
    }
}