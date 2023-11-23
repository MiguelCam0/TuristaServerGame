
using Contracts.IGameManager;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int CheckUser(Player player)
        {
            int check = 0;
            foreach (var playerAux in PlayersAux)
            {
                if(player.IdPlayer == playerAux.IdPlayer)
                {
                    check = 1; break;
                }
            }
            return check;
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
