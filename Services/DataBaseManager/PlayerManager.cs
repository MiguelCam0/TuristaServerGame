using Contracts.IDataBase;
using DataBase;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Contracts.IGameManager;
using EASendMail;
using System.Data.Entity.Validation;

namespace Services.DataBaseManager
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class PlayerManager : IPlayer
    {
        /// <summary>
        /// Busca un jugador en la base de datos utilizando el nombre de usuario (Nickname) y la contraseña.
        /// </summary>
        /// <param name="player">Objeto PlayerSet con el nombre de usuario y la contraseña a buscar.</param>
        /// <returns>
        /// Devuelve el identificador (Id) del jugador si se encuentra en la base de datos.
        /// Si no se encuentra ningún jugador, devuelve 0.
        /// </returns>
        public int PlayerSearch(PlayerSet player)
        {
            int check = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Where(p => p.Nickname == player.Nickname && p.Password == player.Password).FirstOrDefault();

                    if (existingPlayer != null)
                    {
                        if (!currentUsers.ContainsKey(existingPlayer.Id))
                        {
                            check = existingPlayer.Id;
                        }
                        else
                        {
                            check = -1;
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }

            return check;
        }

        /// <summary>
        /// Registra un nuevo jugador en la base de datos.
        /// </summary>
        /// <param name="player">Objeto PlayerSet que representa al jugador a registrar.</param>
        /// <returns>
        /// Devuelve el número de cambios realizados en la base de datos.
        /// Si el registro es exitoso, el valor devuelto debería ser 1; de lo contrario, será 0.
        /// </returns>
        public int RegisterPlayer(PlayerSet player)
        {
            int band = 0;

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    context.PlayerSet.Add(player);
                    band = context.SaveChanges();
                }

            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }

            return band;
        }

        /// <summary>
        /// Actualiza el estado en línea de los amigos en la lista proporcionada.
        /// </summary>
        /// <param name="friends">Lista de objetos FriendList que representa a los amigos.</param>
        /// <returns>
        /// Devuelve la lista de amigos actualizada con el estado en línea actualizado.
        /// </returns>
        private List<FriendList> AreOnline(List<FriendList> friends) { 
            foreach (var friend in friends) {
                foreach (var OnlineUser in currentUsers)
                {
                    if (friend.IdFriend == OnlineUser.Key)
                    { 
                        friend.IsOnline = true;
                    }
                }
            }
            return friends;
        }

        /// <summary>
        /// Obtiene el nombre del jugador asociado al identificador proporcionado.
        /// </summary>
        /// <param name="IdPlayer">Identificador del jugador.</param>
        /// <returns>
        /// Devuelve el nombre del jugador si se encuentra en la base de datos.
        /// Si no se encuentra ningún jugador, devuelve una cadena vacía.
        /// </returns>
        public string GetPlayerName(int IdPlayer)
        {
            string PlayerName = "";

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Where(p => p.Id == IdPlayer).FirstOrDefault();

                    if (existingPlayer != null)
                    {
                        PlayerName = existingPlayer.Nickname;
                    }
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }

            return PlayerName;
        }

        /// <summary>
        /// Obtiene el nombre del jugador asociado al identificador del jugador y al identificador del juego.
        /// </summary>
        /// <param name="idPlayer">Identificador del jugador.</param>
        /// <param name="idGame">Identificador del juego.</param>
        /// <returns>
        /// Devuelve el nombre del jugador si se encuentra en la lista de jugadores del juego actual.
        /// Si no se encuentra ningún jugador con el identificador proporcionado, devuelve una cadena vacía.
        /// </returns>
        public string GetMyPlayersName(int idPlayer, int idGame)
        {
            string PlayerName = "";
            foreach (Player playerInGame in CurrentGames[idGame].PlayersInGame)
            {
                if (playerInGame.IdPlayer == idPlayer)
                {
                    PlayerName = playerInGame.Name;
                    break;
                }
            }

            return PlayerName;
        }

        /// <summary>
        /// Obtiene el objeto idGame asociado al identificador del juego proporcionado.
        /// </summary>
        /// <param name="idGame">Identificador del juego.</param>
        /// <returns>
        /// Devuelve el objeto idGame si se encuentra en la colección de juegos actuales.
        /// Si no se encuentra ningún juego con el identificador proporcionado, devuelve null.
        /// </returns>
        public Game GetGame(int idGame)
        {
            return CurrentGames[idGame];
        }

        public int SendEmail(String verifyCode, String userEmail)
        {
            int result = 0;

            try
            {
                SmtpMail mail = new SmtpMail("TryIt");
                mail.From = "yusgus02@gmail.com";
                mail.To = userEmail;
                mail.Subject = "Codigo de verificacion";
                mail.TextBody = "Tu codigo de verificacion es: " + verifyCode;

                SmtpServer emailServer = new SmtpServer("smtp.gmail.com");

                emailServer.User = "yusgus02@gmail.com";
                emailServer.Password = "nopk fxne wkiy lvpg";
                emailServer.Port = 587;
                emailServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                SmtpClient reciber = new SmtpClient();
                reciber.SendMail(emailServer, mail);

            }
            catch (Exception exception) 
            {
                _ilog.Error(exception.ToString());
                result = 1;
            }

            return result;
        }

        /// <summary>
        /// Obtiene la información del jugador por su identificador único.
        /// </summary>
        /// <param name="idPlayer">Identificador único del jugador.</param>
        /// <returns>Objeto Player que contiene la información del jugador.</returns>
        public Player GetPlayerData(int idPlayer)
        {
            Player playerInfo = new Player();

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var player = context.PlayerSet.Where(p => p.Id == idPlayer).First();
                    playerInfo.Name = player.Nickname;
                    playerInfo.Games = (int)player.Games;
                    playerInfo.GamesWin = (int)player.Wins;
                    playerInfo.Description = player.Description;
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }
            
            return playerInfo;
        }


        public int UpdatePlayerData(int idPlayer, string Description)
        {
            int result = 0;

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var player = context.PlayerSet.Where(playerInfo => playerInfo.Id == idPlayer).First();
                    player.Description = Description;
                    result = context.SaveChanges();
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }
            
            return result;  
        }
    }
}