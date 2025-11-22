using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Miners
{
    public class Game
    {
        /// <summary>
        /// If set to false, then main game loop will end.
        /// </summary>
        protected bool running = true;
        protected bool debug = false;

        /// <summary>
        /// Input object for checking keyboard and mouse state.
        /// </summary>
        protected Input input = null;

        // MOGRE objects.
        protected Mogre.Root root = null;
        protected Mogre.RenderWindow renderWindow = null;
        protected Mogre.SceneManager sceneManager = null;
        protected Mogre.Viewport viewport = null;
        protected Mogre.Camera camera = null;

        // Game objects.
        protected Scene scene = null;
        protected Debug.Axis axis = null;
        protected Map map = null;
        protected Camp camp = null;

        public Mogre.Root Root
        {
            get { return root; }
        }

        public Mogre.RenderWindow RenderWindow
        {
            get { return renderWindow; }
        }

        public Mogre.SceneManager SceneManager
        {
            get { return sceneManager; }
        }

        public Mogre.Viewport Viewport
        {
            get { return viewport; }
        }

        public Mogre.Camera Camera
        {
            get { return camera; }
        }

        public bool Running
        {
            get { return running; }
            set { running = value; }
        }

        public Scene Scene
        {
            get { return scene; }
        }

        public Map Map
        {
            get { return map; }
        }

        public Camp Camp
        {
            get { return camp; }
        }

        public Game(Input input, bool debug = false)
        {
            this.debug = debug;
            this.input = input;
        }

        public bool Init(IntPtr handle)
        {
            // Setup Ogre.
            root = new Mogre.Root("Plugins.cfg", "Ogre.cfg", "Ogre.log");

            if (root.RestoreConfig() == false && root.ShowConfigDialog() == false)
            {
                return false;
            }

            root.Initialise(false);

            // Create render window.
            Mogre.NameValuePairList miscParams = new Mogre.NameValuePairList();
            miscParams["externalWindowHandle"] = handle.ToString();

            renderWindow = root.CreateRenderWindow("Miners", 0, 0, false, miscParams);

            // Load resources.
            Mogre.ConfigFile cf = new Mogre.ConfigFile();
            cf.Load("Resources.cfg", "\t:=", true);

            // Go through all sections & settings in the file.
            Mogre.ConfigFile.SectionIterator seci = cf.GetSectionIterator();

            string secName, typeName, archName;

            // Normally we would use the foreach syntax, which enumerates the values, but in this case we need CurrentKey too.
            while (seci.MoveNext())
            {
                secName = seci.CurrentKey;

                Mogre.ConfigFile.SettingsMultiMap settings = seci.Current;

                foreach (KeyValuePair<string, string> pair in settings)
                {
                    typeName = pair.Key;
                    archName = pair.Value;

                    Mogre.ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
                }
            }

            Mogre.ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

            // Create scene manager.
            sceneManager = root.CreateSceneManager(Mogre.SceneType.ST_GENERIC, "SceneManager");
            sceneManager.AmbientLight = new Mogre.ColourValue(0.5f, 0.5f, 0.5f);

            // Create light.
            Mogre.Light ligth = sceneManager.CreateLight("Light");
            ligth.Type = Mogre.Light.LightTypes.LT_DIRECTIONAL;
            ligth.Direction = new Mogre.Vector3(-1.0f, -1.0f, -1.0f);
            ligth.DiffuseColour = new Mogre.ColourValue(0.25f, 0.25f, 0.0f);

            // Create camera.
            camera = sceneManager.CreateCamera("Camera");
            camera.Position = new Mogre.Vector3(50.0f, 100.0f, 100.0f);
            camera.SetFixedYawAxis(true, Mogre.Vector3.UNIT_Y);
            camera.LookAt(new Mogre.Vector3(50.0f, 0.0f, 50.0f));
            camera.NearClipDistance = 0.5f;

            if (root.RenderSystem.Capabilities.HasCapability(Mogre.Capabilities.RSC_INFINITE_FAR_PLANE))
            {
                camera.FarClipDistance = 0.0f;
            }
            else
            {
                camera.FarClipDistance = 1000.0f;
            }

            // Create viewport.
            viewport = renderWindow.AddViewport(camera);
            viewport.BackgroundColour = Mogre.ColourValue.Black;

            // Attach event listeners.
            root.FrameStarted += new Mogre.FrameListener.FrameStartedHandler(FrameStarted);
            root.FrameRenderingQueued += new Mogre.FrameListener.FrameRenderingQueuedHandler(FrameRenderingQueued);
            root.FrameEnded += new Mogre.FrameListener.FrameEndedHandler(FrameEnded);
      
            return true;
        }

        public virtual void Load()
        {
            // Create scene and objects.
            scene = new Scene("Scene #1", sceneManager);
            scene.Load();
            
            map = new Map("Map #1", scene, new Coordinates(50, 0, 50), 2.0f);
            map.Load();
            
            camp = new Camp("Camp #1", scene, map);
            camp.Position = new Coordinates(20, 0, 30);
            camp.Load();

            scene.Add(map);
            scene.Add(camp);

            // Display debug informations.
            if (debug)
            {
                axis = new Debug.Axis("Axis #1", scene);
                axis.Load();

                scene.Add(axis);
            }

            // Generate random caves and gold deposits.
            map.GenerateRandom(5, 30, Block.TerrainType.Empty);
            map.GenerateRandom(5, 30, Block.TerrainType.Gold);
            map.GenerateRandom(5, 20, Block.TerrainType.Water);
            map.GenerateRandom(5, 20, Block.TerrainType.Cave);
        }

        public virtual void Unload()
        {
            camp.Unload();
            camp = null;

            map.Unload();
            map = null;

            scene.Unload();
            scene = null;

            if (debug)
            {
                axis.Unload();
                axis = null;
            }
        }

        protected bool FrameStarted(Mogre.FrameEvent evt)
        {
            return running;
        }

        protected bool FrameRenderingQueued(Mogre.FrameEvent evt)
        {
            // Move camera.
            Mogre.Vector3 velocity = Mogre.Vector3.ZERO;

            if (input.Keys[(int)Keys.W])
            {
                velocity += Mogre.Vector3.NEGATIVE_UNIT_Z;
            }
            else if (input.Keys[(int)Keys.S])
            {
                velocity += Mogre.Vector3.UNIT_Z;
            }

            if (input.Keys[(int)Keys.A])
            {
                velocity += Mogre.Vector3.NEGATIVE_UNIT_X;
            }
            else if (input.Keys[(int)Keys.D])
            {
                velocity += Mogre.Vector3.UNIT_X;
            }

            camera.Move(velocity * 50.0f * evt.timeSinceLastFrame);

            // Update scene.
            scene.Update(evt.timeSinceLastFrame);

            return running;
        }

        protected bool FrameEnded(Mogre.FrameEvent evt)
        {
            return running;
        }
    }
}
