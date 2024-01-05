using Contracts.IDataBase;
using DataBase;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Contracts.IGameManager;

namespace Services.DataBaseManager
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class PlayerManager : IPlayer
    {
        public int PlayerSearch(PlayerSet player)
        {
            int check = 0;
            
            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Where(p => p.Nickname == player.Nickname && p.Password == player.Password).FirstOrDefault();

                    if (existingPlayer != null)
                    {
                        Console.WriteLine("Entro crack");
                        check = existingPlayer.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en PlayerSearch: " + ex.InnerException);
            }

            return check;
        }

        public int RegisterPlayer(PlayerSet player)
        {
            int band = 0;

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    context.PlayerSet.Add(player);
                    band = context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en RegisterPlayer: " + ex.Message);
            }

            Console.WriteLine(band);
            return band;
        }

        private List<FriendList> AreOnline(List<FriendList> friends) { 
            foreach (var friend in friends) {
                foreach (var OnlineUser in currentUsers)
                {
                    if (friend.IdFriend == OnlineUser.Key)
                    { 
                        friend.IsOnline = true;
                    }
                }
            }
            return friends;
        }

        public string GetPlayerName(int IdPlayer)
        {
            string PlayerName = "";

            try
            {
                using (var context = new TuristaMundialEntitiesDB())
                {
                    var existingPlayer = context.PlayerSet.Where(p => p.Id == IdPlayer).FirstOrDefault();

                    if (existingPlayer != null)
                    {
                        PlayerName = existingPlayer.Nickname;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en PlayerSearch: " + ex.InnerException);
            }

            return PlayerName;
        }

        public string GetMyPlayersName(int idPlayer, int idGame)
        {
            string PlayerName = "";
            foreach (var player in CurrentGames[idGame].PlayersInGame)
            {
                if (player.IdPlayer == idPlayer)
                {
                    PlayerName = player.Name;
                    break;
                }
            }

            return PlayerName;
        }

        public Game GetGame(int Game)
        {
            Console.WriteLine("LA clave del game es: " + Game);
            return CurrentGames[Game];
        }
    }
}