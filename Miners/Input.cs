using System;
using System.Windows.Forms;

namespace Miners
{
    public class Input
    {
        protected Game game = null;

        protected bool[] keys = new bool[256];
        protected bool[] buttons = new bool[3];

        public bool[] Keys { get { return keys; } }
        public bool[] Buttons { get { return buttons; } }

        public Input()
        {
            for (int i = 0; i < keys.Length; ++i)
            {
                keys[i] = false;
            }

            for (int i = 0; i < buttons.Length; ++i)
            {
                buttons[i] = false;
            }
        }

        public void Load(Game game)
        {
            this.game = game;
        }

        public void Click(EventArgs e)
        {
        }

        public void KeyDown(KeyEventArgs e)
        {
            if ((int)e.KeyCode < keys.Length)
            {
                keys[(int)e.KeyCode] = true;
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if ((int)e.KeyCode < keys.Length)
            {
                keys[(int)e.KeyCode] = false;
            }
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                buttons[0] = true;
            }
        }

        public void MouseHover(EventArgs e)
        {
        }

        public void MouseMove(MouseEventArgs e)
        {
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                buttons[0] = false;
            }
        }

        public void MouseWheel(MouseEventArgs e)
        {
            Mogre.Vector3 velocity = Mogre.Vector3.ZERO;

            if (e.Delta > 0)
            {
                velocity = Mogre.Vector3.NEGATIVE_UNIT_Y;
            }
            else if (e.Delta < 0)
            {
                velocity = Mogre.Vector3.UNIT_Y;
            }

            game.Camera.Move(velocity * 10.0f);

            // Limit camera position.
            Mogre.Vector3 position = game.Camera.Position;

            if (position.y < 10.0f)
            {
                position.y = 10.0f;
            }
            else if (position.y > 150.0f)
            {
                position.y = 150.0f;
            }

            game.Camera.Position = position;
        }
    }
}
