using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlowerMall.Models;
using PagedList;

namespace FlowerMall.Controllers
{
    public class FlowerBackController : Controller
    {
        // GET: FlowerBack
        public ActionResult Login()
        {
            return View();
        }
        //登录的方法
        [HttpPost]
        public ActionResult Login(tb_FlowerAdmin admin)
        {
            var login_check = false;
            if(admin.AdminLoginName != null && admin.AdminLoginPwd != null)
            {
                var login_name = admin.AdminLoginName.Replace("'", "”");
                var login_pwd = admin.AdminLoginPwd.Replace("'", "”");
                login_name = TextFilterate(login_name);
                login_pwd = TextFilterate(login_pwd);
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    var a = db.tb_FlowerAdmin.FirstOrDefault(x => x.AdminLoginName == login_name && x.AdminLoginPwd == login_pwd);
                    login_check = (a != null);
                }
            }
            if (login_check)
            {
                //保存session对象
                Session["AdminAK"] = admin.AdminLoginName.Replace("'", "”");
                return Content("<script>window.location.href = \"/FlowerBack/Main\"</script>");
            }
            else
            {
                return Content("<script>alert('账号密码不匹配，请检查你的输入');window.location.href = \"/FlowerBack/Login\"</script>");
            }
        }

        //简单的防注入过滤
        public string TextFilterate(string oldStr)
        {
            oldStr = oldStr.Replace("'", "’");
            oldStr = oldStr.Replace("\"", "”");
            oldStr = oldStr.Replace("\\", "0");
            oldStr = oldStr.Replace("#", "9");
            oldStr = oldStr.Replace("/", "8");
            oldStr = oldStr.Replace("*", "7");
            oldStr = oldStr.Replace(",", "6");
            oldStr = oldStr.Replace(".", "5");
            oldStr = oldStr.Replace("%", "4");
            oldStr = oldStr.Replace("^", "3");
            oldStr = oldStr.Replace("?", "2");
            oldStr = oldStr.Replace("!", "1");
            return oldStr;
        }

