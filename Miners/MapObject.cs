namespace Miners
{
    /// <summary>
    /// Base class for objects that can be placed on map.
    /// </summary>
    public class MapObject : GameObject
    {
        /// <summary>
        /// Map to which object is attached.
        /// </summary>
        protected Map map = null;

        /// <summary>
        /// Objects position on map.
        /// </summary>
        protected Coordinates position = Coordinates.Zero;

        /// <summary>
        /// Objects direction on map.
        /// </summary>
        protected Coordinates direction = new Coordinates(0, 0, -1);

        public Coordinates Position
        {
            get { return position; }
            set { position = value; }
        }

        public Coordinates Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public MapObject(string name, Scene scene, Map map)
            : base(name, scene)
        {
            this.map = map;
        }

        /// <summary>
        /// Moves scene node to block center.
        /// </summary>
        public void SnapToBlockCenter()
        {
            sceneNode.Position = new Mogre.Vector3(
                Position.X * map.BlockSize + map.BlockSize * 0.5f, 0.0f,
                Position.Z * map.BlockSize + map.BlockSize * 0.5f
            );
        }
    }
}
