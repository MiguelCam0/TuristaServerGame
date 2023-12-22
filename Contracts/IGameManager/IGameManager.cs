﻿using Contracts.IDataBase;
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

        [OperationContract(IsOneWay = true)]
        void InitializeGame(Game game);

        [OperationContract(IsOneWay = true)]
        void UpdatePlayerGame(Game game, int idPlayer);

        [OperationContract(IsOneWay = true)]
        void SelectedPiece(Game game, string piece);

        [OperationContract(IsOneWay = true)]
        void UnSelectedPiece(Game game, string piece);
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

        [OperationContract]
        void PreparePieces(Game game, List<Player> playersInGame);

        [OperationContract]
        Piece UptdatePiecePlayer(Game game);

        [OperationContract]
        void BlockPiece(string piece);

        [OperationContract]
        void UnblockPiece(string piece);

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
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public long Money { get; set; }
        [DataMember]
        public bool Jail { get; set; }
        [DataMember]
        public List<Property> properties { get; set; }
        [DataMember]
        public bool loser { get; set; }
        [DataMember]
        public Piece Token { get; set; }
        public IGameManagerCallBack GameManagerCallBack { get; set; }
        public IGamerLogicManagerCallBack GameLogicManagerCallBack { get; set; }
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
    public class Square
    {
        [DataMember]
        public int position;
        [DataMember]
        public int Position { get => position; set => position = value; }
    }

    [DataContract]
    public class Property : Square
    {
        private Property() { }
        public Property(string name, Type_Property type, long buyingCost, long taxes, Property_Situation situation, Player owner, int posX, int posY, string imageSource, string color)
        {
            Name = name;
            Type = type;
            BuyingCost = buyingCost;
            Taxes = taxes;
            Situation = situation;
            Owner = owner;
            PosicitionX = posX;
            PosicitionY = posY;
            ImageSource = imageSource;
            Color = color;
            NumberHouses = 0;
        }

        public enum Type_Property { Jail, Service, Street }
        public enum Property_Situation { Free, Bought, House, Hotel }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Type_Property Type { get; set; }
        [DataMember]
        public long BuyingCost { get; set; }
        [DataMember]
        public long Taxes { get; set; }
        [DataMember]
        public Property_Situation Situation { get; set; }
        [DataMember]
        public Player Owner { get; set; }
        [DataMember]
        public int PosicitionX { get; set; }
        [DataMember]
        public int PosicitionY { get; set; }
        [DataMember]
        public string ImageSource { get; set; }
        [DataMember]
        public string Color { get; set; }
        [DataMember]
        public int NumberHouses { get; set; }
        public IGameManagerCallBack GameManagerCallBack { get; set; }
        public IGameManagerCallBack GameLogicManagerCallBack { get; set; }
    }

    public class Card
    {
        public int Action { get; set; }
        public int RandomCash {  get; set; }

        public Card()
        {
            Action = RandomAction();
            RandomCash = GenerateRandomCash();
        }

        private int RandomAction()
        {
            Random random = new Random();
            int result = random.Next(1, 7);
            return result;
        }

        private int GenerateRandomCash()
        {
            Random random = new Random();
            int result = random.Next(1, 1000);
            return result;
        }
    }
}

