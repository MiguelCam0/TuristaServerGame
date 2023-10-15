using Contracts.IDataBase;
using DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public class PlayerManager : IPlayer
    {
        public int PlayerSearch(Player player)
        {
            int check = 0;

            try
            {
                using (var context = new TuristaMagicalPlacesBDEntities())
                {
                    var existingPlayer = context.PlayerSet.FirstOrDefault(p => p.Nickname == player.Nickname && p.Password == player.Password);

                    if (existingPlayer != null)
                    {
                        check = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en PlayerSearch: " + ex.Message);
            }

            return check;
        }


        public int RegisterPlayer(Player player)
        {
            int band = 0;

            try
            {
                using (var context = new TuristaMagicalPlacesBDEntities())
                {
                    context.PlayerSet.Add(player);
                    context.SaveChanges();
                }
                band = 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en RegisterPlayer: " + ex.Message);
            }

            return band;
        }

    }
}
