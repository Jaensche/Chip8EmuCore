using System.Windows;
using System;
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
            try
            {
                // Reserve the back buffer for updates.
                Bitmap.Lock();
                
                unsafe
                {       
                    for (int y = 0; y < 32; y++)
                    {
                        for (int x = 0; x < 64; x++)
                        {                            
                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = Bitmap.BackBuffer;

                            // Find the address of the pixel to draw.
                            pBackBuffer += y * Bitmap.BackBufferStride;
                            pBackBuffer += x * 4;

                            int color_data;
                            if (Memory[x, y] > 0)
                            {
                                // Compute the pixel's color.
                                color_data = 0 << 16; // R
                                color_data |= 255 << 8;   // G
                                color_data |= 0 << 0;   // B
                            }
                            else
                            {
                                // Compute the pixel's color.
                                color_data = 0 << 16; // R
                                color_data |= 0 << 8;   // G
                                color_data |= 0 << 0;   // B
                            }
                                // Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color_data;                            
                        }
                    }
                }

                // Specify the area of the bitmap that changed.
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, 64, 32));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                Bitmap.Unlock();
            }
        }
    }
}
