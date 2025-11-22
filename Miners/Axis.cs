namespace Miners
{
    namespace Debug
    {
        /// <summary>
        /// Displays global coordinate systems axes. Useful for debugging.
        /// </summary>
        public class Axis : GameObject
        {
            protected Mogre.ManualObject manualObject = null;

            public Axis(string name, Scene scene)
                : base(name, scene)
            {
            }

            public override void Load()
            {
                base.Load();

                // Create manual object.
                manualObject = scene.CreateManualObject(name);
                manualObject.Begin("BaseWhiteNoLighting", Mogre.RenderOperation.OperationTypes.OT_LINE_LIST);

                manualObject.Position(0.0f, 0.0f, 0.0f);
                manualObject.Colour(Mogre.ColourValue.Red);

                manualObject.Position(1.0f, 0.0f, 0.0f);
                manualObject.Colour(Mogre.ColourValue.Red);

                manualObject.Position(0.0f, 0.0f, 0.0f);
                manualObject.Colour(Mogre.ColourValue.Green);

                manualObject.Position(0.0f, 1.0f, 0.0f);
                manualObject.Colour(Mogre.ColourValue.Green);

                manualObject.Position(0.0f, 0.0f, 0.0f);
                manualObject.Colour(Mogre.ColourValue.Blue);

                manualObject.Position(0.0f, 0.0f, 1.0f);
                manualObject.Colour(Mogre.ColourValue.Blue);

                manualObject.End();
                manualObject.UserObject = this;

                // Create scene node.
                sceneNode = scene.CreateSceneNode(name);
                sceneNode.AttachObject(manualObject);
                sceneNode.SetScale(new Mogre.Vector3(100.0f, 100.0f, 100.0f));
            }

            public override void Unload()
            {
                base.Unload();

                scene.DestroyManualObject(manualObject);
                scene.DestroySceneNode(sceneNode);

                manualObject = null;
                sceneNode = null;
            }
        }
    }
}
