using Contracts.IDataBase;
using Contracts.IGameManager;
using Contracts.ISessionManager;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services.DataBaseManager
{
    public partial class PlayerManager : IGameLogicManager
    {
        private static Property[] board;

        public void InitializeBoard()
        {
            board = new Property[40];
            board[0] = new Property("Inicio", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 580, 584, "", "Sin Color");
            board[1] = new Property("Rincón de las Hechiceras", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 520, 584, "..\\ImageResourceManager\\Locations\\_024d8419-bec0-4cfc-b423-e3e333c5ddf8.jpg", "#955436");
            board[2] = new Property("Puertas del Inframundo", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 464, 584, "..\\ImageResourceManager\\Locations\\4.png", "#955436");
            board[3] = new Property("Castillo de Dracula", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 404, 584, "..\\ImageResourceManager\\Locations\\_4f37e2b1-3686-4319-9291-9b5a8a1f8108.jpg", "#955436");
            board[4] = new Property("Ruinas de montedragon", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 344, 584, "..\\ImageResourceManager\\Locations\\3.png", "#955436");
            board[5] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 285, 584, "", "#000000");
            board[6] = new Property("Templo Kinkaku-ji", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 225, 584, "..\\ImageResourceManager\\Locations\\2.png", "#AAE0FA");
            board[7] = new Property("El Castillo de jade", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 165, 584, "..\\ImageResourceManager\\Locations\\_a6c423c0-a88e-441c-999b-f6f734ed9dfd.jpg", "#AAE0FA");
            board[8] = new Property("El Castillo de Himeji", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 105, 584, "..\\ImageResourceManager\\Locations\\_43c70c85-7bf2-436d-a3a1-419044cd4d3e.jpg", "#AAE0FA");
            board[9] = new Property("El Santuario Itsukushima", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 584, "..\\ImageResourceManager\\Locations\\_03e277f2-80cd-4df3-80e2-935f355fcc3f.jpg", "#AAE0FA");
            board[10] = new Property("Carcel", Property.Type_Property.Jail, 1000, 0, Property.Property_Situation.Free, null, 15, 520, "", "#000000");
            board[11] = new Property("Templo de Oro", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 460, "..\\ImageResourceManager\\Locations\\5.png", "#FF5590");
            board[12] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 15, 400, "", "#000000");
            board[13] = new Property("Templo Mayor", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 340, "..\\ImageResourceManager\\Locations\\6.png", "#FF5590");
            board[14] = new Property("Paraiso maya", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 280, "..\\ImageResourceManager\\Locations\\7.png", "#FF5590");
            board[15] = new Property("Tartaro", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 220, "..\\ImageResourceManager\\Locations\\8.png", "#F19C21");
            board[16] = new Property("Las ruinas enterradas", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 160, "..\\ImageResourceManager\\Locations\\_63562009-c467-4dae-b0e6-3b166d50e140.jpg", "#F19C21");
            board[17] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 15, 100, "", "#000000");
            board[18] = new Property("Castillo de sir Cocodrile", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, 40, "..\\ImageResourceManager\\Locations\\_9d2d0810-a80d-4c30-a054-399f3e44d494.jpg", "#F19C21");
            board[19] = new Property("Templo de abbadon", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 15, -50, "..\\ImageResourceManager\\Locations\\_d360d1a1-5568-406e-916e-c13cb32e4e42.jpg", "#F19C21");
            board[20] = new Property("Carcel", Property.Type_Property.Jail, 1000, 0, Property.Property_Situation.Free, null, 106, -50, "", "#000000");
            board[21] = new Property("Puertas al paraiso", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 166, -50, "..\\ImageResourceManager\\Locations\\1.png", "#DA343A");
            board[22] = new Property("Desenbarco del rey", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 226, -50, "..\\ImageResourceManager\\Locations\\9.png", "#DA343A");
            board[23] = new Property("Templo Dune", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 286, -50, "..\\ImageResourceManager\\Locations\\_6710050d-e305-4400-850b-80f255213e75.jpg", "#DA343A");
            board[24] = new Property("El Olimpo", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 346, -50, "..\\ImageResourceManager\\Locations\\10.png", "#000000");
            board[25] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 406, -50, "", "#FEF100");
            board[26] = new Property("Castillo del cielo", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 461, -50, "..\\ImageResourceManager\\Locations\\_7dc849b1-aaf7-4b1f-b522-827e471ff86b.jpg", "#FEF100");
            board[27] = new Property("Asgard", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 521, -50, "..\\ImageResourceManager\\Locations\\11.png", "#FEF100");
            board[28] = new Property("Castillo Roca Rodante", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 581, -50, "..\\ImageResourceManager\\Locations\\12.png", "#FEF100");
            board[29] = new Property("El dorado", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, -50, "..\\ImageResourceManager\\Locations\\13.png", "#FEF100");
            board[30] = new Property("Carcel", Property.Type_Property.Jail, 1000, 0, Property.Property_Situation.Free, null, 664, 45, "", "#000000");
            board[31] = new Property("Jardines tenebrosos", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 105, "..\\ImageResourceManager\\Locations\\14.png", "#1EB35A");
            board[32] = new Property("El Templo", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 165, "..\\ImageResourceManager\\Locations\\15.png", "#1EB35A");
            board[33] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 664, 225, "", "#000000");
            board[34] = new Property("Jardines del edén", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 285, "..\\ImageResourceManager\\Locations\\16.png", "#1EB35A");
            board[35] = new Property("Chazas de los enanos", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 345, "..\\ImageResourceManager\\Locations\\17.png", "#1EB35A");
            board[36] = new Property("Puerto de barcos", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 405, "..\\ImageResourceManager\\Locations\\18.png", "#0172BB");
            board[37] = new Property("Atlantis", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 664, 465, "..\\ImageResourceManager\\Locations\\19.png", "#0172BB");
            board[38] = new Property("Evento", Property.Type_Property.Service, 1000, 0, Property.Property_Situation.Free, null, 664, 525, "", "#000000");
            board[39] = new Property("Invernalia", Property.Type_Property.Street, 1000, 0, Property.Property_Situation.Free, null, 644, 585, "..\\ImageResourceManager\\Locations\\_724ecb4d-9044-489d-b9a2-77c29cf3b664.jpg", "#0172BB");
        }

        public void PlayTurn(Game game)
        {
            Random random = new Random();
            int dieOne = random.Next(1, 6);
            int dieSecond = random.Next(1, 6);
            
            CurrentGames[game.IdGame].Players.Peek().Position = CurrentGames[game.IdGame].Players.Peek().Position + dieOne + dieSecond;
            
            if (CurrentGames[game.IdGame].Players.Peek().Position >= 40)
            {
                CurrentGames[game.IdGame].Players.Peek().Position -= 40;
            }
            
            foreach (var player in CurrentGames[game.IdGame].PlayersInGame)
            {
                player.GameLogicManagerCallBack.PlayDie(dieOne, dieSecond);
                player.GameLogicManagerCallBack.MovePlayerPieceOnBoard(CurrentGames[game.IdGame].Players.Peek(), board[CurrentGames[game.IdGame].Players.Peek().Position - 1]);
                if(player.IdPlayer == CurrentGames[game.IdGame].Players.Peek().IdPlayer)
                {
                    player.GameLogicManagerCallBack.ShowCard(board[player.Position]);
                }
            }

            CurrentGames[game.IdGame].Players.Enqueue(CurrentGames[game.IdGame].Players.Peek());
            CurrentGames[game.IdGame].Players.Dequeue();
        }

        public void PurchaseProperty(Property property, Player player)
        {
            int indice = 0;
            player.Money -= property.BuyingCost;
            foreach (Property propertyAux in board)
            {
                if (propertyAux.Name == property.Name)
                {
                    board[indice].Owner = player;
                    board[indice].Situation = property.Situation;
                    board[indice].NumberHouses = property.NumberHouses;
                    Console.WriteLine("Nombre de la plaza: " + board[indice].Name + " estado de la propiedad: " + board[indice].Situation + " cantidad de casas: " + board[indice].NumberHouses + " Dueño: " + board[indice].Owner.Name);
                    break;
                }
                indice++;
            }
        }

        public void UpdatePlayerService(int idPlayer, int idGame)
        {
            IGamerLogicManagerCallBack context = OperationContext.Current.GetCallbackChannel<IGamerLogicManagerCallBack>();
            foreach (Player player in CurrentGames[idGame].PlayersInGame)
            {
                if (player.IdPlayer == idPlayer)
                {
                    player.GameLogicManagerCallBack = context;
                    break;
                }
            }
        }
    }
}
