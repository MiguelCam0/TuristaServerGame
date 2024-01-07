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
        /// <summary>
        /// Verifica si el juego está completo basándose en el límite máximo de jugadores.
        /// </summary>
        /// <param name="code">Código identificador único del juego.</param>
        /// <returns>0 si el juego no está completo, 1 si el juego está completo.</returns>
        public int IsGameFull(int code)
        {
            int result = 0;

            if (CurrentGames[code].PlayersInGame.Count > 3)
            {
                result = 1;
            }

            return result;
        }

        /// <summary>
        /// Verifica si el juego está en curso basándose en su estado actual.
        /// </summary>
        /// <param name="code">Código identificador único del juego.</param>
        /// <returns>0 si el juego está en curso, 1 si el juego no está en curso.</returns>
        public int IsGameOngoing(int code)
        {
            int result = 1;

            if (CurrentGames[code].Status == Game.GameSituation.ByStart)
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Busca un juego en la colección de juegos actual basándose en su código identificador único.
        /// </summary>
        /// <param name="code">Código identificador único del juego a buscar.</param>
        /// <returns>0 si el juego se encuentra, 1 si el juego no se encuentra.</returns>
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
