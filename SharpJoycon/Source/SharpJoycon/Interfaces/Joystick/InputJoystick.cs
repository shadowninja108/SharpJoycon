using System.Collections.Generic;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick
{
    public abstract class InputJoystick
    {

        public abstract void Poll(PacketData data);

        public abstract int ButtonCount();
        public abstract uint GetButtonData();
        public abstract bool GetButton(int id);
        public abstract Position GetStick(int id);
        public abstract POVDirection GetPov(int id);

        public struct Position
        {
            public int x, y;

            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public enum POVDirection
        {
             None, Up, UpRight, Right, RightDown, Down, DownLeft, Left, LeftUp
        }

        //would have made this an enum method, but they don't exist
        public static int GetPOVMultiplier(POVDirection dir)
        {

            return ((int)dir) - 1;
        }
    }
}
