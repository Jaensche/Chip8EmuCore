namespace ChipEightEmu
{
    public class Keyboard
    {
        public bool[] Memory = new bool[16];

        private readonly object locker = new object();
       
        public Keyboard()
        {
        }

        public void Clear()
        {
            for (int i = 0; i < Memory.Length; i++)
            {
                Memory[i] = false;
            }
        }

        public void KeyPressed(char key, bool pressed)
        {
            lock (locker)
            {
                /*              
                 Keyboard (PC)
                ╔═══╦═══╦═══╦═══╗
                ║ 1 ║ 2 ║ 3 ║ 4 ║
                ╠═══╬═══╬═══╬═══╣
                ║ Q ║ W ║ E ║ R ║
                ╠═══╬═══╬═══╬═══╣
                ║ A ║ S ║ D ║ F ║
                ╠═══╬═══╬═══╬═══╣
                ║ Y ║ X ║ C ║ V ║
                ╚═══╩═══╩═══╩═══╝

                Mapped to (chip8)
                ╔═══╦═══╦═══╦═══╗
                ║ 1 ║ 2 ║ 3 ║ C ║
                ╠═══╬═══╬═══╬═══╣
                ║ 4 ║ 5 ║ 6 ║ D ║
                ╠═══╬═══╬═══╬═══╣
                ║ 7 ║ 8 ║ 9 ║ E ║
                ╠═══╬═══╬═══╬═══╣
                ║ A ║ 0 ║ B ║ F ║
                ╚═══╩═══╩═══╩═══╝            
                 */

                {
                    switch (key)
                    {
                        case '1':
                            Memory[1] = pressed;
                            break;

                        case '2':
                            Memory[2] = pressed;
                            break;

                        case '3':
                            Memory[3] = pressed;
                            break;

                        case '4':
                            Memory[12] = pressed;
                            break;

                        case 'q':
                            Memory[4] = pressed;
                            break;

                        case 'w':
                            Memory[5] = pressed;
                            break;

                        case 'e':
                            Memory[6] = pressed;
                            break;

                        case 'r':
                            Memory[13] = pressed;
                            break;

                        case 'a':
                            Memory[7] = pressed;
                            break;

                        case 's':
                            Memory[8] = pressed;
                            break;

                        case 'd':
                            Memory[9] = pressed;
                            break;

                        case 'f':
                            Memory[14] = pressed;
                            break;

                        case 'y':
                            Memory[10] = pressed;
                            break;

                        case 'x':
                            Memory[0] = pressed;
                            break;

                        case 'c':
                            Memory[11] = pressed;
                            break;

                        case 'v':
                            Memory[15] = pressed;
                            break;
                    }
                }
            }
        }
    }
}
