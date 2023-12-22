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
        public int numberGuestPlayers = 0;
        public void AddGame(Game game)
        {
            CurrentGames.Add(game.IdGame, game);
            CurrentGames[game.IdGame].Players = new Queue<Player>();
            CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
        }

        public void AddPlayerToGame(int Game, Player player)
        {
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[Game].Players.Enqueue(player);
            CurrentGames[Game].PlayersInGame.Add(player);
            CurrentGames[Game].Slot--;
        }

        public void AddGuestPlayerToGame(int Game, int idPlayer)
        {
            Player player = new Player(idPlayer, "JugadorInvitado"+numberGuestPlayers++, 10000);
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[Game].Players.Enqueue(player);
            CurrentGames[Game].PlayersInGame.Add(player);
            CurrentGames[Game].Slot--;
        }

        public void UpdatePlayers(int IdGame)
        {
            foreach (var player in CurrentGames[IdGame].Players)
            {
                player.GameManagerCallBack.UpdateGame();
                player.GameManagerCallBack.AddVisualPlayers();
            }
        }

        public void StartGame(Game game)
        {
            CurrentGames[game.IdGame].Status = Game.Game_Situation.Ongoing;
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
                player.GameLogicManagerCallBack.LoadFriends(CurrentGames[game.IdGame].Players);
            }
        }
    }
}