        public ActionResult Main()
        {
            //
            if(Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                //后台首页查询数据
                //用户--销售商品数--本月营收额--待发货订单
                var m_Info = new MainInfo();
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    var user = db.tb_UserInfo.Where(x => x.UserState == 0); 
                    var userAll = db.tb_UserInfo;
                    m_Info.UserNum = user.Count();
                    m_Info.UserPer = (user.Count() * 100) / userAll.Count() + "%";
                    var good = db.tb_GoodsInfo.Where(x => x.GoodState == 0);
                    var goodAll = db.tb_GoodsInfo;
                    m_Info.GoodNum = good.Count();
                    m_Info.GoodPer = (good.Count() * 100) / goodAll.Count() + "%";
                    var order = db.tb_Order.Where(x => x.OrderState == 1);
                    m_Info.OrderNum = order.Count();
                    string date = DateTime.Now.Year.ToString() + (DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString());
                    var monery = db.tb_ConsumeData.Where(x => x.OrderNumber.ToString().Contains(date) && x.ConType == 0).ToList();
                    decimal? m_monery = 0.00M;
                    foreach (var item in monery)
                    {
                        m_monery += item.ConMoney;
                    }
                    m_Info.ConMoney = m_monery;
                    ViewBag.MainInfo = m_Info;
                    var good_list = db.tb_GoodsInfo.Where(x => x.GoodState == 0).OrderByDescending(x => x.GoodID).ToList();
                    ViewBag.MainGoodList = good_list;
                }
                return View();
            }
            
        }
        //退出
        public ActionResult Exit()
        {
            Session.Clear();
            return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
        }


        //商品管理
        public ActionResult GoodInfo(string gname,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<tb_GoodsInfo> goods = new List<tb_GoodsInfo>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                { 
                    if (gname != null && gname != "")
                    {
                        gname = gname.Replace(" ", "");  
                        goods = db.tb_GoodsInfo.Where(x => x.GoodName.Contains(gname)).ToList();
                    }
                    else
                    {
                        goods = db.tb_GoodsInfo.ToList();
                    }
                    if (goods.Count == 0)
                    {
                        goods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains(gname)).ToList();
                    }
                }
                //分页
                const int pageItems = 10;
                int currentPage = (page ?? 1);      
                IPagedList<tb_GoodsInfo> pageBooks = goods.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.goodlist = pageBooks;
                return View(vgood);
            }

        }
        public ActionResult GoodInfoAjax(string gname,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<tb_GoodsInfo> goods = new List<tb_GoodsInfo>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {                
                    if (gname != null && gname != "")
                    {
                        gname = gname.Replace(" ", "");
                        goods = db.tb_GoodsInfo.Where(x => x.GoodName.Contains(gname)).ToList();

                    }
                    else
                    {
                        goods = db.tb_GoodsInfo.ToList();
                    }
                    if (goods.Count == 0)
                    {
                        goods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains(gname)).ToList();
                    }
                }
                const int pageItems = 10;         
                int currentPage = (page ?? 1);    
                IPagedList<tb_GoodsInfo> pageBooks = goods.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.goodlist = pageBooks;
                return PartialView("_SearchGoodInfo", vgood);
            }
            
        }
        //商品·上架·下架
        public ActionResult GoodState(string gstate,int gid)
        {
            int count = 0;
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                if (gstate == "上架")
                {
                    var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);
                    good.GoodState = 0;
                    count = db.SaveChanges();
                }
                else if (gstate == "下架")
                {
                    var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);
                    good.GoodState = 2;
                    count = db.SaveChanges();
                }
            }
            if(count > 0)
            {
                return Content("true");
            }
            else
            {
                return Content("false");
            }
        }

        //添加商品
        public ActionResult GoodAdd()
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                return View();
            }
            
        }
        [HttpPost]
        public ActionResult GoodAdd(tb_GoodsInfo good, HttpPostedFileBase uploadFile)
        {
            using(FlowerDBEntities db = new FlowerDBEntities())
            {
                good.GoodState = 0;
                good.GoodFreight = 8;
                good.GoodMsales = 0;
                if (uploadFile != null && uploadFile.ContentLength != 0)
                {
                    string newPath = Server.MapPath("~/Images/GoodsInfo/") + uploadFile.FileName;
                    uploadFile.SaveAs(newPath);
                    good.GoodDataImg = "/Images/GoodsInfo/" + uploadFile.FileName;
                    good.GoodMainImg = "/Images/GoodsInfo/" + uploadFile.FileName;
                }
                else
                {
                    good.GoodMainImg = "/Images/GoodsInfo/good_error_1.jpg";
                    good.GoodDataImg = "/Images/GoodsInfo/good_error_1.jpg";
                }
                db.tb_GoodsInfo.Add(good);
                db.SaveChanges();
            }
            return Content("<script>alert('添加成功，即将转到首页。');window.location.href = \"/FlowerBack/Main\"</script>");

        }


        //修改商品
        public ActionResult GoodSet(int gid)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                tb_GoodsInfo good = new tb_GoodsInfo();
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);
                }
                if (good != null)
                {
                    return View(good);
                }
                else
                {
                    return Content("<script>window.location.href = \"/FlowerBack/GoodInfo\"</script>");
                }
            }
            
        }
        [HttpPost]
        public ActionResult GoodSet(tb_GoodsInfo n_good, HttpPostedFileBase uploadFile)
        {
            using(FlowerDBEntities db = new FlowerDBEntities())
            {
                var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == n_good.GoodID);
                good.GoodName = n_good.GoodName;
                good.GoodType = n_good.GoodType;
                good.FlowerLanguage = n_good.FlowerLanguage;
                good.GoodPrice = n_good.GoodPrice;
                good.GoodStock = n_good.GoodStock;
                if (uploadFile != null && uploadFile.ContentLength != 0)
                {
                    string newPath = Server.MapPath("~/Images/GoodsInfo/") + uploadFile.FileName;
                    uploadFile.SaveAs(newPath);
                    good.GoodDataImg = "/Images/GoodsInfo/" + uploadFile.FileName;
                    good.GoodMainImg = "/Images/GoodsInfo/" + uploadFile.FileName;
                }
                db.SaveChanges();
            }
            return Content("<script>alert('信息修改成功');window.location.href = \"/FlowerBack/GoodInfo\"</script>");
        }

        //订单管理
        public ActionResult OrderInfo(string ordernum,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<tb_Order> orders = new List<tb_Order>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (ordernum != null && ordernum != "")
                    {
                        ordernum = ordernum.Replace(" ", "");
                        orders = db.tb_Order.Where(x => x.OrderState != 0 && x.OrderNumber.Contains(ordernum)).OrderByDescending(x => x.PaymentTime).ToList();

                    }
                    else
                    {
                        orders = db.tb_Order.Where(x=>x.OrderState != 0).OrderByDescending(x => x.PaymentTime).ToList();
                    }
                }
                const int pageItems = 10;
                int currentPage = (page ?? 1); 
                IPagedList<tb_Order> pageBooks = orders.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.orderlist = pageBooks;
                return View(vgood);
            }
            
        }
        public ActionResult OrderInfoAjax(string ordernum,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<tb_Order> orders = new List<tb_Order>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (ordernum != null && ordernum != "")
                    {
                        ordernum = ordernum.Replace(" ", "");
                        orders = db.tb_Order.Where(x => x.OrderState != 0 && x.OrderNumber.Contains(ordernum)).OrderByDescending(x => x.PaymentTime).ToList();

                    }
                    else
                    {
                        orders = db.tb_Order.Where(x=>x.OrderState != 0).OrderByDescending(x => x.PaymentTime).ToList();
                    }
                }
                const int pageItems = 10;
                int currentPage = (page ?? 1);
                IPagedList<tb_Order> pageBooks = orders.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.orderlist = pageBooks;
                return PartialView("_SearchOrderInfo", vgood);
            }
        }
        
        //订单详情
        public ActionResult OrderDetails(int oid)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                tb_Order order = new tb_Order();
                List<tb_OrderSp> orderF = new List<tb_OrderSp>();
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    order = db.tb_Order.FirstOrDefault(x => x.OrderID == oid);
                    orderF = db.tb_OrderSp.Where(x => x.OrderID == oid).ToList();
                }
                ViewBag.uOrder = order;
                ViewBag.formOrder = orderF;
                if (order == null)
                {
                    return Content("<script>window.location.href = \"/FlowerBack/OrderInfo\"</script>");
                }
                else
                {
                    return View();
                }
            }

        }
        //订单发货
        public ActionResult OrderExpress(string expnumber,int oid)
        {
            if(expnumber != null && expnumber.Length > 6 && oid > 0)
            {
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    var order = db.tb_Order.FirstOrDefault(x => x.OrderID == oid);
                    if(order != null)
                    {
                        order.OrderState = 2;
                        order.DeliverTime = DateTime.Now;
                        db.SaveChanges();
                        return Content("<script>alert('已核实物流信息');window.location.href='/FlowerBack/OrderDetails?oid=" + oid + "'</script>");
                    }
                    else
                    {
                        return Content("<script>alert('订单异常\nError：order-277');window.location.href='/FlowerBack/OrderIndex'</script>");
                    }
                }
            }
            else
            {
                return Content("<script>alert('订单异常\nError：order-284');window.location.href='/FlowerBack/OrderDetails?oid=" + oid + "'</script>");
            }
        }

        //用户管理
        public ActionResult UserInfo(string u_name,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<UserData> userList = new List<UserData>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (u_name != null && u_name != "")
                    {
                        u_name = u_name.Replace(" ", "");
                        var user = db.tb_UserInfo.Where(x => x.UserLoginName.Contains(u_name)).ToList();
                        foreach (var item in user)
                        {
                            var uAdd = new UserData();
                            uAdd.UserID = item.UserID;
                            uAdd.UserAcc = item.UserLoginName;
                            uAdd.UserName = item.UserName;
                            uAdd.UserImg = item.UserImg;
                            uAdd.UserDatetime = item.UserDatetime;
                            uAdd.UserState = item.UserState;
                            uAdd.UserSealTime = item.SealTime;
                            List<tb_ConsumeData> uConList = new List<tb_ConsumeData>();
                            uConList = db.tb_ConsumeData.Where(x => x.UserID == item.UserID && x.ConType == 0).ToList();
                            decimal uMonery = 0.00M;
                            foreach (var i in uConList)
                            {
                                uMonery += decimal.Parse(i.ConMoney.ToString());
                            }
                            uAdd.UserConMoney = uMonery;
                            userList.Add(uAdd);
                        }

                    }
                    else
                    {
                        var user = db.tb_UserInfo.ToList();
                        foreach (var item in user)
                        {
                            var uAdd = new UserData();
                            uAdd.UserID = item.UserID;
                            uAdd.UserAcc = item.UserLoginName;
                            uAdd.UserName = item.UserName;
                            uAdd.UserImg = item.UserImg;
                            uAdd.UserDatetime = item.UserDatetime;
                            uAdd.UserState = item.UserState;
                            uAdd.UserSealTime = item.SealTime;
                            List<tb_ConsumeData> uConList = new List<tb_ConsumeData>();
                            uConList = db.tb_ConsumeData.Where(x => x.UserID == item.UserID && x.ConType == 0).ToList();
                            decimal uMonery = 0.00M;
                            foreach (var i in uConList)
                            {
                                uMonery += decimal.Parse(i.ConMoney.ToString());
                            }
                            uAdd.UserConMoney = uMonery;

                            userList.Add(uAdd);
                        }
                    }
                }
                const int pageItems = 10;       
                int currentPage = (page ?? 1);    
                IPagedList<UserData> pageBooks = userList.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.userlist = pageBooks;
                return View(vgood);
            }

        }
        public ActionResult UserInfoAjax(string u_name,int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                List<UserData> userList = new List<UserData>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (u_name != null && u_name != "")
                    {
                        u_name = u_name.Replace(" ", "");
                        var user = db.tb_UserInfo.Where(x => x.UserLoginName.Contains(u_name)).ToList();
                        foreach (var item in user)
                        {
                            var uAdd = new UserData();
                            uAdd.UserID = item.UserID;
                            uAdd.UserAcc = item.UserLoginName;
                            uAdd.UserName = item.UserName;
                            uAdd.UserImg = item.UserImg;
                            uAdd.UserDatetime = item.UserDatetime;
                            uAdd.UserState = item.UserState;
                            uAdd.UserSealTime = item.SealTime;
                            List<tb_ConsumeData> uConList = new List<tb_ConsumeData>();
                            uConList = db.tb_ConsumeData.Where(x => x.UserID == item.UserID && x.ConType == 0).ToList();
                            decimal uMonery = 0.00M;
                            foreach (var i in uConList)
                            {
                                uMonery += decimal.Parse(i.ConMoney.ToString());
                            }
                            uAdd.UserConMoney = uMonery;
                            userList.Add(uAdd);
                        }

                    }
                    else
                    {
                        var user = db.tb_UserInfo.ToList();
                        foreach (var item in user)
                        {
                            var uAdd = new UserData();
                            uAdd.UserID = item.UserID;
                            uAdd.UserAcc = item.UserLoginName;
                            uAdd.UserName = item.UserName;
                            uAdd.UserImg = item.UserImg;
                            uAdd.UserDatetime = item.UserDatetime;
                            uAdd.UserState = item.UserState;
                            uAdd.UserSealTime = item.SealTime;
                            List<tb_ConsumeData> uConList = new List<tb_ConsumeData>();
                            uConList = db.tb_ConsumeData.Where(x => x.UserID == item.UserID && x.ConType == 0).ToList();
                            decimal uMonery = 0.00M;
                            foreach (var i in uConList)
                            {
                                uMonery += decimal.Parse(i.ConMoney.ToString());
                            }
                            uAdd.UserConMoney = uMonery;

                            userList.Add(uAdd);
                        }
                    }
                }
                const int pageItems = 10;          
                int currentPage = (page ?? 1);
                IPagedList<UserData> pageBooks = userList.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.userlist = pageBooks;
                return PartialView("_SearchUserInfo", vgood);
            }

        }
        //修改用户状态
        public ActionResult UserState(int uid,string ustate)
        {
            int count = 0;
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                if (ustate == "ice")
                {
                    var user = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    user.UserState = 2;
                    count = db.SaveChanges();
                }
                else if (ustate == "melt")
                {
                    var user = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    user.UserState = 0;
                    count = db.SaveChanges();
                }
            }
            if (count > 0)
            {
                return Content("true");
            }
            else
            {
                return Content("error");
            }
        }

        //用户详情
        public ActionResult UserDetails(int? uid = -1, int? page = 1)
        {
            if (Session["AdminAK"] == null)
            {
                return Content("<script>window.location.href = \"/FlowerBack/Login\"</script>");
            }
            else
            {
                var notfw = true;
                if (notfw || uid == -1)
                {
                    return Content("<script>window.location.href = \"/FlowerBack/UserInfo\"</script>");
                }
                List<tb_Order> orders = new List<tb_Order>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    var u_Info = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    if (u_Info != null)
                    {
                        orders = db.tb_Order.Where(x => x.UserID == uid && x.OrderState != 0 ).OrderByDescending(x => x.PaymentTime).ToList();
                    }
                    else
                    {
                        return Content("<script>window.location.href = \"/FlowerBack/UserInfo\"</script>");
                    }
                }
                const int pageItems = 10;  
                int currentPage = (page ?? 1); 
                IPagedList<tb_Order> pageBooks = orders.ToPagedList(currentPage, pageItems);

                MyPageList vgood = new MyPageList();
                vgood.orderlist = pageBooks;
                return View(vgood);
            }

        }

    }
}