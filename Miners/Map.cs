using System;
using System.Collections.Generic;

namespace Miners
{
    /// <summary>
    /// Represents map (game level). Map consists of cube blocks.
    /// </summary>
    public class Map : GameObject
    {
        /// <summary>
        /// Width and height of map.
        /// </summary>
        protected Coordinates size = Coordinates.Zero;

        /// <summary>
        /// Size of one map block.
        /// </summary>
        protected float blockSize = 0.0f;

        /// <summary>
        /// Random number generator.
        /// </summary>
        protected Random random = null;

        /// <summary>
        /// Block array representing entire level.
        /// </summary>
        protected Block[,] blocks = null;

        /// <summary>
        /// List of all map objects placed on this map.
        /// </summary>
        protected List<MapObject> mapObjects = new List<MapObject>();

        protected Mogre.ManualObject manualObject = null;

        public Coordinates Center
        {
            get { return new Coordinates(size.X / 2, size.Y / 2, size.Z / 2); }
        }

        public float BlockSize
        {
            get { return blockSize; }
        }

        public Block[,] Blocks
        {
            get { return blocks; }
        }

        public Mogre.ManualObject ManualObject
        {
            get { return manualObject; }
        }

        public Coordinates Size
        {
            get { return size; }
        }

        public List<MapObject> MapObjects
        {
            get { return mapObjects; }
        }

        public Map(string name, Scene scene, Coordinates size, float blockSize)
            : base(name, scene)
        {
            this.size = size;
            this.blockSize = blockSize;
            this.random = new Random();
        }

        public override void Load()
        {
            base.Load();

            // Create blocks.
            blocks = new Block[size.X, size.Z];

            // Fill all blocks with rocks.
            for (int z = 0; z < size.Z; ++z)
            {
                for (int x = 0; x < size.X; ++x)
                {
                    Blocks[x, z] = new Block(string.Format("Block {0}:{1}", x, z), scene, this);
                    Blocks[x, z].Position = new Coordinates(x, 0, z);
                }
            }

            // Create solid border.
            for (int z = 0; z < size.Z; ++z)
            {
                for (int x = 0; x < size.X; ++x)
                {
                    if (z == 0 || z == size.Z - 1 || x == 0 || x == size.X - 1)
                    {
                        blocks[x, z].Type = Block.TerrainType.Solid;
                    }
                }
            }

            // Create manual objects.
            manualObject = scene.CreateManualObject(name);
            manualObject.UserObject = this;

            // Generate grid.
            Generate();

            // Create scene node.
            sceneNode = scene.CreateSceneNode(name);
            sceneNode.AttachObject(manualObject);
        }

        public override void Unload()
        {
            base.Unload();

            blocks = null;

            scene.DestroyManualObject(manualObject);
            scene.DestroySceneNode(sceneNode);

            manualObject = null;
            sceneNode = null;

            foreach (MapObject mapObject in mapObjects)
            {
                mapObject.Unload();
            }

            mapObjects.Clear();
        }

        public override void Update(float time)
        {
            base.Update(time);

            // Update each block.
            for (int z = 0; z < size.Z; ++z)
            {
                for (int x = 0; x < size.X; ++x)
                {
                    Blocks[x, z].Update(time);
                }
            }

            // Update each map object.
            foreach (MapObject mapObject in mapObjects)
            {
                mapObject.Update(time);
            }

            Generate();
        }

        public void CreateMonster(Coordinates position)
        {
            // Get current monsters count.
            int count = mapObjects.Count;

            // Create new monster at given position.
            Monster monster = new Monster(string.Format("Monster #{0}", count + 1), scene, this);
            monster.Position = position;
            monster.Load();

            // Add monster to list.
            mapObjects.Add(monster);
        }

        public void CreateTrap(Coordinates position)
        {
            // Get current monsters count.
            int count = mapObjects.Count;

            // Create new monster at given position.
            Trap trap = new Trap(string.Format("Trap #{0}", count + 1), scene, this);
            trap.Position = position;
            trap.Load();

            // Add monster to list.
            mapObjects.Add(trap);
        }

