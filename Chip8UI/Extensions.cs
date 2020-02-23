using System.Windows.Input;

namespace Chip8UI
{
    public static class Extensions
    {
        public static char ToChar(this Key key)
        {
            char c = '\0';
            if ((key >= Key.A) && (key <= Key.Z))
            {
                c = (char)((int)'a' + (int)(key - Key.A));
            }

            else if ((key >= Key.D0) && (key <= Key.D9))
            {
                c = (char)((int)'0' + (int)(key - Key.D0));
            }

            return c;
        }
    }
}
