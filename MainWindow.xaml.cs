using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reactive;
using System.Reactive.Subjects;

namespace ImageManipulator
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property
        HistoryService<Tuple<ImagePixels, ImageStatistic, double, double>> _history;
        HistoryService<Tuple<ImagePixels, ImageStatistic, double, double>> History
        {
            get => _history;
            set
            {
                _history = value;
                _subject.OnNext(value.CurrentState);
            }
        }

        // Łatwiejsze gettery
        private ImagePixels imagePixels => History.CurrentState.Item1;
        private ImageStatistic imageStatistic => History.CurrentState.Item2;

        // Funkcja ułatwiająca dodawanie nowych elementów
        private void PushNewState(Tuple<ImagePixels, ImageStatistic, double, double> t)
        {
            History.NewState(t);
            _subject.OnNext(t);
        }

        // Zostosowanie wzorca Obserwator
        Subject<Tuple<ImagePixels, ImageStatistic, double, double>> _subject;
        IObserver<Tuple<ImagePixels, ImageStatistic, double, double>> _observer;


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropChanged([CallerMemberName] string PropName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropName));
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

        // zmienne do ustawiania suwaków
        private double brith_delta = 0;
        private double contr_delta = 0;
        private double lastContrastValue = 0;
        private double lastBrithnessValue = 0;
        // ostatnia lokalizacja pliku;
        private string pathToFile = string.Empty;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _subject = new Subject<Tuple<ImagePixels, ImageStatistic, double, double>>();
            _observer = Observer.Create<Tuple<ImagePixels, ImageStatistic, double, double>>( (x) => 
            {
                Dispatcher.Invoke(() => { 
                    ImgControl.Source = x.Item1.ToBitmapSource();
                    DrawHistogram(x.Item2.Histogram);
                    SliderJasnosc.Value = x.Item3;
                    SliderKontrast.Value = x.Item4;
                    Processing = false;
                });
            });

            _subject.Subscribe(_observer);  
        }

        #region FileBrowse
        private void BrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.InitialDirectory = (pathToFile == string.Empty)? Directory.GetCurrentDirectory() : pathToFile;
            OpenDialog.Filter = "obrazki|*.png";
            OpenDialog.CheckFileExists = true;

            if (OpenDialog.ShowDialog() == true)
            {
                Processing = true;
                pathToFile = OpenDialog.FileName;

                // zerowanie zmiennych
                lastContrastValue = 0;
                lastBrithnessValue = 0;
                SliderJasnosc.Value = 0;
                SliderKontrast.Value = 0;
                HistogramCanvas.Children.Clear();
                FilePathLabel.Content = OpenDialog.FileName;

                Task.Run(() =>
                {
                    var img = new BitmapImage(new Uri(OpenDialog.FileName));
                    img.Freeze();

                    var imagePixels = new ImagePixels(img);
                    var statistic = new ImageStatistic(imagePixels);
                    var tuple = Tuple.Create(imagePixels, statistic, brith_delta, contr_delta);

                    if (History == null)
                    {
                        History = new HistoryService<Tuple<ImagePixels, ImageStatistic, double, double>>(tuple, 5);
                        History.CanUndoEvent += History_CanUndoEvent;
                        History.CanRedoEvent += History_CanRedoEvent;
                    }
                    else PushNewState(tuple);
                });
            }
        }
        #endregion

        #region EditActions
        private void InvertBtn_Click(object sender, RoutedEventArgs e)
        {
            ImageManipulation(ImageFunctions.InvertColor);
        }


        private void GrayScale1Btn_Click(object sender, RoutedEventArgs e)
        {
            ImageManipulation(ImageFunctions.GrayScale1);
        }

        private void GrayScale2Btn_Click(object sender, RoutedEventArgs e)
        {
            ImageManipulation(ImageFunctions.GrayScale2);
        }

        private void SliderJasnosc_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var value = Math.Round((sender as Slider).Value);
            brith_delta = (short)Math.Round(value - lastBrithnessValue);
            Func<ShortARGB, ShortARGB> fn = (ShortARGB x) => ImageFunctions.Brightness(x, (short)brith_delta);
            ImageManipulation(fn);
            lastBrithnessValue = value;
        }

        private void SliderKontrast_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var value = (sender as Slider).Value;
            contr_delta = Math.Round(value- lastContrastValue);
            Func<ShortARGB, ShortARGB> fn = (ShortARGB x) => ImageFunctions.Contrast(x, (short)contr_delta);
            ImageManipulation(fn);
            lastContrastValue = value;
        }

        // Funkcja ułatwiająca wywoływanie zadań asynchronicznych z manipulowaniem obrazu
        private void ImageManipulation(Func<ShortARGB, ShortARGB> manipulator)
        {
            Processing = true;
            Task.Run(() => {
                var imgPx = imagePixels.ForEachOnPixel(manipulator);
                var statistic = new ImageStatistic(imagePixels);
                PushNewState(Tuple.Create(imgPx, statistic, brith_delta, contr_delta));
            });
        }
        #endregion

        #region HistoryService
        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
             var redo = History.Redo();
            _subject.OnNext(redo);
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            var undo = History.Undo();
            _subject.OnNext(undo);
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


        private void HistogramChanel_Checked(object sender, RoutedEventArgs e)
        {
            if (History == null)
                return;
            
            DrawHistogram(imageStatistic.Histogram);
        }

        #region DrawingFinction
        private void DrawHistogram(BitmapHistogram histogram)
        {
            HistogramCanvas.Children.Clear();
            
            var widthScale = HistogramCanvas.ActualWidth / 256;
            var max = histogram.R.Concat(histogram.G.Concat(histogram.B)).Max();
            var heightScale = HistogramCanvas.ActualHeight / max;

            if(HistogramChanelAchbx.IsChecked == true)
                HistogramCanvas.Children.Add(
                    DrawPoligon(
                        ArrayIntToPoints(histogram.A, widthScale, heightScale), 
                        Colors.LightGray));

            if (HistogramChanelBchbx.IsChecked == true)
                HistogramCanvas.Children.Add(
                    DrawPoligon(
                        ArrayIntToPoints(histogram.B, widthScale, heightScale),
                        Colors.Blue));

            if (HistogramChanelGchbx.IsChecked == true)
                HistogramCanvas.Children.Add(
                    DrawPoligon(
                        ArrayIntToPoints(histogram.G, widthScale, heightScale),
                        Colors.Green));

            if (HistogramChanelRchbx.IsChecked == true)
                HistogramCanvas.Children.Add(
                    DrawPoligon(
                        ArrayIntToPoints(histogram.R, widthScale, heightScale),
                        Colors.Red));
        }

        private ICollection<Point> ArrayIntToPoints(int[] Data, double ScaleWidth =1, double ScaleHeight =1)
        {
            var points = new List<Point>();
            for (var i = 0; i < Data.Length; i++)
                points.Add(new Point(i * ScaleWidth, Data[i] * ScaleHeight));

            return points;
        }

        private Polygon DrawPoligon(ICollection<Point> Data, Color color)
        {
            var poly = new Polygon();
            poly.Stroke = new SolidColorBrush(color);
            poly.Fill = new SolidColorBrush(Color.FromArgb(64, color.R, color.G, color.B));

            poly.Points.Add(new Point(0,0));
            foreach (var d in Data)
                poly.Points.Add(d);
            poly.Points.Add(new Point(Data.Last().X, 0));

            return poly;
        }
        #endregion
    }
}
