namespace Miners
{
    public class Structure : MapObject
    {
        protected Block[,] blocks = null;

        protected Mogre.Entity entity = null;

        public Block[,] Blocks
        {
            get { return blocks; }
            protected set { blocks = value; }
        }

        public Structure(string name, Scene scene, Map map)
            : base(name, scene, map)
        {
        }
    }
}
