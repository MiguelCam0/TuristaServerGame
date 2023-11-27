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
        public List<FriendRequestData> GetFriendRequests(int idPlayer)
        {
            List<FriendRequestData> friendRequests = new List<FriendRequestData>();
            List<FriendRequest> dataBaseData = new List<FriendRequest>();
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

            return friendRequests;
        }
    }
}
