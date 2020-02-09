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
                    case ConsoleKey.NumPad1:
                        Memory[0 + 0 * 4] = true;
                        break;

                    case ConsoleKey.NumPad2:
                        Memory[1 + 0 * 4] = true;
                        break;

                    case ConsoleKey.NumPad3:
                        Memory[2 + 0 * 4] = true;
                        break;

                    case ConsoleKey.NumPad4:
                        Memory[3 + 0 * 4] = true;
                        break;

                    case ConsoleKey.Q:
                        Memory[0 + 1 * 4] = true;
                        break;

                    case ConsoleKey.W:
                        Memory[1 + 1 * 4] = true;
                        break;

                    case ConsoleKey.E:
                        Memory[2 + 1 * 4] = true;
                        break;

                    case ConsoleKey.R:
                        Memory[3 + 1 * 4] = true;
                        break;

                    case ConsoleKey.A:
                        Memory[0 + 2 * 4] = true;
                        break;

                    case ConsoleKey.S:
                        Memory[1 + 2 * 4] = true;
                        break;

                    case ConsoleKey.D:
                        Memory[2 + 2 * 4] = true;
                        break;

                    case ConsoleKey.F:
                        Memory[3 + 2 * 4] = true;
                        break;

                    case ConsoleKey.Y:
                        Memory[0 + 3 * 4] = true;
                        break;

                    case ConsoleKey.X:
                        Memory[1 + 3 * 4] = true;
                        break;

                    case ConsoleKey.C:
                        Memory[2 + 3 * 4] = true;
                        break;

                    case ConsoleKey.V:
                        Memory[3 + 3 * 4] = true;
                        break;
                }
            }
        }
    }
}
