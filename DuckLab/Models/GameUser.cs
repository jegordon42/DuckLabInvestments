//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DuckLab.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GameUser
    {
        public int gameUserId { get; set; }
        public int gameId { get; set; }
        public int userId { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}