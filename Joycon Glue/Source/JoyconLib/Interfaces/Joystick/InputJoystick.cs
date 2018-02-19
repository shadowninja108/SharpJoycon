using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue
{
    public abstract class InputJoystick
    {

        public abstract void Poll(PacketData data);

        public abstract int ButtonCount();
        public abstract uint GetButtonData();
        public abstract bool GetButton(int id);
        public abstract StickPos GetStick(int id);
        public abstract POVDirection GetPov(int id);

        public struct StickPos
        {
            public int x, y;

            public StickPos(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public enum POVDirection
        {
            None, Up, Right, Down, Left,
        }
    }
}
