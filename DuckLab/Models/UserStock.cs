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
    
    public partial class UserStock
    {
        public int userStockId { get; set; }
        public int userId { get; set; }
        public int gameId { get; set; }
        public int companyId { get; set; }
        public Nullable<System.DateTime> timePurchased { get; set; }
        public Nullable<int> quantityPurchased { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}
