﻿using MySqlX.XDevAPI;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket.Model;
using WebSocket.Model.DAO;
using WebSocket.Strategy.Enumerations;

namespace WebSocket.Strategy
{
    /// <summary>
    /// Stratégie de matchmaking permettant de mettre en relation deux joueurs pour une partie.
    /// Cette classe gère la file d'attente et la création/jointure des parties multijoueurs.
    /// </summary>
    public class MatchmakingStrategy : IStrategy
    {
        private GameManager gameManager;

        /// <summary>
        /// Temps en secondes au bout duquel le matchmaking s'annule 
        /// </summary>
        const int TIMEOUT_SECONDS = 20;


        public MatchmakingStrategy()
        {
            this.gameManager = new GameManager();
        }

        /// <summary>
        /// Exécute la logique de matchmaking pour un joueur.
        /// </summary>
        /// <param name="player">Le client/joueur qui demande le matchmaking</param>
        /// <param name="data">Données additionnelles (non utilisées)</param>
        /// <param name="gameType">Type de partie demandée (ici "matchmaking")</param>
        /// <param name="response">Réponse à envoyer au client (modifiée par référence)</param>
        /// <param name="type">Type de réponse à envoyer (modifié par référence)</param>
        public void Execute(Client player, string[] data, string gameType, ref string response, ref string type)
        {
            Server.WaitingPlayers.Enqueue(player);
            int initialNbMatchmakingGames = Server.MatchmakingGames.Count();
            MatchmakingState state;

            Client player1 = Server.WaitingPlayers.Peek();
            int idLobby = initialNbMatchmakingGames + 1;
            if (!Server.Lobbies.ContainsKey(idLobby))
            {
                Server.Lobbies[idLobby] = new Lobby(idLobby);
            }

            if (player == player1) // Le joueur qui rejoint est le premier joueur
            {
                Server.Lobbies[idLobby].Player1 = player;
                string userToken = data[2];
                Server.Lobbies[idLobby].Player1.User = this.gameManager.GetUserByToken(userToken);
                // Attente du second joueur
                state = WaitForCondition(() => Server.WaitingPlayers.Count >= 2, () => !Server.Lobbies.ContainsKey(idLobby));
                if (state == MatchmakingState.OK)
                {
                    Client opponement = Server.Lobbies[idLobby].Player2;
                    string opponentUsername = opponement.User.Name;
                    int opponentElo = opponement.User.Elo;
                    Server.WaitingPlayers.Dequeue();
                    response = $"0-Create-matchmaking-{idLobby}-{opponentUsername}-{opponentElo}";
                }
            }
            else // le joueur qui rejoint est le deuxième joueur
            {
                Server.Lobbies[idLobby].Player2 = player;
                string userToken = data[2];
                Server.Lobbies[idLobby].Player2.User = this.gameManager.GetUserByToken(userToken);
                // Attente de la création de la partie
                state = WaitForCondition(() => Server.MatchmakingGames.Count > initialNbMatchmakingGames, () => !Server.Lobbies.ContainsKey(idLobby) );
                if (state == MatchmakingState.OK)
                {
                    Client opponement = Server.Lobbies[idLobby].Player1;
                    string opponentUsername = opponement.User.Name;
                    int opponentElo = opponement.User.Elo;
                    Server.WaitingPlayers.Dequeue();
                    string idGame = Server.MatchmakingGames.Count().ToString();
                    response = $"{idGame}-Join-matchmaking-{idLobby}-{opponentUsername}-{opponentElo}";
                }
            }
            if(state == MatchmakingState.RETRY)
            {
                Server.WaitingPlayers.Dequeue();
                response = "0-Retry";
            }
            else if (state == MatchmakingState.TIMEOUT) // Si on a attendu mais que personne n'a rejoint au bout de TIMEOUT_SECONDS
            {
                Server.WaitingPlayers.Dequeue();
                response = "0-Timeout";
            }
            type = "Send_";
        }

        /// <summary>
        /// Attend qu'une condition soit remplie pendant une durée maximale définie par TIMEOUT_SECONDS.
        /// </summary>
        /// <param name="condition">Délégué représentant la condition principale à vérifier périodiquement</param>
        /// <param name="cancelCondition">Délégué représentant une condition d'annulation</param>
        /// <returns>
        /// MatchmakingState.OK si la condition principale est remplie avant le délai d'attente,
        /// MatchmakingState.TIMEOUT si le délai d'attente est atteint,
        /// MatchmakingState.RETRY si la condition d'annulation est remplie.
        /// </returns>
        private MatchmakingState WaitForCondition(Func<bool> condition, Func<bool> cancelCondition)
        {
            DateTime startTime = DateTime.Now;
            bool loop = true;
            MatchmakingState state = MatchmakingState.TIMEOUT;
            while (loop)
            {
                if (condition())
                {
                    state = MatchmakingState.OK;
                    loop = false;
                }
                if ((DateTime.Now - startTime).TotalSeconds >= TIMEOUT_SECONDS)
                {
                    state = MatchmakingState.TIMEOUT;
                    loop = false;
                }
                if (cancelCondition())
                {
                    state = MatchmakingState.RETRY;
                    loop = false;
                }
                Thread.Sleep(100);
            }
            return state;
        }
    }
}
