using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IGameManager;
using log4net;
using log4net.Config;
using Services.GameManager;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(PlayerManager));
        public static Dictionary<int, Game> CurrentGames = new Dictionary<int, Game>();
        public void AddGame(Game game)
        {
            CurrentGames.Add(game.IdGame, game);
            CurrentGames[game.IdGame].Players = new Queue<Player>();
            CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
            Console.WriteLine(CurrentGames.First().Key);
        }

        public void AddPlayerToGame(int Game, Player player)
        {
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[Game].Players.Enqueue(player);
            CurrentGames[Game].PlayersInGame.Add(player);
        }

        public void UpdatePlayers(int IdGame)
        {
            foreach (var player in CurrentGames[IdGame].Players)
            {
                try
                {
                    Console.WriteLine(player.GameManagerCallBack);
                    player.GameManagerCallBack.UpdateGame();
                    player.GameManagerCallBack.AddVisualPlayers();
                    Console.WriteLine(player.Name);

                }
                catch(TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        public void StartGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.MoveToGame(game);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
            Board board = new Board();
            CurrentBoards.Add(game.IdGame, board);
        }

        public void InitializeGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.PreparePieces(game, CurrentGames[game.IdGame].PlayersInGame);
                    player.GameLogicManagerCallBack.LoadFriends(CurrentGames[game.IdGame].Players);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }
    }
}
