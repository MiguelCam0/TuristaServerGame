
using Contracts.IGameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {

        public void SendMessage(int idGame, string message)
        {
            foreach (var player in CurrentGames[idGame].Players) 
            {
                player.GameManagerCallBack.GetMessage(message);
            }
            
        }

        public void UpdatePlayerGame(Game game, int idPlayer)
        {
            int turn = 0;
            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.Token = player.GameManagerCallBack.UptdatePiecePlayer(game);
                    player.Token.PartNumber = turn;

                    break;
                }
                turn++;
            }
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

        private static int TotalGuestPlayers = 0;
        public void AddGuestToGame(int idGame, int idPlayer)
        {
            TotalGuestPlayers++;
            Player player = new Player(idPlayer, "JugadorInvitado_"+TotalGuestPlayers);
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[idGame].Players.Enqueue(player);
            CurrentGames[idGame].PlayersInGame.Add(player);
        }
    }
}
