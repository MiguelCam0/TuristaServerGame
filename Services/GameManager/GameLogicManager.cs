using Contracts.IDataBase;
using Contracts.IGameManager;
using DataBase;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web.UI.WebControls;

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

            Player currentPlayer = CurrentGames[game.IdGame].Players.Peek();
            int playerPosition = currentPlayer.Position + 1; //dieOne + dieSecond;
            currentPlayer.Position = playerPosition;

            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameLogicManagerCallBack.PlayDie(dieOne, dieSecond);
            }

            MovePlayer(game.IdGame, playerPosition, ref currentPlayer);
            AdvanceTurn(game.IdGame);
        }

        private void AdvanceTurn(int idGame)
        {
            CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            if (CurrentGames[idGame].Players.Peek().Jail == true && CurrentGames[idGame].Players.Peek().Loser == false)
            {
                CurrentGames[idGame].Players.Peek().Jail = false;
                CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            }
            else if (CurrentGames[idGame].Players.Peek().Loser == true)
            {
                CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            }
        }

        public int MakeRentalPayment(int idOwnerLand, int idRenter, long amountOfRent, int idGame)
        {
            int result = 0;

            foreach (var player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == idRenter)
                {
                    bool band = false;
                    if (player.Money - amountOfRent < 0)
                    {
                        foreach (var property in CurrentBoards[idGame].board)
                        {
                            if(property.Owner != null && property.Owner.IdPlayer == idRenter && property.IsMortgaged == false)
                            {
                                RealizePropertyMortgage(idGame, property, idRenter);
                                player.GameLogicManagerCallBack.UpdatePropertyStatus(property);
                            }

                            if(player.Money - amountOfRent >= 0)
                            {
                                band = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        band = true;
                    }

                    if (!band)
                    {
                        DeclareLosingPlayer(player, idGame); 
                        result = 1; break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if(result == 0)
            {
                foreach (var player in CurrentGames[idGame].Players)
                {
                    if (player.IdPlayer == idOwnerLand)
                    {
                        player.Money += amountOfRent;
                        player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                        player.GameLogicManagerCallBack.NotifyPlayerOfEvent(0);
                    }
                    else if (player.IdPlayer == idRenter)
                    {
                        player.Money -= amountOfRent;
                        player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                        player.GameLogicManagerCallBack.NotifyPlayerOfEvent(1);
                    }
                }
            }

            return result;
        }

        public void UpdatePlayersInGame(int idGame)
        {
            foreach(var player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
            }
        }

        public void PurchaseProperty(Property property, Player player, int idGame)
        {
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == player.IdPlayer)
                {
                    playerAux.Money -= property.DefinitiveCost;
                    CurrentBoards[idGame].RegisterPurchaseProperty(playerAux, property);
                    break;
                }
            }
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
            UpdatePlayersInGame(idGame);
        }

        public void GetActionCard(int idGame, Player player)
        {
            Wildcard wildcard = new Wildcard();

            if (wildcard.Action == 1) { GoToJail(player, idGame); }
            else if(wildcard.Action == 2) { PayAnotherPlayer(idGame, player); }
            else if (wildcard.Action == 3) { PayTaxes(ref player, idGame); }
            else if (wildcard.Action == 4) { GetPay(ref player); }
            else if (wildcard.Action == 5) { player.Position += 3; MovePlayer(idGame, player.Position, ref player); }
            else if (wildcard.Action == 6) { player.Position -= 3; MovePlayer(idGame, player.Position, ref player); }

            player.GameLogicManagerCallBack.NotifyPlayerOfEvent(wildcard.Action);
        }

        private void PayTaxes(ref Player player, int idGame)
        {
            player.Money -= 200;

            if (player.Money < 0)
            {
                player.Money = 0;
            }
        }

        private void GetPay(ref Player player)
        {
            player.Money += 200;
        }

        private void PayAnotherPlayer(int idGame, Player player)
        {
            PayTaxes(ref player, idGame);
            Player playerAux = CurrentGames[idGame].Players.Peek();
            GetPay(ref playerAux);
        }

        public void MovePlayer(int idGame, int playerPosition, ref Player player)
        {
            if (player.Position >= 40)
            {
                player.Money += 200;
                player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                player.Position -= 40;
                playerPosition = player.Position;
            }

            foreach (var playerAux in CurrentGames[idGame].PlayersInGame)
            {
                playerAux.GameLogicManagerCallBack.MovePlayerPieceOnBoard(player, CurrentBoards[idGame].GetProperty(playerPosition));

                if (playerAux.IdPlayer == player.IdPlayer)
                {
                    Property currentProperty = CurrentBoards[idGame].board[playerPosition];
                    if (currentProperty.Owner == null)
                    {
                        playerAux.GameLogicManagerCallBack.ShowCard(CurrentBoards[idGame].GetProperty(playerPosition));
                    }
                    else if (currentProperty.Owner.IdPlayer != player.IdPlayer && currentProperty.IsMortgaged == false)
                    {
                        MakeRentalPayment(currentProperty.Owner.IdPlayer, player.IdPlayer, currentProperty.Taxes, idGame);
                    }
                }
            }
        }

        public void JailPlayer(int idGame, int idPlayer)
        {
            foreach (var player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.Jail = true;
                }
            }
        }

        public void RealizePropertyMortgage(int idGame, Property property, int idPlayer)
        {
            CurrentBoards[idGame].RegisterPropertyMortgage(property);
            foreach(var player in CurrentGames[idGame].Players)
            {
                if(player.IdPlayer == idPlayer)
                {
                    player.Money += property.BuyingCost / 2;
                    player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                }
            }
        }

        public void DeclareLosingPlayer(Player loserPlayer, int idGame)
        {
            foreach (Player player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == loserPlayer.IdPlayer)
                {
                    player.Loser = true;
                }

                player.GameLogicManagerCallBack.RemoveGamePiece(loserPlayer);
                VerifyGameCompletion(idGame);
            }
        }

        public void VerifyGameCompletion(int idGame)
        {
            int activePlayers = 0;
            int idWinner = 0;
            foreach (var player in CurrentGames[idGame].Players)
            {
                if(player.Loser == false)
                {
                    idWinner = player.IdPlayer;
                    activePlayers++;
                }
            }

            if(activePlayers == 1)
            {
                foreach (var player in CurrentGames[idGame].Players)
                {
                    player.GameLogicManagerCallBack.EndGame(idWinner);
                }
            }
        }

        public void PayPropertyMortgage(Game game, int idPlayer, Property mortgagedProperty)
        {
            foreach (var property in CurrentBoards[game.IdGame].board)
            {
                if(property.Name == mortgagedProperty.Name)
                {
                    property.IsMortgaged = false;
                    break;
                }
            }

            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.Money -= mortgagedProperty.BuyingCost / 2;
                    player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                    break;
                }
            }
        }

        public void GoToJail(Player player, int idGame)
        {
            int[] prisonPositions = { 10, 20, 30 };
            int nextPrisonPosition = prisonPositions.FirstOrDefault(pos => player.Position < pos);

            if (nextPrisonPosition == 0)
            {
                player.Position = prisonPositions[0];
            }
            else
            {
                player.Position = nextPrisonPosition;
            }

            MovePlayer(idGame, player.Position, ref player);
            player.GameLogicManagerCallBack.ShowCard(CurrentBoards[idGame].board[player.Position]);
        }
    }
}                                                                                                                                                                                         
