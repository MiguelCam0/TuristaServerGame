using Contracts.IDataBase;
using Contracts.IGameManager;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
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

            int playerPosition = CurrentGames[game.IdGame].Players.Peek().Position + dieOne + dieSecond;
            CurrentGames[game.IdGame].Players.Peek().Position = playerPosition;

            if (playerPosition >= 40)
            {
                CurrentGames[game.IdGame].Players.Peek().Position -= 40;
            }

            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameLogicManagerCallBack.PlayDie(dieOne, dieSecond);
                player.GameLogicManagerCallBack.MovePlayerPieceOnBoard(CurrentGames[game.IdGame].Players.Peek(), CurrentBoards[game.IdGame].GetProperty(playerPosition));
                if (player.IdPlayer == CurrentGames[game.IdGame].Players.Peek().IdPlayer)
                {
                    player.GameLogicManagerCallBack.ShowCard(CurrentBoards[game.IdGame].GetProperty(playerPosition + 1));
                }
            }

            CurrentGames[game.IdGame].Players.Enqueue(CurrentGames[game.IdGame].Players.Peek());
            CurrentGames[game.IdGame].Players.Dequeue();
            UpdatePlayersInGame(game);
        }

        public void UpdatePlayersInGame(Game game)
        {
            foreach(var player in CurrentGames[game.IdGame].Players)
            {
                player.GameLogicManagerCallBack.LoadFriends(CurrentGames[game.IdGame].Players);
            }
        }

        public void PurchaseProperty(Property property, Player player, int idGame)
        {
            player.Money -= property.BuyingCost;
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == player.IdPlayer)
                {
                    playerAux.Money -= property.BuyingCost;
                    Console.WriteLine(playerAux.Name + " dinero: " + playerAux.Money);
                    break;
                }
                
            }
            CurrentBoards[idGame].RegisterPurchaseProperty(player, property);
        }

        public void StartAuction(int idGame, Property property)
        {
            foreach (var player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.OpenAuctionWindow(property);
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

        public void GetActionCard(int idGame)
        {
            Card card = new Card();
            card.Action = 5;
            CurrentGames[idGame].Players.Last().GameLogicManagerCallBack.ShowEvent(card.Action);
            if (card.Action == 1) { CurrentGames[idGame].Players.Last().GameLogicManagerCallBack.GoToJail(); }
            else if(card.Action == 2) { PayAnotherPlayer(idGame, card.RandomCash); }
            else if (card.Action == 3) { CurrentGames[idGame].Players.Last().GameLogicManagerCallBack.PayTaxes(card.RandomCash); }
            else if (card.Action == 4) { CurrentGames[idGame].Players.Last().GameLogicManagerCallBack.GetPay(card.RandomCash); }
            else if (card.Action == 5) { MovePlayer(idGame, card.Action); }
            else if (card.Action == 6) { MovePlayer(idGame, (-1 * card.Action)); }
        }

        private void PayAnotherPlayer(int idGame, int money)
        {
            CurrentGames[idGame].Players.Peek().GameLogicManagerCallBack.PayTaxes(money);
            CurrentGames[idGame].Players.Last().GameLogicManagerCallBack.GetPay(money);
        }

        public void MovePlayer(int idGame, int spaces)
        {
            CurrentGames[idGame].Players.Peek().Position = CurrentGames[idGame].Players.Peek().Position + spaces;
            Console.WriteLine(CurrentGames[idGame].Players.Peek().Name);
           
            if (CurrentGames[idGame].Players.Peek().Position >= 40)
            {
                CurrentGames[idGame].Players.Peek().Position -= 40;
            }

            foreach (var player in CurrentGames[idGame].PlayersInGame)
            {
                player.GameLogicManagerCallBack.MovePlayerPieceOnBoard(CurrentGames[idGame].Players.Peek(), CurrentBoards[idGame].GetProperty(CurrentGames[idGame].Players.Peek().Position));
                
                if (player.IdPlayer == CurrentGames[idGame].Players.Peek().IdPlayer)
                {
                    player.GameLogicManagerCallBack.ShowCard(CurrentBoards[idGame].GetProperty(CurrentGames[idGame].Players.Peek().Position + 1));
                }
            }
        }
    }
}
