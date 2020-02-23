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

        public MainWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            RenderOptions.SetBitmapScalingMode(screen, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(screen, EdgeMode.Aliased);

            _graphics = new Graphics();
            screen.Source = _graphics.Bitmap;
            Matrix m = screen.RenderTransform.Value;
            m.ScaleAt(
                    10,
                    10,
                    0,
                    0);
            screen.RenderTransform = new MatrixTransform(m);

            InitChip8(@"R:\DEV\Chip8EmuCore\games\Delay Timer Test [Matthew Mikolay, 2010].ch8");

            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(EmulationWork);
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += OnProgressChanged;
            _worker.RunWorkerAsync();           
        }

        private void InitChip8(string file)
        {
            int cyclesPer60Hz = ClockFrequency / CounterFrequency;
            _keyboard = new ChipEightEmu.Keyboard();
            _chip8 = new CPU(ref _graphics.Memory, ref _keyboard.Memory, cyclesPer60Hz);
            _chip8.Load(File.ReadAllBytes(file));
        }

        private void EmulationWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            Stopwatch stopWatch = new Stopwatch();
            long millisecondsPerCycle = (long)Math.Round(1000 / (double)ClockFrequency);

            bool emulationIsRunning = true;
            while (emulationIsRunning)
            {
                stopWatch.Restart();
                bool redraw = _chip8.Cycle();
                if (redraw)
                {
                    worker.ReportProgress(1);
                }
                stopWatch.Stop();

                ReadKeys();

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

            WriteRegisters();           
        }              

        private void WriteRegisters()
        {
            string registers = "";
            for (int x = 0; x < 16; x++)
            {
                registers = registers + "V[" + x.ToString("d2") + "]: " + _chip8.V[x] + Environment.NewLine;
            }
            richTextBox.Text = registers;
        }

        private void mnuOpenClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Chip8 Roms (*.ch8)|*.ch8|All files (*.*)|*.*"; 
            if (openFileDialog.ShowDialog() == true)
            {
                InitChip8(openFileDialog.FileName);
            }
        }

        private void mnuExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current?.Shutdown();
        }
    }
}
