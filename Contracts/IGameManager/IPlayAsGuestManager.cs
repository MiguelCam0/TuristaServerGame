using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IGameManager
{
    [ServiceContract]
    public interface IPlayAsGuestManager
    {
        [OperationContract]
        int IsGameFull(int code);
        [OperationContract]
        int SearchGameByCode(int code);
        [OperationContract]
        int IsGameOngoing(int code);
    }
}
