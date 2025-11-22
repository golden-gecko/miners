using System.Collections.Generic;

namespace Miners
{
    public class Scene : ILoadable, IUpdatable
    {
        protected string name = "";
        protected Mogre.SceneManager sceneManager = null;
        protected List<GameObject> gameObjects = new List<GameObject>();

        public Scene(string name, Mogre.SceneManager sceneManager)
        {
            this.name = name;
            this.sceneManager = sceneManager;
        }

        public virtual void Load()
        {
        }

        public virtual void Unload()
        {
            gameObjects.Clear();
        }

        public virtual void Update(float time)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(time);
            }
        }

        public void Add(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        public virtual Mogre.Entity CreateEntity(string entityName, string meshName)
        {
            return sceneManager.CreateEntity(entityName, meshName);
        }

        public virtual Mogre.ManualObject CreateManualObject(string name)
        {
            return sceneManager.CreateManualObject(name);
        }

        public virtual Mogre.SceneNode CreateSceneNode(string name)
        {
            return sceneManager.RootSceneNode.CreateChildSceneNode(name);
        }

        public virtual void DestroyEntity(Mogre.Entity entity)
        {
            sceneManager.DestroyEntity(entity);
        }

        public virtual void DestroyManualObject(Mogre.ManualObject manualObject)
        {
            sceneManager.DestroyManualObject(manualObject);
        }

        public virtual void DestroySceneNode(Mogre.SceneNode sceneNode)
        {
            sceneManager.DestroySceneNode(sceneNode);
        }
    }
}
