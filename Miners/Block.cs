using System;
using System.Collections.Generic;

namespace Miners
{
    /// <summary>
    /// Represents single map element (cube).
    /// </summary>
    public class Block : MapObject
    {
        public enum TerrainType
        {
            Rock,
            Empty,
            Water,
            Gold,
            Grass,
            Solid,
            Cave
        };

        /// <summary>
        /// Current block type.
        /// </summary>
        protected TerrainType type = TerrainType.Rock;

        /// <summary>
        /// List of map objects standing in this block.
        /// </summary>
        protected HashSet<MapObject> mapObjects = new HashSet<MapObject>();

        /// <summary>
        /// Ammount of time needed to flood this block by water.
        /// </summary>
        protected Timer waterFloodTimer = new Timer(2.0f, 0.0f, false);

        public TerrainType Type
        {
            get { return type; }
            set { type = value; }
        }

        public HashSet<MapObject> MapObjects
        {
            get { return mapObjects; }
        }

        public float WaterLevel
        {
            get { return Type == TerrainType.Water ? waterFloodTimer.Current / waterFloodTimer.Maximal : 1.0f; }
        }

        public bool Flooded
        {
            get { return waterFloodTimer.Completed; }

            set
            {
                Type = TerrainType.Water;

                state = State.Idle;

                waterFloodTimer.Completed = true;
            }
        }

        /// <summary>
        /// Returns true if top wall is not present.
        /// </summary>
        public bool Open
        {
            get { return Type == Block.TerrainType.Empty || Type == Block.TerrainType.Grass; }
        }

        /// <summary>
        /// Returns true if top wall is present.
        /// </summary>
        public bool Closed
        {
            get { return Open == false; }
        }

        public Block(string name, Scene scene, Map map, TerrainType type = TerrainType.Rock)
            : base(name, scene, map)
        {
            this.type = type;
        }

        public override void Update(float time)
        {
            base.Update(time);

            // Flood adjacent blocks if they are empty.
            if (waterFloodTimer.Completed)
            {
                // Drown all map objects on this block.
                DestroyMapObjects<MapObject>();

                // Flood adjacent blocks if they are empty.
                foreach (Coordinates coordinates in Coordinates.Directions)
                {
                    Block block = map.GetBlock(Position + coordinates);

                    if (block != null && block.IsFloodable())
                    {
                        block.StartFlooding();
                    }
                }
            }

            // Process block state.
            switch (state)
            {
                // Flood.
                case State.Flooding:
                    Flood(time);
                    break;
            }
        }

        public void Assign(MapObject mapObject)
        {
            mapObjects.Add(mapObject);
        }

        public void Remove(MapObject mapObject)
        {
            mapObjects.Remove(mapObject);
        }

        public bool IsCave()
        {
            return Type == TerrainType.Cave;
        }

        /// <summary>
        /// Returns true if block can be digged by miner.
        /// </summary>
        public bool IsDiggable()
        {
            return Type == TerrainType.Gold || Type == TerrainType.Rock;
        }

        /// <summary>
        /// Returns true if block can be flooded by water.
        /// </summary>
        public bool IsFloodable()
        {
            return Type == TerrainType.Empty || Type == TerrainType.Grass;
        }

        /// <summary>
        /// Returns true if map object can be placed on this block.
        /// </summary>
        public bool IsPassable()
        {
            return Type == TerrainType.Empty || Type == TerrainType.Grass || Type == TerrainType.Water;
        }

        /// <summary>
        /// Returns true, if block is occupied by other object
        /// </summary>
        /// <param name="mapObject">Exclude this map object from search.</param>
        public bool IsOccupied(MapObject mapObject = null)
        {
            // If block contains 0 objects, it is not occupied.
            if (mapObjects.Count == 0)
            {
                return false;
            }

            // If block contains 1 object and it is given object, then it is not occupied.
            // If block contains 1 object and it is not given object, then block is occupied.
            if (mapObjects.Count == 1)
            {
                if (mapObject == null)
                {
                    return true;
                }

                if (mapObjects.Contains(mapObject))
                {
                    return false;
                }

                return true;
            }

            // If block contains more than 1 object, then it is occupied.
            return true;
        }

        public void StartFlooding()
        {
            state = State.Flooding;
            Type = TerrainType.Water;
        }

        /// <summary>
        /// Updates water level and drowns all map objects assigned to this block.
        /// </summary>
        /// <param name="time">Time elapsed from last update cycle.</param>
        protected void Flood(float time)
        {
            // Drown all map objects on this block.
            DestroyMapObjects<MapObject>();

            if (waterFloodTimer.Update(time))
            {
                state = State.Idle;
            }
        }

        /// <summary>
        /// Destroys all map objects of given type assigned to this block.
        /// </summary>
        /// <typeparam name="Type">Type of map object to destroy.</typeparam>
        public void DestroyMapObjects<Type>()
        {
            mapObjects.RemoveWhere(delegate(MapObject mapObject)
            {
                if (mapObject is Type)
                {
                    mapObject.Destroy();

                    return true;
                }

                return false;
            });
        }

        public void StopFlooding()
        {
            state = State.Idle;
        }

        /// <summary>
        /// Changes all cave blocks to given type.
        /// </summary>
        /// <param name="type">Cave block will be change to this type.</param>
        protected void FillCave(TerrainType type)
        {
            // Change block type.
            if (type == TerrainType.Water)
            {
                Flooded = true;
            }
            else
            {
                Type = type;
            }

            // Find adjacent blocks. If they are also caves, change their types too.
            foreach (Coordinates coordinates in Coordinates.Directions)
            {
                Block adjacent = map.GetBlock(position + coordinates);

                if (adjacent != null && adjacent.IsCave())
                {
                    adjacent.FillCave(type);
                }
            }
        }

        /// <summary>
        /// Digs block. When block digging is finished, fills adjacent caves with random type.
        /// </summary>
        public void Dig()
        {
            Type = TerrainType.Empty;

            // Search adjacent blocks for caves.
            List<Block> adjacentBlocks = new List<Block>();

            foreach (Coordinates coordinates in Coordinates.Directions)
            {
                Block adjacent = map.GetBlock(position + coordinates);

                if (adjacent != null && adjacent.IsCave())
                {
                    adjacentBlocks.Add(adjacent);
                }
            }

            // If caves are found, uncover them.
            if (adjacentBlocks.Count > 0)
            {
                // If random number is equal to 0, 1 or 3 we set cave to empty, gold or water.
                // If random numer is equal to 4, we set cave to Empty and place monster inside.
                TerrainType[] types = new TerrainType[3];

                types[0] = TerrainType.Empty;
                types[1] = TerrainType.Gold;
                types[2] = TerrainType.Water;

                int caveTypeNumber = Generator.Next(types.Length + 1);
                TerrainType randomCaveType = TerrainType.Empty;

                if (caveTypeNumber == 3)
                {
                    map.CreateMonster(Position);
                }
                else
                {
                    randomCaveType = types[caveTypeNumber];
                }

                foreach (Block block in adjacentBlocks)
                {
                    block.FillCave(randomCaveType);
                }
            }
        }

        /// <summary>
        /// Returns true if structure can be placed on this block.
        /// </summary>
        public bool CanPlaceStructure()
        {
            return Type == TerrainType.Empty && IsOccupied() == false;
        }
    }
}
