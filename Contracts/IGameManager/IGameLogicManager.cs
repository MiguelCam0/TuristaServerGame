using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
        void PurchaseProperty(Property property, Player player, int idGame);

        [OperationContract(IsOneWay = true)]
        void StartAuction(int idGame, Property property);

        [OperationContract(IsOneWay = true)]
        void MakeBid(int idGame, int IdPlayer, int Bid);

        [OperationContract(IsOneWay = true)]
        void StopAuction(int idGame, int winner, int winnerBid, Property property);

        [OperationContract(IsOneWay = true)]
        void UpdateQueu(int idGame);

        [OperationContract(IsOneWay = true)]
        void GetActionCard(int idGame);

        [OperationContract(IsOneWay = true)]
        void MovePlayer(int idGame, int spaces);
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
        void GoToJail();

        [OperationContract]
        void PayTaxes(int taxes);

        [OperationContract]
        void GetPay(int money);
    }
}
