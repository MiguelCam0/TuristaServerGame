using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace Contracts.IGameManager
{
    [ServiceContract(CallbackContract = typeof(IGamerLogicManagerCallBack))]
    public interface IGameLogicManager
    {
        [OperationContract(IsOneWay = true)]
        void PlayTurn(Game game);
        
        [OperationContract(IsOneWay = true)]
        void UpdatePlayerService(int idPlayer, int idGame);

        [OperationContract(IsOneWay = true)]
        void PurchaseProperty(Property property, Player buyer, int idGame);

        [OperationContract(IsOneWay = true)]
        void StartAuction(int idGame, Property property);

        [OperationContract(IsOneWay = true)]
        void MakeBid(int idGame, int IdPlayer, int Bid);

        [OperationContract(IsOneWay = true)]
        void StopAuction(int idGame, int winner, int winnerBid, Property property);

        [OperationContract(IsOneWay = true)]
        void UpdateQueu(int idGame);

        [OperationContract(IsOneWay = true)]
        void GetActionCard(int idGame, Player player);
        
        [OperationContract(IsOneWay = true)]
        void JailPlayer(int idGame, int idPlayer);

        [OperationContract(IsOneWay = true)]
        void RealizePropertyMortgage(int idGame, Property property, int idPlayer);

        [OperationContract(IsOneWay = true)]
        void DeclareLosingPlayer(Player loserPlayer, int idGame);

        [OperationContract(IsOneWay = true)]
        void PayPropertyMortgage(Game game, int idPlayer, Property mortgagedProperty);

        [OperationContract(IsOneWay =true)]
        void GoToJail(Player player, int idGame);
    }

    [ServiceContract]
    public interface IGamerLogicManagerCallBack
    {
        [OperationContract]
        void PlayDie(int firstDieValue, int SecondDieValue);

        [OperationContract]
        void MovePlayerPieceOnBoard(Player player, Property property);

        [OperationContract]
        void ShowCard(Property property);

        [OperationContract]
        void OpenAuctionWindow(Property property);

        [OperationContract]
        void UpdateBids(int IdPlayer, int Bid);

        [OperationContract]
        void EndAuction(Property property, int winner, int winnerBid);

        [OperationContract]
        void UpdateTurns(Queue<Player> turns);

        [OperationContract]
        void LoadFriends(Queue<Player> friends);

        [OperationContract]
        void ShowEvent(int action);

        [OperationContract]
        void NotifyPlayerOfEvent(int messageNumber);

        [OperationContract]
        void UpgradePlayerMoney(long money);

        [OperationContract]
        void RemoveGamePiece(Player player);

        [OperationContract]
        void EndGame(int idWinner);

        [OperationContract]
        void UpdatePropertyStatus(Property property);
    }
}
