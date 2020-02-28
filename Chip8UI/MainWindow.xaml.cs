using System.Windows;
using ChipEightEmu;
using System.IO;
using System.Diagnostics;
using System;
using System.Threading;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Win32;

namespace Chip8UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ClockFrequency = 540;
        private const int CounterFrequency = 60;

        Graphics _graphics;
        ChipEightEmu.Keyboard _keyboard;

        BackgroundWorker _worker;
        
        CPU _chip8;

        public bool SingleStep { get; set; }
        public bool Step { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            SingleStep = false;
            Step = false;
            int cyclesPer60Hz = ClockFrequency / CounterFrequency;
            _keyboard = new ChipEightEmu.Keyboard();
            _graphics = new Graphics();
            _chip8 = new CPU(ref _graphics.Memory, ref _keyboard.Memory, cyclesPer60Hz);

            this.DataContext = this;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RenderOptions.SetBitmapScalingMode(screen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(screen, EdgeMode.Aliased);
                       
            screen.Source = _graphics.Bitmap;
            Matrix m = screen.RenderTransform.Value;
            m.ScaleAt(10, 10, 0, 0);
            screen.RenderTransform = new MatrixTransform(m);

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(EmulationWork);
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += OnProgressChanged;                    
        }

        private void InitChip8(string file)
        {
            _chip8.Reset();
            _chip8.Load(File.ReadAllBytes(file));

            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
        }

        private void EmulationWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            Stopwatch stopWatch = new Stopwatch();
            long millisecondsPerCycle = (long)Math.Round(1000 / (double)ClockFrequency);

            bool emulationIsRunning = true;
            while (emulationIsRunning)
            {
                if ((SingleStep && Step) || !SingleStep)
                {
                    Step = false;
                    stopWatch.Restart();
                    bool redraw = _chip8.Cycle();
                    if (redraw)
                    {
                        worker.ReportProgress(1);
                    }
                    stopWatch.Stop();
                }                

                ReadKeys();

                WriteRegisters();

                // equal runtime for every cycle
                long elapsedMilliSeconds = stopWatch.ElapsedTicks / (Stopwatch.Frequency / (1000L));

                int millisecondsAhead = (int)(millisecondsPerCycle - elapsedMilliSeconds);
                if (millisecondsAhead > 0)
                {
                    Thread.Sleep(millisecondsAhead);
                }
            }
        }

        private void ReadKeys()
        {
            _keyboard.Clear();

            try
            {
                Application.Current?.Dispatcher.Invoke(delegate
                {
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key != Key.None && System.Windows.Input.Keyboard.IsKeyDown(key))
                        {
                            _keyboard.KeyPressed(key.ToChar(), true);
                        }
                    }
                });
            }
            catch (Exception)
            { }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _graphics.Draw();                      
        }              

        private void WriteRegisters()
        {
            string cpuState = "";
            for (int x = 0; x < 16; x++)
            {
                cpuState = cpuState + "V[" + x.ToString("d2") + "]: " + _chip8.V[x] + Environment.NewLine;
            }

            cpuState = cpuState + "PC: " + _chip8.PC + Environment.NewLine;
            cpuState = cpuState + "I: " + _chip8.I + Environment.NewLine;
            cpuState = cpuState + "SP: " + _chip8.SP + Environment.NewLine;
            cpuState = cpuState + "Delay: " + _chip8.DelayTimer + Environment.NewLine;
            cpuState = cpuState + "Sound: " + _chip8.SoundTimer + Environment.NewLine;

            try
            {
                Application.Current?.Dispatcher.Invoke(delegate
                {
                    richTextBox.Text = cpuState;
                });
            }
            catch (Exception)
            { }            
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chip8 Roms (*.ch8)|*.ch8|All files (*.*)|*.*"; 
            if (openFileDialog.ShowDialog() == true)
            {
                InitChip8(openFileDialog.FileName);
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current?.Shutdown();
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            _chip8.Reset();
        }

        public bool ShiftYRegister
        { 
            get 
            { 
                return _chip8.ShiftYRegister; 
            }
            set 
            { 
                _chip8.ShiftYRegister = value; 
            } 
        }

        public bool StoreLoadIncreasesMemPointer
        {
            get
            {
                return _chip8.StoreLoadIncreasesMemPointer;
            }
            set
            {
                _chip8.StoreLoadIncreasesMemPointer = value;
            }
        }

        private void stepButton_Click(object sender, RoutedEventArgs e)
        {
            Step = true;
        }
    }
}
