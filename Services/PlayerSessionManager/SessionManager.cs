using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IDataBase;
using Contracts.ISessionManager;
using DataBase;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IFriends
    {
        public static Dictionary<int, INotificationsCallBack> currentUsers = new Dictionary<int, INotificationsCallBack>();

        public int MakeFriendRequest(int Sender, string Reciber)
        {
            int Result = 0;
            try
            {
                using (var Context = new TuristaMundialEntitiesDB())
                {
                    Console.WriteLine(Reciber);

                    if (CheckAlredyFriends(Sender, Reciber) == 0)
                    {
                        Result = 1;
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
            Console.WriteLine(Result);

            return Result;
        }

        public void SavePlayerSession(int idPlayer)
        {
            INotificationsCallBack context = OperationContext.Current.GetCallbackChannel<INotificationsCallBack>();
            currentUsers.Add(idPlayer, context);
            NotifyFriendOline(idPlayer);
            Console.WriteLine(currentUsers.Count());
            foreach (var user in currentUsers)
            {
                Console.WriteLine($"Clave: {user.Key}, Valor: {user.Value.ToString()}");
            }
            
        }

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
                        var check = context.friendship.Where(fs => (fs.player1_id == Sender || fs.player1_id == player.Id) && ((fs.player2_id == Sender || fs.player2_id == player.Id))).FirstOrDefault();
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

        public void UpdatePlayerSession(int idPlayer)
        {
            INotificationsCallBack context = OperationContext.Current.GetCallbackChannel<INotificationsCallBack>();
            currentUsers[idPlayer] = context;
        }

        private void NotifyFriendOline(int idPlayer)
        {
            var friends = GetFriends(idPlayer);
            foreach (var friend in friends) 
            {
                if (currentUsers.ContainsKey(friend.IdFriend))
                {
                    currentUsers[friend.IdFriend].UpdateFriendDisplay();
                }
            }
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
