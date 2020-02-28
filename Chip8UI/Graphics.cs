using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChipEightEmu
{
    public class Graphics
    {
        public byte[,] Memory = new byte[64, 32];

        public WriteableBitmap Bitmap { get; set; }

        public Graphics()
        {
            Bitmap = new WriteableBitmap(640, 320, 96, 96, PixelFormats.Bgr32, null);
        }

        public void Draw()
        {
            Bitmap.Clear(Colors.Black);

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                { 
                    if (Memory[x, y] > 0)
                    {
                        Bitmap.SetPixel(x, y, Colors.LightGreen);            
                    }                                            
                }
            }
        }
    }
}
