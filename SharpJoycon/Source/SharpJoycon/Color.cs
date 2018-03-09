namespace SharpJoycon.Source
{
    // because a crossplatform Color class is too much work for Microsoft
    public class Color
    {
        private byte r, g, b, a;

        public Color(byte r, byte g, byte b) : this(r, g, b, 255){
            
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public byte GetRed(){
            return r;
        }

        public byte GetGreen(){
            return g;
        }

        public byte GetBlue(){
            return b;
        }

        public byte GetAlpha(){
            return a;
        }
    }
}
