using System;
using System.Collections.Generic;
using System.Text;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    class NullController : Controller
    {
        public NullController() : base(null) // Controller doesn't access controller so this *should* be fine
        {

        }

        public override Buttons GetButtons()
        {
            return new Buttons();
        }

        public override POVDirection GetPov(int id)
        {
            return POVDirection.None;
        }

        public override Position GetStick(int id)
        {
            return new Position();
        }

        public override int GetStickConfigOffset(ConfigurationInterface.ConfigurationType type)
        {
            return 0;
        }

        public override ConfigurationInterface.AnalogConfiguration ParseAnalogConfiguration(int[] data)
        {
            return new ConfigurationInterface.AnalogConfiguration();
        }

        public override void Poll(HIDInterface.PacketData data)
        {

        }
    }
}
