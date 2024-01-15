using Contracts.IDataBase;
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
using System.Web;
using System.Xml.Linq;

namespace Contracts.IGameManager
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {
        [OperationContract]
        int AddGame(Game game); 

        [OperationContract]
        int AddPlayerToGame(int game, Player player); 
        
        [OperationContract]
        void AddGuestToGame(int idGame, int idPlayer); 

        [OperationContract(IsOneWay = true)]
        void UpdatePlayers(int idGame); 

        [OperationContract(IsOneWay = true)]
        void SendMessage(int idGame, String message); 

        [OperationContract(IsOneWay = true)]
        void StartGame(Game game); 

        [OperationContract(IsOneWay = true)]
        void InitializeGame(Game game);

        [OperationContract]
        int UpdatePlayerGame(Game game, int idPlayer, Piece playersPiece); 

        [OperationContract(IsOneWay = true)]
        void SelectedPiece(Game game, string piece, int idPlayer); 

        [OperationContract(IsOneWay = true)]
        void UnSelectedPiece(Game game, string piece, int idPlayer); 

        [OperationContract(IsOneWay =true)]
        void CheckReadyToStartGame(Game game); 

        [OperationContract(IsOneWay = true)]
        void UnCheckReadyToStartGame(Game game);

        [OperationContract(IsOneWay = true)]
        void InactivateBeginGameControls(int idGame);

        [OperationContract(IsOneWay = true)]
        void CheckTakenPieces(Game game, int idPlayer);

        [OperationContract]
        void InviteFriendToGame(string codeGame, int friendId);
    }

    [ServiceContract]
    public interface IGameManagerCallback
    {
        [OperationContract]
        void AddVisualPlayers();

        [OperationContract]
        int UpdateGame();

        [OperationContract]
        void GetMessage(String message);

        [OperationContract]
        void MoveToGame(Game game);

        [OperationContract]
        void PreparePieces(Game game, List<Player> playersInGame);

        [OperationContract]
        void BlockPiece(string piece, int idPlayer);

        [OperationContract]
        void UnblockPiece(string piece);

        [OperationContract]
        void EnableStartGameButton();

        [OperationContract]
        void DisableStartGameButton();
    }

    [DataContract]
    public class Game
    {
        public enum GameSituation { ByStart, Ongoing, Finished}
        [DataMember]
        public int IdGame { get; set; }
        [DataMember]
        public int Slot { get; set; }
        [DataMember]
        public GameSituation Status { get; set; }
        [DataMember]
        public int NumberPlayersReady { get; set; }
        [DataMember]
        public Queue<Player> Players { get; set; } = new Queue<Player>();
        [DataMember]
        public List<Player> PlayersInGame { get; set; } = new List<Player>();
    }

    [DataContract]
    public class Player
    {
        public Player() { }

        public Player(int idPlayer, string v)
        {
            IdPlayer = idPlayer;
        }

        public Player(int id, string NamePlayer, bool isGuest) {
            IdPlayer = id;
            Name = NamePlayer;
            Money = 500;
            Position = 0;
            Jail = false;
            Guest = isGuest;
            Loser = false;
        }

        [DataMember]
        public int IdPlayer { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public long Money { get; set; }
        [DataMember]
        public bool Jail { get; set; }
        [DataMember]
        public int VotesToExpel { get; set; }
        [DataMember]
        public bool Guest {  get; set; }
        [DataMember]
        public bool Loser { get; set; }
        [DataMember]
        public Piece Piece { get; set; }
        public IGameManagerCallback GameManagerCallback { get; set; }
        public IGamerLogicManagerCallback GameLogicManagerCallback { get; set; }
        [DataMember]
        public int Games {  get; set; } = 0;
        [DataMember]
        public int GamesWin { get; set; } = 0;
        [DataMember]
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && GetType() == obj.GetType())
            {
                Player player = obj as Player;
                if (player.IdPlayer == this.IdPlayer && this.Description.Equals(player.Description))
                {
                    result = true;
                }
            }
            return result;
        }
    }

    [DataContract]
    public class Piece
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ImagenSource { get; set; }
        [DataMember]
        public int PartNumber { get; set; }
    }

    [DataContract]
    public class Property
    {
        public Property(string name, TypeProperty type, long buyingCost, int posX, int posY, string imageSource, string color)
        {
            Name = name;
            Type = type;
            BuyingCost = buyingCost;
            Taxes = (int)Math.Round(0.15 * buyingCost);
            Situation = PropertySituation.Free;
            Owner = null;
            PositionX = posX;
            PositionY = posY;
            ImageSource = imageSource;
            Color = color;
            NumberHouses = 0;
            IsMortgaged = false;
        }

        public Property(string name, int posX, int posY, string imageSource)
        {
            Name = name;
            PositionX = posX;
            PositionY = posY;
            ImageSource = imageSource;
        }


        public enum TypeProperty { Jail, Service, Street }
        public enum PropertySituation { Free, Bought, House, Hotel }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int PositionX { get; set; }
        [DataMember]
        public int PositionY { get; set; }
        [DataMember]
        public string ImageSource { get; set; }
        [DataMember]
        public TypeProperty Type { get; set; }
        [DataMember]
        public long BuyingCost { get; set; }
        [DataMember]
        public long Taxes { get; set; }
        [DataMember]
        public PropertySituation Situation { get; set; }
        [DataMember]
        public Player Owner { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int NumberHouses { get; set; }
        [DataMember]
        public bool IsMortgaged { get; set; }
        public IGameManagerCallback GameManagerCallback { get; set; }
        public IGameManagerCallback GameLogicManagerCallback { get; set; }
    }

    [DataContract]
    public class Wildcard
    {
        [DataMember]
        public int Action { get; set; }
        [DataMember]
        public int RandomCash {  get; set; }

        public Wildcard() {}
    }
}

