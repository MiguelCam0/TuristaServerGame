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
        public int AddGame(Game game)
        {
            int result = 0;
            try
            {
                if(game != null && game.IdGame > 0)
                {
                    CurrentGames.Add(game.IdGame, game);
                    CurrentGames[game.IdGame].Players = new Queue<Player>();
                    CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
                    result = 1;
                }
            }catch (Exception exception)
            {
                _ilog.Error(exception.ToString());
            }
            return result;

            Board board = new Board();
            CurrentBoards.Add(game.IdGame, board);
        }

        /// <summary>
        /// Agrega un jugador a un juego específico en la colección de juegos actuales.
        /// </summary>
        /// <param name="idGame">Identificador del juego al que se agregará el jugador.</param>
        /// <param name="player">Objeto Player que representa al jugador a agregar.</param>
        public int AddPlayerToGame(int idGame, Player player)
        {
            int result = 0;
            try
            {
                if (player != null && player.IdPlayer > 0)
                {
                    if(!CurrentGames[idGame].PlayersInGame.Any(existingPlayer => existingPlayer.IdPlayer == player.IdPlayer))
                    {
                        player.GameManagerCallback = OperationContext.Current.GetCallbackChannel<IGameManagerCallback>();
                        CurrentGames[idGame].Players.Enqueue(player);
                        CurrentGames[idGame].PlayersInGame.Add(player);
                        result = 1;
                    }
                }
            }catch (Exception exception)
            {
                _ilog.Error(exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// Actualiza la información de todos los jugadores en un juego específico.
        /// </summary>
        /// <param name="idGame">Identificador del juego cuyos jugadores se actualizarán.</param>
        public void UpdatePlayers(int idGame)
        {
            if (CurrentGames.ContainsKey(idGame))
            {
                foreach (Player playerInGame in CurrentGames[idGame].Players)
                {
                    try
                    {
                        playerInGame.GameManagerCallback.UpdateGame();
                        playerInGame.GameManagerCallback.AddVisualPlayers();
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
                }
            } 
        }

        /// <summary>
        /// Inicia un juego, actualiza el estado del juego y notifica a todos los jugadores.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego a iniciar.</param>
        public void StartGame(Game game)
        {
            if (game != null && game.IdGame > 0 && CurrentGames[game.IdGame].Status != Game.GameSituation.Ongoing)
            {

                foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
                {
                    try
                    {
                        playerInGame.GameManagerCallback.MoveToGame(game);
                        CurrentGames[game.IdGame].Status = Game.GameSituation.Ongoing;

                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
                }
                UpdateQueu(game.IdGame);
            }
            UpdateQueu(game.IdGame);
        }

        /// <summary>
        /// Inicializa un juego preparando las piezas y cargando la lista de amigos para cada jugador.
        /// </summary>
        /// <param name="game">Objeto Game que representa el juego a inicializar.</param>
        public void InitializeGame(Game game)
        {
            if(game != null && game.IdGame > 0)
            {
                foreach (Player playerInGame in CurrentGames[game.IdGame].PlayersInGame)
                {
                    try
                    {
                        playerInGame.GameManagerCallback.PreparePieces(game, CurrentGames[game.IdGame].PlayersInGame);
                        if (playerInGame.GameLogicManagerCallback != null)
                        {
                            playerInGame.GameLogicManagerCallback.LoadFriends(CurrentGames[game.IdGame].Players);
                        }
                    }
                    catch (TimeoutException exception)
                    {
                        _ilog.Error(exception.ToString());
                    }
                }
            } 
        }
    }
}
