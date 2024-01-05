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

        public int SavePlayerSession(int idPlayer)
        {
            int result;
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
