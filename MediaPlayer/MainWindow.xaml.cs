using Microsoft.Win32;
using System;
using System.IO;
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
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TagLib.File currentFile;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private string fileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        //Create timer for progress bar (only starts once media is loaded) -- see source below
        private void InitializeTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        //Timer tick event -- see source below
        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                progressBar.Minimum = 0;
                progressBar.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                progressBar.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        //Open file event
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            //New Open File Dialog
            OpenFileDialog fd = new OpenFileDialog();
            //Filter only .mp3 files
            fd.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            //If Open File Dialog is successful...
            if(fd.ShowDialog() == true)
            {
                //If file extension is correct...
                if (System.IO.Path.GetExtension(fd.FileName) == ".mp3")
                {
                    fileName = fd.FileName;
                    //Insert file info to TagLib var
                    currentFile = TagLib.File.Create(fd.FileName);
                    buildSongInfo();
                    fillUserForm();
                    //Initialize media player with chosen file
                    mediaPlayer.Source = new Uri(fd.FileName);
                    //If there is art to display, set cover image art -- see source below
                    if (currentFile.Tag.Pictures.Length >= 1)
                    {
                        TagLib.IPicture pic = currentFile.Tag.Pictures[0];
                        MemoryStream ms = new MemoryStream(pic.Data.Data);
                        ms.Seek(0, SeekOrigin.Begin);
                        BitmapImage b = new BitmapImage();
                        b.BeginInit();
                        b.StreamSource = ms;
                        b.EndInit();
                        coverImg.Source = b;
                    }
                    //If there is no art to display, show placeholder art
                    else if (currentFile.Tag.Pictures.Length == 0)
                    {
                        coverImg.Source = new BitmapImage(new Uri(@"images\cover.jpg", UriKind.Relative));
                    }
                    songTitle.Text = System.IO.Path.GetFileNameWithoutExtension(fileName);
                }
                //If file extension is incorrect, display message box
                else
                {
                    MessageBox.Show("Incompatible file type, please select an .mp3 file");
                }
            }
            //Create timer, tick event will use media player 
            InitializeTimer();
        }

        //If Edit button is clicked, stop media (or will cause save error) and show Edit Media Info User Control
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            songInfoBox.Visibility = Visibility.Collapsed;
            editUserConotrol.Visibility = Visibility.Visible;
        }

        //If Player button is clicked, show album art and info
        private void Player_Click(object sender, RoutedEventArgs e)
        {
            buildSongInfo();
            songInfoBox.Visibility = Visibility.Visible;
            editUserConotrol.Visibility = Visibility.Collapsed;
        }

        //Function with if/else tree to build Song Info message (will insert "Unknown X" for null fields)
        private void buildSongInfo()
        {
            if (currentFile != null)
            {
                string msg = "";
                if (currentFile.Tag.Title != null)
                {
                    msg += currentFile.Tag.Title + "\n";
                }
                else if (currentFile.Tag.Title == null)
                {
                    msg += "Unknown Title \n";
                }
                if (currentFile.Tag.AlbumArtists.Length != 0)
                {
                    msg += currentFile.Tag.AlbumArtists[0] + "\n";
                }
                else if (currentFile.Tag.AlbumArtists.Length == 0)
                {
                    msg += "Unknown Artist \n";
                }
                if (currentFile.Tag.Album != null)
                {
                    msg += currentFile.Tag.Album + "\n";
                }
                else if (currentFile.Tag.Album == null)
                {
                    msg += "Unknown Album \n";
                }
                if (currentFile.Tag.Year != 0)
                {
                    msg += currentFile.Tag.Year.ToString();
                }
                else if (currentFile.Tag.Year == 0)
                {
                    msg += "Unknown Year";
                }
                songInfoBox.Text = msg;
            }
        }

        //Funtion to retrieve info from current TagLib file to pre-fill Media Info User Control
        private void fillUserForm()
        {
            if (currentFile != null)
            {
                editUserConotrol.titleBox.Text = currentFile.Tag.Title;
                //If no artists, must insert empty string or will cause error
                if (currentFile.Tag.AlbumArtists.Length > 0)
                {
                    editUserConotrol.artistbox.Text = currentFile.Tag.AlbumArtists[0];
                }
                else if (currentFile.Tag.AlbumArtists.Length == 0)
                {
                    editUserConotrol.artistbox.Text = "";
                }
                editUserConotrol.albumBox.Text = currentFile.Tag.Album;
                //If no year, must insert empty string or will default to 0
                if (currentFile.Tag.Year == 0)
                {
                    editUserConotrol.yearBox.Text = "";
                }
                else if (currentFile.Tag.Year != 0)
                {
                    editUserConotrol.yearBox.Text = currentFile.Tag.Year.ToString();
                }
            }
        }

        //Funcion to save info from Media Info User Control to current TagLib file 
        private void SaveMediaInfoClick(object sender, RoutedEventArgs e)
        {
            //Must first stop media element and set source to null, or will cause an error if media has begun playing
            mediaPlayer.Stop();
            mediaPlayer.Source = null;
            try
            {
                currentFile.Tag.Title = editUserConotrol.titleBox.Text;
                currentFile.Tag.AlbumArtists = new string[] { editUserConotrol.artistbox.Text };
                currentFile.Tag.Album = editUserConotrol.albumBox.Text;
                currentFile.Tag.Year = Convert.ToUInt32(editUserConotrol.yearBox.Text);
                currentFile.Save();
                MessageBox.Show("Saved!");
            } 
            catch {
                MessageBox.Show("Save Error");
            }
            //Reassign correct file to media element
            mediaPlayer.Source = new Uri(fileName);
        }

        private void PlayCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mediaPlayer != null) && (mediaPlayer.Source != null);
        }

        private void PlayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void PauseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void PauseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void StopCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void StopCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(progressBar.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            progressTimer.Text = TimeSpan.FromSeconds(progressBar.Value).ToString(@"hh\:mm\:ss");
        }
    }
}


//References:
//https://www.homeandlearn.co.uk/extras/speech/speech-open-file-button.html
//https://stackoverflow.com/questions/61158159/handling-a-button-click-inside-a-user-control-from-the-main-window
//https://www.wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
//https://stackoverflow.com/questions/17904184/using-taglib-to-display-the-cover-art-in-a-image-box-in-wpf
//https://stackoverflow.com/questions/19294258/forcing-mediaelement-to-release-stream-after-playback
//https://stackoverflow.com/questions/3100837/set-background-image-on-grid-in-wpf-using-c-sharp