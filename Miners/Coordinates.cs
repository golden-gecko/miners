namespace Miners
{
    /// <summary>
    /// Represents map coordinates. Can be used for storing positions and directions.
    /// </summary>
    public class Coordinates
    {
        /// <summary>
        /// Empty coordinates.
        /// </summary>
        public static Coordinates Zero = new Coordinates(0, 0, 0);

        /// <summary>
        /// Unit coordinates.
        /// </summary>
        public static Coordinates Unit = new Coordinates(1, 1, 1);

        /// <summary>
        /// Coordinates pointing to north. North is along negative Z axis.
        /// </summary>
        public static Coordinates North = new Coordinates(0, 0, -1);

        /// <summary>
        /// Coordinates pointing to east. East is along Z axis.
        /// </summary>
        public static Coordinates East = new Coordinates(1, 0, 0);

        /// <summary>
        /// Coordinates pointing to south. South is along Z axis.
        /// </summary>
        public static Coordinates South = new Coordinates(0, 0, 1);

        /// <summary>
        /// Coordinates pointing to west. West is along negative X axis.
        /// </summary>
        public static Coordinates West = new Coordinates(-1, 0, 0);

        /// <summary>
        /// Array of 4 world directions. Usefull when searching for adjacent positions.
        /// </summary>
        public static Coordinates[] Directions = new Coordinates[4]
        {
            North, East, South, West
        };

        /// <summary>
        /// Array of 8 world directions. Usefull when searching for adjacent positions.
        /// </summary>
        public static Coordinates[] Compass = new Coordinates[8]
        {
            North, North + East, East, East + South, South, South + West, West, West + North
        };

        protected int x = 0;
        protected int y = 0;
        protected int z = 0;

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Z
        {
            get { return z; }
            set { z = value; }
        }

        public Coordinates(int x = 0, int y = 0, int z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Coordinates operator +(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Coordinates operator -(Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
