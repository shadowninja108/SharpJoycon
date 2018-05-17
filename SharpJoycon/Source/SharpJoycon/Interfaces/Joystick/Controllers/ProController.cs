using static SharpJoycon.Interfaces.ConfigurationInterface;
using static SharpJoycon.Interfaces.HIDInterface;

namespace SharpJoycon.Interfaces.Joystick.Controllers
{
    public class ProController : Controller
    {
        private LeftJoycon left;
        private RightJoycon right;

        public ProController(NintendoController controller) : base(controller)
        {
            left = new LeftJoycon(controller);
            right = new RightJoycon(controller);
        }

        public ProController(LeftJoycon left, RightJoycon right) : base(null) // super doesn't matter in this case?
        {
            this.left = left;
            this.right = right;
        }

        public override Buttons GetButtons()
        {
            // i hope there is a better way to do this
            Buttons combined = default(Buttons);
            Buttons leftButtons = left.GetButtons();
            Buttons rightButtons = right.GetButtons();
            combined.A = leftButtons.A | rightButtons.A;
            combined.B = leftButtons.B | rightButtons.B;
            combined.capture = leftButtons.capture | rightButtons.capture;
            combined.home = leftButtons.home | rightButtons.home;
            combined.L = leftButtons.L | rightButtons.L;
            combined.minus = leftButtons.minus | rightButtons.minus;
            combined.plus = leftButtons.plus | rightButtons.plus;
            combined.povDown = leftButtons.povDown | rightButtons.povDown;
            combined.povLeft = leftButtons.povLeft | rightButtons.povLeft;
            combined.povRight = leftButtons.povRight | rightButtons.povRight;
            combined.povUp = leftButtons.povUp | rightButtons.povUp;
            combined.R = leftButtons.R | rightButtons.R;
            combined.SL = leftButtons.SL | rightButtons.SL;
            combined.SR = leftButtons.SR | rightButtons.SR;
            combined.stickL = leftButtons.stickL | rightButtons.stickL;
            combined.stickR = leftButtons.stickR | rightButtons.stickR;
            combined.X = leftButtons.X | rightButtons.X;
            combined.Y = leftButtons.Y | rightButtons.Y;
            combined.ZL = leftButtons.ZL | rightButtons.ZL;
            combined.ZR = leftButtons.ZR | rightButtons.ZR;
            return combined;
        }

        public override POVDirection GetPov(int id)
        {
            return left.GetPov(id);
        }

        public override Position GetStick(int id)
        {
            if (id == 0)
                return left.GetStick(0);
            else if (id == 1)
                return right.GetStick(0);
            else
                return default(Position);
        }

        public override int GetStickConfigOffset(int id, ConfigurationType type)
        {
            if (id == 0)
                return left.GetStickConfigOffset(0, type);
            else if (id == 1)
                return right.GetStickConfigOffset(0, type);
            else
                return -1;
        }

        public override AnalogConfiguration ParseAnalogConfiguration(int id, int[] data)
        {
            if (id == 0)
                return left.ParseAnalogConfiguration(0, data);
            else if (id == 1)
                return right.ParseAnalogConfiguration(0, data);
            else
                return default(AnalogConfiguration);
        }

        public override void Poll(PacketData data)
        {
            left.Poll(data);
            right.Poll(data);
        }
    }
}
