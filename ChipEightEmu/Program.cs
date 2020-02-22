using System.IO;
using System.Diagnostics;
using System;
using System.Threading;

namespace ChipEightEmu
{
    class Program
    {
        const int ClockFrequency = 540;
        const int CounterFrequency = 60;

        static void Main()
        {
            Keyboard keyboard = new Keyboard();
            Graphics graphics = new Graphics();
            long millisecondsPerCycle = (long)Math.Round(((1000) / (double)ClockFrequency));
            int cyclesPer60Hz = ClockFrequency / CounterFrequency;
            CPU chip8 = new CPU(ref graphics.Memory, ref keyboard.Memory, cyclesPer60Hz);            
            
            chip8.Load(File.ReadAllBytes(@"R:\DEV\Chip8EmuCore\games\Space Flight.ch8"));

            Console.SetWindowSize(65, 33);
            Console.SetBufferSize(65, 33);

            Stopwatch stopWatch = new Stopwatch();
                 

            bool emulationIsRunning = true;     
            while (emulationIsRunning)
            {
                keyboard.ReadKeys();
                stopWatch.Restart();
                bool redraw = chip8.Cycle();
                if (redraw)
                {
                    graphics.DrawGraphics();
                }
                stopWatch.Stop();

                // equal runtime for every cycle
                long elapsedMilliSeconds = stopWatch.ElapsedTicks / (Stopwatch.Frequency / (1000L));

                int millisecondsAhead = (int)(millisecondsPerCycle - elapsedMilliSeconds);
                if (millisecondsAhead > 0)
                {
                    Thread.Sleep(millisecondsAhead);
                }
            }
        }
    }
}
