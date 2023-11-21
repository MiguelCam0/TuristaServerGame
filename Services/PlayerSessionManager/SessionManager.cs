using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IDataBase;
using Contracts.ISessionManager;
using DataBase;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IPlayerManager
    {
        public static Dictionary<int, IPlayerManagerCallBack> currentUsers = new Dictionary<int, IPlayerManagerCallBack>();

        public int MakeFriendRequest(int IDPlayer, string namePlayer)
        {
            int Result = 0;
            try
            {
                using (var Context = new TuristaMundialEntitiesDB())
                {
                    Console.WriteLine(namePlayer);

                    if (CheckAlredyFriends(IDPlayer, namePlayer) == 0)
                    {
                        Result = 1;
                    }
                    else
                    {
                        var SecondPlayer = Context.PlayerSet.Where(P => P.Nickname == namePlayer).First();
                        if (Context.FriendRequest.Where(r => r.PlayerSet1ID == IDPlayer && r.PlayerSet2ID == SecondPlayer.Id).FirstOrDefault() == null)
                        {            
                            FriendRequest Request = new FriendRequest();
                            Request.PlayerSet1ID = IDPlayer;
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
            IPlayerManagerCallBack context = OperationContext.Current.GetCallbackChannel<IPlayerManagerCallBack>();
            currentUsers.Add(idPlayer, context);
            NotifyFriendOline(idPlayer);
            Console.WriteLine(currentUsers.Count());
            foreach (var kvp in currentUsers)
            {
                Console.WriteLine($"Clave: {kvp.Key}, Valor: {kvp.Value.ToString()}");
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

        private int CheckAlredyFriends(int idPlayer, String playerName)
        {
            int result = 0;
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var player = context.PlayerSet.Where(p => p.Nickname == playerName).FirstOrDefault();
                    if (player != null)
                    {
                        var check = context.friendship.Where(fs => (fs.player1_id == idPlayer || fs.player1_id == player.Id) && ((fs.player2_id == idPlayer || fs.player2_id == player.Id))).FirstOrDefault();
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
            IPlayerManagerCallBack context = OperationContext.Current.GetCallbackChannel<IPlayerManagerCallBack>();
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
    }
}
