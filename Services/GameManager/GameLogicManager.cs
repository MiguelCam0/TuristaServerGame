using Contracts.IGameManager;
using Contracts.ISessionManager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameLogicManager
    {
        public static int Turn = 0;
        public void PlayTurn(Game game)
        {
            Random random = new Random();
            int dieOne = random.Next(1, 6);
            int dieSecond = random.Next(1, 6);

            CurrentGames[game.IdGame].Players.Peek().Position = CurrentGames[game.IdGame].Players.Peek().Position + dieOne + dieSecond;


            if (CurrentGames[game.IdGame].Players.Peek().Position >= 40)
            {
                CurrentGames[game.IdGame].Players.Peek().Position -= 40;
            }

            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameLogicManagerCallBack.PlayDie(dieOne, dieSecond);
                player.GameLogicManagerCallBack.MovePlayerPieceOnBoard(CurrentGames[game.IdGame].Players.Peek(), Turn);
                
            }

            if(Turn == CurrentGames[game.IdGame].PlayersInGame.Count - 1)
            {
                Turn = 0;
            }
            else
            {
                Turn++;
            }
        }

        public void UpdatePlayerService(int idPlayer, int idGame)
        {
            IGamerLogicManagerCallBack context = OperationContext.Current.GetCallbackChannel<IGamerLogicManagerCallBack>();
            foreach (Player player in CurrentGames[idGame].PlayersInGame)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.GameLogicManagerCallBack = context;
                    break;
                }
            }
        }

    }
}
