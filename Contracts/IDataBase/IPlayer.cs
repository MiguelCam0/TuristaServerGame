using Contracts.IGameManager;
using DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IDataBase
{
    [ServiceContract]
    public interface IPlayer
    {
        [OperationContract]
        int RegisterPlayer(PlayerSet player);
        [OperationContract]
        int PlayerSearch(PlayerSet player);
        [OperationContract]
        string GetPlayerName(int IdPlayer);

        [OperationContract]
        List<FriendList> GetFriends(int idPlayer);

        [OperationContract]
        List<FriendRequestData> GetFriendRequests(int idPlayer);

        [OperationContract]
        Game GetGame(int Game);

    }

    [DataContract]
    public class FriendList 
    {

        [DataMember]
        public int IdFriend { get; set; }

        [DataMember]
        public string FriendName { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }

    }

    [DataContract]
    public class FriendRequestData
    {
        [DataMember]
        public int IDRequest { get; set; }
        [DataMember]
        public string SenderName { get; set;}

    }
}
