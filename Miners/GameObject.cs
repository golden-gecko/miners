using System;

namespace Miners
{
    /// <summary>
    /// Temporary solution for storing object state. It will be moved to new class.
    /// </summary>
    public partial class GameObject
    {
        public enum State
        {
            Idle,
            Flooding,
            Moving,
            Digging,
            Waiting,
            Killed
        };

        /// <summary>
        /// Current game object state.
        /// </summary>
        protected State state = State.Idle;

        public State GetState()
        {
            return state;
        }
    }

    /// <summary>
    /// Base class for all game objects.
    /// </summary>
    public partial class GameObject : ILoadable, IUpdatable
    {
        protected string name = "";
        protected Scene scene = null;
        protected Mogre.SceneNode sceneNode = null;

        public string Name
        {
            get { return name; }
        }

        public Mogre.SceneNode SceneNode
        {
            get { return sceneNode; }
        }

        public GameObject(string name, Scene scene)
        {
            this.name = name;
            this.scene = scene;
        }

        public virtual void Load()
        {
        }

        public virtual void Unload()
        {
        }

        public virtual void Update(float time)
        {
        }

        public virtual void Destroy()
        {
        }
    }
}
