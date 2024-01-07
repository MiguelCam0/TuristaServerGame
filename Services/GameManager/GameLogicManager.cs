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
        private static readonly Dictionary<int, Board> CurrentBoards = new Dictionary<int, Board>();

        public void PlayTurn(Game game)
        {
            try
            {
                Random random = new Random();
                int dieOne = random.Next(1, 6);
                int dieSecond = random.Next(1, 6);

                Player currentPlayer = GetCurrentPlayer(game);

                int playerPosition = currentPlayer.Position + 1;//dieOne + dieSecond;
                currentPlayer.Position = playerPosition;

                UpdatingDiceNumbersInGame(game, dieOne, dieSecond);

                MoveAndAdvance(game, playerPosition, ref currentPlayer);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private Player GetCurrentPlayer(Game game)
        {
            return CurrentGames[game.IdGame].Players.Peek();
        }

        private void UpdatingDiceNumbersInGame(Game game, int dieOne, int dieSecond)
        {
            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                try
                {
                    player.GameLogicManagerCallBack.PlayDie(dieOne, dieSecond);
                }
                catch (Exception ex)
                {
                    DeclareLosingPlayer(player, game.IdGame);
                    HandleNotificationException(player, ex);
                    throw;
                }
            }
        }

        private void MoveAndAdvance(Game game, int playerPosition, ref Player currentPlayer)
        {
            try
            {
                MovePlayer(game.IdGame, playerPosition, ref currentPlayer);
                AdvanceTurn(game.IdGame);
            }
            catch (Exception ex)
            {
                HandleMovementException(ex);
            }
        }


        private void HandleException(Exception ex)
        {
            Console.WriteLine($"Error en el bloque principal: {ex.Message}");
        }

        private void HandleNotificationException(Player player, Exception ex)
        {
            Console.WriteLine($"Error en PlayDie para el jugador {player.Name}: {ex.Message}");
        }

        private void HandleMovementException(Exception ex)
        {
            Console.WriteLine($"Error en MovePlayer o AdvanceTurn: {ex.Message}");
        }


        private void AdvanceTurn(int idGame)
        {
            CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            if (CurrentGames[idGame].Players.Peek().Jail && !CurrentGames[idGame].Players.Peek().Loser)
            {
                CurrentGames[idGame].Players.Peek().Jail = false;
                CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            }
            else if (CurrentGames[idGame].Players.Peek().Loser)
            {
                CurrentGames[idGame].Players.Enqueue(CurrentGames[idGame].Players.Dequeue());
            }
        }

        public int MakeRentalPayment(int idOwnerLand, int idRenter, long amountOfRent, int idGame)
        {
            int result = 0;

            try
            {
                foreach (var player in CurrentGames[idGame].Players)
                {
                    if (player.IdPlayer == idRenter)
                    {
                        bool isPaymentSuccessful = ProcessRentalPayment(player, amountOfRent, idGame);

                        if (!isPaymentSuccessful)
                        {
                            DeclareLosingPlayer(player, idGame);
                            result = 1;
                        }
                        break;
                    }
                }

                if (result == 0)
                {
                    ProcessPaymentForLandlord(idOwnerLand, amountOfRent, idGame);
                    ProcessPaymentForRenter(idRenter, amountOfRent, idGame);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                result = 2;
            }

            return result;
        }

        private bool ProcessRentalPayment(Player renter, long amountOfRent, int idGame)
        {
            bool isPaymentSuccessful;
            if (renter.Money - amountOfRent < 0)
            {
                isPaymentSuccessful = TryMortgageProperties(renter, amountOfRent, idGame);
            }
            else
            {
                renter.Money -= amountOfRent;
                isPaymentSuccessful = true;
            }

            try
            {
                renter.GameLogicManagerCallBack.UpgradePlayerMoney(renter.Money);
                renter.GameLogicManagerCallBack.NotifyPlayerOfEvent(isPaymentSuccessful ? 1 : 0);
            }
            catch (Exception ex)
            {
                HandleException(ex); 
            }

            return isPaymentSuccessful;
        }

        private bool TryMortgageProperties(Player renter, long amountOfRent, int idGame)
        {
            bool result = false;

            foreach (var property in CurrentBoards[idGame].board)
            {
                if (property.Owner != null && property.Owner.IdPlayer == renter.IdPlayer && !property.IsMortgaged)
                {
                    try
                    {
                        RealizePropertyMortgage(idGame, property, renter.IdPlayer);
                        renter.GameLogicManagerCallBack.UpdatePropertyStatus(property);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                        throw;
                    }
                }

                if (renter.Money - amountOfRent >= 0)
                {
                    result = true;
                }
            }

            return result;
        }

        private void ProcessPaymentForLandlord(int idOwnerLand, long amountOfRent, int idGame)
        {
            foreach (var player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == idOwnerLand)
                {
                    try
                    {
                        player.Money += amountOfRent;
                        player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                        player.GameLogicManagerCallBack.NotifyPlayerOfEvent(0);
                    }
                    catch (Exception ex)
                    {
                        player.Money -= amountOfRent;
                        HandleException(ex);
                        throw;
                    }
                    
                }
            }
        }

        private void ProcessPaymentForRenter(int idRenter, long amountOfRent, int idGame)
        {
            foreach (var player in CurrentGames[idGame].Players)
            {
                if(player.IdPlayer == idRenter)
                {
                    try
                    {
                        player.Money -= amountOfRent;
                        player.GameLogicManagerCallBack.UpgradePlayerMoney(player.Money);
                        player.GameLogicManagerCallBack.NotifyPlayerOfEvent(1);
                    }
                    catch (Exception ex)
                    {
                        player.Money += amountOfRent;
                        HandleException(ex);
                        throw;
                    }
                    break;
                }
            }
        }

        public void UpdatePlayersInGame(int idGame)
        {
            foreach(var player in CurrentGames[idGame].Players)
            {
                player.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
            }
        }

        public void PurchaseProperty(Property property, Player buyer, int idGame)
        {
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == buyer.IdPlayer)
                {
                    if(property.DefinitiveCost == 0)
                    {
                        playerAux.Money -= property.BuyingCost;
                    }
                    else
                    {
                        playerAux.Money -= property.DefinitiveCost;
                    }
                    
                    CurrentBoards[idGame].RegisterPurchaseProperty(playerAux, property);
                    break;
                }
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

        public void UpdateQueu(int idGame)
        {
            Queue<Player> turns = CurrentGames[idGame].Players;
            foreach(Player player in turns)
            {
                if(!player.Loser)
                {
                    player.GameLogicManagerCallBack.UpdateTurns(turns);
                }
            }
            UpdatePlayersInGame(idGame);
        }

        public void GetActionCard(int idGame, int idPlayer, Wildcard wildcard)
        {
            Player player = GetPlayer(idGame, idPlayer);
            if (wildcard.Action == 2) { GoToJail(player, idGame); }
            else if(wildcard.Action == 3) { PayAnotherPlayer(idGame, player); }
            else if (wildcard.Action == 4) { PayTaxes(ref player, idGame); }
            else if (wildcard.Action == 5) { GetPay(ref player, idGame); }
            else if (wildcard.Action == 6) { player.Position += 3; MovePlayer(idGame, player.Position, ref player); }
            else if (wildcard.Action == 7) { player.Position -= 3; MovePlayer(idGame, player.Position, ref player); }
        }

        private Player GetPlayer(int idGame, int idPlayer)
        {
            Player player = null;
            
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == idPlayer)
                {
                    player = playerAux;
                    break;
                }
            }

            return player;
        }

        private void PayTaxes(ref Player player, int idGame)
        {
            foreach(var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == player.IdPlayer)
                {
                    player.Money -= 200;

                    if (player.Money < 0)
                    {
                        player.Money = 0;
                    }
                    playerAux.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
                }
            }
        }

        private void GetPay(ref Player player, int idGame)
        {
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if (playerAux.IdPlayer == player.IdPlayer)
                {
                    player.Money += 200;
                    playerAux.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
                }
            }
        }

        private void PayAnotherPlayer(int idGame, Player player)
        {
            PayTaxes(ref player, idGame);
            Player playerAux = CurrentGames[idGame].Players.Peek();
            GetPay(ref playerAux, idGame);
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
                if(player.IdPlayer != loserPlayer.IdPlayer)
                {
                    player.GameLogicManagerCallBack.RemoveGamePiece(loserPlayer);
                }
            }
        }

        public void VerifyGameCompletion(int idGame)
        {
            int activePlayers = 0;
            int idWinner = 0;
            foreach (var player in CurrentGames[idGame].Players)
            {
                if(!player.Loser)
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

        public void ModifyProperty(Property property, int idGame)
        {
            foreach (var propertyAux in CurrentBoards[idGame].board)
            {
                if(propertyAux.Name == property.Name)
                {
                    propertyAux.Taxes = property.Taxes;
                    propertyAux.NumberHouses = property.NumberHouses;
                    propertyAux.Situation = property.Situation;
                    break;
                }
            }
        }

        public void ExpelPlayer(int idPlayer, int idGame)
        {
            var playersCopy = CurrentGames[idGame].Players.ToList();

            foreach (var player in playersCopy)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.VotesToExpel++;
                    if (player.VotesToExpel == CurrentGames[idGame].Players.Count - (1 + GetNumberLosersInGame(idGame)))
                    {
                        player.GameLogicManagerCallBack.ExitToGame();
                        DeclareLosingPlayer(player, idGame);

                        if (CurrentGames[idGame].Players.Peek().IdPlayer == idPlayer)
                        {
                            AdvanceTurn(idGame);
                            UpdateQueu(idGame);
                        }
                    }
                }
                player.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
            }
        }

        private int GetNumberLosersInGame(int idGame)
        {
            int numberLosers = 0;

            foreach (var player in CurrentGames[idGame].Players)
            {
                if(player.Loser)
                {
                    numberLosers++;
                }
            }

            return numberLosers;
        }
    }
}                                                                                                                                                                                         
