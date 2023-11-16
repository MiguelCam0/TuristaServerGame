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

namespace Services.DataBaseManager
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class PlayerManager : IPlayer
    {

        public int PlayerSearch(PlayerSet player)
        {
            int check = 0;
            Console.WriteLine("Entro crack");
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Where(p => p.Nickname == player.Nickname && p.Password == player.Password).FirstOrDefault();

                    if (existingPlayer != null)
                    {          
                        check = existingPlayer.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en PlayerSearch: " + ex.InnerException);
            }

            return check;
        }

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
            catch (Exception ex)
            {
                Console.WriteLine("Error en RegisterPlayer: " + ex.Message);
            }

            Console.WriteLine(band);
            return band;
        }

        public List<FriendList> GetFriends(int idPlayer)
        {
            List<friendship> friendsList = new List<friendship>();
            List<FriendList> friends = new List<FriendList>();
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    friendsList = context.friendship.Where(friend => friend.PlayerSet.Id == idPlayer).ToList();
                    foreach (var friend in friendsList)
                    {
                        var friendData = new FriendList();
                        friendData.IdFriend = friend.PlayerSet1.Id;
                        friendData.FriendName = friend.PlayerSet1.Nickname;
                        friends.Add(friendData);
                    }

                    friendsList = context.friendship.Where(friend => friend.PlayerSet1.Id == idPlayer).ToList();
                    foreach (var friend in friendsList)
                    {
                        var friendData = new FriendList();
                        friendData.IdFriend = friend.PlayerSet.Id;
                        friendData.FriendName = friend.PlayerSet.Nickname;
                        friends.Add(friendData);
                    }

                }
            }
            catch (SqlException exception)
            {
                Console.WriteLine("Error en GetFriends: " + exception.Message);
            }

            friends = AreOnline(friends);
            return friends;
        }

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

        public List<FriendRequestData> GetFriendRequests(int idPlayer)
        {
            List<FriendRequestData> friendRequests = new List<FriendRequestData>();
            List<FriendRequest> dataBaseData = new List<FriendRequest>();
            using (var Context = new TuristaMundialEntitiesDB())
            {
                dataBaseData = Context.FriendRequest.Where(P=> P.PlayerSet2ID == idPlayer).ToList();

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

            return friendRequests;
        }

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
            catch (Exception ex)
            {
                Console.WriteLine("Error en PlayerSearch: " + ex.InnerException);
            }

            return PlayerName;
        }

        public Game GetGame(int Game)
        {
            Console.WriteLine("LA clave del game es: " + Game);
            return CurrentGames[Game];
        }
    }
}