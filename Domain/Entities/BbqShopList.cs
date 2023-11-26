using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class BbqShopList
    {
        public string? Id { get; set; }        
        public string? PersonId { get; set; }        
        public double? Meat{ get; set; }
        public double? Vegetables { get; set; }
        public ShopListStatus Status { get; set; }
    }

    public enum ShopListStatus
    {        
        Will,
        WillNot
    }
}