        public void GenerateRandom(int maxCount, int maxSize, Block.TerrainType type)
        {
            for (int i = 0; i < maxCount; ++i)
            {
                // Get start point for first cave. Cave must be placed on rock block and should not stick to any non-rock blocks.
                int x = 0;
                int z = 0;

                do
                {
                    x = random.Next(size.X);
                    z = random.Next(size.Z);
                }
                while (ValidatePositionForCave(new Coordinates(x, 0, z), type) == false);

                for (int j = 0; j < maxSize; ++j)
                {
                    // Change x or z coordinate.
                    int xx = x;
                    int zz = z;

                    // Get next cave block.
                    if (random.Next(2) == 0)
                    {
                        xx = x + random.Next(3) - 1;
                    }
                    else
                    {
                        zz = z + random.Next(3) - 1;
                    }

                    if (xx < 1)
                    {
                        xx = 1;
                    }
                    else if (xx > size.X - 2)
                    {
                        xx = size.X - 2;
                    }

                    if (zz < 1)
                    {
                        zz = 1;
                    }
                    else if (zz > size.Z - 2)
                    {
                        zz = size.Z - 2;
                    }

                    // If given or adjacent blocks are not rock, skip this block.
                    if (ValidatePositionForCave(new Coordinates(xx, 0, zz), type) == false)
                    {
                        continue;
                    }

                    Blocks[xx, zz].Type = type;

                    if (type == Block.TerrainType.Water)
                    {
                        Blocks[xx, zz].Flooded = true;
                    }

                    x = xx;
                    z = zz;
                }
            }
        }

        protected bool ValidatePositionForCave(Coordinates position, Block.TerrainType caveType)
        {
            // Check given position.
            Block block = GetBlock(position);

            if (block == null)
            {
                return false;
            }

            if (block.Type != Block.TerrainType.Rock && block.Type != caveType)
            {
                return false;
            }

            // Check adjacent positions.
            foreach (Coordinates coordinates in Coordinates.Compass)
            {
                Block adjacent = GetBlock(position + coordinates);

                if (adjacent == null)
                {
                    return false;
                }

                if (adjacent.Type != Block.TerrainType.Rock && adjacent.Type != caveType)
                {
                    return false;
                }
            }

            return true;
        }

        public void Merge(Block[,] blocks, Coordinates position)
        {
            for (int z = 0; z <= blocks.GetUpperBound(0); ++z)
            {
                for (int x = 0; x <= blocks.GetUpperBound(1); ++x)
                {
                    Blocks[position.X + x, position.Z + z].Type = blocks[x, z].Type;
                }
            }
        }

        public Block GetBlock(Coordinates position)
        {
            if (IsPositionInside(position))
            {
                return Blocks[position.X, position.Z];
            }

            return null;
        }

        public bool IsPositionInside(Coordinates position)
        {
            if (position.X < 0)
            {
                return false;
            }

            if (position.X > Size.X - 1)
            {
                return false;
            }

            if (position.Z < 0)
            {
                return false;
            }

            if (position.Z > Size.Z - 1)
            {
                return false;
            }

            return true;
        }

        public bool UpdateBlock(Block.TerrainType type, Coordinates position)
        {
            Block block = GetBlock(position);

            if (block == null)
            {
                return false;
            }

            // Allow placing solid blocks only on passable (non-water) blocks.
            if (type == Block.TerrainType.Solid && (block.Type == Block.TerrainType.Water || block.IsPassable() == false || block.IsOccupied()))
            {
                return false;
            }

            block.Type = type;

            return true;
        }

