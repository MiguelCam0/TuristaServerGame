using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IDataBase;
using Contracts.IGameManager;
using log4net;
using log4net.Config;
using Services.GameManager;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {
        private static readonly ILog _ilog = LogManager.GetLogger(typeof(PlayerManager));
        public static Dictionary<int, Game> CurrentGames = new Dictionary<int, Game>();

        /// <summary>
        /// Agrega un nuevo juego a la colección de juegos actuales.
        /// </summary>
        /// <param name="game">Objeto idGame que representa el juego a agregar.</param>
        public void AddGame(Game game)
        {
            CurrentGames.Add(game.IdGame, game);
            CurrentGames[game.IdGame].Players = new Queue<Player>();
            CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
        }

        /// <summary>
        /// Agrega un jugador a un juego específico en la colección de juegos actuales.
        /// </summary>
        /// <param name="idGame">Identificador del juego al que se agregará el jugador.</param>
        /// <param name="player">Objeto Player que representa al jugador a agregar.</param>
        public void AddPlayerToGame(int idGame, Player player)
        {
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[idGame].Players.Enqueue(player);
            CurrentGames[idGame].PlayersInGame.Add(player);
        }

        /// <summary>
        /// Actualiza la información de todos los jugadores en un juego específico.
        /// </summary>
        /// <param name="idGame">Identificador del juego cuyos jugadores se actualizarán.</param>
        public void UpdatePlayers(int idGame)
        {
            Console.WriteLine("Entra");
            foreach (var player in CurrentGames[idGame].Players)
            {
                try
                {
                    player.GameManagerCallBack.UpdateGame();
                    player.GameManagerCallBack.AddVisualPlayers();
                }
                catch(TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
            Console.WriteLine("Sale");
        }

        /// <summary>
        /// Inicia un juego, actualiza el estado del juego y notifica a todos los jugadores.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego a iniciar.</param>
        public void StartGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    CurrentGames[game.IdGame].Status = Game.GameSituation.Ongoing;
                    player.GameManagerCallBack.MoveToGame(game);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
            Board board = new Board();
            CurrentBoards.Add(game.IdGame, board);
            UpdateQueu(game.IdGame);
        }

        /// <summary>
        /// Inicializa un juego preparando las piezas y cargando la lista de amigos para cada jugador.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego a inicializar.</param>
        public void InitializeGame(Game game)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                try
                {
                    player.GameManagerCallBack.PreparePieces(game, CurrentGames[game.IdGame].PlayersInGame);
                    player.GameLogicManagerCallBack.LoadFriends(CurrentGames[game.IdGame].Players);
                }
                catch (TimeoutException exception)
                {
                    _ilog.Error(exception.ToString());
                }
            }
        }

        public int Ping()
        {
            return 1;
        }
    }
}
