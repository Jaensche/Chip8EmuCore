using System.IO;
using System.Threading;
using System;

namespace ChipEightEmu
{
    class Program
    {
        static void Main(string[] args)
        {
            CPU chip8 = new CPU();
            chip8.Init();

            chip8.Load(File.ReadAllBytes("C:\\Users\\Jan\\Downloads\\rand.ch8"));

            Console.SetWindowSize(64, 32);
            Console.SetBufferSize(64, 32);            

            while (true)
            {
                chip8.Cycle();


                
            }
        }
    }
}
