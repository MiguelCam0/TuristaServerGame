using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IGameManager;

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
            foreach(var player in CurrentGames[IdGame].Players)
            {
                player.GameManagerCallBack.UpdateGame();
                player.GameManagerCallBack.AddVisualPlayers();
            }
        }

        public void StartGame(Game game)
        {
            foreach(var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.MoveToGame(game);
            }
        }

        public void InitializeGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.PreparePieces(game, PlayersAux);
            }
            InitializeBoard();
        }

        public void SelectedPiece(Game game, string token)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.BlockPiece(token);
            }
        }

        public void UnSelectedPiece(Game game, string token)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.UnblockPiece(token);
            }
        }
    }
}
