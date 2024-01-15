using Contracts.IDataBase;
using Contracts.IGameManager;
using DataBase;
using log4net;
using Services.GameManager;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
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
            Random random = new Random();
            int dieOne = random.Next(1, 6);
            int dieSecond = random.Next(1, 6);

            Player currentPlayer = GetCurrentPlayer(game);

            try
            {    
                currentPlayer.GameLogicManagerCallback.ShowButtonsForEnd();
            }
            catch (TimeoutException exception)
            {
                _ilog.Error(exception.ToString());
                DeclareLosingPlayer(currentPlayer, game.IdGame);
            }

            int playerPosition = currentPlayer.Position + dieOne + dieSecond;
            currentPlayer.Position = playerPosition;

            AdvanceTurn(game.IdGame);
            MovePlayer(game.IdGame, playerPosition, ref currentPlayer);
            UpdatingDiceNumbersInGame(game, dieOne, dieSecond);

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
            foreach (Player playerInGame in CurrentGames[game.IdGame].Players)
            {
                try
                {
                    playerInGame.GameLogicManagerCallback.PlayDie(dieOne, dieSecond);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                    DeclareLosingPlayer(playerInGame, game.IdGame);
                }
            }
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

            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == idRenter)
                {
                    bool isPaymentSuccessful = ProcessRentalPayment(playerInGame, amountOfRent, idGame);

                    if (!isPaymentSuccessful)
                    {
                        DeclareLosingPlayer(playerInGame, idGame);
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

            foreach (Property property in CurrentBoards[idGame].board)
            {
                if (property.Owner != null && property.Owner.IdPlayer == renter.IdPlayer && !property.IsMortgaged)
                {
                    try
                    {
                        RealizePropertyMortgage(idGame, property, renter.IdPlayer);
                        renter.GameLogicManagerCallback.UpdatePropertyStatus(property);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(renter, idGame);
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
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == idOwnerLand)
                {
                    try
                    {
                        playerInGame.Money += amountOfRent;
                        playerInGame.GameLogicManagerCallback.UpgradePlayerMoney(playerInGame.Money);
                        playerInGame.GameLogicManagerCallback.NotifyPlayerOfEvent(0);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, idGame);
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
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if(playerInGame.IdPlayer == idRenter)
                {
                    try
                    {
                        playerInGame.Money -= amountOfRent;
                        playerInGame.GameLogicManagerCallback.UpgradePlayerMoney(playerInGame.Money);
                        playerInGame.GameLogicManagerCallback.NotifyPlayerOfEvent(1);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, idGame);
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
            foreach(Player playerInGame in CurrentGames[idGame].Players)
            {
                try
                {
                    if(playerInGame.GameLogicManagerCallback != null)
                    {
                        playerInGame.GameLogicManagerCallback.LoadFriends(CurrentGames[idGame].Players);
                    }
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                    DeclareLosingPlayer(playerInGame, idGame);
                }
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
            Game currentGame = CurrentGames[idGame];

            foreach (Player playerInGame in currentGame.Players)
            {
                if (playerInGame.IdPlayer == buyer.IdPlayer)
                {
                    playerInGame.Money -= property.BuyingCost;
                    CurrentBoards[idGame].RegisterPurchaseProperty(playerInGame, property);
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
            IGamerLogicManagerCallback context = OperationContext.Current.GetCallbackChannel<IGamerLogicManagerCallback>();
            foreach (Player playerInGame in CurrentGames[idGame].PlayersInGame)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    playerInGame.GameLogicManagerCallback = context;
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
            foreach(Player playerInGame in CurrentGames[idGame].Players)
            {
                if(!playerInGame.Loser)
                {
                    try
                    {
                        if(playerInGame.GameLogicManagerCallback != null)
                        {
                            playerInGame.GameLogicManagerCallback.UpdateTurns(CurrentGames[idGame].Players);
                        }    
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, idGame);
                    }
                    catch(NullReferenceException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
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
            
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if(playerInGame.IdPlayer == idPlayer)
                {
                    player = playerInGame;
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
            foreach(Player playerInGame in CurrentGames[idGame].Players)
            {
                if(playerInGame.IdPlayer == player.IdPlayer)
                {
                    player.Money -= 200;

                    if (player.Money < 0)
                    {
                        player.Money = 0;
                    }

                    try
                    {
                        playerInGame.GameLogicManagerCallback.LoadFriends(CurrentGames[idGame].Players);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(player, idGame);
                    }
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
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == player.IdPlayer)
                {
                    player.Money += 200;
                    try
                    {
                        playerInGame.GameLogicManagerCallback.LoadFriends(CurrentGames[idGame].Players);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(player, idGame);
                    }
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

                try
                {
                    player.GameLogicManagerCallback.UpgradePlayerMoney(player.Money);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                    DeclareLosingPlayer(player, idGame);
                }
                
                player.Position -= 40;
                playerPosition = player.Position;
            }

            foreach (Player playerInGame in CurrentGames[idGame].PlayersInGame)
            {
                playerInGame.GameLogicManagerCallback.MovePlayerPieceOnBoard(player, CurrentBoards[idGame].GetProperty(playerPosition));

                if (playerInGame.IdPlayer == player.IdPlayer)
                {
                    Property currentProperty = CurrentBoards[idGame].board[playerPosition];
                    if (currentProperty.Owner == null)
                    {
                        try
                        {
                            playerInGame.GameLogicManagerCallback.ShowCard(CurrentBoards[idGame].GetProperty(playerPosition));
                        }
                        catch (TimeoutException exception)
                        {
                            _ilog.Error(exception.ToString());
                            DeclareLosingPlayer(player, idGame);
                        }
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
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    playerInGame.Jail = true;
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
            foreach(Player playerInGame in CurrentGames[idGame].Players)
            {
                if(playerInGame.IdPlayer == idPlayer)
                {
                    try
                    {
                        playerInGame.Money += property.BuyingCost / 2;
                        playerInGame.GameLogicManagerCallback.UpgradePlayerMoney(playerInGame.Money);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, idGame);
                    }
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
            foreach (Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == loserPlayer.IdPlayer)
                {
                    AddGameToProfile(playerInGame.IdPlayer);
                    playerInGame.Loser = true;
                }

                if(playerInGame.IdPlayer != loserPlayer.IdPlayer)
                {
                    try
                    {
                        playerInGame.GameLogicManagerCallback.RemoveGamePiece(loserPlayer);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, idGame);
                    }
                }
            }

            VerifyGameCompletion(idGame);
        }

        /// <summary>
        /// Verifica si el juego ha sido completado, determina al ganador y realiza acciones asociadas.
        /// </summary>
        /// <param name="gameId">Identificador único del juego.</param>
        public void VerifyGameCompletion(int gameId)
        {
            List<Player> activePlayers = GetActivePlayers(gameId);

            if (activePlayers.Count == 1)
            {
                int winnerId = activePlayers[0].IdPlayer;
                RegisterWinner(winnerId);
                NotifyEndGame(activePlayers, winnerId, gameId);
            }
        }

        /// <summary>
        /// Obtiene la lista de jugadores activos en un juego específico, excluyendo a los perdedores.
        /// </summary>
        /// <param name="gameId">Identificador único del juego.</param>
        /// <returns>Lista de jugadores activos.</returns>
        private List<Player> GetActivePlayers(int gameId)
        {
            List<Player> activePlayers = new List<Player>();

            foreach (Player playerInGame in CurrentGames[gameId].Players)
            {
                if (!playerInGame.Loser)
                {
                    activePlayers.Add(playerInGame);
                }
            }

            return activePlayers;
        }

        /// <summary>
        /// Notifica el fin del juego a todos los jugadores en el juego actual, indicando al ganador.
        /// </summary>
        /// <param name="playersInGame">Lista de jugadores en el juego actual.</param>
        /// <param name="winnerId">Identificador único del jugador ganador.</param>
        /// <param name="gameId">Identificador único del juego.</param>
        private void NotifyEndGame(List<Player> playersInGame, int winnerId, int gameId)
        {
            foreach (Player playerInGame in playersInGame)
            {
                try
                {
                    playerInGame.GameLogicManagerCallback.EndGame(winnerId);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                    DeclareLosingPlayer(playerInGame, gameId);
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
            foreach (Property property in CurrentBoards[game.IdGame].board)
            {
                if(property.Name == mortgagedProperty.Name)
                {
                    property.IsMortgaged = false;
                    break;
                }
            }

            foreach (Player playerInGame in CurrentGames[game.IdGame].Players)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    try
                    {
                        playerInGame.Money -= mortgagedProperty.BuyingCost / 2;
                        playerInGame.GameLogicManagerCallback.UpgradePlayerMoney(playerInGame.Money);
                        break;
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                        DeclareLosingPlayer(playerInGame, game.IdGame);
                    }
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

            try
            {
                player.GameLogicManagerCallback.ShowCard(CurrentBoards[idGame].board[player.Position]);
            }
            catch(TimeoutException exception)
            {           
                _ilog.Error(exception.ToString());
                DeclareLosingPlayer(player, idGame);
            }

        }

        /// <summary>
        /// Modifica las características de una propiedad en el tablero del juego.
        /// </summary>
        /// <param name="property">Propiedad con las nuevas características.</param>
        /// <param name="idGame">Identificador único del juego.</param>
        public void ModifyProperty(Property property, int idGame)
        {
            foreach (Property boardProperty in CurrentBoards[idGame].board)
            {
                if(boardProperty.Name == property.Name)
                {
                    boardProperty.Taxes = property.Taxes;
                    boardProperty.NumberHouses = property.NumberHouses;
                    boardProperty.Situation = property.Situation;
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
            DateTime date = DateTime.Now;
            using(var context = new TuristaMundialEntitiesDB())
            {
                try
                {
                    var playerInfo = context.PlayerSet.Where(player => player.Id == idPlayer).First();
                    playerInfo.BanEnd = date;
                    context.SaveChanges();
                }
                catch (SqlException exception)
                {
                    _ilog.Error(exception.ToString());
                }
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

            foreach (var playerInGame in CurrentGames[idGame].Players)
            {
                if(playerInGame.Loser)
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
            foreach(Player playerInGame in CurrentGames[idGame].Players)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    playerInGame.Money -= constructionCost;
                    break;
                }
            }
        }
    }
}                                                                                                                                                                                         
