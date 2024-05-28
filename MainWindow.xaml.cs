using crosszeros;
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
using static crosszeros.GameLogic;
using static crosszeros.GameDataAccess;

namespace crosszeros
{
    public partial class MainWindow : Window
    {
        public GameLogic _GameLogic = new GameLogic();
        public MainWindow()
        {
            InitializeComponent();
            var dataAccess = new GameDataAccess("database.db");
            _GameLogic = new GameLogic(dataAccess);
            LoadGameHistory();
        }

        


        private void PlayerClicksSpace(object sender, RoutedEventArgs e)
        {
            var space = (Button)sender;
            if (!String.IsNullOrWhiteSpace(space.Content?.ToString())) return;
            space.Content = _GameLogic.CurrentPlayer;

            var coordinates = space.Tag.ToString().Split(',');
            var xValue = int.Parse(coordinates[0]);
            var yValue = int.Parse(coordinates[1]);

            var buttonPosition = new Position() { x = xValue, y = yValue };
            _GameLogic.UpdateBoard(buttonPosition, _GameLogic.CurrentPlayer);
            if (_GameLogic.PlayerWin())
            {
                UpdateUI();
                _GameLogic.dataAccess.SaveGameResult(_GameLogic.CurrentPlayer);
                LoadGameHistory();

                return;
            }
            _GameLogic.SetNextPlayer();
            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (var button in gridBoard.Children.OfType<Button>())
            {
                var coordinates = button.Tag.ToString().Split(',');
                var xValue = int.Parse(coordinates[0]);
                var yValue = int.Parse(coordinates[1]);

                button.Content = _GameLogic.CurrentBoard[xValue, yValue];
            }

            if (_GameLogic.PlayerWin())
            {
                WinScreen.Text = $"Победа игрока {_GameLogic.CurrentPlayer}";
                WinScreen.Visibility = Visibility.Visible;
                resetGameState();
                return;
            }

            if (_GameLogic.isDraw())
            {
                WinScreen.Text = "Ничья";
                WinScreen.Visibility = Visibility.Visible;
                resetGameState();
                return;
            }

           WinScreen.Visibility = Visibility.Collapsed;
        }

        public void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            foreach (var control in gridBoard.Children)
            {
                if (control is Button)
                {
                    ((Button)control).Content = String.Empty;
                }
            }

            resetGameState();
            WinScreen.Visibility = Visibility.Collapsed;
        }

        private void botLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int selectedItem = botLevelComboBox.SelectedIndex;
            if (selectedItem == 0)
            {
                _GameLogic.bot_mode = 0;
            }
            else if (selectedItem == 1)
            {
                _GameLogic.bot_mode = 1;
            }
        }


        public void resetGameState()
        {
            var dataAccess = new GameDataAccess("database.db");
            _GameLogic = new GameLogic(dataAccess);
            _GameLogic.bot_mode = botLevelComboBox.SelectedIndex;
            LoadGameHistory();
        }

        private void LoadGameHistory()
        {
            List<GameResult> lastFiveGames = _GameLogic.dataAccess.GetLastFiveGames();
            gamesHistoryListBox.Items.Clear();
            foreach (var gameResult in lastFiveGames)
            {
                gamesHistoryListBox.Items.Add($"ID игры: {gameResult.Id}, Победитель: {gameResult.Winner}");
            }
        }
    }
}