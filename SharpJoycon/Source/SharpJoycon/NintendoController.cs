using HidSharp;
using SharpJoycon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

//joycon m e n
namespace SharpJoycon
{
    public class NintendoController
    {
        private HidDevice device;

        private HIDInterface hid;
        private CommandInterface command;
        private SPIInterface spi;
        private ConfigurationInterface config;
        private HardwareInterface hardware;
        private ControllerInterface controller;
        private HomeLEDInterface homeLED;
        private IMUInterface imu;

        private ConnectionType connectionType;

        public NintendoController(HidDevice device)
        {
            this.device = device;

            CommandInterface command = GetCommands();
            try
            {
                command.SendCommand(0x80, new byte[] { 0x01 }, 1);
                connectionType = ConnectionType.USB;
            } catch (Exception)
            {
                connectionType = ConnectionType.Bluetooth;
            }
            if(connectionType == ConnectionType.USB)
            {
                command.SendCommand(0x80, new byte[] { 0x02 }, 1);
                command.SendCommand(0x80, new byte[] { 0x03 }, 1);
                command.SendCommand(0x80, new byte[] { 0x02 }, 1);
                command.SendCommand(0x80, new byte[] { 0x04 }, 1);
            }
        }

        public HidDevice GetRawHID() => device;

        public HIDInterface GetHID()
        {
            hid = hid ?? new HIDInterface(this);
            return hid;
        }

        public SPIInterface GetSPI()
        {
            spi = spi ?? new SPIInterface(this);
            return spi;
        }

        public CommandInterface GetCommands()
        {
            command = command ?? new CommandInterface(this);
            return command;
        }

        public ConfigurationInterface GetConfig()
        {
            config = config ?? new ConfigurationInterface(this);
            return config;
        }

        public ControllerInterface GetController()
        {
            controller = controller ?? new ControllerInterface(this);
            return controller;
        }

        public HardwareInterface GetHardware()
        {
            hardware = hardware ?? new HardwareInterface(this);
            return hardware;
        }

        public HomeLEDInterface GetHomeLED()
        {
            homeLED = homeLED ?? new HomeLEDInterface(this);
            return homeLED;
        }

        public IMUInterface GetIMU()
        {
            imu = imu ?? new IMUInterface(this);
            return imu;
        }

        public void Poll()
        {
            HIDInterface.PacketData data = GetHID().ReadPacket();
            GetHID().Poll(data);
            GetSPI().Poll(data);
            GetCommands().Poll(data);
            GetConfig().Poll(data);
            GetController().Poll(data);
            GetHardware().Poll(data);
            GetHomeLED().Poll(data);
            GetIMU().Poll(data);
        }

        public static List<NintendoController> Discover()
        {
            List<HidDevice> list = DeviceList.Local.GetHidDevices().ToList();
            List<NintendoController> controllers = new List<NintendoController>();
            foreach(HidDevice device in list){
                if (device.VendorID == 0x057e) //don't filter by ProductID because i use "official" ways of detecting what controller it is
                    controllers.Add(new NintendoController(device));
            }
            Console.WriteLine($"{controllers.Count} controller(s) found.");
            return controllers;
        }

        public enum ConnectionType
        {
            Bluetooth, USB
        }
    }
}
