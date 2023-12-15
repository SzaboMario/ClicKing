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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Működés: Alap esetben minden gomb cyan, ha kattintunk valamire átvált pirosra, 
    /// majd ha kattintunk egy másikra akkoráadja neki az értékét(nevét) és minden újra cyan lesz és a kiválasztások is eltünnek
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamReader sr;
        private StreamWriter sw;
        private Stopwatch start;
        private readonly Random rnd = new Random();
        private string tmp = "", firstBtnName = "", secondBtnName = "";
        private Button firstBtn, secondBtn;
        readonly List<Button> btnList = new List<Button>();
        private int rowColumnNumber;
        private bool win;
        public MainWindow()
        {
            InitializeComponent();
            number.Text = "3";
            initializeGame();
            LoadLeaderBoardFile();
        }

        private void timeTh()
        {
            while (start.IsRunning)
            {
                timeLabel.Dispatcher.Invoke(() => { timeLabel.Text = start.ElapsedMilliseconds.ToString()+"ms"; });               
                Thread.Sleep(10);
            }
        }

        private void LoadLeaderBoardFile()
        {
            if (File.Exists("times.txt"))
            {
                scoreGrid.Items.Clear();
                PlayerData pd;
                sr = new StreamReader("times.txt");
                while (!sr.EndOfStream)
                {
                    pd = new PlayerData(sr.ReadLine());
                    scoreGrid.Items.Add(pd);
                }
                sr.Close();
            }
        }
        private void WriteLeaderBoardToFile(string leaderBoard)
        {
            sw = new StreamWriter("times.txt");
            sw.Write(leaderBoard);
            sw.Flush();
            sw.Close();
        }
        private class PlayerData
        {
            public PlayerData(string player)
            {
                string[] playArr = player.Split(",");
                this.PlayerName = playArr[0];
                this.PlayerTime = playArr[1];
            }
            public string PlayerName { get; set; }
            public string PlayerTime { get; set; }
        }
        private void WriteWinnerToTable(string winner)
        {
            PlayerData pd;         
            List<PlayerData> playerDataList = new List<PlayerData>();
            //2. hozzáadjuk a jelenlegi nyertest 
            //4. kiiratjuk a scorelistet
            //5. fájlba írjuk a listát
            pd = new PlayerData(winner);
            foreach (PlayerData it in scoreGrid.Items)
            {
                if (it.PlayerName==pd.PlayerName)
                {
                    scoreGrid.Items.Remove(it);
                    break;
                }
            }
            scoreGrid.Items.Add(pd);//score táblához adás teszt miatt
        }

        private void initializeGame() 
        {
            rowColumnNumber = Convert.ToInt32(number.Text);
            if (rowColumnNumber <= 10 && rowColumnNumber >= 3)
            {
                table.Children.Clear();
                table.ColumnDefinitions.Clear();
                table.RowDefinitions.Clear();
                for (int i = 1; i <= rowColumnNumber; i++)
                {
                    ColumnDefinition colDef1 = new ColumnDefinition();
                    RowDefinition rowDef1 = new RowDefinition();
                    table.ColumnDefinitions.Add(colDef1);
                    table.RowDefinitions.Add(rowDef1);
                }
                int coun = 1;
                for (int i = 0; i < table.ColumnDefinitions.Count; i++)
                {
                    for (int j = 0; j < table.RowDefinitions.Count; j++)
                    {
                        Button btn = new Button();
                        btn.Name = $"Btn{coun}";
                        Grid.SetRow(btn, i);
                        Grid.SetColumn(btn, j);
                        btnList.Add(btn);
                        btn.Click += new RoutedEventHandler(game3);
                        table.Children.Add(btn);
                        coun++;
                    }
                }
                gameInitialize(rowColumnNumber);
                foreach (Button item in table.Children)
                {
                    item.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Please set 3-10");
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            initializeGame();
            foreach (Button item in table.Children)
            {
                item.IsEnabled = true;
            }
            start = new Stopwatch();
            start.Start();
            Thread t1 =new Thread( new ThreadStart(timeTh));
            t1.Start();
            GC.Collect();
        }
        private void number_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        private void gameInitialize(int num)
        {
            List<int> intList = new List<int>(Enumerable.Range(1, num * num));
            var result = intList.Select(x => new { value = x, order = rnd.Next() })
             .OrderBy(x => x.order).Select(x => x.value).ToList();
            foreach (Button item in table.Children)
            {
                item.Content = result[0];
                item.Background = Brushes.Cyan;
                item.FontSize = 40;
                result.RemoveAt(0);
            }
        }
        private void game3(object sender, RoutedEventArgs e)
        {
            foreach (Button item in table.Children)
            {
                item.Background = Brushes.Cyan;
            }
            if (firstBtnName.Length == 0)
            {
                firstBtn = (Button)sender;
                firstBtn.Background = Brushes.Red;
                firstBtnName = firstBtn.Content.ToString();
                tmp = firstBtnName;
            }
            else
            {
                secondBtn = (Button)sender;
                secondBtnName = secondBtn.Content.ToString();
                firstBtn.Content = secondBtnName;
                secondBtn.Content = tmp;
                firstBtnName = ""; secondBtnName = ""; tmp = "";
                CheckResult(secondBtn);
            }
        }
      
        private void CheckResult(Button second)
        {//
            foreach (Button item in table.Children)
            {
                if ("Btn" + item.Content.ToString() != item.Name)
                {
                    win = false;
                    return;
                }
                else
                {
                    win = true;
                }
            }
            if (win)
            {
                string sLeaderBoard = "";
                foreach (Button item in table.Children)
                {
                    item.IsEnabled = false;
                }
                string pName = name.Text;
                if (pName.Length < 1)
                {
                    pName = "Guest";
                }
                start.Stop();
                TimeSpan ts = start.Elapsed;
                WriteWinnerToTable($"{pName},{ts.TotalMilliseconds}");
                foreach (PlayerData item in scoreGrid.Items)
                {
                    sLeaderBoard += $"{item.PlayerName},{item.PlayerTime}\r\n";
                }
                WriteLeaderBoardToFile(sLeaderBoard);
                MessageBoxResult res;
                res = MessageBox.Show($"{pName}!\nNyertél!\nIdőd: {ts.Minutes}perc, {ts.Seconds}másodperc.\nMég egy játék?", "Win", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Button_Click("", new RoutedEventArgs());
                }
                else
                {
                  //  Environment.Exit(0);
                }
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
