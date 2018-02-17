using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Joycon_Glue.Source.JoyconLib.Interfaces.Joystick.Controller;
using Joycon_Glue.Source.Joystick.Controllers;
using static Joycon_Glue.InputJoystick;

namespace Joycon_Glue.Source.Joystick
{
    class GluedJoycons
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

        public bool GetButton(int id)
        {
            Controller leftController = leftJoycon.GetController().GetJoystick();
            Controller rightController = rightJoycon.GetController().GetJoystick();
            return leftController.GetButton(id) || rightController.GetButton(id);
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
