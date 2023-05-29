using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FUNKY
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Rap> RapList = new List<Rap>();
        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        public void Load() // Načtení dat ze souboru
        {
            RapList.Clear(); // Vytvoření prostoru pro nová a aktuální data
            DirectoryInfo Dinfo = new DirectoryInfo("../../../SaveFiles");
            foreach (FileInfo F in Dinfo.GetFiles())
            {
                using (TextReader TR = new StreamReader(F.FullName))
                {
                    Uri RapUri = new Uri(TR.ReadLine(),UriKind.Relative); // Nastavení odkazu na mp3 soubor
                    List<List<int>> ArrowList = new List<List<int>>(); // Získání a uložení dat o šipkách
                    for (int i = 0; i < 4;i++)
                    {
                        ArrowList.Add(new List<int>());
                        foreach (string s in TR.ReadLine().Split(","))
                        {
                            if (!string.IsNullOrEmpty(s))
                                ArrowList[i].Add(int.Parse(s));
                        }
                    }
                    Rap NewRap = new Rap(RapUri, F.Name.Split(".")[0], ArrowList); // Vytvoření classy a uložení hodnot
                    RapList.Add(NewRap);
                }
            }
            SongListComboBox.ItemsSource = null;
            SongListComboBox.ItemsSource = RapList;
        }

        private void StartClick(object sender, RoutedEventArgs e) // Začít hrát
        {
            if (SongListComboBox.SelectedItem!=null)
            {
                RapWindow X = new RapWindow((SongListComboBox.SelectedItem as Rap),this);
                X.ShowDialog();
            }
        }
        private void EditClick(object sender, RoutedEventArgs e) // Upravit soubor
        {
            if (SongListComboBox.SelectedItem != null)
            {
                Editor X = new Editor(SongListComboBox.SelectedItem as Rap);
                X.Closed += (s, e) => Load();
                X.ShowDialog();
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e) // Speciální efekt na tlačitka
        {
            (sender as Button).Content = ">" + (sender as Button).Content;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e) // Zrušení speciálního efektu na tlačitka
        {
            (sender as Button).Content = (sender as Button).Content.ToString().Split(">")[1];
        }
        private void CreateNewFile(object sender, RoutedEventArgs e) // Vytvoření nové třídy pro vytváření nového souboru
        {
            Rap NewRap = new Rap(new Uri("../../../Raps/ ", UriKind.Relative), "NewRap", new List<List<int>>() { new List<int>() , new List<int>() , new List<int>() , new List<int>() });
            Editor X = new Editor(NewRap);
            X.Closed += (s, e) => Load();
            X.ShowDialog();
        }
    }
}
