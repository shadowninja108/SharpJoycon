using SharpJoycon.Utilities;
using System;
using System.Linq;
using static SharpJoycon.Interfaces.ConfigurationInterface;

namespace SharpJoycon.Interfaces
{
    public class IMUInterface : AbstractInterface
    {
        private IMUConfiguration config;
        private IMUData data;

        public IMUInterface(NintendoController controller) : base(controller)
        {
            config = controller.GetConfig().GetIMUConfiguration(ConfigurationType.Factory);
        }

        public override void Poll(HIDInterface.PacketData packet)
        {
            byte[] data = packet.rawData.Skip(12).Take(12).ToArray();
            int[] ints = data.ToInt16();
            this.data = new IMUData
            {
                xAcc = (int)(ints[0] * (16000d / 65535d / 1000d)),
                yAcc = (int)(ints[1] * (16000d / 65535d / 1000d)),
                zAcc = (int)(ints[2] * (16000d / 65535d / 1000d)),
                xGyro = (int)(ints[3] * (4588d / 65535d)),
                yGyro = (int)(ints[4] * (4588d / 65535d)),
                zGyro = (int)(ints[5] * (4588d / 65535d))
            };


        }

        public IMUData GetData()
        {
            return data;
        }

        public struct IMUData
        {
            public int xAcc, yAcc, zAcc, xGyro, yGyro, zGyro;
        }
    }
}
