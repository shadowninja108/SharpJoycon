using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue.Source.Joystick.Controllers.Interfaces
{
    public abstract class AbstractInterface
    {
        private NintendoController controller;

        public AbstractInterface(NintendoController controller)
        {
            this.controller = controller;
        }

        public abstract void Poll(PacketData data);

        //might add an abstract Poll method later
    }
}
