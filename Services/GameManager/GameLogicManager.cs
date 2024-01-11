using Contracts.IDataBase;
using Contracts.IGameManager;
using DataBase;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.ServiceModel;
using System.Web.UI.WebControls;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameLogicManager
    {
        private static readonly Dictionary<int, Board> CurrentBoards = new Dictionary<int, Board>();

        /// <summary>
        /// Inicia y ejecuta el turno de juego para un jugador en específico.
        /// </summary>
        /// <param name="game">Instancia del juego en curso.</param>
        public void PlayTurn(Game game)
        {
            try
            {
                Random random = new Random();
                int dieOne = random.Next(1, 6);
                int dieSecond = random.Next(1, 6);

                Player currentPlayer = GetCurrentPlayer(game);

                int playerPosition = currentPlayer.Position + dieOne + dieSecond;
                currentPlayer.Position = playerPosition;

                UpdatingDiceNumbersInGame(game, dieOne, dieSecond);

                MoveAndAdvance(game, playerPosition, ref currentPlayer);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        /// <summary>
        /// Obtiene el jugador actual que está en turno en el juego especificado.
        /// </summary>
        /// <param name="game">Instancia del juego en curso.</param>
        /// <returns>Objeto de tipo Player que representa al jugador actual en turno.</returns>
        private Player GetCurrentPlayer(Game game)
        {
            return CurrentGames[game.IdGame].Players.Peek();
        }

        /// <summary>
        /// Actualiza los números de los dados en el juego notificando a todos los jugadores.
        /// </summary>
        /// <param name="game">Instancia del juego en curso.</param>
        /// <param name="dieOne">Número obtenido en el primer dado.</param>
        /// <param name="dieSecond">Número obtenido en el segundo dado.</param>
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

        /// <summary>
        /// Mueve al jugador a la posición indicada y avanza al siguiente turno en el juego.
        /// </summary>
        /// <param name="game">Instancia del juego en curso.</param>
        /// <param name="playerPosition">Nueva posición a la que se mueve el jugador.</param>
        /// <param name="currentPlayer">Referencia al jugador actual en turno.</param>
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

        /// <summary>
        /// Avanza al siguiente turno en el juego, gestionando la cola de jugadores.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Realiza el pago de alquiler entre un inquilino y un propietario de una propiedad en el juego.
        /// </summary>
        /// <param name="idOwnerLand">Identificador único del propietario de la tierra.</param>
        /// <param name="idRenter">Identificador único del inquilino.</param>
        /// <param name="amountOfRent">Cantidad de dinero a pagar como alquiler.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <returns>0 si el pago se realizó con éxito, 1 si el inquilino perdió y 2 si ocurrió una excepción.</returns>
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

        /// <summary>
        /// Procesa el pago de alquiler para un jugador inquilino en el juego.
        /// </summary>
        /// <param name="renter">Jugador inquilino que realiza el pago.</param>
        /// <param name="amountOfRent">Cantidad de dinero a pagar como alquiler.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <returns>
        /// True si el pago se realizó con éxito.
        /// False si el jugador no tiene suficiente dinero y se intentaron hipotecar propiedades para cubrir el pago.
        /// </returns>
        private bool ProcessRentalPayment(Player renter, long amountOfRent, int idGame)
        {
            bool isPaymentSuccessful;
            if (renter.Money - amountOfRent < 0)
            {
                isPaymentSuccessful = TryMortgageProperties(renter, amountOfRent, idGame);
            }
            else
            {
                isPaymentSuccessful = true;
            }
 
            return isPaymentSuccessful;
        }

        /// <summary>
        /// Intenta hipotecar propiedades del jugador inquilino para cubrir el pago de alquiler insuficiente.
        /// </summary>
        /// <param name="renter">Jugador inquilino que intenta hipotecar propiedades.</param>
        /// <param name="amountOfRent">Cantidad de dinero necesaria para el pago de alquiler.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <returns>
        /// True si se hipotecaron propiedades con éxito y el jugador tiene suficiente dinero para el pago de alquiler.
        /// False si no se pudieron hipotecar propiedades o el jugador aún no tiene suficiente dinero después del intento de hipotecar.
        /// </returns>
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
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Procesa el pago de alquiler para el propietario de una propiedad.
        /// </summary>
        /// <param name="idOwnerLand">Identificador del jugador propietario de la propiedad.</param>
        /// <param name="amountOfRent">Cantidad de dinero a pagar como alquiler.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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
                        HandleException(ex);
                        throw;
                    }
                    
                }
            }
        }

        /// <summary>
        /// Procesa el pago de alquiler para el jugador inquilino.
        /// </summary>
        /// <param name="idRenter">Identificador del jugador inquilino.</param>
        /// <param name="amountOfRent">Cantidad de dinero a pagar como alquiler.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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
                        HandleException(ex);
                        throw;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Actualiza la información de los jugadores en el juego, cargando la lista de amigos para cada jugador.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        public void UpdatePlayersInGame(int idGame)
        {
            foreach(var player in CurrentGames[idGame].Players)
            {
                //player.GameLogicManagerCallBack.LoadFriends(CurrentGames[idGame].Players);
            }
        }

        /// <summary>
        /// Realiza la compra de una propiedad por parte de un jugador en el juego.
        /// </summary>
        /// <param name="property">Propiedad que se va a comprar.</param>
        /// <param name="buyer">Jugador que realiza la compra.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        public void PurchaseProperty(Property property, Player buyer, int idGame)
        {
            foreach (var playerAux in CurrentGames[idGame].Players)
            {
                if(playerAux.IdPlayer == buyer.IdPlayer)
                {
                    playerAux.Money -= property.BuyingCost;              
                    CurrentBoards[idGame].RegisterPurchaseProperty(playerAux, property);
                    break;
                }
            }
        }

        /// <summary>
        /// Actualiza el servicio de lógica de juego para un jugador específico en el juego.
        /// </summary>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Actualiza la cola de turnos y notifica a los jugadores no perdedores sobre los turnos actuales.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        public void UpdateQueu(int idGame)
        {
            Queue<Player> turns = CurrentGames[idGame].Players;
            foreach(Player player in turns)
            {
                if(!player.Loser)
                {
                    //player.GameLogicManagerCallBack.UpdateTurns(turns);
                }
            }
            UpdatePlayersInGame(idGame);
        }

        /// <summary>
        /// Realiza la acción asociada a una carta de acción para un jugador en el juego.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <param name="wildcard">Carta de acción a ejecutar.</param>
        public void GetActionCard(int idGame, int idPlayer, Wildcard wildcard)
        {
            Player player = GetPlayer(idGame, idPlayer);
            if (wildcard.Action == 2) { GoToJail(player, idGame); }
            else if(wildcard.Action == 3) { PayAnotherPlayer(idGame, player); }
            else if (wildcard.Action == 4) { PayTaxes(ref player, idGame); }
            else if (wildcard.Action == 5) { GetPay(ref player, idGame); }
            else if (wildcard.Action == 6) { player.Position += 4; MovePlayer(idGame, player.Position, ref player); }
            else if (wildcard.Action == 7) { player.Position -= 3; MovePlayer(idGame, player.Position, ref player); }
        }

        /// <summary>
        /// Obtiene el jugador asociado a un identificador único de juego y jugador.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <returns>Objeto Player asociado al identificador único del jugador en el juego.</returns>
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

        /// <summary>
        /// Realiza el pago de impuestos por parte de un jugador.
        /// </summary>
        /// <param name="player">Objeto Player que realizará el pago de impuestos.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Recibe un pago de 200 unidades monetarias.
        /// </summary>
        /// <param name="player">Objeto Player que recibirá el pago.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Realiza el pago de impuestos por parte de un jugador y realiza un pago a otro jugador.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="player">Jugador que realizará el pago de impuestos.</param>
        private void PayAnotherPlayer(int idGame, Player player)
        {
            PayTaxes(ref player, idGame);
            Player playerAux = CurrentGames[idGame].Players.Peek();
            GetPay(ref playerAux, idGame);
        }

        /// <summary>
        /// Mueve al jugador en el tablero de juego y realiza acciones asociadas al nuevo posicionamiento.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="playerPosition">Posición a la que se moverá el jugador.</param>
        /// <param name="player">Jugador que se moverá en el tablero.</param>
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

        /// <summary>
        /// Envía al jugador a la cárcel en el juego especificado.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="idPlayer">Identificador único del jugador a enviar a la cárcel.</param>
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

        /// <summary>
        /// Realiza la hipoteca de una propiedad en el juego especificado y proporciona fondos al jugador correspondiente.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <param name="property">Propiedad que se va a hipotecar.</param>
        /// <param name="idPlayer">Identificador único del jugador asociado a la propiedad.</param>
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

        /// <summary>
        /// Declara a un jugador como perdedor en el juego especificado y realiza acciones asociadas.
        /// </summary>
        /// <param name="loserPlayer">Jugador que se declara como perdedor.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        public void DeclareLosingPlayer(Player loserPlayer, int idGame)
        {
            foreach (Player player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == loserPlayer.IdPlayer)
                {
                    AddGameToProfile(player.IdPlayer);
                    player.Loser = true;
                }
                if(player.IdPlayer != loserPlayer.IdPlayer)
                {
                    player.GameLogicManagerCallBack.RemoveGamePiece(loserPlayer);
                }
            }

            VerifyGameCompletion(idGame);
        }

        /// <summary>
        /// Verifica si el juego ha sido completado, determina al ganador y realiza acciones asociadas.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
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
                    RegisterWinner(idWinner);
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

        /// <summary>
        /// Declara a un jugador como ganador en el juego especificado y lo registra en la base de datos
        /// <param name="idPlayer">Jugador que se declara como ganador.</param>
        private void RegisterWinner(int idPlayer)
        {
            using (var context = new TuristaMundialEntitiesDB())
            {
                Console.WriteLine("GANOOOOOOOOOOOOO");
                var player = context.PlayerSet.Where(p => p.Id == idPlayer).First();
                player.Wins++;
                player.Games++;
                context.PlayerSet.AddOrUpdate(player);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Se añade una partida jugada al historial de partidas del usuario
        /// <param name="idPlayer">Jugador que termino una partida.</param>
        private void AddGameToProfile(int idPlayer)
        {
            using (var context = new TuristaMundialEntitiesDB())
            {
                Console.WriteLine("PERDIOOOOOOOOOOOOOOOOOOOOOOOOOOOOoo");
                var player = context.PlayerSet.Where(p => p.Id == idPlayer).First();
                player.Games++;
                context.PlayerSet.AddOrUpdate(player);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Paga la hipoteca de una propiedad, deshipotecando la propiedad y realizando ajustes financieros al jugador.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <param name="mortgagedProperty">Objeto Property que representa la propiedad hipotecada.</param>
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

        /// <summary>
        /// Envía al jugador a la cárcel en función de su posición actual en el tablero del juego.
        /// </summary>
        /// <param name="player">Jugador que será enviado a la cárcel.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Modifica las características de una propiedad en el tablero del juego.
        /// </summary>
        /// <param name="property">Propiedad con las nuevas características.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Expulsa a un jugador del juego si acumula suficientes votos para ser expulsado.
        /// </summary>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <param name="idGame">Identificador único del juego.</param>
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

        /// <summary>
        /// Obtiene el número de jugadores perdedores en el juego.
        /// </summary>
        /// <param name="idGame">Identificador único del juego.</param>
        /// <returns>Número de jugadores perdedores en el juego.</returns>
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

        /// <summary>
        /// Paga los costos de construcción de casas o hoteles para un jugador en el juego.
        /// </summary>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <param name="constructionCost">Costo de construcción a pagar por el jugador.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        public void PayConstruction(int idPlayer, long constructionCost, int idGame)
        {
            foreach(Player player in CurrentGames[idGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.Money -= constructionCost;
                    break;
                }
            }
        }

        public bool ConnectionExists()
        {
            return true;
        }
    }
}                                                                                                                                                                                         
