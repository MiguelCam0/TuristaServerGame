using Contracts.IDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.ISessionManager
{
    [ServiceContract(CallbackContract = typeof(INotificationsCallBack))]
    public interface IFriends
    {
        [OperationContract(IsOneWay = true)]
        void SavePlayerSession(int idPlayer);

        [OperationContract]
        int MakeFriendRequest(int IDPlayer, String namePlayer);

        [OperationContract]
        int AcceptFriendRequest(int IdRequest);

        [OperationContract]
        int RejectFriendRequest(int IdRequest);

        [OperationContract(IsOneWay = true)]
        void UpdatePlayerSession(int idPlayer);
    }

    [ServiceContract]
    public interface INotificationsCallBack
    {
        [OperationContract]
        void UpdateFriendRequest();

        [OperationContract]
        void UpdateFriendDisplay();

    }
}
