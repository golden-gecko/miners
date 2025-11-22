namespace Miners
{
    class Trap : Structure
    {
        /// <summary>
        /// Number of monsters killed by this trap.
        /// </summary>
        /// 
        protected uint monstersKilled = 0;

        public uint MonstersKilled
        {
            get { return monstersKilled; }
        }

        public Trap(string name, Scene scene, Map map)
            : base(name, scene, map)
        {
        }

        public override void Load()
        {
            base.Load();

            Blocks = new Block[1, 1];

            for (int z = 0; z <= blocks.GetUpperBound(0); ++z)
            {
                for (int x = 0; x <= blocks.GetUpperBound(1); ++x)
                {
                    Blocks[x, z] = new Block(string.Format("Trap block {0}:{1}", x, z), scene, map, Block.TerrainType.Empty);
                }
            }

            entity = scene.CreateEntity(name, "WoodPallet.mesh");
            entity.UserObject = this;

            sceneNode = scene.CreateSceneNode(name);
            sceneNode.AttachObject(entity);
            sceneNode.SetPosition(position.X * map.BlockSize + map.BlockSize * 0.5f, 0.0f, position.Z * map.BlockSize + map.BlockSize * 0.5f);
            sceneNode.SetScale(Mogre.Vector3.UNIT_SCALE * 0.2f);

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
        }

        public override void Update(float time)
        {
            base.Update(time);

            // Kill monsters standing on this trap.
            Block block = map.GetBlock(position);

            if (block != null)
            {
                foreach (MapObject mapObject in block.MapObjects)
                {
                    if (mapObject is Monster)
                    {
                        mapObject.Destroy();
                    }
                }
            }
        }
    }
}
