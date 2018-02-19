using SharpJoycon.Interfaces.SPI;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    public class SPIInterface : AbstractInterface
    {

        private SPIAccessor accessor;

        public SPIInterface(NintendoController controller) : base(controller)
        {
            accessor = new SPIAccessor(controller);
        }

        public SPIAccessor GetAccessor()
        {
            return accessor;
        }

        public override void Poll(PacketData data)
        {
            // nothing to read
        }

    }
}
