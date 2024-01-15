
using Contracts.IDataBase;
using Contracts.IGameManager;
using DataBase;
using EASendMail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {
        /// <summary>
        /// Envía un mensaje a todos los jugadores de un juego específico.
        /// </summary>
        /// <param name="idGame">Identificador del juego al que se enviará el mensaje.</param>
        /// <param name="message">Mensaje a enviar a los jugadores.</param>
        public void SendMessage(int idGame, string message)
        {
            if (!message.Equals(""))
            {
                foreach (Player playerInGame in CurrentGames[idGame].Players)
                {
                    try
                    {
                        playerInGame.GameManagerCallback.GetMessage(message);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
                }
            }
        }

        private readonly object LockObject = new object();

        /// <summary>
        /// Actualiza la información de juego de un jugador, asignando una nueva pieza y número de turno.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="idPlayer">Identificador único del jugador a actualizar.</param>
        /// <param name="playersPiece">Objeto Piece que representa la nueva pieza del jugador.</param>
        /// <returns>Entero que indica el resultado de la operación (0 si no se actualizó, 1 si se actualizó correctamente).</returns>
        public int UpdatePlayerGame(Game game, int idPlayer, Piece playersPiece)
        {
            int result = 0;
            int turn = 0;

            Monitor.Enter(LockObject);
            try
            {
                if (CheckPieceAvailability(game, playersPiece.Name))
                {
                    foreach (Player playerInGame in CurrentGames[game.IdGame].Players)
                    {
                        if (playerInGame.IdPlayer == idPlayer)
                        {
                            try
                            {
                                playerInGame.Piece = playersPiece;
                                result = 1;
                            }
                            catch (TimeoutException exception)
                            {
                                _ilog.Error(exception.ToString());
                            }

                            playerInGame.Piece.PartNumber = turn;
                            break;
                        }
                        turn++;
                    }
                }
            }
            finally
            {
                Monitor.Exit(LockObject);
            }

            return result;
        }

        /// <summary>
        /// Notifica a todos los jugadores en un juego que una pieza específica ha sido seleccionada por un jugador.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="piece">Nombre de la pieza seleccionada.</param>
        /// <param name="idPlayer">Identificador único del jugador que seleccionó la pieza.</param>
        public void SelectedPiece(Game game, string piece, int idPlayer)
        {            
            foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    playerInGame.GameManagerCallback.BlockPiece(piece, idPlayer);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        /// <summary>
        /// Verifica la disponibilidad de una pieza específica en el juego.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="selectedPiece">Nombre de la pieza seleccionada.</param>
        /// <returns>True si la pieza está disponible, False si está ocupada por otro jugador.</returns>
        private bool CheckPieceAvailability(Game game, string selectedPiece)
        {
            bool result = true;

            foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
            {
                if (playerInGame.Piece != null && playerInGame.Piece.Name == selectedPiece)
                {
                    result = false; break;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Notifica a todos los jugadores en un juego que una pieza específica ha sido deseleccionada por un jugador.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="piece">Nombre de la pieza deseleccionada.</param>
        /// <param name="idPlayer">Identificador único del jugador que deseleccionó la pieza.</param>
        public void UnSelectedPiece(Game game, string piece, int idPlayer)
        {
            foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
            {
                if(playerInGame.IdPlayer == idPlayer)
                {
                    playerInGame.Piece = null;
                }

                try
                {
                    playerInGame.GameManagerCallback.UnblockPiece(piece);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        private static int TotalGuestPlayers = 0;

        /// <summary>
        /// Agrega un jugador invitado a un juego específico.
        /// </summary>
        /// <param name="idGame">Identificador del juego al que se agregará el jugador invitado.</param>
        /// <param name="idPlayer">Identificador del jugador invitado.</param>
        public void AddGuestToGame(int idGame, int idPlayer)
        {
            TotalGuestPlayers++;
            Player guestPlayer = new Player(idPlayer, "Invitado0" + TotalGuestPlayers);

            try
            {
                guestPlayer.GameManagerCallback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
            }
            catch (TimeoutException exception)
            {
                _ilog.Error(exception.ToString());
            }

            CurrentGames[idGame].Players.Enqueue(guestPlayer);
            CurrentGames[idGame].PlayersInGame.Add(guestPlayer);
        }

        /// <summary>
        /// Verifica si todos los jugadores en un juego específico están listos para comenzar el juego.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego.</param>
        public void CheckReadyToStartGame(Game game)
        {
            CurrentGames[game.IdGame].NumberPlayersReady++;
            if (CurrentGames[game.IdGame].NumberPlayersReady == CurrentGames[game.IdGame].PlayersInGame.Count && CurrentGames[game.IdGame].PlayersInGame.Count > 1)
            {
                foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
                {
                    try
                    {
                        playerInGame.GameManagerCallback.EnableStartGameButton();
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Anula la verificación de si todos los jugadores en un juego específico están listos para comenzar el juego.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego.</param>
        public void UnCheckReadyToStartGame(Game game)
        {
            CurrentGames[game.IdGame].NumberPlayersReady--;
            if (CurrentGames[game.IdGame].NumberPlayersReady != CurrentGames[game.IdGame].PlayersInGame.Count)
            {
                InactivateBeginGameControls(game.IdGame);
            }
        }

        /// <summary>
        /// Desactiva los controles de inicio de juego para todos los jugadores en un juego específico.
        /// </summary>
        /// <param name="idGame">Identificador del juego al que se aplicará la desactivación.</param>
        public void InactivateBeginGameControls(int idGame)
        {
            foreach (Player playerInGame in CurrentGames[idGame].PlayersInGame)
            {
                try
                {
                    playerInGame.GameManagerCallback.DisableStartGameButton();
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        /// <summary>
        /// Invita a un amigo a unirse a un juego enviándole un correo electrónico con un código de verificación.
        /// </summary>
        /// <param name="codeGame">Código de verificación del juego.</param>
        /// <param name="friendId">Identificador único del amigo a invitar.</param>
        public void InviteFriendToGame(string codeGame, int friendId)
        {
            try
            {
                
                SmtpMail mail = new SmtpMail("TryIt");
                mail.From = "yusgus02@gmail.com";
                mail.To = GetFriendEmail(friendId);
                mail.Subject = "Codigo de verificacion";
                mail.TextBody = "Tu codigo de verificacion es: " + codeGame;

                SmtpServer emailServer = new SmtpServer("smtp.gmail.com");

                emailServer.User = "yusgus02@gmail.com";
                emailServer.Password = "nopk fxne wkiy lvpg";
                emailServer.Port = 587;
                emailServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                SmtpClient reciber = new SmtpClient();
                reciber.SendMail(emailServer, mail);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Obtiene una lista de nombres de piezas seleccionadas por jugadores en un juego específico.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <returns>Lista de nombres de piezas seleccionadas.</returns>
        private List<string> GetSelectedPieces(Game game)
        {
            List<string> pieces = new List<string>();

            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if(player.Piece != null)
                {
                    pieces.Add(player.Piece.Name);
                }
            }

            return pieces;

        }

        /// <summary>
        /// Verifica las piezas seleccionadas por otros jugadores en el juego y notifica al jugador especificado.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego actual.</param>
        /// <param name="idPlayer">Identificador único del jugador a notificar.</param>
        public void CheckTakenPieces(Game game, int idPlayer)
        {
            List<string> pieces = GetSelectedPieces(game);

            foreach (Player playerInGame in CurrentGames[game.IdGame].Players)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    if (pieces != null && pieces.Count > 0)
                    {
                        foreach (var piece in pieces)
                        {
                            playerInGame.GameManagerCallback.BlockPiece(piece, -1);
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Obtiene la dirección de correo electrónico de un amigo mediante su identificador único.
        /// </summary>
        /// <param name="idFriend">Identificador único del amigo.</param>
        /// <returns>Dirección de correo electrónico del amigo.</returns>
        private string GetFriendEmail(int idFriend)
        {
            string friendEmail;
            using (var Context = new TuristaMundialEntitiesDB())
            {
                var friend = Context.PlayerSet.Where(P => P.Id == idFriend).First();
                friendEmail = friend.eMail;
            }

            return friendEmail;
        }
    }
}
