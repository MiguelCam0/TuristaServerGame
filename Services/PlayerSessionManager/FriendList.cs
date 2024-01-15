using Contracts.IDataBase;
using Contracts.IPlayerSessionManager;
using DataBase;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IFriendList
    {
        /// <summary>
        /// Obtiene la lista de amigos para un jugador específico.
        /// </summary>
        /// <param name="idPlayer">ID del jugador para el cual se obtiene la lista de amigos.</param>
        /// <returns>Lista de amigos del jugador, incluyendo información como ID y nombre del amigo.</returns>
        public List<FriendList> GetFriends(int idPlayer)
        {
            List<friendship> friendsList = new List<friendship>();
            List<FriendList> friends = new List<FriendList>();
            
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    friendsList = context.friendship.Where(friend => friend.PlayerSet.Id == idPlayer).ToList();
                    if(friendsList != null)
                    {
                        foreach (var friend in friendsList)
                        {
                            var friendData = new FriendList();
                            friendData.IdFriend = friend.PlayerSet1.Id;
                            friendData.FriendName = friend.PlayerSet1.Nickname;
                            friendData.Games = (int)friend.PlayerSet1.Games;
                            friendData.GamesWins = (int)friend.PlayerSet1.Wins;
                            if (friend.PlayerSet1.Games > 0)
                            {
                                friendData.WinRate = ((friend.PlayerSet1.Wins / friend.PlayerSet1.Games) * 100).ToString() + "%";
                            }
                            else
                            {
                                friendData.WinRate = "N/A";
                            }
                            friends.Add(friendData);
                        }
                    }
                    if (friendsList != null)
                    {
                        friendsList = context.friendship.Where(friend => friend.PlayerSet1.Id == idPlayer).ToList();
                        foreach (var friend in friendsList)
                        {
                            var friendData = new FriendList();
                            friendData.IdFriend = friend.PlayerSet.Id;
                            friendData.FriendName = friend.PlayerSet.Nickname;
                            friendData.Games = (int)friend.PlayerSet.Games;
                            friendData.GamesWins = (int)friend.PlayerSet.Wins;
                            if (friend.PlayerSet1.Games > 0)
                            {
                                friendData.WinRate = ((friend.PlayerSet.Wins / friend.PlayerSet.Games) * 100).ToString() + "%";
                            }
                            else
                            {
                                friendData.WinRate = "N/A";
                            }
                            friends.Add(friendData);
                        }
                    }
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }

            friends = AreOnline(friends);
            return friends;
        }

        /// <summary>
        /// Obtiene las solicitudes de amistad pendientes para un jugador específico.
        /// </summary>
        /// <param name="idPlayer">ID del jugador para el cual se obtienen las solicitudes de amistad.</param>
        /// <returns>Lista de datos de solicitudes de amistad, incluyendo el nombre del remitente y el ID de la solicitud.</returns>
        public List<FriendRequestData> GetFriendRequests(int idPlayer)
        {
            List<FriendRequestData> friendRequests = new List<FriendRequestData>();
            List<FriendRequest> dataBaseData = new List<FriendRequest>();

            try
            {

                using (var Context = new TuristaMundialEntitiesDB())
                {
                    dataBaseData = Context.FriendRequest.Where(P => P.PlayerSet2ID == idPlayer).ToList();

                    foreach (var data in dataBaseData)
                    {
                        FriendRequestData request = new FriendRequestData
                        {
                            SenderName = data.PlayerSet.Nickname,
                            IDRequest = data.IDRequest
                        };
                        friendRequests.Add(request);
                    }
                }
            }
            catch (SqlException exception)
            {
                _ilog.Error(exception.ToString());
            }

            return friendRequests;
        }
    }
}
