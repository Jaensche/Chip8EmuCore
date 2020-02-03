using System;

namespace ChipEightEmu
{
    public class CPU
    {
        /*
        0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
        0x200-0xFFF - Program ROM and work RAM
        */
        byte[] memory = new byte[4096];

        byte[] v = new byte[16];

        ushort I;
        ushort pc;

        byte[] gfx = new byte[64 * 32];

        byte delay_timer;
        byte sound_timer;

        ushort[] stack = new ushort[32];
        ushort sp;

        byte[] key = new byte[16];

        static readonly byte[] FONTSET =
        {
          0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
          0x20, 0x60, 0x20, 0x20, 0x70, // 1
          0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
          0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
          0x90, 0x90, 0xF0, 0x10, 0x10, // 4
          0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
          0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
          0xF0, 0x10, 0x20, 0x40, 0x40, // 7
          0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
          0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
          0xF0, 0x90, 0xF0, 0x90, 0x90, // A
          0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
          0xF0, 0x80, 0x80, 0x80, 0xF0, // C
          0xE0, 0x90, 0x90, 0x90, 0xE0, // D
          0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
          0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        Random rand;

        public CPU()
        {
            rand = new Random();
        }

        public void Init()
        {
            pc = 0x200;  // Program counter starts at 0x200
            I = 0;      // Reset index register
            sp = 0;      // Reset stack pointer
            
            // Clear display	
            
            // Clear stack
            
            // Clear registers V0-VF
            for(int i=0; i<v.Length; i++)
            {
                v[i] = 0;
            }

            // Clear memory
            for (int i = 0; i < memory.Length; i++)
            {
                memory[i] = 0;
            }

            // Load fontset
            for (int i = 0; i < 80; ++i)
            {
                memory[i] = FONTSET[i];
            }

            // Reset timers
        }

        public void Cycle()
        {
            // Load Opcode
            ushort opcode = (ushort)(memory[pc] << 8 | (byte)memory[pc + 1]);

            Console.WriteLine(opcode.ToString("X4"));

            // Decode Opcode
            // Execute Opcode
            DecodeExecute(opcode);

            // Update timers
            if (delay_timer > 0)
                --delay_timer;

            if (sound_timer > 0)
            {
                if (sound_timer == 1)
                    Console.WriteLine("BEEP!");
                --sound_timer;
            }
        }

        public void Load(byte[] programCode)
        {
            const int PROGRAM_MEM_START = 0x200;

            for (int i = 0; i < programCode.Length; i++)
            {
                memory[PROGRAM_MEM_START + i] = programCode[i];
            }
        }

        private void DecodeExecute(ushort opcode)
        {
            switch (opcode & 0xF000)
            {
                // Some opcodes //

                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // 0x00E0: Clears the screen        
                            {
                                Console.WriteLine("Clear Screen");
                                pc += 2;
                            }
                            break;

                        case 0x000E: // 0x00EE: Returns from subroutine          
                            {
                                sp--;
                                pc = stack[sp];
                                pc += 2;
                            }
                            break;

                        default:
                            {
                                Console.WriteLine("Unknown opcode: ", opcode);
                            }
                            break;
                    }
                    break;

                case 0x1000: // 1NNN: Jumps to address NNN. 
                    { 
                        pc = (ushort)(opcode & 0x0FFF); 
                    }
                    break;

                case 0x2000: // 2NNN: Calls subroutine at NNN. 
                    {
                        stack[sp] = pc;
                        sp++;
                        pc = (ushort)(opcode & 0x0FFF);
                    }
                    break;

                case 0x3000: // 3XNN: Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        if (v[x] == nn)
                        {
                            pc += 4;
                        }
                        pc += 2;
                    }
                    break;

                case 0x4000: // 4XNN: Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        if (v[x] != nn)
                        {
                            pc += 4;
                        }
                        pc += 2;
                    }
                    break;

                case 0x5000: // 5XY0: Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        if (v[x] == y)
                        {
                            pc += 4;
                        }
                        pc += 2;
                    }
                    break;

