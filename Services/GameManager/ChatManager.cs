
using Contracts.IGameManager;
using EASendMail;
using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var player in CurrentGames[idGame].Players) 
            {
                try
                {
                    player.GameManagerCallBack.GetMessage(message);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
            
        }

        /// <summary>
        /// Actualiza la información del jugador en un juego específico.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego.</param>
        /// <param name="idPlayer">Identificador del jugador cuya información se actualizará.</param>
        public void UpdatePlayerGame(Game game, int idPlayer)
        {
            int turn = 0;
            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    try
                    {
                        player.Piece = player.GameManagerCallBack.UptdatePiecePlayer(game);
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }

                    player.Piece.PartNumber = turn;
                    break;
                }
                turn++;
            }
        }

        /// <summary>
        /// Notifica a todos los jugadores en un juego específico sobre la selección de una pieza.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego.</param>
        /// <param name="piece">Token asociado a la pieza seleccionada.</param>
        public void SelectedPiece(Game game, string piece)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.BlockPiece(piece);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        /// <summary>
        /// Notifica a todos los jugadores en un juego específico sobre la liberación de una pieza.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego.</param>
        /// <param name="piece">Pieza que se liberará y estará disponible para selección.</param>
        public void UnSelectedPiece(Game game, string piece)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.UnblockPiece(piece);
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
            Player player = new Player(idPlayer, "Invitado0" + TotalGuestPlayers);

            try
            {
                player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            }
            catch (TimeoutException exception)
            {
                _ilog.Error(exception.ToString());
            }

            CurrentGames[idGame].Players.Enqueue(player);
            CurrentGames[idGame].PlayersInGame.Add(player);
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
                foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
                {
                    try
                    {
                        player.GameManagerCallBack.EnableStartGameButton();
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
            foreach (var player in CurrentGames[idGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.DisableStartGameButton();
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        public void InviteFriendToGame(string codeGame, string friendEmail)
        {
            try
            {
                SmtpMail mail = new SmtpMail("TryIt");
                mail.From = "yusgus02@gmail.com";
                mail.To = friendEmail;
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
    }
}
