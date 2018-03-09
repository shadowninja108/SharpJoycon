using System;

using SharpJoycon.Interfaces.Joystick;
using SharpJoycon.Interfaces.Joystick.Controllers;

using static SharpJoycon.Interfaces.Joystick.InputJoystick;

namespace SharpJoycon
{
    public class GluedJoycons
    {

        private NintendoController leftJoycon;
        private NintendoController rightJoycon;

        public GluedJoycons(NintendoController leftJoycon, NintendoController rightJoycon)
        {
            this.leftJoycon = leftJoycon;
            this.rightJoycon = rightJoycon;
        }

        public int ButtonCount()
        {
            Controller leftController = leftJoycon.GetController().GetJoystick();
            Controller rightController = rightJoycon.GetController().GetJoystick();
            return Math.Max(leftController.ButtonCount(), rightController.ButtonCount()); // rather keep the button count in one place (they are both the same value)
        }

        public uint GetButtonData()
        {
            Controller leftController = leftJoycon.GetController().GetJoystick();
            //Controller rightController = rightJoycon.GetController().GetJoystick();
            return leftController.GetButtonData(); //| rightController.GetButtonData();
        }

        public POVDirection GetPov()
        {
            Controller leftController = leftJoycon.GetController().GetJoystick();
            return leftController.GetPov(0);
        }

        public StickPos GetStick(int id)
        {
            Controller leftController = leftJoycon.GetController().GetJoystick();
            Controller rightController = rightJoycon.GetController().GetJoystick();
            switch (id)
            {
                case 0:
                    return leftController.GetStick(0);
                case 1:
                    return rightController.GetStick(0);
                default:
                    return new InputJoystick.StickPos();
            }
        }

        public void Poll()
        {
            leftJoycon.Poll();
            rightJoycon.Poll();
        }
    }
}
