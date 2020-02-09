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
        byte[] _mem = new byte[4096];

        byte[] _v = new byte[16];

        ushort _I;
        ushort _pc;

        byte[,] _gfx;

        byte _delay_timer;
        byte _sound_timer;

        ushort[] _stack = new ushort[32];
        ushort _sp;

        bool[] _keys;

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

        Random _rand;

        private bool _redrawFlag;

        int _instructionCount = 0;

        public CPU(ref byte[,] gfx, ref bool[] keys)
        {
            _gfx = gfx;
            _keys = keys;

            Init();
        }

        public void Init()
        {
            _pc = 0x200;  // Program counter starts at 0x200
            _I = 0;      // Reset index register
            _sp = 0;      // Reset stack pointer

            // Clear screen
            ClearScreen();

            // Clear registers V0-VF
            for (int i = 0; i < _v.Length; i++)
            {
                _v[i] = 0;
            }

            // Clear memory
            for (int i = 0; i < _mem.Length; i++)
            {
                _mem[i] = 0;
            }

            // Load fontset
            for (int i = 0; i < 80; ++i)
            {
                _mem[i] = FONTSET[i];
            }

            // Reset timers
            _delay_timer = 0;
            _sound_timer = 0;

            _rand = new Random();
            _redrawFlag = false;
        }

        public bool Cycle(int cyclesPer60Hz)
        {
            // reset redraw
            _redrawFlag = false;

            // Load Opcode
            ushort opcode = (ushort)(_mem[_pc] << 8 | (byte)_mem[_pc + 1]);

            // Decode Opcode
            // Execute Opcode
            DecodeExecute(opcode);

            // Update timers
            _instructionCount++;
            if (_instructionCount == cyclesPer60Hz)
            {
                if (_delay_timer > 0)
                    --_delay_timer;

                if (_sound_timer > 0)
                {
                    if (_sound_timer == 1)
                        Console.Beep();
                    --_sound_timer;
                }
                _instructionCount = 0;
            }

            return _redrawFlag;
        }

        public void Load(byte[] programCode)
        {
            const int PROGMEMSTART = 0x200;

            for (int i = 0; i < programCode.Length; i++)
            {
                _mem[PROGMEMSTART + i] = programCode[i];
            }
        }

        private void DecodeExecute(ushort opcode)
        {
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // 0x00E0: Clears the screen        
                            {
                                ClearScreen();
                                _pc += 2;
                            }
                            break;

                        case 0x000E: // 0x00EE: Returns from subroutine          
                            {
                                _sp--;
                                _pc = _stack[_sp];
                                _pc += 2;
                            }
                            break;

                        default:
                            {
                                throw new Exception("unknown opcode: " + opcode);
                            }
                    }
                    break;

                case 0x1000: // 1NNN: Jumps to address NNN. 
                    {
                        ushort nnn = (ushort)(opcode & 0x0FFF);
                        _pc = nnn;
                    }
                    break;

                case 0x2000: // 2NNN: Calls subroutine at NNN. 
                    {
                        _stack[_sp] = _pc;
                        _sp++;
                        _pc = (ushort)(opcode & 0x0FFF);
                    }
                    break;

                case 0x3000: // 3XNN: Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        if (_v[x] == nn)
                        {
                            _pc += 2;
                        }
                        _pc += 2;
                    }
                    break;

                case 0x4000: // 4XNN: Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        if (_v[x] != nn)
                        {
                            _pc += 2;
                        }
                        _pc += 2;
                    }
                    break;

                case 0x5000: // 5XY0: Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        if (_v[x] == _v[y])
                        {
                            _pc += 2;
                        }
                        _pc += 2;
                    }
                    break;

                case 0x6000: // 6XNN: Sets VX to NN. 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        _v[x] = nn;
                        _pc += 2;
                    }
                    break;

                case 0x7000: // 7XNN: Adds NN to VX. (Carry flag is not changed) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        _v[x] += nn;
                        _pc += 2;
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
                                    _v[x] = _v[y];
                                    _pc += 2;
                                }
                                break;

                            case 0x0001: // 8XY1: Sets VX to VX or VY. (Bitwise OR operation) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    _v[x] = (byte)(_v[x] | _v[y]);
                                    _pc += 2;
                                }
                                break;

                            case 0x0002: // 8XY2: Sets VX to VX and VY. (Bitwise AND operation) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    _v[x] = (byte)(_v[x] & _v[y]);
                                    _pc += 2;
                                }
                                break;

                            case 0x0003: // 8XY3: Sets VX to VX xor VY. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    _v[x] = (byte)(_v[x] ^ _v[y]);
                                    _pc += 2;
                                }
                                break;

                            case 0x0004: // 8XY4: Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    if ((_v[x] + _v[y]) > 254)
                                    {
                                        _v[15] = 1;
                                    }
                                    else
                                    {
                                        _v[15] = 0;
                                    }
                                    _pc += 2;
                                }
                                break;

                            case 0x0005: // 8XY5: VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    if ((_v[x] - _v[y]) < 0)
                                    {
                                        _v[15] = 0;
                                    }
                                    else
                                    {
                                        _v[15] = 1;
                                    }
                                    _pc += 2;
                                }
                                break;

                            case 0x0006: // 8XY6: Stores the least significant bit of VX in VF and then shifts VX to the right by 1.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    _v[15] = (byte)(_v[x] & 0x01);
                                    _v[x] = (byte)(_v[x] >> 1);
                                    _pc += 2;
                                }
                                break;

                            case 0x0007: // 8XY7: Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    byte y = (byte)((opcode & 0x00F0) >> 4);
                                    if ((_v[y] - _v[x]) < 0)
                                    {
                                        _v[x] = (byte)(254 + _v[x] - _v[y]);
                                        _v[15] = 0;
                                    }
                                    else
                                    {
                                        _v[x] = (byte)(_v[x] - _v[y]);
                                        _v[15] = 1;
                                    }
                                    _pc += 2;
                                }
                                break;

                            case 0x000E: // 8XYE: Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    _v[15] = (byte)((_v[x] & 0x80) >> 7);
                                    _v[x] = (byte)(_v[x] << 1);
                                    _pc += 2;
                                }
                                break;

                            default:
                                {
                                    throw new Exception("unknown opcode: " + opcode);
                                }
                        }
                    }
                    break;

                case 0x9000: // 9XY0: Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block) 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        if (_v[x] != _v[y])
                        {
                            _pc += 2;
                        }
                        _pc += 2;
                    }
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    {
                        ushort nnn = (ushort)(opcode & 0x0FFF);
                        _I = nnn;
                        _pc += 2;
                    }
                    break;

                case 0xB000: // BNNN: Jumps to the address NNN plus V0. 
                    {
                        ushort nnn = (ushort)(opcode & 0x0FFF);
                        _pc = (ushort)(nnn + _v[0]);
                    }
                    break;

                case 0xC000: // CXNN: Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN. 
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte nn = (byte)(opcode & 0x00FF);
                        _v[x] = (byte)(_rand.Next(0, 254) & nn);
                        _pc += 2;
                    }
                    break;

                case 0xD000: /*DXYN: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                    Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change after the execution of this instruction. 
                    As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen 
                    */
                    {
                        byte x = (byte)((opcode & 0x0F00) >> 8);
                        byte y = (byte)((opcode & 0x00F0) >> 4);
                        byte n = (byte)((opcode & 0x000F));

                        // get sprite from memory
                        byte[] sprite = new byte[n];
                        for (int a = 0; a < n; a++)
                        {
                            sprite[a] = _mem[_I + a];
                        }

                        // VF should be 0 when no pixels are flipped set to unset
                        _v[0x0F] = 0;

                        for (int yCounter = 0; yCounter < n; yCounter++)
                        {
                            for (int xCounter = 0; xCounter < 8; xCounter++)
                            {
                                int xAddress = _v[x] + xCounter;
                                int yAddress = _v[y] + yCounter;

                                // wrap around x and y addresses
                                xAddress = xAddress % 64;
                                yAddress = yAddress % 32;

                                bool pixel = (sprite[yCounter] & (0b10000000 >> xCounter)) > 0;
                                
                                // set VF to 1 if one pixel is flipped from set to unset
                                if(_gfx[xAddress, yAddress] == 1 && !pixel)
                                {
                                    _v[0x0F] = 1;
                                }

                                // write bit to graphics
                                _gfx[xAddress, yAddress] = (byte)(pixel ? 1 : 0);                                
                            }
                        }

                        _redrawFlag = true;

                        _pc += 2;
                    }
                    break;

                case 0xE000:
                    {
                        switch (opcode & 0x00FF)
                        {
                            case 0x009E: // EX9E : Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    if (_keys[_v[x]] == true)
                                    {
                                        _pc += 4;
                                    }
                                    _pc += 2;
                                }
                                break;

                            case 0x00A1: // EXA1 : Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)  
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    if (_keys[_v[x]] == false)
                                    {
                                        _pc += 2;
                                    }
                                    _pc += 2;
                                }
                                break;

                            default:
                                {
                                    throw new Exception("unknown opcode: " + opcode);
                                }
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
                                    _v[x] = _delay_timer;
                                    _pc += 2;
                                }
                                break;
                            case 0x000A: // FX0A : A key press is awaited, and then stored in VX. (Blocking Operation. All instruction halted until next key event) 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    for(int i=0; i< _keys.Length; i++)
                                    {
                                        if (_keys[i] == true)
                                        {
                                            _v[x] = (byte)i;
                                            _pc += 2;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case 0x0015: // FX15 : Sets the delay timer to VX. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    _delay_timer = _v[x];
                                    _pc += 2;
                                }
                                break;
                            case 0x0018: // FX18 : Sets the sound timer to VX. 
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    _sound_timer = _v[x];
                                    _pc += 2;
                                }
                                break;
                            case 0x001E: // FX1E : Adds VX to I. VF is set to 1 when there is a range overflow (I+VX>0xFFF), and to 0 when there isn't.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    if ((_I + _v[x]) > 0xFFF)
                                    {
                                        _I = (ushort)(0xFFF - _I + _v[x]);
                                        _v[15] = 1;
                                    }
                                    else
                                    {
                                        _I = (ushort)(_I + _v[x]);
                                        _v[15] = 0;
                                    }
                                    _pc += 2;
                                }
                                break;
                            case 0x0029: // FX29 : Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    _I = (byte)(5 * _v[x]);
                                    _pc += 2;
                                }
                                break;
                            case 0x0033: // FX33  : 
                                /*
                                 * Stores the binary-coded decimal representation of VX, with the most significant of three digits at the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2. 
                                 * (In other words, take the decimal representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.) 
                                 */
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    
                                    _mem[_I] = (byte)((_v[x] / 100) % 10);
                                    _mem[_I + 1] = (byte)((_v[x] / 10) % 10);
                                    _mem[_I + 2] = (byte)(_v[x] % 10);

                                    _pc += 2;
                                }
                                break;
                            case 0x0055: // FX55 : Stores V0 to VX (including VX) in memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    for (int i = 0; i <= x; i++)
                                    {
                                        _mem[_I + i] = _v[i];
                                    }
                                    //_I = (ushort)(_I + x + 1); // TODO: check if I needs to be changed
                                    _pc += 2;
                                }
                                break;
                            case 0x0065: // FX65 : Fills V0 to VX (including VX) with values from memory starting at address I. The offset from I is increased by 1 for each value written, but I itself is left unmodified.
                                {
                                    byte x = (byte)((opcode & 0x0F00) >> 8);
                                    for (int i = 0; i <= x; i++)
                                    {
                                        _v[i] = _mem[_I + i];
                                    }
                                    //_I = (ushort)(_I + x + 1); // TODO: check if I needs to be changed
                                    _pc += 2;
                                }
                               
                                break;

                            default:
                                {
                                    throw new Exception("unknown opcode: " + opcode);
                                }
                        }
                    }
                    break;

                default:
                    {
                        throw new Exception("unknown opcode: " + opcode);
                    }
            }
        }

        private void ClearScreen()
        {
            for (int y = 0; y < _gfx.GetLength(1); y++)
            {
                for (int x = 0; x < _gfx.GetLength(0); x++)
                {
                    _gfx[x, y] = 0;
                }
            }
        }
    }
}
