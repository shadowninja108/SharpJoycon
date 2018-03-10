using SharpJoycon.Interfaces.SPI;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    public class SPIInterface : AbstractInterface
    {

        private SPIStream stream;

        public SPIInterface(NintendoController controller) : base(controller)
        {
            stream = new SPIStream(controller);
        }

        public SPIStream GetStream()
        {
            return stream;
        }

        public override void Poll(PacketData data)
        {
            // nothing to read
        }

    }
}
