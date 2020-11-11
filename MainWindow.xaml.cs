using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ImageManipulator
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        ImagePixels imagePixels;
        HistoryService<ImagePixels> history;

        public event PropertyChangedEventHandler PropertyChanged;
        private bool _Processing = false;
        public bool Processing
        {
            get => _Processing;
            set
            {
                _Processing = !value;
                OnPropChanged();
            }
        }
    
        private string pathToFile = string.Empty;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region brownFile
        private void BrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.InitialDirectory = (pathToFile == string.Empty)? Directory.GetCurrentDirectory() : pathToFile;
            OpenDialog.Filter = "obrazki|*.png";
            OpenDialog.CheckFileExists = true;

            if (OpenDialog.ShowDialog() == true)
            {
                pathToFile = OpenDialog.FileName;
                FilePathLabel.Content = OpenDialog.FileName;
                Processing = true;
                Task.Run(() =>
                {
                    var img = new BitmapImage(new Uri(OpenDialog.FileName));
                    img.Freeze();

                    imagePixels = new ImagePixels(img);
                    history = new HistoryService<ImagePixels>(imagePixels, 20);
                    history.CanRedoEvent += History_CanRedoEvent;
                    history.CanUndoEvent += History_CanUndoEvent;

                    Dispatcher.Invoke(() =>
                    {
                        ImgControl.Source = imagePixels.ToBitmapSource();
                        Processing = false;
                    });
                });
            }
        }
        #endregion

        #region EditActions
        private void InvertBtn_Click(object sender, RoutedEventArgs e)
        {
            Processing = true;
            Task.Run(() => {
                imagePixels = imagePixels.ForEachOnPixel(InvertColor);
                history.NewState(imagePixels);
                var img = imagePixels.ToBitmapSource((x)=> (byte)x);

                Dispatcher.Invoke(() => {
                    ImgControl.Source = img;
                    Processing = false;
                });
            });
        }


        private void GrayScale1Btn_Click(object sender, RoutedEventArgs e)
        {
            Processing = true;
            Task.Run(() => {
                imagePixels = imagePixels.ForEachOnPixel(GrayScale1);
                history.NewState(imagePixels);
                var img = imagePixels.ToBitmapSource();

                Dispatcher.Invoke(() => {
                    ImgControl.Source = img;
                    Processing = false;
                });
            });
        }

        private void GrayScale2Btn_Click(object sender, RoutedEventArgs e)
        {
            Processing = true;
            Task.Run(() => {
                imagePixels = imagePixels.ForEachOnPixel(GrayScale2);
                history.NewState(imagePixels);
                var img = imagePixels.ToBitmapSource();

                Dispatcher.Invoke(() => {
                    ImgControl.Source = img;
                    Processing = false;
                });
            });
        }

        private void SliderJasnosc_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Processing = true;
            var value = (int)Math.Round((sender as Slider).Value);
            Func<IntARGB, IntARGB> fn = (IntARGB x) => Brightness(x, value);

            Task.Run(() => {
                imagePixels = imagePixels.ForEachOnPixel(fn);
                history.NewState(imagePixels);
                var img = imagePixels.ToBitmapSource();

                Dispatcher.Invoke(() => {
                    ImgControl.Source = img;
                    Processing = false;
                });
            });
        }

        private void SliderKontrast_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Processing = true;
            var value = (int)Math.Round((sender as Slider).Value);
            Func<IntARGB, IntARGB> fn = (IntARGB x) => Contrast(x, value);

            Task.Run(() => {
                imagePixels = imagePixels.ForEachOnPixel(fn);
                history.NewState(imagePixels);
                var img = imagePixels.ToBitmapSource();

                Dispatcher.Invoke(() => {
                    ImgControl.Source = img;
                    Processing = false;
                });
            });
        }
        #endregion

        #region HistoryService
        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            var img = history.Redo();
            ImgControl.Source = img.ToBitmapSource();
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            var img = history.Undo();
            ImgControl.Source = img.ToBitmapSource();
        }

        private void History_CanUndoEvent(BoolArgs args)
        {
            Dispatcher.Invoke(() => {
                UndoBtn.IsEnabled = args.value;
            });
        }

        private void History_CanRedoEvent(BoolArgs args)
        {
            Dispatcher.Invoke(() => {
                RedoBtn.IsEnabled = args.value;
            });
        }
        #endregion

        private void OnPropChanged([CallerMemberName] string PropName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropName));

        #region ImageMethod
        private IntARGB Contrast(IntARGB color, int value)
        {
            if (color.R + color.G + color.B > 382.5)
                return new IntARGB(color.A, color.R + value, color.G + value, color.B + value);
            return new IntARGB(color.A, color.R - value, color.G - value, color.B - value);
        }

        private IntARGB Brightness(IntARGB color, int value) 
            => new IntARGB(color.A, color.R + value, color.G + value, color.B + value);

        private IntARGB GrayScale2(IntARGB colorInput)
        {
            var R = (int)Math.Round(colorInput.R * 0.299);
            var G = (int)Math.Round(colorInput.G * 0.587);
            var B = (int)Math.Round(colorInput.G * 0.114);
            var gray = R + G + B;

            return new IntARGB(colorInput.A, gray, gray, gray);
        }

        private IntARGB GrayScale1(IntARGB colorInput)
        {
            var gray = (int)Math.Round((colorInput.R + colorInput.G + colorInput.B) / 3f);

            return new IntARGB(colorInput.A, gray, gray, gray);
        }

        private IntARGB InvertColor(IntARGB colorInput)
        {

            return new IntARGB(colorInput.A, 
                255 - ImagePixels.IntToByte(colorInput.R), 
                255 - ImagePixels.IntToByte(colorInput.G), 
                255 - ImagePixels.IntToByte(colorInput.B));
        }
        #endregion
    }
}
