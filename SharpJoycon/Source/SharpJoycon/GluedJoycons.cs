using System;
using System.Collections.Generic;
using SharpJoycon.Interfaces;
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
            if (leftJoycon == null)
                Console.WriteLine("Left joycon is missing!");
            this.rightJoycon = rightJoycon;
            if (rightJoycon == null)
                Console.WriteLine("Right joycon is missing!");
        }

        public int ButtonCount()
        {
            Controller leftController = GetLeftJoycon();
            Controller rightController = GetRightJoycon();
            return Math.Max(leftController.ButtonCount(), rightController.ButtonCount()); // rather keep the button count in one place (they are both the same value)
        }

        public uint GetButtonData()
        {
            Controller leftController = GetLeftJoycon();
            Controller rightController = GetRightJoycon();
            return leftController.GetButtonData() | rightController.GetButtonData();
        }

        public List<POVDirection> GetPov()
        {
            Controller leftController = GetLeftJoycon();
            return leftController.GetPov(0);
        }

        public StickPos GetStick(int id)
        {
            Controller leftController = GetLeftJoycon();
            Controller rightController = GetRightJoycon();
            switch (id)
            {
                case 0:
                    return leftController.GetStick(0);
                case 1:
                    return rightController.GetStick(0);
                default:
                    return new StickPos();
            }
        }

        public Controller GetLeftJoycon()
        {
            if (!ReferenceEquals(leftJoycon, null))
                return leftJoycon.GetController().GetJoystick();
            else
               return new NullController();
        }

        public Controller GetRightJoycon()
        {
            if (!ReferenceEquals(rightJoycon, null))
                return rightJoycon.GetController().GetJoystick();
            else
                return new NullController();
        }

        public void Poll()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if(!ReferenceEquals(leftJoycon, null))
                leftJoycon.Poll();
            //stopwatch.Stop();
            //Console.WriteLine($"Polling the left joycon took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
            //stopwatch.Start();
            if (!ReferenceEquals(rightJoycon, null))
                rightJoycon.Poll();
            //stopwatch.Stop();
            //Console.WriteLine($"Polling the right joycon took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
        }
    }
}
