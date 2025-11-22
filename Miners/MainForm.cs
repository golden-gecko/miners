using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Miners
{
    public partial class MainForm : Form
    {
        protected Game game = null;
        protected Input input = null;

        protected MapObject selected = null;
        protected MapObject hovered = null;

        public MainForm()
        {
            InitializeComponent();

            // Get command line arguments.
            List<string> args = new List<string>(Environment.GetCommandLineArgs());

            bool debug = args.Contains("--debug");

            // Create input.
            input = new Input();

            // Create game.
            game = new Game(input, debug);

            if (game.Init(panel1.Handle))
            {
                // Load input.
                input.Load(game);

                // Load game.
                game.Load();

                // Attach input events.
                MouseWheel += new MouseEventHandler(MainForm_MouseWheel);

                // Start timer.
                timer1.Enabled = true;

                // Show form and run main loop.
                Show();
                Run();
            }
            else
            {
                Close();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (game.Root.ShowConfigDialog())
            {
                MessageBox.Show("You have to restart the game to apply new settings.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            game.Running = false;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            input.KeyDown(e);

            switch (e.KeyCode)
            {
                // Allow closing application, by pressing Escape key.
                case Keys.Escape:
                    Close();
                    break;

                // Shortcuts for structures.
                case Keys.B:
                    radioButton2.Checked = true;
                    break;

                case Keys.T:
                    radioButton3.Checked = true;
                    break;
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            input.KeyUp(e);
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            input.MouseWheel(e);
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            game.Camera.AspectRatio = (float)panel1.Width / (float)panel1.Height;
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fullscreenToolStripMenuItem.Checked)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Update FPS on title bar.
            Text = string.Format("Miners v0.1b - {0:0} FPS", game.RenderWindow.AverageFPS);

            // Update camp in list view.
            ListViewItem item = listView1.FindItemWithText(game.Camp.Name);

            if (item == null)
            {
                item = new ListViewItem(game.Camp.Name);
                item.Tag = game.Camp;

                listView1.Items.Add(item);
            }

            // Update miners in list view.
            foreach (Miner miner in game.Camp.Miners)
            {
                item = listView1.FindItemWithText(miner.Name);

                if (item == null)
                {
                    item = new ListViewItem(miner.Name);
                    item.SubItems.Add(miner.GetState().ToString());
                    item.Tag = miner;

                    listView1.Items.Add(item);
                }
                else
                {
                    item.SubItems[1].Text = miner.GetState().ToString();
                }
            }

            // Update monsters in list view.
            foreach (MapObject mapObject in game.Map.MapObjects)
            {
                item = listView1.FindItemWithText(mapObject.Name);

                if (item == null)
                {
                    item = new ListViewItem(mapObject.Name);
                    item.SubItems.Add(mapObject.GetState().ToString());
                    item.Tag = mapObject;

                    listView1.Items.Add(item);
                }
                else
                {
                    item.SubItems[1].Text = mapObject.GetState().ToString();
                }
            }

            // Update selected object.
            propertyGrid1.SelectedObject = propertyGrid1.SelectedObject;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
            }
        }

        private void panel1_MouseHover(object sender, EventArgs e)
        {
            input.MouseHover(e);

            // Focus panel to enable mouse wheel zooming.
            panel1.Focus();
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            input.Click(e);

            if (radioButton2.Checked)
            {
                PlaceBarrier();

                radioButton2.Checked = false;
            }
            else if (radioButton3.Checked)
            {
                PlaceTrap();

                radioButton3.Checked = false;
            }
            else
            {
                SelectObject();
            }
        }

        private void PlaceBarrier()
        {
            // Get mouse relative position on panel.
            Point mouse = panel1.PointToClient(Cursor.Position);

            // Create ray and execute query.
            Mogre.Ray ray = game.Camera.GetCameraToViewportRay(
                (float)mouse.X / (float)game.Viewport.ActualWidth, (float)mouse.Y / (float)game.Viewport.ActualHeight
            );

            Mogre.RaySceneQueryResult result = Raycast(ray);

            // Check if map was clicked.
            foreach (Mogre.RaySceneQueryResultEntry entry in result)
            {
                if (entry.movable != null && entry.movable.UserObject != null)
                {
                    if (entry.movable.UserObject is Map)
                    {
                        // Get map point.
                        Mogre.Vector3[] vertices = new Mogre.Vector3[4];

                        vertices[0] = new Mogre.Vector3(0.0f, game.Map.BlockSize, 0.0f);
                        vertices[1] = new Mogre.Vector3(0.0f, game.Map.BlockSize, game.Map.Size.Z * game.Map.BlockSize);
                        vertices[2] = new Mogre.Vector3(game.Map.Size.X * game.Map.BlockSize, game.Map.BlockSize, game.Map.Size.Z * game.Map.BlockSize);
                        vertices[3] = new Mogre.Vector3(game.Map.Size.X * game.Map.BlockSize, game.Map.BlockSize, 0.0f);

                        int[] indices = new int[6];

                        indices[0] = 0;
                        indices[1] = 1;
                        indices[2] = 3;

                        indices[3] = 1;
                        indices[4] = 2;
                        indices[5] = 3;

                        for (int i = 0; i < indices.Length; i += 3)
                        {
                            Mogre.Pair<bool, float> hit = Mogre.Math.Intersects(
                                ray, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], true, false
                            );

                            if (hit.first)
                            {
                                Mogre.Vector3 point = ray.GetPoint(hit.second);

                                // Get map position.
                                Coordinates coords = new Coordinates(
                                    (int)Math.Floor(point.x / game.Map.BlockSize), 0,
                                    (int)Math.Floor(point.z / game.Map.BlockSize)
                                );

                                // Place barrier.
                                game.Map.UpdateBlock(Block.TerrainType.Solid, coords);
                            }
                        }
                    }
                }
            }
        }

        private void PlaceTrap()
        {
            // Get mouse relative position on panel.
            Point mouse = panel1.PointToClient(Cursor.Position);

            // Create ray and execute query.
            Mogre.Ray ray = game.Camera.GetCameraToViewportRay(
                (float)mouse.X / (float)game.Viewport.ActualWidth, (float)mouse.Y / (float)game.Viewport.ActualHeight
            );

            Mogre.RaySceneQueryResult result = Raycast(ray);

            // Check if map was clicked.
            foreach (Mogre.RaySceneQueryResultEntry entry in result)
            {
                if (entry.movable != null && entry.movable.UserObject != null)
                {
                    if (entry.movable.UserObject is Map)
                    {
                        // Get map point.
                        Mogre.Vector3[] vertices = new Mogre.Vector3[4];

                        vertices[0] = new Mogre.Vector3(0.0f, game.Map.BlockSize, 0.0f);
                        vertices[1] = new Mogre.Vector3(0.0f, game.Map.BlockSize, game.Map.Size.Z * game.Map.BlockSize);
                        vertices[2] = new Mogre.Vector3(game.Map.Size.X * game.Map.BlockSize, game.Map.BlockSize, game.Map.Size.Z * game.Map.BlockSize);
                        vertices[3] = new Mogre.Vector3(game.Map.Size.X * game.Map.BlockSize, game.Map.BlockSize, 0.0f);

                        int[] indices = new int[6];

                        indices[0] = 0;
                        indices[1] = 1;
                        indices[2] = 3;

                        indices[3] = 1;
                        indices[4] = 2;
                        indices[5] = 3;

                        for (int i = 0; i < indices.Length; i += 3)
                        {
                            Mogre.Pair<bool, float> hit = Mogre.Math.Intersects(
                                ray, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], true, false
                            );

                            if (hit.first)
                            {
                                Mogre.Vector3 point = ray.GetPoint(hit.second);

                                // Get map position.
                                Coordinates coords = new Coordinates(
                                    (int)Math.Floor(point.x / game.Map.BlockSize), 0,
                                    (int)Math.Floor(point.z / game.Map.BlockSize)
                                );

                                // Place trap only on passable blocks.
                                Block block = game.Map.GetBlock(coords);

                                if (block != null && block.CanPlaceStructure())
                                {
                                    game.Map.CreateTrap(coords);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Mogre.RaySceneQueryResult Raycast(Mogre.Ray ray)
        {
            Mogre.RaySceneQuery query = game.SceneManager.CreateRayQuery(ray);
            query.SetSortByDistance(true);

            return query.Execute();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            input.MouseDown(e);
        }

        private void SelectObject()
        {
            // Get mouse relative position on panel.
            Point mouse = panel1.PointToClient(Cursor.Position);

            // Create ray and execute query.
            Mogre.Ray ray = game.Camera.GetCameraToViewportRay(
                (float)mouse.X / (float)game.Viewport.ActualWidth, (float)mouse.Y / (float)game.Viewport.ActualHeight
            );

            Mogre.RaySceneQueryResult result = Raycast(ray);

            // Remove selection from previous object.
            if (selected != null)
            {
                selected.SceneNode.ShowBoundingBox = false;
                selected = null;
            }

            // Apply selection to new object.
            foreach (Mogre.RaySceneQueryResultEntry entry in result)
            {
                if (entry.movable != null && entry.movable.UserObject != null)
                {
                    if (entry.movable.UserObject is Camp || entry.movable.UserObject is Miner || entry.movable.UserObject is Monster)
                    {
                        selected = entry.movable.UserObject as MapObject;
                        selected.SceneNode.ShowBoundingBox = true;

                        propertyGrid1.SelectedObject = selected;
                    }
                }
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            input.MouseMove(e);

            // Get mouse relative position on panel.
            Point mouse = panel1.PointToClient(Cursor.Position);

            // Create ray and execute query.
            Mogre.Ray ray = game.Camera.GetCameraToViewportRay(
                (float)mouse.X / (float)game.Viewport.ActualWidth, (float)mouse.Y / (float)game.Viewport.ActualHeight
            );

            Mogre.RaySceneQuery query = game.SceneManager.CreateRayQuery(ray);
            query.SetSortByDistance(true);

            Mogre.RaySceneQueryResult result = query.Execute();

            // Remove selection from previous object only if it is not selected.
            if (hovered != null)
            {
                if (selected != hovered)
                {
                    hovered.SceneNode.ShowBoundingBox = false;
                }

                hovered = null;
            }

            // Apply selection to new object.
            foreach (Mogre.RaySceneQueryResultEntry entry in result)
            {
                if (entry.movable != null && entry.movable.UserObject != null)
                {
                    if (entry.movable.UserObject is Camp || entry.movable.UserObject is Miner || entry.movable.UserObject is Monster)
                    {
                        hovered = entry.movable.UserObject as MapObject;
                        hovered.SceneNode.ShowBoundingBox = true;
                    }
                }
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            input.MouseUp(e);
        }

        public void Run()
        {
            while (game.Running)
            {
                Application.DoEvents();

                game.Root.RenderOneFrame();
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear UI.
            propertyGrid1.SelectedObject = null;
            listView1.Items.Clear();

            // Unload game.
            game.Unload();

            // Load new game.
            game.Load();
        }

        private void wireframeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wireframeToolStripMenuItem.Checked)
            {
                game.Camera.PolygonMode = Mogre.PolygonMode.PM_WIREFRAME;
            }
            else
            {
                game.Camera.PolygonMode = Mogre.PolygonMode.PM_SOLID;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }
    }
}
