using System;
using System.Text;

namespace ChipEightEmu
{
    public class Graphics
    {
        public byte[,] Memory = new byte[64, 32];

        public  void DrawGraphics()
        {
            Console.Clear();
            for (int y = 0; y < 32; y++)
            {
                StringBuilder line = new StringBuilder();
                for (int x = 0; x < 64; x++)
                {
                    if (Memory[x, y] != 0)
                    {
                        line.Append("█");
                    }
                    else
                    {
                        line.Append(" ");
                    }
                }
                Console.WriteLine(line.ToString());
            }
        }
    }
}
