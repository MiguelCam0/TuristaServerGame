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
        Game GetGame(int Game);
        [OperationContract]
        string GetMyPlayersName(int idPlayer, int idGame);

        [OperationContract]
        int SendEmail(String verifyCode, String userEmail);

        [OperationContract]
        Player GetPlayerData(int idPlayer);

        [OperationContract]
        int UpdatePlayerData(int idPlayer, String Description);

        [OperationContract]
        int LogOut(int idPlayer);

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

        [DataMember]
        public int Games {  get; set; }

        [DataMember]
        public int GamesWins {  get; set; }

        [DataMember]
        public String WinRate{ get; set; }

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
