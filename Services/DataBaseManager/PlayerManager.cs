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
using EASendMail;

namespace Services.DataBaseManager
{

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class PlayerManager : IPlayer
    {

        public int PlayerSearch(PlayerSet player)
        {
            int check = 0;
            //Pruebas TuristaMundialEntitiesDB
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

        public Game GetGame(int Game)
        {
            Console.WriteLine("LA clave del game es: " + Game);
            return CurrentGames[Game];
        }

        public int SendEmail(String verifyCode, String userEmail)
        {
            int result = 0;

            try
            {
                SmtpMail mail = new SmtpMail("TryIt");
                mail.From = "yusgus02@gmail.com";
                mail.To = userEmail;
                mail.Subject = "Codigo de verificacion";
                mail.TextBody = "Tu codigo de verificacion es: " + verifyCode;

                SmtpServer emailServer = new SmtpServer("smtp.gmail.com");

                emailServer.User = "yusgus02@gmail.com";
                emailServer.Password = "nopk fxne wkiy lvpg";
                emailServer.Port = 587;
                emailServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                SmtpClient reciber = new SmtpClient();
                reciber.SendMail(emailServer, mail);

            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
                result = 1;
            }

            return result;
        }
    }
}