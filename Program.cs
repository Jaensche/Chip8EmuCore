using System.IO;
using System;

namespace ChipEightEmu
{
    class Program
    {
        static void Main(string[] args)
        {
            CPU chip8 = new CPU();
            chip8.Init();

            chip8.Load(File.ReadAllBytes("C:\\Users\\Jan\\Downloads\\SQRT.ch8"));

            while (true)
            {
                Console.ReadKey();
                chip8.Cycle();
            }
        }
    }
}
