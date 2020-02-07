using System.IO;
using System.Diagnostics;
using System;
using System.Threading;

namespace ChipEightEmu
{
    class Program
    {
        const int ClockSpeedInHz = 540;
        const int CounterSpeedInHz = 60;

        static void Main(string[] args)
        {
            CPU chip8 = new CPU();
            chip8.Init();

            chip8.Load(File.ReadAllBytes("C:\\Users\\Jan\\Downloads\\IBM.ch8"));

            Console.SetWindowSize(64, 32);
            Console.SetBufferSize(64, 32);

            Stopwatch stopWatch = new Stopwatch();
            int microsecondsPerCycle = 1000000 / ClockSpeedInHz;
            
            bool emulationIsRunning = true;

            int cyclesPer60Hz = ClockSpeedInHz / CounterSpeedInHz;

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
