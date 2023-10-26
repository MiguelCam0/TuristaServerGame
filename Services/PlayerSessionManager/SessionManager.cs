using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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
                    var SecondPlayer = Context.PlayerSet.Where(P => P.Nickname == namePlayer).First();

                    if (SecondPlayer == null)
                    {
                        Console.WriteLine("NO lo encontro");
                        Result = 1;
                    }
                    else
                    {
                        FriendRequest Request = new FriendRequest();
                        Request.PlayerSet1ID = IDPlayer;
                        Request.PlayerSet2ID = SecondPlayer.Id;
                        Request.StatusRequest = 1;
                        Context.FriendRequest.Add(Request);
                        Result = Context.SaveChanges();
                        Console.WriteLine("ALGO PASO");
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
            OperationContext.Current.GetCallbackChannel<IPlayerManagerCallBack>().LookForFriends();
            currentUsers.Add(idPlayer, context);
            Console.WriteLine(currentUsers.Count());
            foreach (var kvp in currentUsers)
            {
                Console.WriteLine($"Clave: {kvp.Key}, Valor: {kvp.Value.ToString()}");
            }
        }
    }
}
