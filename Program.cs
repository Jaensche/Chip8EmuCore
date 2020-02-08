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

        static void Main(string[] args)
        {
            CPU chip8 = new CPU();
            chip8.Init();

            chip8.Load(File.ReadAllBytes(@"R:\DEV\Chip8EmuCore\games\IBM.ch8"));

            Console.SetWindowSize(64, 32);
            Console.SetBufferSize(64, 32);

            Stopwatch stopWatch = new Stopwatch();
            int microsecondsPerCycle = 1000000 / ClockFrequency;
            int cyclesPer60Hz = ClockFrequency / CounterFrequency;

            bool emulationIsRunning = true;     
            while (emulationIsRunning)
            {
                stopWatch.Restart();

                chip8.Cycle(cyclesPer60Hz);

                long elapsedMicroseconds = stopWatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                int millisecondsAhead = (int)((elapsedMicroseconds - microsecondsPerCycle) / 1000);
                if (millisecondsAhead > 0)
                {
                    Thread.Sleep(millisecondsAhead);
                }
            }
        }
    }
}
