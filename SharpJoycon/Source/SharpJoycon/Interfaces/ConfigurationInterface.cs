using SharpJoycon.Interfaces.Joystick.Controllers;

namespace SharpJoycon.Interfaces
{
    public class ConfigurationInterface : AbstractInterface
    {
        private ControllerInterface controller;
        private SPIInterface spi;
        private HardwareInterface hardware;

        public ConfigurationInterface(NintendoController controller) : base(controller)
        {
            this.controller = controller.GetController();
            this.spi = controller.GetSPI();
            this.hardware = controller.GetHardware();
        }

        public AnalogConfiguration GetAnalogConfiguration(ConfigurationType type)
        {
            Controller joystick = controller.GetJoystick();
            byte mode = hardware.GetReportMode();
            byte[] data = spi.GetAccessor().Read(joystick.GetAnalogConfigOffset(type), 0x12);
            int[] parsedData = ParseAnalogConfiguration(data);
            return joystick.ParseAnalogConfiguration(parsedData);
        }

        public int[] ParseAnalogConfiguration(byte[] data)
        {
            int[] config = new int[6];
            config[0] = (data[1] << 8) & 0xF00 | data[0];
            config[1] = (data[2] << 4) | (data[1] >> 4);
            config[2] = (data[4] << 8) & 0xF00 | data[3];
            config[3] = (data[5] << 4) | (data[4] >> 4);
            config[4] = (data[7] << 8) & 0xF00 | data[6];
            config[5] = (data[8] << 4) | (data[7] >> 4);

            return config;
        }

        public override void Poll(HIDInterface.PacketData data)
        {
            // nothing to read
        }

        public struct AnalogConfiguration
        {
            public int xMax, yMax, xCenter, yCenter, xMin, yMin;
        }

        public enum ConfigurationType
        {
            Factory, User
        }
    }
}
