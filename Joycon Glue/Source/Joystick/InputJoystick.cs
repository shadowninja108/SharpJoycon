using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace Joycon_Glue
{
    public abstract class InputJoystick
    {

        protected SimplifiedHidDevice hid;

        private bool firstPoll = false;

        public InputJoystick(SimplifiedHidDevice hid)
        {
            this.hid = hid;
        }

        public virtual void Poll()
        {
            if (firstPoll)
            {
                hid.GetHidDevice().MonitorDeviceEvents = true;
                hid.GetHidDevice().Read(Poll);
                firstPoll = true;
            }
        }

        public abstract void Poll(HidDeviceData data);
        public abstract int ButtonCount();
        public abstract bool GetButton(int id);
        public abstract StickPos GetStick(int id);
        public abstract POVDirection GetPov();

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
            Up, Down, Left, Right, None
        }
    }
}
