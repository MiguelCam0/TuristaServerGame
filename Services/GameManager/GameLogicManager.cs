using Contracts.IGameManager;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameLogicManager
    {
        public static Dictionary<int, Board> CurrentBoards = new Dictionary<int, Board>();

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
                    player.GameLogicManagerCallBack.ShowCard(CurrentBoards[game.IdGame].GetProperty(CurrentGames[game.IdGame].Players.Peek().Position + 1));
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

        public void StartAuction(int idGame)
        {
            foreach (var player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.OpenAuctionWindow();
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

        public void MakeBid(int idGame, int IdPlayer, int Bid)
        {
            foreach (Player player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.UpdateBids(IdPlayer, Bid);
            }
        }

        public void StopAuction(int idGame,int winner, int winnerBid, Property property)
        {
            foreach (Player player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.EndAuction(property, winner, winnerBid);
            }
        }

        public void UpdateQueu(int idGame)
        {
            Queue<Player> turns = CurrentGames[idGame].Players;
            foreach(Player player in turns)
            {
                player.GameLogicManagerCallBack.UpdateTurns(turns);
            }
        }
    }
}
