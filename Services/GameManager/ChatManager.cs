
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
                if(idPlayer == player.IdPlayer)
                {
                    if(CheckUser(player) == 0)
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
                            Token = player.GameManagerCallBack.UptdatePiecePlayer(game),
                        };
                        playerAux.Token.PartNumber = turn;
                        PlayersAux.Add(playerAux);
                    } 
                    else
                    {
                        foreach (var playerAux in PlayersAux)
                        {
                            if (player.IdPlayer == playerAux.IdPlayer)
                            {
                                playerAux.Token = player.GameManagerCallBack.UptdatePiecePlayer(game);
                                playerAux.Token.PartNumber = turn;
                                break;
                            }
                        }
                    } 
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
    }
}
