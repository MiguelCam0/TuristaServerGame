using Contracts.IDataBase;
using Contracts.IGameManager;
using Contracts.ISessionManager;
using Microsoft.Win32;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameLogicManager
    {
        public static Dictionary<int,Board> CurrentBoards = new Dictionary<int,Board>();

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
                player.GameLogicManagerCallBack.MovePlayerPieceOnBoard(CurrentGames[game.IdGame].Players.Peek(), CurrentBoards[game.IdGame].GetProperty(CurrentGames[game.IdGame].Players.Peek().Position));
                if (player.IdPlayer == CurrentGames[game.IdGame].Players.Peek().IdPlayer)
                {
                    player.GameLogicManagerCallBack.ShowCard(CurrentBoards[game.IdGame].GetProperty(CurrentGames[game.IdGame].Players.Peek().Position));
                }
            }

            CurrentGames[game.IdGame].Players.Enqueue(CurrentGames[game.IdGame].Players.Peek());
            CurrentGames[game.IdGame].Players.Dequeue();
        }

        public void PurchaseProperty(Property property, Player player, int idGame)
        {
            player.Money -= property.BuyingCost;
            CurrentBoards[idGame].RegisterPurchaseProperty(player, property);
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
