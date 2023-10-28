﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.ISessionManager
{
    [ServiceContract(CallbackContract = typeof(IPlayerManagerCallBack))]
    public interface IPlayerManager
    {
        [OperationContract(IsOneWay = true)]
        void SavePlayerSession(int idPlayer);

        [OperationContract]
        int MakeFriendRequest(int IDPlayer, String namePlayer);

        [OperationContract]
        int AcceptFriendRequest(int IdRequest);

        [OperationContract]
        int RejectFriendRequest(int IdRequest);
    }

    [ServiceContract]
    public interface IPlayerManagerCallBack
    {
        [OperationContract]
        void UpdateFriendRequest();

    }
}