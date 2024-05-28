using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.Sqlite;

[assembly: InternalsVisibleToAttribute("TicTacToeTests")]
namespace crosszeros
{
    public class GameResult
    {
        public int Id { get; set; }
        public string Winner { get; set; }
    }

    public class GameDataAccess
    {
        private string connectionString;

        public GameDataAccess(string dbPath)
        {
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

               
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS GameResults (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Winner TEXT
                )";
                createTableCmd.ExecuteNonQuery();
            }
        }

        public void SaveGameResult(string winner)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = "INSERT INTO GameResults (Winner) VALUES (@Winner)";
                insertCmd.Parameters.AddWithValue("@Winner", winner);
                insertCmd.ExecuteNonQuery();
            }
        }

        public List<GameResult> GetLastFiveGames()
        {
            List<GameResult> games = new List<GameResult>();
            string connectionString = "Data Source=database.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT Id,Winner FROM GameResults ORDER BY id DESC LIMIT 5";
                using (var command = new SqliteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                games.Add(new GameResult{Id = Convert.ToInt32(reader["Id"]),Winner = reader["Winner"].ToString()});
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                               
                            }
                        }
                    }
                }
            }

            return games;
        }

    }

    public class GameLogic()
    {

        public string CurrentPlayer { get; set; } = X;
        public int bot_mode = 0;
        internal const string X = "X";
        internal const string O = "O";
        private string[,] Board = new string[3, 3];
        private Random random = new Random();
        public string[,] CurrentBoard => Board;
        ///        public GameDataAccess dataAccess = new GameDataAccess("database.db");
        public  GameDataAccess dataAccess;

        public GameLogic(GameDataAccess dataAccess) : this()
        {
            this.dataAccess = dataAccess;
        }

        public void SetNextPlayer()
        {
            if (CurrentPlayer == X)
            {
                CurrentPlayer = O;
                BotMove();
            }
            else
            {
                CurrentPlayer = X;
            }
        }


        public bool PlayerWin()
        {
            return PlayerWin(Board);
        }

        public bool isDraw()
        {
            return isDraw(Board);
        }
        public bool isDraw(string[,] board)
        {
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (String.IsNullOrWhiteSpace(board[i, 0]))
                    {
                            return false;
                    }
                }
            }
            return true;
        }


        public bool PlayerWin(string[,] board)
        {
            if (RowWins(board)) return true;
            if (ColumnWins(board)) return true;
            if (DiagWins(board)) return true;

            return false;
        }

        public bool RowWins(string[,] board)
        {
            for (var i = 0; i < 3; i++)
            {
                if (!String.IsNullOrWhiteSpace(board[i, 0]))
                {
                    if (board[i, 0] == board[i, 1] && board[i, 0] == board[i, 2])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ColumnWins(string[,] board)
        {
            for (var i = 0; i < 3; i++)
            {
                if (!String.IsNullOrWhiteSpace(board[0, i]))
                {
                    if (board[0, i] == board[1, i] && board[0, i] == board[2, i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool DiagWins(string[,] board)
        {
            if (!String.IsNullOrWhiteSpace(board[1, 1]))
            {
                if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
                {
                    return true;
                }

                if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateBoard(Position postion, string value)
        {
            Board[postion.x, postion.y] = value;
        }
        public void BotMove()
        {
            if (bot_mode == 0)
            {
                var emptyPositions = Enumerable.Range(0, 3)
                    .SelectMany(x => Enumerable.Range(0, 3), (x, y) => new Position(x, y))
                    .Where(pos => String.IsNullOrWhiteSpace(Board[pos.x, pos.y]))
                    .ToList();

                if (emptyPositions.Any())
                {
                    var move = emptyPositions[random.Next(emptyPositions.Count)];
                    UpdateBoard(move, O);
                    if (!PlayerWin())
                    {
                        CurrentPlayer = X;
                    }
                    else{ dataAccess.SaveGameResult(CurrentPlayer); }
                }
            }
            else
            {
                Position bestMove = FindBestMove(Board);
                UpdateBoard(bestMove, O);
                if (!PlayerWin())
                {
                    CurrentPlayer = X;
                }
                else { dataAccess.SaveGameResult(CurrentPlayer); }

            }
        }
        public Position FindBestMove(string[,] board)
        {
            int bestScore = int.MinValue;
            Position bestMove = new Position(-1, -1);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (String.IsNullOrWhiteSpace(board[i, j]))
                    {
                        
                        board[i, j] = O;
                        int score = Minimax(board, 0, false);
                        board[i, j] = "";
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = new Position(i, j);
                        }
                    }
                }
            }

            return bestMove;
        }

        public int Minimax(string[,] board, int depth, bool isMaximizing)
        {

            if (PlayerWin(board))
            {
                return isMaximizing ? -1 : 1;
            }
            else if (isDraw(board))
            {
                return 0;
            }

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (String.IsNullOrWhiteSpace(board[i, j]))
                        {
                            board[i, j] = O;
                            int score = Minimax(board, depth + 1, false);
                            board[i, j] = "";
                            bestScore = Math.Max(bestScore, score);
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (String.IsNullOrWhiteSpace(board[i, j]))
                        {
                            board[i, j] = X;
                            int score = Minimax(board, depth + 1, true);
                            board[i, j] = "";
                            bestScore = Math.Min(bestScore, score);
                        }
                    }
                }
                return bestScore;
            }
        }

        public struct Position
        {
            public int x { get; set; }
            public int y { get; set;  }

            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

    }

}

