using System;
using System.IO;

using SharpJoycon.Interfaces.Joystick.Controllers;
using SharpJoycon.Interfaces.SPI;
using SharpJoycon.Utilities;

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
            spi = controller.GetSPI();
            hardware = controller.GetHardware();
        }

        public AnalogConfiguration GetAnalogConfiguration(ConfigurationType type)
        {
            Controller joystick = controller.GetJoystick();
            SPIStream stream = spi.GetStream();
            stream.Seek(joystick.GetStickConfigOffset(type), SeekOrigin.Begin);
            byte[] data = stream.Read(0, 0x12);
            int[] parsedData = ParseAnalogConfiguration(data);
            return joystick.ParseAnalogConfiguration(parsedData);
        }

        public StickParameters GetStickParameters()
        {
            SPIStream stream = spi.GetStream();
            stream.Seek(0x6086, SeekOrigin.Begin);
            byte[] data = stream.Read(0, 0x11);
            StickParameters param = new StickParameters();
            param.deadzone = data[2];
            param.rangeRatio = data[3];
            return param;
        }

        public IMUConfiguration GetIMUConfiguration(ConfigurationType type)
        {
            SPIStream stream = spi.GetStream();
            int offset;
            switch (type)
            {
                case ConfigurationType.Factory:
                    offset = 0x6020;
                    break;
                case ConfigurationType.User:
                    offset = 0x8028;
                    break;
                default:
                    goto case ConfigurationType.Factory;
            }
            stream.Seek(offset, SeekOrigin.Begin);
            int[] data = stream.Read(0, 0x18).ToInt16();
            IMUConfiguration config = new IMUConfiguration();
            config.xAcc.origin = data[0];
            config.yAcc.origin = data[1];
            config.zAcc.origin = data[2];
            config.xAcc.sensitivity = data[3];
            config.yAcc.sensitivity = data[4];
            config.zAcc.sensitivity = data[5];
            config.xGyro.origin = data[6];
            config.yGyro.origin = data[7];
            config.zGyro.origin = data[8];
            config.xGyro.sensitivity = data[9];
            config.yGyro.sensitivity = data[10];
            config.zGyro.sensitivity = data[11];
            return config;
        }

        public int[] ParseAnalogConfiguration(byte[] data)
        {
            //uint16
            ushort[] unsignedData = new ushort[data.Length];
            for(int i = 0; i < data.Length; i++) {
                unsignedData[i] = data[i];
            }
            int[] config = new int[6];
            config[0] = (unsignedData[1] << 8) & 0xF00 | unsignedData[0];
            config[1] = (unsignedData[2] << 4) | (unsignedData[1] >> 4);
            config[2] = (unsignedData[4] << 8) & 0xF00 | unsignedData[3];
            config[3] = (unsignedData[5] << 4) | (unsignedData[4] >> 4);
            config[4] = (unsignedData[7] << 8) & 0xF00 | unsignedData[6];
            config[5] = (unsignedData[8] << 4) | (unsignedData[7] >> 4);

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

        public struct StickParameters
        {
            public int deadzone, rangeRatio;
        }

        public struct IMUConfiguration
        {
            public AxisCalibration xAcc, yAcc, zAcc, xGyro, yGyro, zGyro;
        }

        public struct AxisCalibration
        {
            public int origin, sensitivity;
        }

        public enum ConfigurationType
        {
            Factory, User
        }
    }
}
