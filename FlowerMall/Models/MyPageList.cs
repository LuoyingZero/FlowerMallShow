using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace FlowerMall.Models
{
    public class MyPageList
    {
        public IPagedList<tb_GoodsInfo> goodlist { set; get; }

        public IPagedList<tb_Order> orderlist { set; get; }
        public IPagedList<UserData> userlist { set; get; }

        public IPagedList<tb_Comments> commlist { get; set; }
    }
}