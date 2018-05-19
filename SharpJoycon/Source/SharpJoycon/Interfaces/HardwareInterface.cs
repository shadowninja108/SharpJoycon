using System;
using System.IO;
using SharpJoycon.Interfaces.SPI;
using SharpJoycon.Utilities;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    //TODO: interface seems a bit overloaded, might split later
    public class HardwareInterface : AbstractInterface
    {
        private CommandInterface command;
        private SPIInterface spi;
        private DeviceInfo deviceInfo;

        private byte reportMode;

        public HardwareInterface(NintendoController controller) : base(controller)
        {
            command = controller.GetCommands();
            spi = controller.GetSPI();
        }

        public ControllerType GetControllerType()
        {
            switch (GetDeviceInfo().type)
            {
                case 1:
                    return ControllerType.LeftJoycon;
                case 2:
                    return ControllerType.RightJoycon;
                case 3:
                    return ControllerType.ProController;
                default:
                    return ControllerType.Unknown;
            }
        }

        public DeviceInfo GetDeviceInfo()
        {
            if(deviceInfo.Equals(default(DeviceInfo)))
            {
                byte[] data = command.SendSubcommand(0x01, 0x02, null).Data;
                MemoryStream stream = new MemoryStream(data);
                deviceInfo = new DeviceInfo();
                deviceInfo.firmware = BitConverter.ToInt16(stream.Take(2), 0);
                deviceInfo.type = stream.Take();
                deviceInfo.unknown1 = stream.Take();
                deviceInfo.macAddress = stream.Take(5);
                deviceInfo.unknown2 = stream.Take();
                deviceInfo.SPIColorsChanged = stream.Take();
            }
            return deviceInfo;
        }

        public void SetPlayerLights(PlayerLightState state)
        {
            byte[] data = new byte[2];
            switch (state)
            {
                // empty requires all zeros, so it will default to that anyway!
                case PlayerLightState.Player1:
                    data[0] = 1;
                    break;
                case PlayerLightState.Player2:
                    data[0] = 2;
                    break;
                case PlayerLightState.Player3:
                    data[0] = 4;
                    break;
                case PlayerLightState.Player4:
                    data[0] = 8;
                    break;
                case PlayerLightState.Keep:
                    data[0] = 200;
                    data[1] = 10;
                    break;
            }
            command.SendSubcommand(0x1, 0x30, data);
        }

        // should be moved to RumbleInterface when implemented
        public void SetVibration(bool enable)
        {
            command.SendSubcommand(0x1, 0x48, new byte[] { Convert.ToByte(enable) });
        }

        // should be moved to IMUInterface when implemented
        public void SetIMU(bool enable)
        {
            command.SendSubcommand(0x01, 0x40, new byte[] { Convert.ToByte(enable) });
        }

        // will change to enum when all modes are documented
        public void SetReportMode(byte mode)
        {
            reportMode = mode;
            command.SendSubcommand(0x01, 0x03, new byte[] { mode });
        }

        public byte GetReportMode()
        {
            // don't know how to check so i just hope that it never changes
            return reportMode;
        }

        //should this go in ConfigurationInterface?
        public Color GetBodyColor()
        {
            SPIStream stream = spi.GetStream();
            stream.Seek(0x6050, SeekOrigin.Begin);
            return new Color(stream.Read(0x3));
        }
        public Color GetButtonColor()
        {
            SPIStream stream = spi.GetStream();
            stream.Seek(0x6053, SeekOrigin.Begin);
            return new Color(stream.Read(0x3));
        }

        public void SetBodyColor(Color c)
        {
            SPIStream stream = spi.GetStream();
            stream.Seek(0x6050, SeekOrigin.Begin);
            stream.Write(c.ToBytes());
        }

        public void SetButtonColor(Color c)
        {
            SPIStream stream = spi.GetStream();
            stream.Seek(0x6053, SeekOrigin.Begin);
            stream.Write(c.ToBytes());
        }

        public override void Poll(PacketData data)
        {
            // nothing to read
        }

        public enum PlayerLightState
        {
            Empty, Player1, Player2, Player3, Player4, Keep
        }


        public enum ControllerType
        {
            LeftJoycon, RightJoycon, ProController, Unknown
        }

        public struct DeviceInfo
        {
            public int firmware;
            public byte type, unknown1;
            public byte[] macAddress;
            public byte unknown2, SPIColorsChanged;
        }

    }
}