                case 0x6000: // 6XNN: Sets VX to NN. 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        v[x] = nn;
                        pc += 2;                        
                    }
                    break;

                case 0x7000: // 7XNN: Adds NN to VX. (Carry flag is not changed) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        v[x] += nn;
                        pc += 2;
                    }
                    break;

                case 0x8000:
                    {
                        switch (opcode & 0x000F)
                        {
                            case 0x0000: // 8XY0 : Sets VX to the value of VY. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    v[x] = v[y];
                                    pc += 2;
                                }
                                break;

                            case 0x0001: // 8XY1: Sets VX to VX or VY. (Bitwise OR operation) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    v[x] = (byte)(v[x] | v[y]);
                                    pc += 2;
                                }
                                break;

                            case 0x0002: // 8XY2: Sets VX to VX and VY. (Bitwise AND operation) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    v[x] = (byte)(v[x] & v[y]);
                                    pc += 2;
                                }
                                break;

                            case 0x0003: // 8XY3: Sets VX to VX xor VY. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    v[x] = (byte)(v[x] ^ v[y]);
                                    pc += 2;
                                }
                                break;

                            case 0x0004: // 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);  
                                    if((v[x] + v[y]) > 254)
                                    {
                                        //v[x] = (byte)(254 - v[x] + v[y]);
                                        v[15] = 1;
                                    }
                                    else
                                    {
                                        //v[x] = (byte)(v[x] + v[y]);
                                        v[15] = 0;
                                    }
                                    pc += 2;
                                }
                                break;

                            case 0x0005: // 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    if ((v[x] - v[y]) < 0)
                                    {
                                        v[15] = 0;
                                    }
                                    else
                                    {
                                        v[15] = 1;
                                    }
                                    pc += 2;
                                }
                                break;

                            case 0x0006: // 8XY6: Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    v[15] = (byte)(v[x] & 0x01);
                                    v[x] = (byte)(v[x] >> 4);
                                    pc += 2;
                                }
                                break;

                            case 0x0007: // 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    if ((v[x] - v[y]) < 0)
                                    {
                                        v[x] = (byte)(254 + v[x] - v[y]);
                                        v[15] = 0;
                                    }
                                    else
                                    {
                                        v[x] = (byte)(v[x] - v[y]);
                                        v[15] = 1;
                                    }
                                    pc += 2;
                                }
                                break;

                            case 0x000E: // 8XYE: Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    v[15] = (byte)(v[x] & 0x80);
                                    v[x] = (byte)(v[x] << 4);
                                    pc += 2;
                                }
                                break;

                            default:
                                {
                                    Console.WriteLine("Unknown opcode: ", opcode);
                                }
                                break;
                        }
                    }
                    break;

                case 0x9000: // 9XY0: Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        if(v[x] != v[y])
                        {
                            pc += 4;
                        }
                        pc += 2;
                    }
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    {
                        I = (ushort)(opcode & 0x0FFF);
                        pc += 2;
                    }
                    break;

                case 0xB000: // BNNN: Jumps to the address NNN plus V0. 
                    {
                        pc = (ushort)((opcode & 0x0FFF) + v[0]);
                    }
                    break;

                case 0xC000: // CXNN: Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN. 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        v[x] = (byte)(rand.Next(0, 254) & nn);
                        pc += 2;
                    }
                    break;

                case 0xD000: /*DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                    Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change after the execution of this instruction. 
                    As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen 
                    */
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        Console.WriteLine("Draw Sprite");
                        pc += 2;
                    }
                    break;

                case 0xE000: 
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x009E: // EX9E : Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    Console.WriteLine("Key Pressed? " + v[x]);
                                    //if(key()==Vx) pc += 4;
                                    pc += 2;
                                }
                                break;

                            case 0x00A1: // EXA1 : Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)  
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    Console.WriteLine("Key Pressed? " + v[x]);
                                    //if(key()!=Vx) pc += 4;
                                    pc += 2;
                                }
                                break;

                            default:
                                {
                                    Console.WriteLine("Unknown opcode: ", opcode);
                                }
                                break;
                        }
                    }
                    break;

                case 0xF000:
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x0007: // FX07 : Sets VX to the value of the delay timer. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    v[x] = delay_timer;
                                    pc += 2;
                                }
                                break;
                            case 0x000A: // FX0A : A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event) 
                                {
                                    Console.WriteLine("Await Key");
                                    pc += 2;
                                }
                                break;
                            case 0x0015: // FX15 : Sets the delay timer to VX. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    delay_timer = v[x];
                                    pc += 2;
                                }
                                break;
                            case 0x0018: // FX18 : Sets the sound timer to VX. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    sound_timer = v[x];
                                    pc += 2;
                                }
                                break;
                            case 0x001E: // FX1E : Adds VX to I. VF is set to 1 when there is a range overflow (I+VX>0xFFF), and to 0 when there isn't.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    if ((I + v[x]) > 0xFFF)
                                    {
                                        I = (ushort)(0xFFF - I + v[x]);
                                        v[15] = 1;
                                    }
                                    else
                                    { 
                                        I = (ushort)(I + v[x]);
                                        v[15] = 0;
                                    }
                                    pc += 2;
                                }
                                break;
                            case 0x0029: // FX29 : Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                {
                                    Console.WriteLine("Sprite Font");
                                    pc += 2;
                                }
                                break;
                            case 0x0033: // FX33  : 
                                /*
                                 * Stores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
                                 * (In other words, take the decimal representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.) 
                                 */
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte temp = v[x];

                                    int hundreds = temp / 100;
                                    int tens = (temp - hundreds) / 10;
                                    int ones = (temp - hundreds - tens);

                                    memory[I] = (byte)hundreds;
                                    memory[I + 1] = (byte)tens;
                                    memory[I + 2] = (byte)ones;

                                    pc += 2;
                                }
                                break;
                            case 0x0055: // FX55 : Stores V0 to VX (including VX) in memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                                {
                                    for(int i=0; i<16; i++)
                                    {
                                        memory[I + i] = v[i];
                                    }
                                    pc += 2;
                                }
                                break;
                            case 0x0065: // FX65 : Fills V0 to VX (including VX) with values from memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                                {
                                    for(int i = 0; i < 16; i++)
                                    {
                                        v[i] = memory[I + i];
                                    }
                                    pc += 2;
                                }
                                break;

                            default:
                                {
                                    Console.WriteLine("Unknown opcode: ", opcode);
                                }
                                break;
                        }
                    }
                    break;

                default:
                    { 
                        Console.WriteLine("Unknown opcode: ", opcode); 
                    }
                    break;
            }
        }

       
    }
}
