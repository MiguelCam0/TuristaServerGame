﻿using System;
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
    }
}
