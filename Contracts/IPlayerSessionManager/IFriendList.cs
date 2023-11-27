using Contracts.IDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IPlayerSessionManager
{
    [ServiceContract]
    public interface IFriendList
    {
        [OperationContract]
        List<FriendRequestData> GetFriendRequests(int idPlayer);


        [OperationContract]
        List<FriendList> GetFriends(int idPlayer);
    }
}
