using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FUNKY
{
    public partial class Editor : Window
    {
        BrushConverter BConv = new System.Windows.Media.BrushConverter();
        MediaPlayer PlayerOfMedia = new MediaPlayer();
        DispatcherTimer GameTimer = new DispatcherTimer();
        bool TimerOn = false;
        Rap CurrentRap;
        List<Image> Arrows;
        int LastPos = 0;
        public Editor(Rap currentrap) // Načtení okna, timeru, proměnných
        {
            InitializeComponent();
            GameTimer.Interval = TimeSpan.FromMilliseconds(15);
            GameTimer.Tick += (s, e) => { TimeSlider.Value++; };
            Arrows = new List<Image>() { Left1Arrow, Left2Arrow, Right1Arrow, Right2Arrow };
            CurrentRap = currentrap;
            SongUriBox.Text = CurrentRap.SongUri.ToString().Split("/").Last();
            SongNameBox.Text = CurrentRap.RapName;
            LoadMediaPlayer(null,null);
        }

        private async void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // Reakce na změnu hodnoty na slideru
        {
            if (TimeSlider.Value - LastPos > 2 && TimerOn == true) // Zatavení písničky pokud se slider přesune manuálně
                Button_Click(null,null);
            LastPos = (int)TimeSlider.Value;
            TimeLabel.Content = TimeSlider.Value.ToString();
            int i = 0;
            foreach (List<int> X in CurrentRap.ArrowTimeData)
            {
                if (X.Contains((int)Math.Round(TimeSlider.Value/10)*10)) // Kontrola, jestli v seznamu šipek není šipka, která se má objevit
                {
                    Arrows[i].OpacityMask = Brushes.White; // Zvýraznění šipky
                }
                else
                {
                    Arrows[i].OpacityMask = (Brush)BConv.ConvertFromString("#7FFFFFFF"); // Vrácení šipky na původní stav
                }
                i++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Přehrát/Zastavit pisničku
        {
            if (TimerOn == false) // Zapnutí
            {
                TimeSlider.Value = 0;
                PlayerOfMedia.Position = TimeSpan.Zero;
                TimerOn = true;
                PlayerOfMedia.Play();
                GameTimer.Start();
            }
            else // Vypnutí
            {
                TimerOn = false;
                PlayerOfMedia.Pause();
                GameTimer.Stop();
            }
        }
        public async void EnableArrow(int Arrow) // Změna šipek co jsou momentálně vypnuté/zapnuté
        {
            if (Arrows[Arrow].OpacityMask == Brushes.White) // Vypnutí
            {
                CurrentRap.ArrowTimeData[Arrow].Remove((int)Math.Round(TimeSlider.Value / 10) * 10);
                Arrows[Arrow].OpacityMask = (Brush)BConv.ConvertFromString("#7FFFFFFF");
            }
            else // Zapnutí
            {
                CurrentRap.ArrowTimeData[Arrow].Add((int)Math.Round(TimeSlider.Value / 10) * 10);
                Arrows[Arrow].OpacityMask = Brushes.White;
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e) // Detekce zmáčknutí šipek
        {
            switch (e.Key)
            {
                case Key.Left:
                    EnableArrow(0);
                    break;
                case Key.Up:
                    EnableArrow(1);
                    break;
                case Key.Down:
                    EnableArrow(2);
                    break;
                case Key.Right:
                    EnableArrow(3);
                    break;
            }
        }

        private void Next(object sender, RoutedEventArgs e) // Posunutí slideru o jednu hodnotu dopředu
        {
            TimeSlider.Value++;
        }
        private void Back(object sender, RoutedEventArgs e) // Posunutí slideru o jednu hodnotu dozadu
        {
            TimeSlider.Value--;
        }
        public void Save(object sender, RoutedEventArgs e) // uložení do textového souboru
        {
            // Kontrola, zda nechybí žádné údaje
            if (!string.IsNullOrWhiteSpace(SongNameBox.Text) && !string.IsNullOrWhiteSpace(SongUriBox.Text))
            {
                // Kontrola, zda jméno je originální
                if (!File.Exists("../../../SaveFiles/" + SongNameBox.Text + ".txt") || MessageBox.Show("A song with the same name was found!\nDo you want to delete it?\n!NOT DELETING IT WON'T SAVE THIS FILE!","DELETE FILE",MessageBoxButton.YesNo,MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    if (CurrentRap.RapName != SongNameBox.Text && File.Exists("../../../SaveFiles/" + CurrentRap.RapName + ".txt")) // Zeptat se na smazání starého souboru, pokud byl nový soubor přejmenován
                    {
                        if (MessageBox.Show("You have renamed this file.\nDo you want to delete the old file?", "DELETE OLD FILE?", MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes && File.Exists("../../../SaveFiles/" + CurrentRap.RapName + ".txt"))
                        {
                            File.Delete("../../../SaveFiles/" + CurrentRap.RapName + ".txt");
                        }
                    }
                    // Nastavení nového jména a uri
                    CurrentRap.RapName = SongNameBox.Text;
                    CurrentRap.SongUri = new Uri("../../../Raps/" + SongUriBox.Text, UriKind.Relative);
                    // Zapsání dat do textového souboru
                    using (TextWriter TW = new StreamWriter("../../../SaveFiles/" + CurrentRap.RapName + ".txt"))
                    {
                        TW.WriteLine(CurrentRap.SongUri);
                        foreach (List<int> ArrowList in CurrentRap.ArrowTimeData)
                        {
                            foreach (int I in ArrowList)
                                TW.Write(I + ",");
                            TW.Write("\n");
                        }
                    }
                    this.Close();
                }
            }
            else if (string.IsNullOrWhiteSpace(SongNameBox.Text)) // Varování o chybějícím jménu
                MessageBox.Show("Song name is missing!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            else // Varování o chybějící uri
                MessageBox.Show("MP3 file name is missing!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public void Delete(object sender, RoutedEventArgs e) // Funkce pro mazání souboru
        {
            if (MessageBox.Show("Do you really want to delete this file forever?\nThis action can't be reversed.","ARE YOU SURE?",MessageBoxButton.YesNo,MessageBoxImage.Hand) == MessageBoxResult.Yes && File.Exists("../../../SaveFiles/" + CurrentRap.RapName + ".txt"))
            {
                File.Delete("../../../SaveFiles/" + CurrentRap.RapName + ".txt");
                this.Close();
            }
        }
        public void LoadMediaPlayer(object sender, RoutedEventArgs e) // Načtení media playeru
        {
            PlayerOfMedia.Close();
            PlayerOfMedia.Open(new Uri("../../../Raps/" + SongUriBox.Text, UriKind.Relative));
            // Úprava slideru kvůli změně mp3 souboru
            PlayerOfMedia.MediaOpened += (s, e) =>
            {
                TimeSlider.Maximum = (int)PlayerOfMedia.NaturalDuration.TimeSpan.TotalMilliseconds / 15;
            };
            this.Closed += (s, e) =>
            {
                PlayerOfMedia.Close();
            };
        }
    }
}
