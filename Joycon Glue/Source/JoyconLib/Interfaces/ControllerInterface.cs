using Joycon_Glue.Source.JoyconLib.Interfaces.Joystick.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joycon_Glue.Source.Joystick.Controllers.Interfaces
{
    //TODO: name could be better
    public class ControllerInterface : AbstractInterface
    {
        private Controller joystick;

        public ControllerInterface(NintendoController controller) : base(controller)
        {
            switch(controller.GetHardware().GetControllerType()){
                case HardwareInterface.ControllerType.LeftJoycon:
                    joystick = new LeftJoycon(controller);
                    break;
                case HardwareInterface.ControllerType.RightJoycon:
                    joystick = new RightJoycon(controller);
                    break;
                default:
                    Console.WriteLine("Unsupported controller! Sorry!");
                    break;
            }
        }

        public Controller GetJoystick()
        {
            return joystick;
        }

        public override void Poll(HIDInterface.PacketData data)
        {
            GetJoystick().Poll(data);
        }
    }
}
