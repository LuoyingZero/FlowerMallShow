//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FlowerMall.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tb_ConsumeData
    {
        public int ConID { get; set; }
        public Nullable<int> UserID { get; set; }
        public Nullable<decimal> ConMoney { get; set; }
        public string ConRemarks { get; set; }
        public Nullable<int> ConType { get; set; }
        public string OrderNumber { get; set; }
        public Nullable<System.DateTime> ConTime { get; set; }
    }
}
