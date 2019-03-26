using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DuckLab.Models;

namespace DuckLab.ViewModels
{
    public class GameViewModel
    {
        public GameViewModel()
        {
            this.Players = new HashSet<Player>();
        }

        public virtual Game game { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }

    public class Player
    {
        public string name { get; set; }
        public double gameBalance { get; set; }
    }
}