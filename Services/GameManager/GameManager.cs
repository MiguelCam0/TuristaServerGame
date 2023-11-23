using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IGameManager;
using Services.GameManager;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {
        public static Dictionary<int, Game> CurrentGames = new Dictionary<int, Game>();
        public static List<Player> PlayersAux = new List<Player>();
        public void AddGame(Game game)
        {
            CurrentGames.Add(game.IdGame, game);
            CurrentGames[game.IdGame].Players = new Queue<Player>();
            CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
            Console.WriteLine(CurrentGames.First().Key);
        }

        void PreparePlayers(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                Player playerAux = new Player
                {
                    IdPlayer = player.IdPlayer,
                    Name = player.Name,
                    properties = new List<Property>(),
                    loser = false,
                    Position = -1,
                    Jail = false,
                    Money = 2000000,
                    Token = player.Token
                };
                PlayersAux.Add(playerAux);
            }
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
                Console.WriteLine(player.GameManagerCallBack);
                player.GameManagerCallBack.UpdateGame();
                player.GameManagerCallBack.AddVisualPlayers();
                Console.WriteLine(player.Name);
            }
        }

        public void StartGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.MoveToGame(game);
            }
            Board board = new Board();
            CurrentBoards.Add(game.IdGame, board);
        }

        public void InitializeGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.PreparePieces(game, CurrentGames[game.IdGame].PlayersInGame);
            }
        }

        public void UnSelectedToken(Game game, string token)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.UnblockToken(token);
            }
        }

        public void UpdateGameServer(int idPlayer, Game game)
        {
            CurrentGames[game.IdGame] = game;
            var thisGame = CurrentGames[game.IdGame];
            foreach (var player in thisGame.Players)
            {
                if (player.IdPlayer != idPlayer)
                {
                    player.GameManagerCallBack.UpdateGame();
                }
            }
        }

        public void UpdateCallBackPlayer(int idGame, int idPlayer)
        {
            throw new NotImplementedException();
        }
    }
}
