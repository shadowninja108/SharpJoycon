using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Joycon_Glue.Source.Joystick.Controllers.Interfaces.HIDInterface;

namespace Joycon_Glue.Source.Joystick.Controllers.Interfaces
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
                PacketData packet;
                byte[] data;
                while (true)
                {
                    packet = command.SendSubcommand(0x01, 0x02, null);
                    byte[] header = packet.header;
                    data = packet.data;
                    if (header[13] == 0x82 && header[14] == 02)
                        break;
                }
                deviceInfo = new DeviceInfo();
                byte[] firmware = new byte[2];
                Array.Copy(data, firmware, 2);
                deviceInfo.firmware = BitConverter.ToInt16(firmware, 0);
                deviceInfo.type = data[2];
                deviceInfo.unknown1 = data[3];
                deviceInfo.macAddress = new byte[5];
                Array.Copy(data, 5, deviceInfo.macAddress, 0, 5);
                deviceInfo.unknown2 = data[10];
                deviceInfo.SPIColorsChanged = data[11];
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

        public void SetVibration(bool enable)
        {
            command.SendSubcommand(0x1, 0x48, new byte[] { Convert.ToByte(enable) });
        }

        // should be moved to IMUInterface when implemented?
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
            // hopefully doesn't change without being told
            return reportMode;
        }

        //should this go in ConfigurationInterface?
        public Color getBodyColor()
        {
            byte[] bytes = spi.GetAccessor().Read(0x6050, 0x3);
            return Color.FromRgb(bytes[0], bytes[1], bytes[2]);
        }

        public override void Poll(HIDInterface.PacketData data)
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
