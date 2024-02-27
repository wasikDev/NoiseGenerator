using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.Intrinsics;
using CSLibrary;
namespace NoiseGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        static extern int generate_num();

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        static extern int clamp_color_value_below(ref int colorComponent);

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        static extern int add_noise_asm(int[] pixelData, int pixelDataLength, int noisePixelIndex, int noiseColor);

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        private static extern int generate_random_index(int arrayLength);

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        private static extern void add_noise_to_pixel(int[] pixelData, int pixelIndex, int noiseValue);

        

        private int NumberOfThreads { get; set; } = 1; // Wartość domyślna

        public MainWindow()
        {
            InitializeComponent();
            int processorThreadCount = Environment.ProcessorCount;
            threadsSlider.Value = processorThreadCount;
            threadsSlider.ValueChanged += threadsSlider_ValueChanged;
        }

        private void threadsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            NumberOfThreads = (int)e.NewValue; // Aktualizacja liczby wątków na podstawie suwaka
        }

        private BitmapImage savedBitmapImage;

        private void ChooseImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Obrazy (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                savedBitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                sourceImageControl.Source = savedBitmapImage;
            }
        }



        private void ShowInAnotherPlaceButton_Click(object sender, RoutedEventArgs e)
        {
            // Sprawdź, czy obraz został wybrany
            if (savedBitmapImage == null)
            {
                MessageBox.Show("Najpierw wybierz zdjęcie.");
                return; // Przerywamy metodę, jeśli obraz nie został wybrany
            }

            // Sprawdź, który rodzaj szumu został wybrany
            if (radioNoise1.IsChecked == true)
            {
                // Wybierz implementację na podstawie wybranego języka
                if (radioCSharp.IsChecked == true)
                {
                    AddNoise1(); // Implementacja w C#
                }
                else if (radioAsm.IsChecked == true)
                {
                    ASMNoise1(); // Implementacja w ASM
                }
            }
            else if (radioNoise2.IsChecked == true)
            {
                if (radioCSharp.IsChecked == true)
                {
                    AddNoise2(); // Implementacja w C#
                }
                else if (radioAsm.IsChecked == true)
                {
                    ASMNoise2(); // Implementacja w ASM
                }

            }
            else if (radioNoise3.IsChecked == true)
            {
                if (radioCSharp.IsChecked == true)
                {
                    AddNoise3(); // Implementacja w C#
                }
                else if (radioAsm.IsChecked == true)
                {
                    ASMNoise3(); // Implementacja w ASM
                }
            }
        }



        private void AddNoise1()
        {
            Stopwatch stopwatch = new Stopwatch();
           

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            // Rozpoczęcie zadania w wielu wątkach
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };
            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {


                Random rand = new Random();

                //Class1 klasa1 = new Class1();
                // int x = 0;
                //x = klasa1.randTest(x);

                int noise = rand.Next(-25, 25); // Szum w zakresie -25 do 25
                int a = (pixelData[i] >> 24) & 0xff;
                int r = ((pixelData[i] >> 16) & 0xff) + noise;
                int g = ((pixelData[i] >> 8) & 0xff) + noise;
                int b = (pixelData[i] & 0xff) + noise;
                stopwatch.Start();
                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));
                stopwatch.Stop();
                pixelData[i] = (a << 24) | (r << 16) | (g << 8) | b;
            });

            // Zapisanie zmian w bitmapie
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            // Aktualizacja UI po zakończeniu przetwarzania
            this.Dispatcher.Invoke(() =>
            {
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

               
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
                executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AddNoise1: " + elapsedTime;
            });
        }
        private void ASMNoise1()
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            // Rozpoczęcie zadania w wielu wątkach
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            // Stopwatch poza pętlą
            Stopwatch stopwatch = new Stopwatch();

            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {
                Random rand = new Random();
                int noise = rand.Next(-25, 25); // Szum w zakresie -25 do 25
                int a = (pixelData[i] >> 24) & 0xff;
                int r = ((pixelData[i] >> 16) & 0xff) + noise;
                int g = ((pixelData[i] >> 8) & 0xff) + noise;
                int b = (pixelData[i] & 0xff) + noise;

                // Start pomiaru czasu
                stopwatch.Start();

                r = clamp_color_value_below(ref r);
                g = clamp_color_value_below(ref g);
                b = clamp_color_value_below(ref b);

                // Koniec pomiaru czasu
                stopwatch.Stop();

                pixelData[i] = (a << 24) | (r << 16) | (g << 8) | b;
            });

            // Wyniki czasowe
            TimeSpan timeTaken = stopwatch.Elapsed;
            string elapsedTime = String.Format("{2:00}.{3:00}",
                timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
            executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AsmNoise1: " + elapsedTime;
        


        // Zapisanie zmian w bitmapie
        writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            // Aktualizacja UI po zakończeniu przetwarzania
            this.Dispatcher.Invoke(() =>
            {
                int d = 300;
                int e = -300;
                d = clamp_color_value_below(ref d);
                e = clamp_color_value_below(ref e);
                int s = 100;
                s = clamp_color_value_below(ref s);
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

               



            });
        }

        private void AddNoise2()
        {
            Stopwatch stopwatch = new Stopwatch();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            int noisePixels = (int)(pixelData.Length * 0.2); // Zakładamy, że chcemy zmienić 5% pikseli
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, noisePixels, parallelOptions, (i, state) =>
            {
                stopwatch.Start();
                Random rand = new Random((int)DateTime.Now.Ticks & (0x0000FFFF + i));
                int randomPixelIndex = rand.Next(pixelData.Length);
                int noiseColor = rand.NextDouble() < 0.5 ? -16777216 : -1; // Czarny albo biały w ARGB
                pixelData[randomPixelIndex] = noiseColor;
                stopwatch.Stop();
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update UI
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
                executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AddNoise2: " + elapsedTime;
            });
        }

        private void ASMNoise2()
        {

            Stopwatch stopwatch = new Stopwatch();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            int noisePixels = (int)(pixelData.Length * 0.04); // Zakładamy, że chcemy zmienić 5% pikseli
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, noisePixels, parallelOptions, (i, state) =>
            {
                stopwatch.Start();
                Random rand = new Random((int)DateTime.Now.Ticks & (0x0000FFFF + i));
                int randomPixelIndex = generate_random_index(pixelData.Length);
                int noiseColor = rand.NextDouble() < 0.5 ? -16777216 : -1; // Czarny albo biały w ARGB
                add_noise_asm(pixelData, pixelData.Length, randomPixelIndex, noiseColor);
                stopwatch.Stop();   
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update UI
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
                executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AsmNoise2: " + elapsedTime;
            });
        }







        private void AddNoise3()
        {
            Stopwatch stopwatch = new Stopwatch(); ;

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {
                Random random = new Random();
                double offset = random.NextDouble(); 

                double noise = (Math.Sin(2 * Math.PI * i / (width * 10) + offset) + 1) / 2;
                noise *= 255;
                int noiseValue = (int)noise;

                stopwatch.Start();

                pixelData[i] = Class1.AddNoiseToPixel(pixelData[i], noiseValue);
                
                stopwatch.Stop();
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
                executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AddNoise3: " + elapsedTime;
            });
        }



        private void ASMNoise3()
        {
            Stopwatch stopwatch = new Stopwatch();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {
                
                Random random = new Random();
                double offset = random.NextDouble();

                double noise = (Math.Sin(2 * Math.PI * i / (width * 10) + offset) + 1) / 2;
                noise *= 255;
                int noiseValue = (int)noise;

                stopwatch.Start();
                add_noise_to_pixel(pixelData, noiseValue, i);
                stopwatch.Stop();
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds);
                executionTimeTextBlock_Kopiuj.Text = "Czas wykonania AsmNoise3: " + elapsedTime;
            });
        }







    }










}





