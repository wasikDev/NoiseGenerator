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
        static extern void clamp_color_value(ref int colorComponent);

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        public static extern void generate_noise_values(byte[] noiseValues, int noisePixels);

        [DllImport(@"C:\Users\szorsz\source\repos\NoiseGenerator\x64\Debug\JAAsm.dll")]
        public static extern void generate_noise_pixels(IntPtr pixelArray, IntPtr noiseArray, int pixelCount);

        private int NumberOfThreads { get; set; } = 1; // Wartość domyślna

        public MainWindow()
        {
            InitializeComponent();
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
            stopwatch.Start();

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
                int noise = rand.Next(-25, 25); // Szum w zakresie -25 do 25
                int a = (pixelData[i] >> 24) & 0xff;
                int r = ((pixelData[i] >> 16) & 0xff) + noise;
                int g = ((pixelData[i] >> 8) & 0xff) + noise;
                int b = (pixelData[i] & 0xff) + noise;

                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                pixelData[i] = (a << 24) | (r << 16) | (g << 8) | b;
            });

            // Zapisanie zmian w bitmapie
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            // Aktualizacja UI po zakończeniu przetwarzania
            this.Dispatcher.Invoke(() =>
            {
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                stopwatch.Stop();
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
                executionTimeTextBlock.Text = "Czas wykonania AddNoise1: " + elapsedTime;
            });
        }
        private void ASMNoise1()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            // Rozpoczęcie zadania w wielu wątkach
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };
            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {
                // Random rand = generate_noise();
                int noise = generate_num(); // Szum w zakresie -25 do 25
                int a = (pixelData[i] >> 24) & 0xff;
                int r = ((pixelData[i] >> 16) & 0xff) + noise;
                int g = ((pixelData[i] >> 8) & 0xff) + noise;
                int b = (pixelData[i] & 0xff) + noise;

                clamp_color_value(ref r);
                clamp_color_value(ref g);
                clamp_color_value(ref b);

                pixelData[i] = (a << 24) | (r << 16) | (g << 8) | b;
            });

            // Zapisanie zmian w bitmapie
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            // Aktualizacja UI po zakończeniu przetwarzania
            this.Dispatcher.Invoke(() =>
            {
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                stopwatch.Stop();
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
                executionTimeTextBlock.Text = "Czas wykonania AddNoise1: " + elapsedTime;
            });
        }

        private void AddNoise2()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            int noisePixels = (int)(pixelData.Length * 0.05); // Zakładamy, że chcemy zmienić 5% pikseli
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, noisePixels, parallelOptions, (i, state) =>
            {
                Random rand = new Random((int)DateTime.Now.Ticks & (0x0000FFFF + i));
                int randomPixelIndex = rand.Next(pixelData.Length);
                int noiseColor = rand.NextDouble() < 0.5 ? -16777216 : -1; // Czarny albo biały w ARGB
                pixelData[randomPixelIndex] = noiseColor;
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update UI
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                stopwatch.Stop();
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
                executionTimeTextBlock.Text = "Czas wykonania AddNoise1: " + elapsedTime;
            });
        }

        private void ASMNoise2()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            int noisePixels = (int)(pixelData.Length * 0.05); // Zakładamy, że chcemy zmienić 5% pikseli
            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            byte[] noiseValues = new byte[noisePixels];
            generate_noise_values(noiseValues, noisePixels); // Generowanie szumu w ASM

            Parallel.For(0, noisePixels, parallelOptions, (i, state) =>
            {
                Random rand = new Random((int)DateTime.Now.Ticks & (0x0000FFFF + i));
                int randomPixelIndex = rand.Next(pixelData.Length);
                int noiseColor = noiseValues[i] < 128 ? -16777216 : -1; // Czarny albo biały w ARGB
                pixelData[randomPixelIndex] = noiseColor;
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update UI
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                stopwatch.Stop();
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
                executionTimeTextBlock.Text = "Czas wykonania ASMNoise2: " + elapsedTime;
            });
        }






        private void AddNoise3()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

            ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = NumberOfThreads };

            Parallel.For(0, pixelData.Length, parallelOptions, i =>
            {
                double noise = (Math.Sin(2 * Math.PI * i / 500) + 1) / 2; // Przykład z użyciem funkcji sinus
                noise *= 255; // skalujemy do zakresu kolorów

                int a = (pixelData[i] >> 24) & 0xff;
                int r = ((pixelData[i] >> 16) & 0xff) + (int)noise;
                int g = ((pixelData[i] >> 8) & 0xff) + (int)noise;
                int b = (pixelData[i] & 0xff) + (int)noise;

                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                pixelData[i] = (a << 24) | (r << 16) | (g << 8) | b;
            });

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update UI
                destinationImageControl.Source = writeableBitmap;
                destinationImageControl.Visibility = Visibility.Visible;

                stopwatch.Stop();
                TimeSpan timeTaken = stopwatch.Elapsed;
                string elapsedTime = String.Format("{2:00}.{3:00}",
                    timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
                executionTimeTextBlock.Text = "Czas wykonania AddNoise1: " + elapsedTime;
            });
        }

        private void ASMNoise3()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Załaduj obraz i przygotuj dane pikseli
            WriteableBitmap writeableBitmap = new WriteableBitmap(savedBitmapImage);
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int[] pixelData = new int[width * height];
            writeableBitmap.CopyPixels(pixelData, width * 4, 0);

           
            int[] noiseData = new int[pixelData.Length];
            for (int i = 0; i < pixelData.Length; i++)
            {
                double noiseValue = (Math.Sin(2 * Math.PI * i / 500) + 1) / 2; // Generowanie szumu
                noiseData[i] = (int)(noiseValue * 255); // Przeskalowanie szumu do zakresu 0-255
            }

            // Konwersja tablicy pikseli i szumu do wskaźników
            GCHandle pixelHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            GCHandle noiseHandle = GCHandle.Alloc(noiseData, GCHandleType.Pinned);

            // Wywołanie funkcji asemblerowej
            generate_noise_pixels(pixelHandle.AddrOfPinnedObject(), noiseHandle.AddrOfPinnedObject(), pixelData.Length);

            stopwatch.Stop();

            // Zapisanie zmodyfikowanych pikseli z powrotem do bitmapy
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            // Oczyszczenie uchwytów
            pixelHandle.Free();
            noiseHandle.Free();

            // Aktualizacja UI
            destinationImageControl.Source = writeableBitmap;

            // Wyświetlanie czasu wykonania
            
            TimeSpan timeTaken = stopwatch.Elapsed;
            string elapsedTime = String.Format("{2:00}.{3:00}",
                timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds, timeTaken.Milliseconds / 10);
            executionTimeTextBlock.Text = "Czas wykonania AddNoise1: " + elapsedTime;
        }
    }




}

