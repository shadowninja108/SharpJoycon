using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections.Generic;

using vJoyInterfaceWrap;
using HidLibrary;

using Joycon_Glue.Source.Joystick;

using Joycon_Glue.Source.Joystick.Controllers.Interfaces;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HardwareInterface;


namespace Joycon_Glue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        vJoy.JoystickState state;
        vJoy joystick;
        GluedJoycons glued;
        private uint vjd = 0;

        public MainWindow()
        {
            joystick = new vJoy();
            if (joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy enabled!");
                Console.WriteLine("vJoy ver: " + joystick.GetvJoyVersion());
                Console.WriteLine("Manufacturer: " + joystick.GetvJoyManufacturerString());
                Console.WriteLine("Product: " + joystick.GetvJoyProductString());
                Console.WriteLine("Serial No. : " + joystick.GetvJoySerialNumberString());
                Console.WriteLine("Searching for available joystick...");
                for (uint i = 1; i < 16; i++)
                {
                    Console.WriteLine($"Trying joystick {i}...");
                    var status = joystick.GetVJDStatus(i);
                    Console.WriteLine($"Joystick status: {status}");
                    if (new VjdStat[] { VjdStat.VJD_STAT_OWN, VjdStat.VJD_STAT_FREE }.Contains(status))// is it owned already or ready to own?
                    {
                        vjd = i;
                        break;
                    }
                }
                if (vjd != 0) // joystick id can never be 0, not using -1 to avoid casting
                {
                    Console.WriteLine("Attempting to acquire joystick...");
                    if (joystick.AcquireVJD(vjd))
                    {
                        Console.WriteLine("Success!");
                        Thread thread = new Thread(run)
                        {
                            IsBackground = true
                        };
                        thread.Start();
                    }
                    else
                    {
                        Console.WriteLine("Failed to acquire joystick!");
                    }
                }
                else
                {
                    Console.WriteLine("No joysticks available!");
                }

            }
            else
            {
                Console.WriteLine("vJoy not enabled!");
            }
        }

        private void run()
        {
            // pretty much following this guy's implementation
            // https://github.com/mfosse/JoyCon-Driver

           IList<HidDevice> devices = HidDevices.Enumerate(0x057e).ToList();
            NintendoController leftJoycon = null;
            NintendoController rightJoycon = null;
            foreach (HidDevice device in devices)
            {
                device.OpenDevice();
                Console.WriteLine("Detected Nintendo device.");
                NintendoController controller = new NintendoController(device);
                HardwareInterface hardware = controller.GetHardware();
                CommandInterface command = controller.GetCommands();
               // command.SendSubcommand(0x1, 0x1, new byte[] { 0x1 });
                //command.SendSubcommand(0x1, 0x1, new byte[] { 0x2 });
                //command.SendSubcommand(0x1, 0x1, new byte[] { 0x3 });
                hardware.SetReportMode(0x3F); // 60hz update mode
                hardware.SetVibration(true);
                hardware.SetIMU(true);
                hardware.SetPlayerLights(PlayerLightState.Player1);

                switch (hardware.GetControllerType())
                {
                    case ControllerType.LeftJoycon:
                        Console.WriteLine("Left Joycon detected.");
                        leftJoycon = controller;
                        break;
                    case ControllerType.RightJoycon:
                        Console.WriteLine("Right Joycon detected.");
                        rightJoycon = controller;
                        break;
                    default:
                        Console.WriteLine("Unrecognized device.");
                        break;
                }
                hardware.SetReportMode(0x30); // 60hz update mode
            }
            Console.WriteLine("Found all devices.");
            glued = new GluedJoycons(leftJoycon, rightJoycon);
            joystick.ResetVJD(vjd);
            while (true)
            {
                glued.Poll();
                state = new vJoy.JoystickState();
                //Console.WriteLine($"Elasped time: {stopwatch.Elapsed.TotalSeconds }");

                //buttons
                state.Buttons = glued.GetButtonData();


                //pov
                state.bHats = 0;
                //int povValue = (int)glued.GetPov() -1;
                // joystick.SetDiscPov(povValue, vjd, 1);

                //sticks
                InputJoystick.StickPos leftPos = glued.GetStick(0);
                state.AxisX = leftPos.x;
                state.AxisY = leftPos.y;

                InputJoystick.StickPos rightPos = glued.GetStick(1);
                state.AxisXRot = rightPos.x;
                state.AxisYRot = rightPos.y;

                joystick.UpdateVJD(vjd, ref state);
                //Thread.Sleep(16);
            }
        }
    }    
}
