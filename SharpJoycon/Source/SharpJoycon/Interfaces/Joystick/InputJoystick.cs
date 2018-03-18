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
        public abstract StickPos GetStick(int id);
        public abstract List<POVDirection> GetPov(int id);

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
             Up, Right, Down, Left,
        }

        //would have made this an enum method, but they don't exist
        public static int GetPOVMultiplier(POVDirection dir)
        {
            switch (dir)
            {
                case POVDirection.Up:
                    return 1;
                case POVDirection.Right:
                    return 3;
                case POVDirection.Down:
                    return 5;
                case POVDirection.Left:
                    return 7;
            }
            return 0;
        }
    }
}
