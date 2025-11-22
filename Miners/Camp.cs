using System.Collections.Generic;

namespace Miners
{
    /// <summary>
    /// Represents camp which creates miners.
    /// </summary>
    public class Camp : Structure
    {
        /// <summary>
        /// Time needed to create next miner.
        /// </summary>
        protected Timer minerCreateTimer = new Timer(2.0f);

        /// <summary>
        /// Maximal number of alive miners on the map.
        /// </summary>
        protected int maxAliveMinersCount = 20;

        /// <summary>
        /// List of created miners.
        /// </summary>
        protected List<Miner> miners = new List<Miner>();

        /// <summary>
        /// Total number of points acquired by this camp.
        /// </summary>
        protected int points = 0;

        /// <summary>
        /// Amount of gold owned by this camp.
        /// </summary>
        protected int gold = 0;

        public List<Miner> Miners
        {
            get { return miners; }
        }

        public int Points
        {
            get { return points; }
        }

        public int Gold
        {
            get { return gold; }
        }

        public Camp(string name, Scene scene, Map map)
            : base(name, scene, map)
        {
        }

        public override void Load()
        {
            base.Load();

            Blocks = new Block[3, 3];

            for (int z = 0; z <= blocks.GetUpperBound(0); ++z)
            {
                for (int x = 0; x <= blocks.GetUpperBound(1); ++x)
                {
                    Blocks[x, z] = new Block(string.Format("Camp block {0}:{1}", x, z), scene, map, Block.TerrainType.Grass);
                }
            }

            entity = scene.CreateEntity(name, "tudorhouse.mesh");
            entity.UserObject = this;

            sceneNode = scene.CreateSceneNode(name);
            sceneNode.AttachObject(entity);
            sceneNode.SetPosition((position.X + 1) * map.BlockSize + map.BlockSize * 0.5f, 2.5f, (position.Z + 1) * map.BlockSize + map.BlockSize * 0.5f);
            sceneNode.SetScale(Mogre.Vector3.UNIT_SCALE * 0.0035f);

            map.Merge(blocks, position);
        }

        public override void Unload()
        {
            base.Unload();

            blocks = null;

            scene.DestroyEntity(entity);
            scene.DestroySceneNode(sceneNode);

            entity = null;
            sceneNode = null;

            foreach (Miner miner in miners)
            {
                miner.Unload();
            }

            miners.Clear();
        }

        public override void Update(float time)
        {
            base.Update(time);

            // Count alive miners.
            List<Miner> aliveMiners = miners.FindAll(delegate(Miner miner)
            {
                return miner.GetState() != Miner.State.Killed;
            });

            // Create new miner.
            if (aliveMiners.Count < maxAliveMinersCount && minerCreateTimer.Update(time))
            {
                // Create new miner, only if starting block is not occupied by other miner.
                if (map.Blocks[Position.X + 1, Position.Z + 1].IsOccupied() == false)
                {
                    // Get current miners count.
                    int minersCount = miners.Count;

                    // Create new miner.
                    Miner miner = new Miner(string.Format("Miner #{0}", minersCount + 1), scene, map, this);

                    // Set miner position to this camp position.
                    miner.Position = new Coordinates(Position.X + 1, 0, Position.Z + 1);

                    // Load miner.
                    miner.Load();

                    // Add miner to list.
                    miners.Add(miner);

                    ++points;
                }
            }

            // Update each miner.
            foreach (Miner miner in miners)
            {
                miner.Update(time);
            }
        }

        /// <summary>
        /// Adds gold to camps vault. This method should be called by miner.
        /// </summary>
        /// <param name="amount">Amount of gold to add to camps vault.</param>
        public void IncreaseGold(int amount)
        {
            gold += amount;
            points += amount;
        }
    }
}
