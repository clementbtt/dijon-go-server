﻿using Go_logic;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace WebSocket
{
    public class Interpreter
    {
        
        public Interpreter()
        {
            
        }

        public string Interpret(string message, Client client)
        {

            int idGame = Convert.ToInt32(message.Split("/")[0]);
            message = message.Split("/")[1];
            string type = message.Split(":")[0];
            string response = "";
            switch (type)
            {
                case "Stone": this.PlaceStone(client, idGame, message.Split(':')[1], ref response);break;
                case "Create": this.CreateGame(client, ref response); break;
                case "Join": this.JoinGame(client, idGame, ref response);break;

            }
            return response;

        }


        private void PlaceStone(Client player, int idGame, string coordinates, ref string response)
        {
            if(idGame != 0)
            {
                Game game = Server.Games[idGame];
                if(game.CurrentTurn == player)
                {
                    try
                    {
                        int x = Convert.ToInt32(coordinates.Split("-")[0]);
                        int y = Convert.ToInt32(coordinates.Split("-")[1]);

                        game.PlaceStone(x, y);

                        game.ChangeTurn();
                        response = $"{idGame}/{game.StringifyGameBoard()}";
                    }
                    catch (Exception e)
                    {
                        response = $"{idGame}/Error:{e.Message}";
                    }
                }
            }
        }

        private void CreateGame(Client client, ref string response)
        {
            int id  = Server.Games.Count + 1;
            Server.Games[id] = new Game(client);
            Server.Games[id].Player1 = client;
            response = $"{id}/";
        }

        private void JoinGame(Client client, int idGame, ref string reponse)
        {
            // exception
            Server.Games[idGame].AddPlayer(client);
            reponse = $"{idGame}/";
        }

    }
}
