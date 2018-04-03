using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces
{
    public abstract class AbstractInterface
    {
        protected NintendoController controller;

        public AbstractInterface(NintendoController controller)
        {
            this.controller = controller;
        }

        public abstract void Poll(PacketData data);

        //might add an abstract Poll method later
    }
}
