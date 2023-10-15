using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IDataBase
{
    [ServiceContract]
    public interface IPlayer
    {
        [OperationContract]
        int RegisterPlayer(Player player);
        [OperationContract]
        int PlayerSearch(Player player);
    }
}
