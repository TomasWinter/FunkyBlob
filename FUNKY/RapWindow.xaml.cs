using System;
using System.Collections.Generic;
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
    public partial class RapWindow : Window
    {
        Rap CurrentRap;
        List<string> Pochvaly = new List<string>() { "NICE","SICK","MEGA","COOL"};
        List<string> Urazky = new List<string>() { "NOT GOOD", "MISS", "BAD", "UNCOOL" };
        MediaPlayer PlayerOfMedia = new MediaPlayer();
        MediaPlayer PlayerOfFail = new MediaPlayer();
        List<List<int>> ArrowTimeData = new List<List<int>>();
        List<List<Image>> CurrentArrows = new List<List<Image>>() { new List<Image>(), new List<Image>(), new List<Image>(), new List<Image>() };
        List<Image> IndicatorArrows;
        DispatcherTimer GameTimer = new DispatcherTimer();
        int CurrentTime = 0;
        int CurrentScore = 0;
        int Tolerance = 50;
        int PochvalaCount = 0;
        int MaxScore = 0;
        double MaxTime = 9000000;
        public RapWindow(Rap RapClass,MainWindow originalwindow) // Načtení okna, hudby, zvukového efektu a nastavení proměnných
        {
            InitializeComponent();
            PlayerOfFail.Open(new Uri("FailSound.mp3",UriKind.Relative));
            CurrentRap = RapClass;
            CountScore();
            ScoreLabel.Content = "SCORE: " + CurrentScore + "/" + MaxScore;
            ArrowTimeData = RapClass.ArrowTimeData;
            PlayerOfMedia.Open(RapClass.SongUri);
            GameTimer.Interval = TimeSpan.FromMilliseconds(15);
            GameTimer.Tick += GameTimerTicked;
            PlayerOfMedia.MediaOpened += (s,e) => { 
                GameTimer.Start(); 
                MaxTime = PlayerOfMedia.NaturalDuration.TimeSpan.TotalMilliseconds / 15 + 100; // Určení délky písničky
            };
            IndicatorArrows = new List<Image>() { Left2Arrow, Left1Arrow, Right1Arrow, Right2Arrow };
            this.Closing += (s, e) =>
            {
                PlayerOfMedia.Close();
                PlayerOfFail.Close();
            };
        }

        public void CountScore() // Spočítání největšího možného skóre v písničce 
        {
            foreach (List<int> L in CurrentRap.ArrowTimeData)
            {
                foreach (int I in L)
                {
                    MaxScore++;
                }
            }
        }

        public async void GameTimerTicked(object sender, EventArgs e) // Detekce posunu času
        {
            CurrentTime++;
            TimeLabel.Content = CurrentTime;
            CreateArrows();
            MoveArrows();
            if (CurrentTime == 100) // Spuštění písničky po uplnynutí intervalu
                PlayerOfMedia.Play();
            if (CurrentTime > MaxTime) // zavření okna po konci písničky
                this.Close();
        }
        public async void MoveArrows() // Pohnutí se šipkami
        {
            int i = 0;
            foreach (List<Image> CurrentList in CurrentArrows)
            {
                for (int y = 0; y <= CurrentList.Count() - 1; y++)
                {
                    Image CurrentObject = CurrentList[y];
                    CurrentObject.Margin = new Thickness(CurrentObject.Margin.Left, CurrentObject.Margin.Top - 5, CurrentObject.Margin.Right, CurrentObject.Margin.Bottom + 5); // Posun šipky
                    if (CurrentObject.Margin.Top <= IndicatorArrows[i].Margin.Top - Tolerance) // Kontrola, jestli je šipka za pozicí ve které jde zmáčknout
                    {
                        // Smazání šipky a ubírání skóre
                        CurrentScore--;
                        FailSound();
                        MainGrid.Children.Remove(CurrentObject);
                        CurrentList.Remove(CurrentObject);
                        ScoreLabel.Content = "SCORE: " + CurrentScore + "/" + MaxScore;
                        PochvalaLabel.Content = Urazky[new Random().Next(Urazky.Count - 1)];
                        ChangeText();
                    }
                }
                i++;
            }
        }
        public async void CreateArrows() // Vytvoření šipek
        {
            int i = 0;
            foreach (List<int> CurrentList in ArrowTimeData)
            {
                if (CurrentList.Contains(CurrentTime - 20)) // Kontrola, jestli v seznamu šipek není šipka, která se má objevit (-20 kvůli zpoždění)
                {
                    // Vytvoření nové šipky
                    Image image = new Image();
                    image.Source = IndicatorArrows[i].Source;
                    image.Margin = new Thickness(IndicatorArrows[i].Margin.Left, IndicatorArrows[i].Margin.Top + 400, IndicatorArrows[i].Margin.Right, IndicatorArrows[i].Margin.Bottom - 400);
                    MainGrid.Children.Add(image);
                    CurrentArrows[i].Add(image);
                }
                i++;
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e) // Zmáčknutí klávesy
        {
            switch (e.Key)
            {
                case Key.Left:
                    CheckForHit(0);
                    break;
                case Key.Up:
                    CheckForHit(1);
                    break;
                case Key.Down:
                    CheckForHit(2);
                    break;
                case Key.Right:
                    CheckForHit(3);
                    break;
            }
        }
        public async void CheckForHit(int ColumnNum) // Hlavní funkce pro kontrolu zmáčknutí
        {
            IndicatorArrows[ColumnNum].OpacityMask = Brushes.White; // Zvýraznění zmáčklé šipky
            bool X = CheckForHit2(ColumnNum);
            if (X == false) // Ubere skóre při nepovedeném pokusu
            {
                CurrentScore--;
                FailSound();
                PochvalaLabel.Content = Urazky[new Random().Next(Urazky.Count - 1)]; // Změna oznamovacího textu
                ChangeText();
            }
            else
            {
                PochvalaLabel.Content = Pochvaly[new Random().Next(Pochvaly.Count-1)]; // Změna oznamovacího textu
                ChangeText();
            }
            ScoreLabel.Content = "SCORE: " + CurrentScore + "/" + MaxScore;
            await Task.Delay(100);
            BrushConverter BConv = new System.Windows.Media.BrushConverter();
            IndicatorArrows[ColumnNum].OpacityMask = (Brush)BConv.ConvertFromString("#7FFFFFFF"); // Uvedení šipky do původního stavu
        }
        public bool CheckForHit2(int ColumnNum) // Zkontroluje všechy existující šipky v určitém sloupci, aby zjistila jestli se uživatel trefil
        {
            foreach (Image CurrentArrow in CurrentArrows[ColumnNum])
            {
                if (IndicatorArrows[ColumnNum].Margin.Top + Tolerance >= CurrentArrow.Margin.Top && CurrentArrow.Margin.Bottom >= IndicatorArrows[ColumnNum].Margin.Bottom - Tolerance && MainGrid.Children.Contains(CurrentArrow))
                {
                    // Zjištění a odměnění úspšného pokusu + vymazání šipky
                    CurrentScore++;
                    CurrentArrows[ColumnNum].Remove(CurrentArrow);
                    MainGrid.Children.Remove(CurrentArrow);
                    return true;
                }
            }
            return false;
        }
        public async void ChangeText() // Funkce pro zmizení oznamovacího textu
        {
            PochvalaCount++;
            await Task.Delay(500);
            PochvalaCount--;
            if (PochvalaCount == 0) {
                PochvalaLabel.Content = "";
            }
        }
        public async void FailSound() // Přehrání zvuku selhání
        {
            PlayerOfFail.Stop();
            PlayerOfFail.Play();
        }
    }
}
