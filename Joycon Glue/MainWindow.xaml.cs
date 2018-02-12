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
                    if (new VjdStat[] { VjdStat.VJD_STAT_OWN, VjdStat.VJD_STAT_FREE}.Contains(status))// is it owned already or ready to own?
                    {
                        vjd = i;
                        break;
                    }
                }
                if (vjd != 0) // joystick id can never be 0, not using -1 to avoid casting
                {
                    Console.WriteLine("Attempting to acquire joystick...");
                    if(joystick.AcquireVJD(vjd))
                    {
                        Console.WriteLine("Success!");
                        Thread thread = new Thread(run)
                        {
                            IsBackground = true
                        };
                        thread.Start();
                    } else
                    {
                        Console.WriteLine("Failed to acquire joystick!");
                    }
                } else
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
            LeftJoycon leftJoycon = null;
            RightJoycon rightJoycon = null;
            foreach (HidDevice device in devices)
            {
                device.OpenDevice();
                Console.WriteLine("Detected Nintendo device.");
                SimplifiedHidDevice simple = new SimplifiedHidDevice(device);
                NintendoController controller = new UnknownNintendoController(simple);
                controller.SetReportMode(0x3F);
                switch (device.Attributes.ProductId)
                {
                    case 8198:
                        leftJoycon = new LeftJoycon(simple);
                        break;
                    case 8199:
                        rightJoycon = new RightJoycon(simple);
                        break;
                }
                Console.WriteLine($"SPI read: {controller.getBodyColor()}");
                controller.SetPlayerLights(NintendoController.PlayerLightState.Player1);
                controller.SetVibration(true);
                controller.SetIMU(true);
                controller.SetReportMode(0x30);
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
                int povValue = -1;
                switch (glued.GetPov())
                {
                    case InputJoystick.POVDirection.Up:
                        povValue = 0;
                        break;
                    case InputJoystick.POVDirection.Right:
                        povValue = 1;
                        break;
                    case InputJoystick.POVDirection.Down:
                        povValue = 2;
                        break;
                    case InputJoystick.POVDirection.Left:
                        povValue = 3;
                        break;
                }
                joystick.SetDiscPov(povValue, vjd, 1);

                //sticks
                InputJoystick.StickPos leftPos = glued.GetStick(0);
                InputJoystick.StickPos rightPos = glued.GetStick(1);
                joystick.SetAxis(leftPos.x, vjd, HID_USAGES.HID_USAGE_X);
                joystick.SetAxis(leftPos.y, vjd, HID_USAGES.HID_USAGE_Y);
                joystick.SetAxis(rightPos.x, vjd, HID_USAGES.HID_USAGE_RX);
                joystick.SetAxis(rightPos.x, vjd, HID_USAGES.HID_USAGE_RY);
                Thread.Sleep(20);
            }
        }
        
}
