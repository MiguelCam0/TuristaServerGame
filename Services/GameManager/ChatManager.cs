
using Contracts.IGameManager;
using EASendMail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameManager
    {

        public void SendMessage(int idGame, string message)
        {
            foreach (var player in CurrentGames[idGame].Players) 
            {
                player.GameManagerCallBack.GetMessage(message);
            }
            
        }

        public void UpdatePlayerGame(Game game, int idPlayer)
        {
            int turn = 0;
            foreach (var player in CurrentGames[game.IdGame].Players)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.Token = player.GameManagerCallBack.UptdatePiecePlayer(game);
                    player.Token.PartNumber = turn;

                    break;
                }
                turn++;
            }
        }

        public void SelectedPiece(Game game, string token)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.BlockPiece(token);
            }
        }

        public void UnSelectedPiece(Game game, string token)
        {
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameManagerCallBack.UnblockPiece(token);
            }
        }

        private static int TotalGuestPlayers = 0;
        public void AddGuestToGame(int idGame, int idPlayer)
        {
            TotalGuestPlayers++;
            Player player = new Player(idPlayer, "Invitado0"+TotalGuestPlayers);
            player.GameManagerCallBack = OperationContext.Current.GetCallbackChannel<IGameManagerCallBack>();
            CurrentGames[idGame].Players.Enqueue(player);
            CurrentGames[idGame].PlayersInGame.Add(player);
        }


        public void CheckReadyToStartGame(Game game)
        {
            CurrentGames[game.IdGame].NumberPlayersReady++;
            if (CurrentGames[game.IdGame].NumberPlayersReady == CurrentGames[game.IdGame].PlayersInGame.Count && CurrentGames[game.IdGame].PlayersInGame.Count > 1)
            {
                foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
                {
                    player.GameManagerCallBack.EnableStartGameButton();
                }
            }

        }

        public void UnCheckReadyToStartGame(Game game)
        {
            CurrentGames[game.IdGame].NumberPlayersReady--;
            if (CurrentGames[game.IdGame].NumberPlayersReady != CurrentGames[game.IdGame].PlayersInGame.Count)
            {
                InactivateBeginGameControls(game.IdGame);
            }
        }

        public void InactivateBeginGameControls(int idGame)
        {
            foreach (var player in CurrentGames[idGame].PlayersInGame)
            {
                player.GameManagerCallBack.DisableStartGameButton();
            }
        }

        public void InviteFriendToGame(string codeGame, string friendEmail)
        {
            try
            {
                SmtpMail mail = new SmtpMail("TryIt");
                mail.From = "yusgus02@gmail.com";
                mail.To = friendEmail;
                mail.Subject = "Codigo de verificacion";
                mail.TextBody = "Tu codigo de verificacion es: " + codeGame;

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
            }
        }
    }
}
