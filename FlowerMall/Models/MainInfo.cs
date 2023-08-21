using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowerMall.Models
{
    public class MainInfo
    {
        public int UserNum { get; set; }
        public string UserPer { get; set; }
        public int GoodNum { get; set; }
        public string GoodPer { get; set; }
        public int OrderNum { get; set; }
        public string OrderPer { get; set; }
        public decimal? ConMoney { get; set; }
    }
}