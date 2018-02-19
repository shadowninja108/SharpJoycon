using HidLibrary;
using Joycon_Glue.Source.Joystick.Controllers.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.ConfigurationInterface;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;
using static NintendoController;

namespace Joycon_Glue.Source.JoyconLib.Interfaces.Joystick.Controller
{
    public abstract class Controller : InputJoystick
    {

        protected NintendoController controller;

        public Controller(NintendoController controller)
        {
            this.controller = controller;
        }

        public abstract Buttons GetButtons();

        public override uint GetButtonData()
        {
            Buttons buttons = GetButtons();
            bool[] bits = new bool[ButtonCount()];
            for (int i = 1; i <= bits.Length; i++)
            {
                bits[i - 1] = GetButton(i);
            }
            BitArray array = new BitArray(bits);
            int[] ints = new int[1];
            array.CopyTo(ints, 0);
            return (uint) ints[0];
        }

        public override bool GetButton(int id)
        {
            Buttons buttons = GetButtons();
            // should follow xbox controller "standard"
            switch (id)
            {
                case 1:
                    return buttons.A;
                case 2:
                    return buttons.B;
                case 3:
                    return buttons.X;
                case 4:
                    return buttons.Y;
                case 5:
                    return buttons.ZL;
                case 6:
                    return buttons.ZR;
                case 7:
                    return buttons.minus;
                case 8:
                    return buttons.plus;
                case 9:
                    return buttons.stickL;
                case 10:
                    return buttons.stickR;
                case 11:
                    return buttons.L;
                case 12:
                    return buttons.R;
                case 13:
                    return buttons.home;
                case 14:
                    return buttons.SL;
                case 15:
                    return buttons.SR;
                case 16:
                    return buttons.capture;
                default:
                    return false;
            }
        }

        public abstract int GetAnalogConfigOffset(ConfigurationType type);
        public abstract AnalogConfiguration ParseAnalogConfiguration(int[] data);

        public override int ButtonCount()
        {
            return 16;
        }

        public struct Buttons
        {
            public bool povUp, povDown, povLeft, povRight, stickL, stickR, capture, home, minus, plus, A, B, X, Y, L, R, ZL, ZR, SL, SR;
        }
    }
}
