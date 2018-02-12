using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Joycon_Glue.Source.Joystick.Controllers;

namespace Joycon_Glue.Source.Joystick
{
    class GluedJoycons
    {

        private NintendoController leftJoycon;
        private NintendoController rightJoycon;

        public GluedJoycons(LeftJoycon leftJoycon, RightJoycon rightJoycon)
        {
            this.leftJoycon = (NintendoController) leftJoycon ?? new UnknownNintendoController(null);
            this.rightJoycon = (NintendoController) rightJoycon ?? new UnknownNintendoController(null);
        }

        public int ButtonCount()
        {
            return Math.Max(leftJoycon.ButtonCount(), rightJoycon.ButtonCount()); // rather keep the button count in one place (they are both the same value)
        }

        public bool GetButton(int id)
        {
            return leftJoycon.GetButton(id) || rightJoycon.GetButton(id);
        }

        public InputJoystick.POVDirection GetPov()
        {
            return leftJoycon.GetPov();
        }

        public NintendoController.StickPos GetStick(int id)
        {
            switch (id)
            {
                case 0:
                    return leftJoycon.GetStick(0);
                case 1:
                    return rightJoycon.GetStick(0);
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
