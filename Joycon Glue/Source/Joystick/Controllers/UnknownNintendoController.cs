using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace Joycon_Glue.Source.Joystick.Controllers
{
    class UnknownNintendoController : NintendoController
    {
        public UnknownNintendoController(SimplifiedHidDevice device) : base(device)
        {

        }

        public override int ButtonCount()
        {
            return 0;
        }

        public override AnalogConfiguration GetAnalogConfiguration(ConfigurationType type)
        {
            throw new NotImplementedException();
        }

        public override Buttons GetButtons()
        {
            return new Buttons();
        }

        public override POVDirection GetPov()
        {
            return POVDirection.None;
        }

        public override StickPos GetStick(int id)
        {
            return new StickPos(0, 0);
        }

        public override void Poll(HidDeviceData data)
        {
            return;
        }

        public override void Poll()
        {
            return;
        }
    }
}
