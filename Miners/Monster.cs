using System;

namespace Miners
{
    /// <summary>
    /// Represents single monster. Monsters can kill miners.
    /// </summary>
    public class Monster : MapObject
    {
        protected Mogre.Entity entity = null;

        /// <summary>
        /// Move speed.
        /// </summary>
        protected float speed = 6.0f;

        /// <summary>
        /// Probability to change direction.
        /// </summary>
        protected float chanceToChangeDirection = 0.2f;

        /// <summary>
        /// Amount of time needed to move to adjacent block.
        /// Timer maximum value is calculated before move based on block size and miner speed.
        /// </summary>
        protected Timer moveTimer = new Timer(1.0f);

        /// <summary>
        /// Amount of time miner will wait (i.e. wait for other miner to move out of its way).
        /// </summary>
        protected Timer waitTimer = new Timer(0.1f);

        /// <summary>
        /// Total number of blocks traveled by this miner.
        /// </summary>
        protected uint blocksTraveled = 0;

        /// <summary>
        /// Total number of miners killed by this monster.
        /// </summary>
        protected uint minersKilled = 0;

        public uint BlocksTraveled
        {
            get { return blocksTraveled; }
        }

        public uint MinersKilled
        {
            get { return minersKilled; }
        }

        public float Speed
        {
            get { return speed; }
        }

        public float ChanceToChangeDirection
        {
            get { return chanceToChangeDirection; }
        }

        public Monster(string name, Scene scene, Map map)
            : base(name, scene, map)
        {
            this.direction = new Coordinates(0, 0, 1);
        }

        public override void Load()
        {
            base.Load();

            entity = scene.CreateEntity(name, "ogrehead.mesh");
            entity.UserObject = this;

            sceneNode = scene.CreateSceneNode(name);
            sceneNode.AttachObject(entity);
            sceneNode.SetPosition(position.X * map.BlockSize + map.BlockSize * 0.5f, 0.4f, position.Z * map.BlockSize + map.BlockSize * 0.5f);
            sceneNode.SetScale(Mogre.Vector3.UNIT_SCALE * 0.02f);

            Turn();

            map.GetBlock(Position).Assign(this);
        }

        public override void Unload()
        {
            base.Unload();

            scene.DestroyEntity(entity);
            scene.DestroySceneNode(sceneNode);

            entity = null;
            sceneNode = null;
        }

        public override void Update(float time)
        {
            base.Update(time);

            // Search adjacent block for miners and kill them.
            foreach (Coordinates coordinates in Coordinates.Directions)
            {
                Block adjacent = map.GetBlock(position + coordinates);

                if (adjacent != null)
                {
                    adjacent.DestroyMapObjects<Miner>();
                }
            }

            // Process miner state.
            switch (state)
            {
                // Make turn (random).
                // If block in front is diggable, change state to digging.
                // If block in front is passable, change state to moving.
                // Change to waiting.
                case State.Idle:
                    if (Generator.Chance(chanceToChangeDirection))
                    {
                        Turn();
                    }

                    if (map.GetBlock(Position + Direction).IsPassable() && map.GetBlock(Position + Direction).IsOccupied(this) == false)
                    {
                        state = State.Moving;
                    }
                    else
                    {
                        state = State.Waiting;
                    }
                    break;

                // If block in front is passable, move to it.
                // Change to idle.
                case State.Moving:
                    if (map.GetBlock(Position + Direction).IsPassable() && map.GetBlock(Position + Direction).IsOccupied(this) == false)
                    {
                        Move(time);
                    }
                    else
                    {
                        state = State.Idle;
                    }
                    break;

                // Change to idle.
                case State.Waiting:
                    if (waitTimer.Update(time))
                    {
                        state = State.Idle;
                    }
                    break;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            state = State.Killed;

            sceneNode.SetVisible(false);
        }

        protected void Turn()
        {
            // Randomly turn left or right.
            Coordinates direction = new Coordinates();

            if (Direction.X != 0)
            {
                direction.X = 0;
                direction.Z = Generator.Next(2) == 1 ? 1 : -1;
            }
            else
            {
                direction.X = Generator.Next(2) == 1 ? 1 : -1;
                direction.Z = 0;
            }

            Direction = direction;

            // Update direction of scene node.
            sceneNode.SetDirection(direction.X, 0.0f, direction.Z, Mogre.Node.TransformSpace.TS_PARENT, Mogre.Vector3.UNIT_Z);
        }

        protected virtual void Move(float time)
        {
            // Assign miner to block in front.
            map.GetBlock(Position + Direction).Assign(this);

            // Calculate time needed to move to another block.
            moveTimer.Maximal = map.BlockSize / speed;

            // Move scene node towards block in front.
            sceneNode.Translate(Mogre.Vector3.UNIT_Z * speed * time, Mogre.Node.TransformSpace.TS_LOCAL);

            if (moveTimer.Update(time))
            {
                // Remove miner from current block.
                map.GetBlock(Position).Remove(this);

                // Update position.
                Position += Direction;

                // Snap to center, to prevent any errors caused by move.
                SnapToBlockCenter();

                // Update stats.
                ++blocksTraveled;
            }
        }
    }
}
