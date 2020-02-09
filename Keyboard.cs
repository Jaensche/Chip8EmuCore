using System;

namespace ChipEightEmu
{
    public class Keyboard
    {
        public bool[] Memory = new bool[16];
       
        public Keyboard()
        {           
        }

        public void ReadKeys()
        {
            /*
            chip8
            ╔═══╦═══╦═══╦═══╗
            ║ 1 ║ 2 ║ 3 ║ C ║
            ╠═══╬═══╬═══╬═══╣
            ║ 4 ║ 5 ║ 6 ║ D ║
            ╠═══╬═══╬═══╬═══╣
            ║ 7 ║ 8 ║ 9 ║ E ║
            ╠═══╬═══╬═══╬═══╣
            ║ A ║ 0 ║ B ║ F ║
            ╚═══╩═══╩═══╩═══╝

            mapped to
            ╔═══╦═══╦═══╦═══╗
            ║ 1 ║ 2 ║ 3 ║ 4 ║
            ╠═══╬═══╬═══╬═══╣
            ║ Q ║ W ║ E ║ R ║
            ╠═══╬═══╬═══╬═══╣
            ║ A ║ S ║ D ║ F ║
            ╠═══╬═══╬═══╬═══╣
            ║ Y ║ X ║ C ║ V ║
            ╚═══╩═══╩═══╩═══╝
             */

            for (int i = 0; i < Memory.Length; i++)
            {
                Memory[i] = false;
            }

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                        Memory[1] = true;
                        break;

                    case ConsoleKey.D2:
                        Memory[2] = true;
                        break;

                    case ConsoleKey.D3:
                        Memory[3] = true;
                        break;

                    case ConsoleKey.D4:
                        Memory[12] = true;
                        break;

                    case ConsoleKey.Q:
                        Memory[4] = true;
                        break;

                    case ConsoleKey.W:
                        Memory[5] = true;
                        break;

                    case ConsoleKey.E:
                        Memory[6] = true;
                        break;

                    case ConsoleKey.R:
                        Memory[13] = true;
                        break;

                    case ConsoleKey.A:
                        Memory[7] = true;
                        break;

                    case ConsoleKey.S:
                        Memory[8] = true;
                        break;

                    case ConsoleKey.D:
                        Memory[9] = true;
                        break;

                    case ConsoleKey.F:
                        Memory[14] = true;
                        break;

                    case ConsoleKey.Y:
                        Memory[10] = true;
                        break;

                    case ConsoleKey.X:
                        Memory[0] = true;
                        break;

                    case ConsoleKey.C:
                        Memory[11] = true;
                        break;

                    case ConsoleKey.V:
                        Memory[15] = true;
                        break;
                }
            }
        }
    }
}
