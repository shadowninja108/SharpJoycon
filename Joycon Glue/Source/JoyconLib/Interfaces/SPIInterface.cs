using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Joycon_Glue.Source.Joystick.Controllers.Interfaces;
using Joycon_Glue.Source.JoyconLib.Interfaces;

namespace Joycon_Glue.Source.Joystick.Controllers
{
    public class SPIInterface : AbstractInterface
    {

        private SPIAccessor accessor;

        public SPIInterface(NintendoController controller) : base(controller)
        {
            accessor = new SPIAccessor(controller);
        }

        public SPIAccessor GetAccessor()
        {
            return accessor;
        }

        public override void Poll(HIDInterface.PacketData data)
        {
            // nothing to read
        }

    }
}
