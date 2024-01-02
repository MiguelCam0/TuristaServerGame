using Contracts.IGameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IPlayAsGuestManager
    {
        public int IsGameFull(int code)
        {
            int result = 0;

            if (CurrentGames[code].PlayersInGame.Count > 3)
            {
                result = 1;
            }

            return result;
        }

        public int IsGameOngoing(int code)
        {
            int result = 1;

            if (CurrentGames[code].Status == Game.GameSituation.ByStart)
            {
                result = 0;
            }

            return result;
        }

        public int SearchGameByCode(int code)
        {
            int result = 1;

            if (CurrentGames.ContainsKey(code))
            {
                result = 0;
            }

            return result;
        }
    }
}
