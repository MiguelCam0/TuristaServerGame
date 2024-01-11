using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IDataBase;
using Contracts.IPlayerSessionManager;
using Contracts.ISessionManager;
using DataBase;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IFriends, IFriendList
    {
        public static Dictionary<int, INotificationsCallBack> currentUsers = new Dictionary<int, INotificationsCallBack>();

        /// <summary>
        /// Envía una solicitud de amistad desde un jugador remitente a un jugador destinatario.
        /// </summary>
        /// <param name="Sender">ID del jugador remitente.</param>
        /// <param name="Reciber">Nombre del jugador destinatario.</param>
        /// <returns>
        /// 0: La solicitud se envió con éxito.
        /// 1: Los jugadores ya son amigos.
        /// 2: El jugador destinatario no existe.
        /// 3: Ya hay una solicitud de amistad pendiente entre los jugadores.
        /// </returns>
        public int MakeFriendRequest(int Sender, string Reciber)
        {
            int Result = 0;
            try
            {
                using (var Context = new TuristaMundialEntitiesDB())
                {
                    if (VerifyUserExistence(Reciber) == 1)
                    {
                        Result = 2;
                    } 
                    else if (CheckAlredyFriends(Sender, Reciber) == 0)
                    {
                        Result = 1;
                    }
                    else if (CheckPreviousFriendRequest(Sender, Reciber) == 1)
                    {
                        Result = 3;
                    }
                    else
                    {
                        var SecondPlayer = Context.PlayerSet.Where(P => P.Nickname == Reciber).First();
                        if (Context.FriendRequest.Where(r => r.PlayerSet1ID == Sender && r.PlayerSet2ID == SecondPlayer.Id).FirstOrDefault() == null)
                        {
                            FriendRequest Request = new FriendRequest();
                            Request.PlayerSet1ID = Sender;
                            Request.PlayerSet2ID = SecondPlayer.Id;
                            Context.FriendRequest.Add(Request);
                            Result = Context.SaveChanges();
                            NotifyRequest(SecondPlayer.Id);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("Error en RegisterPlayer: " + ex.Message);
            }
            return Result;
        }

        /// <summary>
        /// Guarda la sesión de notificaciones de un jugador.
        /// </summary>
        /// <param name="idPlayer">ID del jugador cuya sesión se va a guardar.</param>
        public void SavePlayerSession(int idPlayer)
        {
            INotificationsCallBack context = OperationContext.Current.GetCallbackChannel<INotificationsCallBack>();
            if (currentUsers.ContainsKey(idPlayer))
            {
                currentUsers[idPlayer] = context;
            }
            else
            {
                currentUsers.Add(idPlayer, context);
                NotifyFriendOline(idPlayer);
            }
        }

        /// <summary>
        /// Acepta una solicitud de amistad y establece una conexión de amistad entre dos jugadores.
        /// </summary>
        /// <param name="IdRequest">ID de la solicitud de amistad que se va a aceptar.</param>
        /// <returns>Entero que indica el resultado de la operación (0 si hay un error, otro valor si es exitoso).</returns>
        public int AcceptFriendRequest(int IdRequest)
        {
            int result = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var request = context.FriendRequest.Where(r => r.IDRequest == IdRequest).First();
                    friendship friendship = new friendship();
                    friendship.PlayerSet = request.PlayerSet;
                    friendship.PlayerSet1 = request.PlayerSet1;
                    context.friendship.Add(friendship);
                    context.FriendRequest.Remove(request);
                    result = context.SaveChanges();
                    NotifyFriendOline(friendship.PlayerSet1.Id);
                }
            }catch(Exception ex) 
            {
                Console.WriteLine("Error en CheckAlredyFriends: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Rechaza una solicitud de amistad y la elimina de la base de datos.
        /// </summary>
        /// <param name="IdRequest">ID de la solicitud de amistad que se va a rechazar.</param>
        /// <returns>Entero que indica el resultado de la operación (0 si hay un error, otro valor si es exitoso).</returns>
        public int RejectFriendRequest(int IdRequest)
        {
            int result = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var request = context.FriendRequest.Where(r => r.IDRequest == IdRequest).First();
                    context.FriendRequest.Remove(request);
                    result = context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CheckAlredyFriends: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Comprueba si dos jugadores ya son amigos.
        /// </summary>
        /// <param name="Sender">ID del jugador que envía la solicitud de amistad.</param>
        /// <param name="Reciber">Nickname del jugador que recibe la solicitud de amistad.</param>
        /// <returns>Entero que indica el resultado de la comprobación (0 si ya son amigos, 1 si no son amigos o hay un error).</returns>
        private int CheckAlredyFriends(int Sender, String Reciber)
        {
            int result = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var player = context.PlayerSet.Where(p => p.Nickname == Reciber).FirstOrDefault();
                    if (player != null)
                    {
                        var check = context.friendship.Where(fs => (fs.player1_id == Sender || fs.player1_id == player.Id) 
                        && ((fs.player2_id == Sender || fs.player2_id == player.Id))).FirstOrDefault();
                        if(check == null)
                        {
                            result = 1;
                        }
                    }
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine("Error en CheckAlredyFriends: " + ex.Message); 
            }
            return result;
        }

        /// <summary>
        /// Verifica la existencia de un jugador en la base de datos.
        /// </summary>
        /// <param name="userName">Nombre de usuario del jugador a verificar.</param>
        /// <returns>Entero que indica el resultado de la verificación (0 si el jugador existe, 1 si no existe o hay un error).</returns>
        private int VerifyUserExistence(String userName)
        {
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Any(p => p.Nickname == userName);
                    return existingPlayer ? 0 : 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CheckUserExistence: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// Verifica si existe una solicitud de amistad previa entre dos jugadores.
        /// </summary>
        /// <param name="sender">ID del jugador que envía la solicitud.</param>
        /// <param name="receiver">Nombre de usuario del jugador que recibe la solicitud.</param>
        /// <returns>Entero que indica el resultado de la verificación (1 si hay una solicitud previa, 0 si no hay).</returns>
        public int CheckPreviousFriendRequest(int Sender, string Reciber)
        {
            int result = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var secondPlayer = context.PlayerSet.FirstOrDefault(p => p.Nickname == Reciber);
                    if (secondPlayer != null)
                    {
                        var existingRequest = context.FriendRequest
                            .Where(r => (r.PlayerSet1ID == Sender && r.PlayerSet2ID == secondPlayer.Id) || (r.PlayerSet1ID == secondPlayer.Id && r.PlayerSet2ID == Sender))
                            .FirstOrDefault();
                        if (existingRequest != null)
                        {
                            result = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CheckPreviousFriendRequest: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Notifica a un jugador específico acerca de una nueva solicitud de amistad.
        /// </summary>
        /// <param name="idPlayer">ID del jugador a notificar.</param>
        private void NotifyRequest(int idPlayer)
        {
            foreach(var user in currentUsers)
            {
                if(idPlayer == user.Key)
                {
                    Console.WriteLine(idPlayer);
                    user.Value.UpdateFriendRequest();
                    Console.WriteLine("Notifica la request");
                }
            }

        }

        /// <summary>
        /// Actualiza la sesión de notificaciones de un jugador.
        /// </summary>
        /// <param name="idPlayer">ID del jugador cuya sesión se actualizará.</param>
        public void UpdatePlayerSession(int idPlayer)
        {
            INotificationsCallBack context = OperationContext.Current.GetCallbackChannel<INotificationsCallBack>();
            currentUsers[idPlayer] = context;
        }

        /// <summary>
        /// Notifica a los amigos de un jugador que el jugador está en línea.
        /// </summary>
        /// <param name="idPlayer">ID del jugador que está en línea.</param>
        private void NotifyFriendOline(int idPlayer)
        {
            var friends = GetFriends(idPlayer);
            foreach (var friend in friends) 
            {
                if (currentUsers.ContainsKey(friend.IdFriend))
                {
                    try
                    {
                        currentUsers[friend.IdFriend].UpdateFriendDisplay();
                    }catch (Exception ex) { Console.WriteLine(ex.InnerException); }
                }
            }
        }

        public int LogOut(int idPlayer)
        {
            int result = 0;
            try
            {
                Console.WriteLine("SALIOOOOOOOOOOOOOOOOOOOOO");
                currentUsers.Remove(idPlayer);
                result = 1;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.InnerException);
            }
            return result;
        }
    }
}
