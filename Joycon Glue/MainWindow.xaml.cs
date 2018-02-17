using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections.Generic;

using vJoyInterfaceWrap;
using HidLibrary;
using System.Text;
using System.Runtime.InteropServices;
using Joycon_Glue.Source.Joystick;
using Joycon_Glue.Source.Joystick.Controllers;
using Microsoft.Win32.SafeHandles;
using System.IO;
using Joycon_Glue.Source.Joystick.Controllers.Interfaces;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HardwareInterface;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        vJoy joystick;
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
            GluedJoycons glued = new GluedJoycons(leftJoycon, rightJoycon);
            joystick.ResetVJD(vjd);
            while (true)
            {
                glued.Poll();

                //buttons
                for (int i = 1; i <= glued.ButtonCount(); i++)
                {
                    // Console.WriteLine($"{i} = {device.GetButton(i)}");
                    joystick.SetBtn(glued.GetButton(i), vjd, (uint)i);
                }

                //pov
                int povValue = (int)glued.GetPov() -1;
                joystick.SetDiscPov(povValue, vjd, 1);

                //sticks
                InputJoystick.StickPos leftPos = glued.GetStick(0);
                InputJoystick.StickPos rightPos = glued.GetStick(1);
                joystick.SetAxis(leftPos.x, vjd, HID_USAGES.HID_USAGE_X);
                joystick.SetAxis(leftPos.y, vjd, HID_USAGES.HID_USAGE_Y);
                joystick.SetAxis(rightPos.x, vjd, HID_USAGES.HID_USAGE_RX);
                joystick.SetAxis(rightPos.y, vjd, HID_USAGES.HID_USAGE_RY);
                Thread.Sleep(16);
            }
        }
    }    
}
