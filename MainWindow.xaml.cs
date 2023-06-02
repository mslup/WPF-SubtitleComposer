using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace lab8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ObservableCollection<Subtitle> SubtitleList { get; set; }
        public CollectionViewSource ViewSource { get; set; }
        public bool IsPlaying { get; set; }

        public MainWindow()
        {
            SubtitleList = new ObservableCollection<Subtitle>();
            InitializeComponent();
            ViewSource = new CollectionViewSource();
            ViewSource.Source = SubtitleList;
            ViewSource.SortDescriptions.Add(new SortDescription("ShowTime",
                ListSortDirection.Ascending));
            DataContext = ViewSource;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1); 
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (Video.Source == null)
                return;
            if (!Video.NaturalDuration.HasTimeSpan)
                return;
            if (!IsPlaying)
                return;

            VideoTimeSlider.Value = (double)Video.Position.TotalSeconds /
                (double)Video.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is not Subtitle item)
                return;

            var lastTime = SubtitleList.Select(x => x.HideTime).Max();
            item.ShowTime = lastTime;
            item.HideTime = lastTime;
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Subtitle Composer", "About", MessageBoxButton.OK,
                MessageBoxImage.Information, MessageBoxResult.OK);
        }
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki wideo (*.mp4;*.avi;*.mkv)" +
                "|*.mp4;*.avi;*.mkv" +
                "|Wszystkie pliki (*.*)|*.*";

            if (openFileDialog.ShowDialog() == false)
                return;

            Video.Source = new Uri(openFileDialog.FileName);
            Video.Pause();
            VideoTimeSlider.Value = 0;
            VideoTime.Text = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
        }
        private void OpenSubtitlesMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SaveSubtitlesMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SaveTranslationMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Video.Play();
            IsPlaying = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Video.Pause();
            IsPlaying = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Video.Stop();
            IsPlaying = false;
            Video.Position = TimeSpan.Zero;
            VideoTimeSlider.Value = 0;
        }

        private void VideoTimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Video == null)
                return;

            if (!Video.NaturalDuration.HasTimeSpan) 
                return;

            var position = TimeSpan.FromSeconds(VideoTimeSlider.Value 
                * Video.NaturalDuration.TimeSpan.TotalSeconds);
            var ShowableSubtitles = SubtitleList.Where(sub =>
                sub.ShowTime <= position && sub.HideTime >= position).
                ToList();

            ShowableSubtitles.Sort((sub1, sub2) => 
                sub1.ShowTime.CompareTo(sub2.ShowTime));

            var strBuilder = new StringBuilder();
            foreach (var sub in ShowableSubtitles)
            {
                string str = TranslationMenuItem.IsChecked ?
                    sub.Translation :
                    sub.Text;

                if (string.IsNullOrEmpty(str))
                    str = "";
                
                strBuilder.AppendLine(str.Trim());
            }

            string text = strBuilder.ToString().Trim();
            SubtitleTextBlock.Text = text;
            SubtitleTextBlock.Visibility = text.Length > 0 ? Visibility.Visible : Visibility.Hidden; 
            Video.Position = position;
            VideoTime.Text = Video.Position.ToString(@"hh\:mm\:ss");
        }

        private void VideoVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Video == null)
                return;

            Video.Volume = VideoVolumeSlider.Value;
        }

        private void VideoTimeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Video == null)
                return;
            IsPlaying = false;
            Video.Pause();
        }

        private void VideoTimeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Video == null)
                return;
            IsPlaying = true;
            Video.Play();
        }

        private void MediaPlayerGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var sign = Math.Sign(e.Delta);
            VideoVolumeSlider.Value += sign * VideoVolumeSlider.SmallChange;
        }

        private void MediaPlayerGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                Video.Pause();
                return;
            }

            IsPlaying = true;
            Video.Play();
        }

        private void SubtitleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DataGrid)sender).SelectedItem is not Subtitle item)
                return;

            if (!Video.NaturalDuration.HasTimeSpan)
                return;

            VideoTimeSlider.Value = (double)item.ShowTime.TotalSeconds /
                (double)Video.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan lastTime = TimeSpan.Zero; 
            if (SubtitleList.Count > 0)
                lastTime = SubtitleList.Select(x => x.HideTime).Max();

            var newItem = new Subtitle(lastTime, lastTime, "", "");
            SubtitleList.Add(newItem);
            SubtitleDataGrid.SelectedItem = newItem;
        }

        private void AddAfterItem_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan lastTime = TimeSpan.Zero;
            if (SubtitleList.Count > 0)
                lastTime = SubtitleDataGrid.SelectedItems.OfType<Subtitle>().
                Select(x => x.HideTime).Max();

            var newItem = new Subtitle(lastTime, lastTime, "", "");
            SubtitleList.Add(newItem);
            SubtitleDataGrid.SelectedItem = newItem;
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SubtitleDataGrid.SelectedItems.OfType<Subtitle>().ToArray())
                SubtitleList.Remove(item);
        }
    }

    public class Subtitle : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        

        private TimeSpan _showTime;
        private TimeSpan _hideTime;
        private TimeSpan _duration;
        private string _text;
        private string _translation;

        public TimeSpan ShowTime
        {
            get => _showTime;
            set
            {
                _showTime = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HideTime));
            }
        }
        public TimeSpan HideTime
        {
            get => _hideTime;
            set
            {
                _hideTime = value;
                _duration = value - _showTime;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }
        public TimeSpan Duration
        {
            get => _hideTime - _showTime;
            set
            {
                _duration = value;
                _hideTime = _showTime + _duration;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HideTime));
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public Subtitle()
        {
            _showTime = TimeSpan.Zero;
            _hideTime = TimeSpan.Zero;
            _text = string.Empty;
            _translation = string.Empty;
        }

        public Subtitle(TimeSpan showTime, TimeSpan hideTime, 
            string text, string translation)
        {
            _showTime = showTime;
            _hideTime = hideTime;
            _text = text;
            _translation = translation;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
