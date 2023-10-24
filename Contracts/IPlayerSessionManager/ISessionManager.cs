using System;
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

    }

    [ServiceContract]
    public interface IPlayerManagerCallBack
    {
        [OperationContract]
        void LookForFriends();

    }
}
