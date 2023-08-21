using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlowerMall.Models
{
    public class UserData
    {
        public int UserID { get; set; }          //用户ID
        public string UserAcc { get; set; }         //用户账号--手机号
        public string UserName { get; set; }        //用户昵称
        public string UserImg { get; set; }         //用户头像
        public decimal UserConMoney { get; set; }   //消费总额
        public DateTime? UserDatetime { get; set; }  //注册时间
        public int? UserState { get; set; }          //账号状态
        public DateTime? UserSealTime { get; set; }  //解封时间
    }
}