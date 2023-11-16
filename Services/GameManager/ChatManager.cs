
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
            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if(idPlayer == player.IdPlayer)
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
                        Token = player.GameManagerCallBack.UptdatePiecePlayer(game)
                    };
                    PlayersAux.Add(playerAux);
                }
            }
        }
    }
}
