using System;

using SharpJoycon.Interfaces.Joystick.Controllers;
using static SharpJoycon.Interfaces.HardwareInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    //TODO: name could be better
    public class ControllerInterface : AbstractInterface
    {
        private Controller joystick;
        private ControllerType type;

        public ControllerInterface(NintendoController controller) : base(controller)
        {
            type = controller.GetHardware().GetControllerType();
            CreateJoystick();
        }

        public Controller CreateJoystick()
        {
            switch (type)
            {
                case ControllerType.LeftJoycon:
                    joystick = new LeftJoycon(controller);
                    break;
                case ControllerType.RightJoycon:
                    joystick = new RightJoycon(controller);
                    break;
                case ControllerType.ProController:
                    joystick = new ProController(controller);
                    break;
                default:
                    Console.WriteLine("Unsupported controller! Sorry!");
                    break;
            }
            return joystick;
        }

        public Controller GetJoystick()
        {
            return joystick;
        }

        // will override current stored Joystick
        public Controller CombineWith(NintendoController controller)
        {
            LeftJoycon left = null;
            RightJoycon right = null;
            switch (type)
            {
                case ControllerType.LeftJoycon:
                    left = (LeftJoycon) GetJoystick();
                    right = (RightJoycon)controller.GetController().GetJoystick();
                    break;
                case ControllerType.RightJoycon:
                    left = (LeftJoycon)controller.GetController().GetJoystick();
                    right = (RightJoycon)GetJoystick();
                    break;
            }
            return new ProController(left, right);
        }

        public override void Poll(PacketData data)
        {
            GetJoystick()?.Poll(data);
        }
    }
}
