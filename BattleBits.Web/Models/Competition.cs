using System;
using System.Collections.Generic;

namespace BattleBits.Web.Models
{
    public class Competition
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public virtual ISet<Game> Games { get; protected set; } = new HashSet<Game>();

        public string Name { get; set; }

        public int NumberCount { get; set; } = 24;

        public Game CreateGame()
        {
            var game = new Game(NumberCount);
            Games.Add(game);
            return game;
        }
    }
}