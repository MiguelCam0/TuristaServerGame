using Contracts.ISessionManager;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IGameManager
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallBack))]
    public interface IGameManager
    {
        [OperationContract]
        void AddGame(Game game);

        [OperationContract]
        void AddPlayerToGame(int Game, Player player);

        [OperationContract(IsOneWay = true)]
        void UpdatePlayers(int IdGame);

        [OperationContract(IsOneWay = true)]
        void SendMessage(int idGame, String message);

        [OperationContract(IsOneWay = true)]
        void StartGame(Game game);
    }

    [ServiceContract]
    public interface IGameManagerCallBack
    {
        [OperationContract]
        void AddVisualPlayers();

        [OperationContract]
        int UpdateGame();

        [OperationContract]
        void GetMessage(String message);

        [OperationContract]
        void MoveToGame(Game game);
    }

    [DataContract]
    public class Game
    {
        [DataMember]
        public int IdGame { get; set; }
        [DataMember]
        public Queue<Player> Players { get; set; } = new Queue<Player>();
        [DataMember]
        public List<Player> PlayersInGame { get; set; } = new List<Player>();
    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public int IdPlayer { get; set; }
        [DataMember]
        public string Name { get; set; }

        public IGameManagerCallBack GameManagerCallBack { get; set; }
    }
}
