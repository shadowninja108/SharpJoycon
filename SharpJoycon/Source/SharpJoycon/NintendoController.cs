using HidSharp;
using SharpJoycon.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public NintendoController(HidDevice device)
        {
            this.device = device;
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

        public void Poll()
        {
            HIDInterface.PacketData data = GetHID().ReadPacket();
            GetHID().Poll(data);
            GetSPI().Poll(data);
            GetCommands().Poll(data);
            GetConfig().Poll(data);
            GetController().Poll(data);
            GetHardware().Poll(data);
        }

        public static List<NintendoController> Discover()
        {
            List<HidDevice> list = DeviceList.Local.GetHidDevices().ToList();
            List<NintendoController> controllers = new List<NintendoController>();
            foreach(HidDevice device in list){
                if (device.VendorID == 0x057e)
                    controllers.Add(new NintendoController(device));
            }
            return controllers;
        }
    }
}
