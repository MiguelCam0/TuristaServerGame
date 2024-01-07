using Contracts.IDataBase;
using Contracts.IGameManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GameManager
{
    public class Board
    {
        public Property[] board;

        public Board() 
        {
            CreateBoard();
        }

        /// <summary>
        /// Crea y inicializa las propiedades del tablero del juego.
        /// </summary>
        public void CreateBoard()
        {
            board = new Property[40];
            board[0] = new Property("Inicio", 580, 584, "");
            board[1] = new Property("Bosque de las Hechiceras", Property.TypeProperty.Street, 90,580, 584, "..\\GameResources\\Pictures\\PropertyImage\\_024d8419-bec0-4cfc-b423-e3e333c5ddf8.jpg", "#955436");
            board[2] = new Property("Puertas del Inframundo", Property.TypeProperty.Street, 150,520, 584, "..\\GameResources\\Pictures\\PropertyImage\\4.png", "#955436");
            board[3] = new Property("Fortaleza de Dracula", Property.TypeProperty.Street, 190, 464, 584, "..\\GameResources\\Pictures\\PropertyImage\\_4f37e2b1-3686-4319-9291-9b5a8a1f8108.jpg", "#955436");
            board[4] = new Property("Ruinas Montedragon", Property.TypeProperty.Street, 120, 404, 584, "..\\GameResources\\Pictures\\PropertyImage\\3.png", "#955436");
            board[5] = new Property("Cuervo Mensajero", 344, 584, "..\\GameResources\\Pictures\\PropertyImage\\R.jpg");
            board[6] = new Property("Templo Dorado Kinkaku-ji", Property.TypeProperty.Street, 420, 285, 584, "..\\GameResources\\Pictures\\PropertyImage\\2.png", "#AAE0FA");
            board[7] = new Property("El Palacio de jade", Property.TypeProperty.Street, 450, 225, 584, "..\\GameResources\\Pictures\\PropertyImage\\_a6c423c0-a88e-441c-999b-f6f734ed9dfd.jpg", "#AAE0FA");
            board[8] = new Property("El Castillo de los Sueños", Property.TypeProperty.Street, 490, 165, 584, "..\\GameResources\\Pictures\\PropertyImage\\_43c70c85-7bf2-436d-a3a1-419044cd4d3e.jpg", "#AAE0FA");
            board[9] = new Property("Santuario Mistico Itsukushima", Property.TypeProperty.Street, 470, 105, 584, "..\\GameResources\\Pictures\\PropertyImage\\_03e277f2-80cd-4df3-80e2-935f355fcc3f.jpg", "#AAE0FA");
            board[10] = new Property("Carcel", 15, 584, "");
            board[11] = new Property("Templo del Crepúsculo", Property.TypeProperty.Street, 220, 15, 520, "..\\GameResources\\Pictures\\PropertyImage\\5.png", "#FF5590");
            board[12] = new Property("Ruinas Celestiales de los Dragones", Property.TypeProperty.Street, 250, 15, 460, "..\\GameResources\\Pictures\\PropertyImage\\_2d866987-c2e5-4f7d-94f5-4b26e903d97d.jpg", "#FF5590");
            board[13] = new Property("Santuario Espectral", Property.TypeProperty.Street, 270, 15, 400, "..\\GameResources\\Pictures\\PropertyImage\\6.png", "#FF5590");
            board[14] = new Property("Paraiso maya", Property.TypeProperty.Street, 200, 15, 340, "..\\GameResources\\Pictures\\PropertyImage\\7.png", "#FF5590");
            board[15] = new Property("Abyssal Tartarus", Property.TypeProperty.Street, 300, 15, 280, "..\\GameResources\\Pictures\\PropertyImage\\8.png", "#F19C21");
            board[16] = new Property("Ciudad Subterránea", Property.TypeProperty.Street, 300, 15, 220, "..\\GameResources\\Pictures\\PropertyImage\\_63562009-c467-4dae-b0e6-3b166d50e140.jpg", "#F19C21");
            board[17] = new Property("Cuervo Mensajero", 15, 160, "..\\GameResources\\Pictures\\PropertyImage\\R.jpg");
            board[18] = new Property("Fortaleza de Sir Cocodrile", Property.TypeProperty.Street, 320, 15, 100, "..\\GameResources\\Pictures\\PropertyImage\\_9d2d0810-a80d-4c30-a054-399f3e44d494.jpg", "#F19C21");
            board[19] = new Property("Templo de abbadon", Property.TypeProperty.Street, 350, 15, 40, "..\\GameResources\\Pictures\\PropertyImage\\_d360d1a1-5568-406e-916e-c13cb32e4e42.jpg", "#F19C21");
            board[20] = new Property("Carcel", 15, -50, "");
            board[21] = new Property("Puertas al paraiso", Property.TypeProperty.Street, 390, 106, -50, "..\\GameResources\\Pictures\\PropertyImage\\1.png", "#DA343A");
            board[22] = new Property("Costa del Rey Errante", Property.TypeProperty.Street, 400, 166, -50, "..\\GameResources\\Pictures\\PropertyImage\\9.png", "#DA343A");
            board[23] = new Property("Oasis Prohibido", Property.TypeProperty.Street, 420, 226, -50, "..\\GameResources\\Pictures\\PropertyImage\\_6710050d-e305-4400-850b-80f255213e75.jpg", "#DA343A");
            board[24] = new Property("Cima Celestial", Property.TypeProperty.Street, 420, 286, -50, "..\\GameResources\\Pictures\\PropertyImage\\10.png", "#DA343A");
            board[25] = new Property("Barco Velero", 346, -50, "..\\GameResources\\Pictures\\PropertyImage\\_5bcd8918-db19-4740-a79c-599af71d7a15.jpg");
            board[26] = new Property("Fortaleza Flotante", Property.TypeProperty.Street, 400, 406, -50, "..\\GameResources\\Pictures\\PropertyImage\\_7dc849b1-aaf7-4b1f-b522-827e471ff86b.jpg", "#FEF100");
            board[27] = new Property("Asgard", Property.TypeProperty.Street, 480, 461, -50, "..\\GameResources\\Pictures\\PropertyImage\\11.png", "#FEF100");
            board[28] = new Property("Castillo Roca Rodante", Property.TypeProperty.Street, 300, 521, -50, "..\\GameResources\\Pictures\\PropertyImage\\12.png", "#FEF100");
            board[29] = new Property("El Dorado Perdido", Property.TypeProperty.Street, 400, 581, -50, "..\\GameResources\\Pictures\\PropertyImage\\13.png", "#FEF100");
            board[30] = new Property("Carcel", 664, -50, "");
            board[31] = new Property("Jardines Tenebrosos", Property.TypeProperty.Street, 220, 664, 45, "..\\GameResources\\Pictures\\PropertyImage\\14.png", "#1EB35A");
            board[32] = new Property("Santuario Oculto", Property.TypeProperty.Street, 250, 664, 105, "..\\GameResources\\Pictures\\PropertyImage\\15.png", "#1EB35A");
            board[33] = new Property("Barco Velero", 664, 165, "..\\GameResources\\Pictures\\PropertyImage\\_5bcd8918-db19-4740-a79c-599af71d7a15.jpg");
            board[34] = new Property("Jardines del Edén", Property.TypeProperty.Street, 250, 664, 225, "..\\GameResources\\Pictures\\PropertyImage\\16.png", "#1EB35A");
            board[35] = new Property("Khazad-dûm", Property.TypeProperty.Street, 200, 664, 285, "..\\GameResources\\Pictures\\PropertyImage\\17.png", "#1EB35A");
            board[36] = new Property("Desenbarco del Rey", Property.TypeProperty.Street, 380, 664, 345, "..\\GameResources\\Pictures\\PropertyImage\\18.png", "#0172BB");
            board[37] = new Property("Atlantida", Property.TypeProperty.Street, 310, 664, 405, "..\\GameResources\\Pictures\\PropertyImage\\19.png", "#0172BB");
            board[38] = new Property("Abismo Sombrío", Property.TypeProperty.Street, 300, 664, 465, "..\\GameResources\\Pictures\\PropertyImage\\_b3cf0b6f-c48c-423e-87fc-e58ec6c18ab5.jpg", "#0172BB");
            board[39] = new Property("Invernalia", Property.TypeProperty.Street, 310, 644, 525, "..\\GameResources\\Pictures\\PropertyImage\\_724ecb4d-9044-489d-b9a2-77c29cf3b664.jpg", "#0172BB");
        }

        /// <summary>
        /// Obtiene la propiedad en una posición específica del tablero.
        /// </summary>
        /// <param name="position">Posición en el tablero para obtener la propiedad.</param>
        /// <returns>Objeto Property que representa la propiedad en la posición especificada.</returns>
        public Property GetProperty(int position)
        {
            return board[position];
        }

        /// <summary>
        /// Registra la compra de una propiedad por parte de un jugador.
        /// </summary>
        /// <param name="player">Jugador que realiza la compra.</param>
        /// <param name="property">Propiedad que se está comprando.</param>
        /// <returns>Entero que indica el resultado de la operación:
        /// 1 - Operación exitosa (propiedad comprada).
        /// 0 - La propiedad ya tiene dueño (no se puede comprar).</returns>
        public int RegisterPurchaseProperty(Player player, Property property)
        {
            int result = 1;
            Property foundProperty = board.FirstOrDefault(p => p.Name == property.Name);

            if (foundProperty != null)
            {
                result = 0;
                foundProperty.Owner = player;
                foundProperty.Situation = property.Situation;
                foundProperty.NumberHouses = property.NumberHouses;
                foundProperty.Taxes = property.Taxes;
            }

            return result;
        }

        /// <summary>
        /// Registra una hipoteca en una propiedad específica del tablero.
        /// </summary>
        /// <param name="property">Propiedad para la cual se registra la hipoteca.</param>
        /// <returns>Entero que indica el resultado de la operación:
        /// 1 - Operación exitosa (hipoteca registrada).
        /// 0 - La propiedad no existe en el tablero (hipoteca no registrada).</returns>
        public int RegisterPropertyMortgage(Property property)
        {
            int result = 1;
            Property foundProperty = board.FirstOrDefault(p => p.Name == property.Name);

            if (foundProperty != null)
            {
                result = 0;
                foundProperty.IsMortgaged = true;
            }

            return result;
        }
    }
}
