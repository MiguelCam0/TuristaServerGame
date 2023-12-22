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

            if (CurrentGames[code].Slot < 0)
            {
                result = 1;
            }

            return result;
        }

        public int IsGameOngoing(int code)
        {
            int result = 1;

            if (CurrentGames[code].Status == Game.Game_Situation.ByStart)
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
