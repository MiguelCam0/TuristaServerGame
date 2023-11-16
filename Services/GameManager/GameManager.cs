﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts.IGameManager;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {
        public static Dictionary<int, Game> CurrentGames = new Dictionary<int, Game>();
        public void AddGame(Game game)
        {
            CurrentGames.Add(game.IdGame, game);
            CurrentGames[game.IdGame].Players = new Queue<Player>();
            CurrentGames[game.IdGame].PlayersInGame = new List<Player>();
            Console.WriteLine(CurrentGames.First().Key);
        }

        public void AddPlayerToGame(int Game, Player player)
        {
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>(); 
            CurrentGames[Game].Players.Enqueue(player);
            CurrentGames[Game].PlayersInGame.Add(player);
            Console.WriteLine("Numero total de jugadores actuales: " + CurrentGames[Game].Players.Count());
        }

        public void UpdatePlayers(int IdGame)
        {
            foreach(var player in CurrentGames[IdGame].Players)
            {
                Console.WriteLine(player.GameManagerCallBack);
                player.GameManagerCallBack.UpdateGame();
                player.GameManagerCallBack.AddVisualPlayers();
                Console.WriteLine(player.Name);
            }
        }

        public void StartGame(Game game)
        {
            foreach(var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.MoveToGame(game);
            }
        }

        public void UpdateGameServer(int idPlayer, Game game)
        {
            CurrentGames[game.IdGame] = game;
            var thisGame = CurrentGames[game.IdGame];
            foreach (var player in thisGame.Players)
            {
                if(player.IdPlayer != idPlayer)
                {
                    //player.GameManagerCallBack.UpdateGame();
                }
            }
        }

        public void UpdateCallBackPlayer(int idGame, int idPlayer)
        {
            var game = CurrentGames[idGame];
            foreach(var player in game.Players)
            {
                if(player.IdPlayer == idPlayer)
                {
                    Console.WriteLine(player.Name);
                    player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
                    Console.WriteLine(player.GameManagerCallBack + "ES NULO O no");
                }
            }
        }
    }
}
