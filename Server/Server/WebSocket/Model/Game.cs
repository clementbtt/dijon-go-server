﻿using GoLogic;
using GoLogic.Score;
using GoLogic.Serializer;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Text.Json;
using ZstdSharp.Unsafe;

namespace WebSocket.Model
{
    /// <summary>
    /// Classe qui représente une partie de jeu de go
    /// </summary>
    public class Game
    {
        private Client player1;
        private Client player2;
        private Client currentTurn;
        private GameBoard gameBoard;
        private GameLogic logic;
        private BoardSerializer boardSerializer;
        private ScoreRule score;
        private string rule;
        private int size;
        private int id;

        /// <summary>
        /// Proprité qui indique si la partie est pleine
        /// </summary>
        public bool IsFull
        {
            get
            {
                return player1 != null && player2 != null;
            }
        }

        /// <summary>
        /// Récupérer ou modifier le joueur 1
        /// </summary>
        public Client Player1 { get => player1; set => player1 = value; }


        /// <summary>
        /// Récupérer ou modifier le joueur 2
        /// </summary>
        public Client Player2 { get => player2; set => player2 = value; }


        /// <summary>
        /// Récupérer ou modifier le joueur qui doit jouer
        /// </summary>
        public Client CurrentTurn { get => currentTurn; }


        /// <summary>
        /// Récupérer ou modifier la taille du plateau
        /// </summary>
        public int Size { get => size; set => size = value; }


        public string Rule { get => rule; set => rule = value; }


        /// <summary>
        /// Récupérer ou modifier l'identifiant de la partie
        /// </summary>
        public int Id { get => id; set => id = value; }


        /// <summary>
        /// Constructeur de la classe Game
        /// </summary>
        public Game(int size, string rule)
        {
            this.id = Server.Games.Count + 1;
            this.size = size;
            this.gameBoard = new GameBoard(size);
            this.logic = new GameLogic(gameBoard);
            this.boardSerializer = new BoardSerializer(this.logic);
            this.rule = rule;
            switch (this.rule)
            {
                case "c": this.score = new ChineseScoreRule(gameBoard);break;
                case "j": this.score = new JapaneseScoreRule(gameBoard);break;
            }
            
            
        }


        /// <summary>
        /// Ajouter un joueur à la partie
        /// </summary>
        /// <param name="player">Joueur à ajouter</param>
        public void AddPlayer(Client player)
        {
            if (this.player1 == null)
            {
                this.player1 = player;
                this.currentTurn = player;
            }
            else if (this.player2 == null)
            { 
                this.player2 = player;
            }
            else
            {
                // throw exception
            }
           
        }


        /// <summary>
        /// Placer une pierre sur le plateau
        /// </summary>
        /// <param name="x">Coordonées en x de la pierre</param>
        /// <param name="y">Coordonnées en y de la pierre</param>
        public void PlaceStone(int x, int y)
        {
            logic.PlaceStone(x, y);
        }


        /// <summary>
        /// Changement de tour
        /// </summary>
        public void ChangeTurn()
        {
            currentTurn = currentTurn == player1 ? player2 : player1;
        }


        /// <summary>
        /// Conversiond de l'état de la partie en chaine de caractère
        /// </summary>
        /// <returns>état de la partie en string</returns>
        public string StringifyGameBoard()
        {
            return boardSerializer.ChecksGobanForKo(logic, logic.CurrentTurn);
        }

        /// <summary>
        /// Récupérer le score de la partie
        /// </summary>
        /// <returns>Score de la partie sous forme de tuple</returns>
        public (int, int) GetScore()
        {
            return score.CalculateScore();
        }
        
        /// <summary>
        /// Récupère les pierres noires et blanches capturées
        /// </summary>
        /// <returns>Tuple d'entier des pierres noires et blanches</returns>
        public (int, int) GetCapturedStone()
        {
            return (gameBoard.CapturedBlackStones, gameBoard.CapturedWhiteStones);
        }


        /// <summary>
        /// Passer son tour
        /// </summary>
        public void SkipTurn()
        {
            logic.SkipTurn();
            ChangeTurn();
        }


        /// <summary>
        /// Test si la partie est terminée
        /// </summary>
        /// <returns>True si la partie est terminée, False sinon</returns>
        public bool TestWin()
        {
            return logic.IsEndGame;
        }
    }
}