        protected void Generate()
        {
            manualObject.Clear();
            manualObject.Begin("Blocks", Mogre.RenderOperation.OperationTypes.OT_TRIANGLE_LIST);

            // Set texture coords.
            float[,] texCoords = new float[Enum.GetValues(typeof(Block.TerrainType)).Length, 2];

            texCoords[0, 0] = 0.000f;
            texCoords[0, 1] = 0.125f;

            texCoords[1, 0] = 0.125f;
            texCoords[1, 1] = 0.250f;

            texCoords[2, 0] = 0.250f;
            texCoords[2, 1] = 0.375f;

            texCoords[3, 0] = 0.375f;
            texCoords[3, 1] = 0.500f;

            texCoords[4, 0] = 0.500f;
            texCoords[4, 1] = 0.625f;

            texCoords[5, 0] = 0.625f;
            texCoords[5, 1] = 0.750f;

            texCoords[6, 0] = 0.750f;
            texCoords[6, 1] = 0.875f;

            // Create vertices. Lower and upper.
            /*
            for (int z = 0; z <= size.Z; ++z)
            {
                for (int x = 0; x <= size.X; ++x)
                {
                    manualObject.Position(x * BlockSize, 0.0f, z * BlockSize);
                    manualObject.Normal(Mogre.Vector3.UNIT_Y);
                    manualObject.TextureCoord(0.0f, 1.0f);

                    manualObject.Position(x * BlockSize, BlockSize, z * BlockSize);
                    manualObject.Normal(Mogre.Vector3.NEGATIVE_UNIT_Y);
                    manualObject.TextureCoord(0.5f, 1.0f);
                }
            }
            //*/

            // Create indices.
            for (int z = 0; z < size.Z; ++z)
            {
                for (int x = 0; x < size.X; ++x)
                {
                    /*
                    // Get block type.
                    int blockType = (int)Blocks[x, z].Type;

                    // Bottom.
                    if (GetBlock(new Coordinates(x, 0, z)).Open)
                    {
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 0);

                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 1)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 0);
                    }

                    // Top.
                    if (GetBlock(new Coordinates(x, 0, z)).Closed)
                    {
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 1);

                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 1)) * 2 + 1);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 1);
                    }

                    // Left.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.West).Closed)
                    {
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 0);

                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 0);
                    }

                    // Right.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.East).Closed)
                    {
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 0);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 1)) * 2 + 1);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 1);

                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 0);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 1)) * 2 + 0);
                        manualObject.Index((uint)((z + 1) * (size.Z + 1) + (x + 1)) * 2 + 1);
                    }

                    // Front.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.North).Closed)
                    {
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 1);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 1);

                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 0)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 0);
                        manualObject.Index((uint)((z + 0) * (size.Z + 1) + (x + 1)) * 2 + 1);
                    }
                    //*/

                    //*
                    int blockType = (int)Blocks[x, z].Type;

                    manualObject.Position((x + 0) * BlockSize, 0.0f, (z + 0) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 0], 0.0f);

                    manualObject.Position((x + 0) * BlockSize, 0.0f, (z + 1) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 0], 0.125f);

                    manualObject.Position((x + 1) * BlockSize, 0.0f, (z + 1) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 1], 0.125f);

                    manualObject.Position((x + 1) * BlockSize, 0.0f, (z + 0) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 1], 0.0f);

                    float blockHeight = BlockSize * Blocks[x, z].WaterLevel;

                    manualObject.Position((x + 0) * BlockSize, blockHeight, (z + 0) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.NEGATIVE_UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 0], 0.0f);

                    manualObject.Position((x + 0) * BlockSize, blockHeight, (z + 1) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.NEGATIVE_UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 0], 0.125f);

                    manualObject.Position((x + 1) * BlockSize, blockHeight, (z + 1) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.NEGATIVE_UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 1], 0.125f);

                    manualObject.Position((x + 1) * BlockSize, blockHeight, (z + 0) * BlockSize);
                    manualObject.Normal(Mogre.Vector3.NEGATIVE_UNIT_Y);
                    manualObject.TextureCoord(texCoords[blockType, 1], 0.0f);

                    uint offset = (uint)(z * size.X + x) * 8;

                    // Bottom.
                    if (GetBlock(new Coordinates(x, 0, z)).Open)
                    {
                        manualObject.Index(offset + 0);
                        manualObject.Index(offset + 1);
                        manualObject.Index(offset + 3);

                        manualObject.Index(offset + 1);
                        manualObject.Index(offset + 2);
                        manualObject.Index(offset + 3);
                    }

                    // Top.
                    if (GetBlock(new Coordinates(x, 0, z)).Closed)
                    {
                        manualObject.Index(offset + 4);
                        manualObject.Index(offset + 5);
                        manualObject.Index(offset + 7);

                        manualObject.Index(offset + 5);
                        manualObject.Index(offset + 6);
                        manualObject.Index(offset + 7);
                    }

                    /*
                    // Left.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.West).Closed)
                    {
                        manualObject.Index(offset + 4);
                        manualObject.Index(offset + 5);
                        manualObject.Index(offset + 1);

                        manualObject.Index(offset + 4);
                        manualObject.Index(offset + 1);
                        manualObject.Index(offset + 0);
                    }

                    // Right.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.East).Closed)
                    {
                        manualObject.Index(offset + 2);
                        manualObject.Index(offset + 6);
                        manualObject.Index(offset + 7);

                        manualObject.Index(offset + 3);
                        manualObject.Index(offset + 2);
                        manualObject.Index(offset + 7);
                    }

                    // Front.
                    if (GetBlock(new Coordinates(x, 0, z)).Open && GetBlock(new Coordinates(x, 0, z) + Coordinates.North).Closed)
                    {
                        manualObject.Index(offset + 4);
                        manualObject.Index(offset + 0);
                        manualObject.Index(offset + 7);

                        manualObject.Index(offset + 7);
                        manualObject.Index(offset + 0);
                        manualObject.Index(offset + 3);
                    }
                    //*/
                }
            }

            manualObject.End();
        }
    }
}
