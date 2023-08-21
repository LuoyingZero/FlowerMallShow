using FlowerMall.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlowerMall.Controllers
{
    public class FlowerUIController : Controller
    {
        // GET: FlowerUI
        public ActionResult Index()
        {
            List<tb_GoodsInfo> Lovegoods;//爱情鲜花
            List<tb_GoodsInfo> Affegoods;//亲情鲜花
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                Lovegoods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains("爱情") && x.GoodState != 2).ToList();
                Affegoods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains("亲情") && x.GoodState != 2).ToList();
            }
            ViewBag.LoveInfo = Lovegoods;
            ViewBag.AffeInfo = Affegoods;
            return View();
        }

        //登录
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(tb_UserInfo myuser,string yzm)
        {
            if(Session["yzmcode"].ToString()  != yzm.ToLower())
                {
                    return Content("notcode");
                }
            tb_UserInfo user;
            //执行数据库验证
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                user = db.tb_UserInfo.FirstOrDefault(m => m.UserLoginName == myuser.UserLoginName && m.UserLoginPwd == myuser.UserLoginPwd);

                if (user != null)
                {
                    //判断账号情况
                    int ustate = Convert.ToInt32(user.UserState);
                    if (ustate != 0)
                    {
                        if (ustate == 2)
                        {
                            //永久封停
                            return Content("forever");
                        }
                        else if (ustate == 1)
                        {
                            //被冻结，判断解封时间
                            DateTime seal = Convert.ToDateTime(user.SealTime);
                            DateTime nowTime = DateTime.Now;
                            if (seal > nowTime)
                            {
                                return Content("nologin");
                            }
                            else
                            {
                                //到达解封时间，修改用户状态
                                user.UserState = 0;
                                db.SaveChanges();
                                Session["username"] = user.UserName.Length > 0 ? user.UserName : "请设置昵称";
                                return Content("true");
                            }
                        }
                        else
                        {
                            return Content("forever");
                        }
                    }
                    Session["username"] = user.UserName.Length > 0 ? user.UserName : "请设置昵称";
                    Session["userid"] = user.UserID;
                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }
        }


        //注册
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(tb_UserInfo user, HttpPostedFileBase uploadFile)
        {
            using(FlowerDBEntities db = new FlowerDBEntities())
            {
                var u = db.tb_UserInfo.FirstOrDefault(x => x.UserLoginName == user.UserTel);
                if (u == null)
                {
                    //判断文件是否存在
                    if (uploadFile != null && uploadFile.ContentLength != 0)
                    {
                        //拼接路径
                        string newPath = Server.MapPath("~/Images/userhead/") + uploadFile.FileName;
                        //保存文件
                        uploadFile.SaveAs(newPath);
                        //保存文件名至数据库
                        user.UserImg = "/Images/UserHead/" + uploadFile.FileName;
                    }
                    else
                    {
                        user.UserImg = "/Images/UserHead/headimg_def1.jpg";
                    }
                    user.UserLoginName = user.UserTel;
                    user.UserDatetime = DateTime.Now;
                    db.tb_UserInfo.Add(user);
                    db.SaveChanges();

                    var n = db.tb_UserInfo.FirstOrDefault(x => x.UserLoginName == user.UserLoginName);
                    Session["username"] = n.UserName;
                    Session["userid"] = n.UserID;

                    return Content("<script>alert('注册成功，即将转到首页。');window.location.href = \"/FlowerUI/Index\"</script>");
                }
                else
                {
                    return Content("<script>alert('该账号已存在。');window.location.href = \"/FlowerUI/Register\"</script>");
                }
            }
        }

        //退出
        public ActionResult Exit()
        {
            //清除Session对象
            Session.Clear();
            //Session.Remove("UserID");
            //返回首页
            return Content("<script>window.location.href = \"/FlowerUI/Index\"</script>");
        }


        //详情页
        public ActionResult Details(int id,int? page = 1)
        {
            tb_GoodsInfo good = new tb_GoodsInfo();
            List<tb_GoodsInfo> goodlist = new List<tb_GoodsInfo>();
            List<tb_ReceAddress> rece = new List<tb_ReceAddress>();
            List<tb_Comments> comm = new List<tb_Comments>();
            using(FlowerDBEntities db = new FlowerDBEntities())
            {
                //商品详情信息
                good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == id && x.GoodState != 2);
                //商品评价信息
                comm = db.tb_Comments.Where(x => x.GoodID == id).OrderByDescending(m => m.CommTime).ToList();
                //推荐商品信息
                Random rd = new Random();
                //查询所有在售商品id
                int[] idl = db.tb_GoodsInfo.Where(x => x.GoodState == 0).Select(a => a.GoodID).ToArray();
                for (int i = 0; i < 5; i++)
                {
                    //通过生成随机数的方式，获取数组内随机商品id
                    var ss = idl[rd.Next(0, idl.Length)];
                    //根据随机id查询不同的商品，并填充到List<T>中
                    var rec = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == ss);
                    goodlist.Add(rec);
                }
                ViewBag.recogood = goodlist;

                if (Session["userid"] != null)
                {
                    
                    var uid = int.Parse(Session["userid"].ToString());
                    //用户收货地址
                    rece = db.tb_ReceAddress.Where(x => x.UserID == uid).ToList();
                    ViewBag.UserRece = rece;
                }
                if (good == null)
                {
                    return Content("<script>alert('该商品不存在或已下架。');window.location.href = \"/FlowerUI/Index\"</script>");
                }
            }
            ViewBag.gooddata = good;
            //分页
            const int pageItems = 5;            //每页显示的数据
            int currentPage = (page ?? 1);      //当前页码
            IPagedList<tb_Comments> pageBooks = comm.ToPagedList(currentPage, pageItems);

            MyPageList vgood = new MyPageList();
            vgood.commlist = pageBooks;
            //ViewBag.commlist = vgood.commlist;
            return View(vgood);
            //return View(good);
        }

        //查询收货地址
        public ActionResult SelUserRece(int rid)
        {
            string srece = "";
            using(FlowerDBEntities db = new FlowerDBEntities())
            {
                var rece = db.tb_ReceAddress.FirstOrDefault(x => x.ReceID == rid);
                srece += rece.ReceName + "^" + rece.ReceTel + "^" + rece.ReceAddr;
            }
            return Content(srece);
        }

        //加入购物车
        public ActionResult InsertShop(int gid,int gnum)
        {
            if (Session["userid"] == null) {
                return Content("未登录");
            }
            else
            {
                var uid = int.Parse(Session["userid"].ToString());
                tb_ShopCart cart = new tb_ShopCart();
                tb_GoodsInfo good;
                int count = 0;
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    //查询当前商品信息
                    good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);
                    //判断库存
                    int kc = int.Parse(good.GoodStock.ToString());
                    if (kc > 0 && gnum <= kc)
                    {
                        //判断购物车是否拥有该商品
                        var req = db.tb_ShopCart.FirstOrDefault(x => x.GoodID == gid && x.UserID == uid);
                        if (req == null)
                        {

                            //填充购物车信息
                            cart.UserID = uid;
                            cart.GoodID = gid;
                            cart.GoodName = good.GoodName;
                            cart.GoodPrice = good.GoodPrice;
                            cart.GoodNum = gnum;
                            cart.GoodImg = good.GoodMainImg;
                            cart.GoodInfo = good.FlowerLanguage;
                            //保存到数据库
                            db.tb_ShopCart.Add(cart);
                        }
                        else
                        {
                            req.GoodNum += gnum;
                        }

                        count = db.SaveChanges();
                    }
                    else
                    {
                        return Content("无库存");
                    }
                }
                if (count > 0)
                {
                    return Content("true");
                }
                else
                {
                    return Content("false");
                }
            }
        }

        //购买商品·验证
        public ActionResult ReBuy(int gid,int gnum)
        {
            if (Session["userid"] == null)
            {
                return Content("未登录");
            }
            else
            {
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);
                    if(gnum > good.GoodStock)
                    {
                        return Content("notkc");
                    }
                    else
                    {
                        return Content("true");
                    }
                }
            }
        }
        
        //购买商品-单件
        public ActionResult AloneBuy(int gid,int gnum,string rece_name,string rece_tel,string rece_area)
        {

            if (Session["userid"] == null)
            {
                return Content("未登录");
            }else
            {
                var uid = int.Parse(Session["userid"].ToString());
                //生成订单号
                DateTime now = DateTime.Now;
                //获取拼接形式的，精确到毫秒
                string a = now.ToString("yyyyMMddHHmmssff");
                string u = String.Format("{0:0000}", uid);
                string ddh = a + u;
                string CreateTime = DateTime.Now.ToString();//订单创建时间

                tb_Order order = new tb_Order();//订单表·主表
                tb_OrderSp orderSp = new tb_OrderSp();//订单表·从表
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    //查询用户信息
                    //var user = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    //保存地址信息
                    var new_addr = new tb_ReceAddress();
                    new_addr.UserID = uid;
                    new_addr.ReceName = rece_name;
                    new_addr.ReceAddr = rece_area;
                    new_addr.ReceTel = rece_tel;
                    var u_Addr = db.tb_ReceAddress.Where(x => x.UserID == uid);
                    //判断地址数量
                    if (u_Addr.Count() < 10)
                    {
                        db.tb_ReceAddress.Add(new_addr);
                    }
                    else
                    {
                        var old_Addr = db.tb_ReceAddress.FirstOrDefault(x => x.UserID == uid);
                        old_Addr.ReceName = rece_name;
                        old_Addr.ReceAddr = rece_area;
                        old_Addr.ReceTel = rece_tel;
                    }

                    //查询商品信息
                    var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == gid);//@
                    //修改库存以及销量
                    if (gnum > good.GoodStock)
                    {
                        return Content("无库存");
                    }
                    else
                    {
                        good.GoodStock -= gnum;     //减少库存
                        good.GoodMsales += gnum;    //增加销量

                        //计算总价
                        var spSum = good.GoodPrice * gnum;
                        //计算运费
                        var spFreight = good.GoodPrice * gnum >= 49 ? 0 : 8;//@
                                                                            //填充订单数据·主表
                        order.OrderNumber = ddh;            //订单号
                        order.UserID = uid;                 //用户id
                        order.GoodFreight = spFreight;      //运费
                        order.OrderMoney = (spSum += spFreight).ToString();//总金额
                        order.CreateTime = DateTime.Parse(CreateTime);      //创建时间
                        order.OrderState = 0;//订单状态，默认未付款
                        order.GoodNames += good.GoodName;//新增·添加商品名字到主表
                        order.ReceName = rece_name;         //收货人姓名
                        order.ReceTel = rece_tel;           //收货人手机号
                        order.ReceAddr = rece_area;         //收货地址
                        order.UserVisible = 0;              //订单状态 默认0正常
                                                            //保存订单表·主表
                        db.tb_Order.Add(order);
                        db.SaveChanges();
                        //根据订单号查询主表数据
                        var selorder = db.tb_Order.FirstOrDefault(x => x.OrderNumber == ddh);

                        //填充订单数据·从表
                        orderSp.OrderID = selorder.OrderID; //主表id
                        orderSp.GoodID = gid;           //商品id
                        orderSp.GoodName = good.GoodName;//商品名称
                        orderSp.GoodPrice = good.GoodPrice;//商品单价
                        orderSp.GoodImg = good.GoodMainImg;//商品图片
                        orderSp.GoodNums = gnum;        //商品数量
                        orderSp.GoodSum = good.GoodPrice * gnum;//总金额
                        //保存订单数据·从表
                        db.tb_OrderSp.Add(orderSp);
                        db.SaveChanges();
                    }
                }
                return Content("true");
            }
        }

        

        //购物车页
        public ActionResult Cart()
        {
            if (Session["userid"] == null)
            {
                return Content("<script>alert('登录后才可以访问购物车');window.location.href = \"/FlowerUI/Index\"</script>");
            }
            else
            {
                var uid = int.Parse(Session["userid"].ToString());
                List<tb_ShopCart> list = new List<tb_ShopCart>();
                List<tb_ReceAddress> rece = new List<tb_ReceAddress>();
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    list = db.tb_ShopCart.Where(x => x.UserID == uid).ToList();
                    rece = db.tb_ReceAddress.Where(x => x.UserID == uid).ToList();
                    ViewBag.UserRece = rece;
                }
                return View(list);
            }
        }
        //购物车·删除商品
        public ActionResult CartDel(int id)
        {
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                var cart = db.tb_ShopCart.FirstOrDefault(x => x.ShopID == id);
                db.tb_ShopCart.Remove(cart);
                db.SaveChanges();
            }
            return Content("<script>window.location.href = \"/FlowerUI/Cart\"</script>");
        }
        //购物车·修改数量          //购物车id，操作类型（增加&减少)
        public ActionResult CartSetNum(int cid, string cstate)
        {
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                if (cstate == "增加")
                {
                    //获取id，查询数量，进行修改
                    var cart = db.tb_ShopCart.FirstOrDefault(x => x.ShopID == cid);
                    cart.GoodNum++;
                    db.SaveChanges();
                    return Content("true");
                }
                if (cstate == "减少")
                {
                    //获取id，查询数量，进行修改
                    var cart = db.tb_ShopCart.FirstOrDefault(x => x.ShopID == cid);
                    int? num = cart.GoodNum;
                    if (num > 1)
                    {
                        //如果大于1才减少。
                        cart.GoodNum--;
                        db.SaveChanges();
                        return Content("true");
                    }
                    else
                    {
                        return Content("notone");
                    }
                }
            }
            return View();
        }
        //购物车·计算总价
        public ActionResult CartSum(int[] myArray)
        {
            List<tb_ShopCart> carts = new List<tb_ShopCart>();
            int? cNum = 0;
            decimal? cSum = 0;
            var ret = "";
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                foreach (var item in myArray)
                {
                    var cart = db.tb_ShopCart.FirstOrDefault(x => x.ShopID == item);
                    cNum += cart.GoodNum;
                    cSum += cart.GoodNum * cart.GoodPrice;
                    carts.Add(cart);
                }
            }
            ret = cNum + "?" + cSum;
            return Content(ret);
        }
        //购物车·结算功能(购买商品-多件)
        public ActionResult CartPay(int[] idArray, string rece_name, string rece_tel, string rece_area)
        {
            if (Session["userid"] == null)
            {
                return Content("未登录");
            }
            else
            {
                var uid = int.Parse(Session["userid"].ToString());
                //参数为购物车id数组，先根据id数组计算出商品总价以及商品信息
                //后期追加参数·收货信息id
                tb_Order order = new tb_Order();
                List<tb_OrderSp> orderSpT = new List<tb_OrderSp>();
                string oNames = "";
                decimal? oMonery = 0.00M;

                //参考订单表操作，新建订单主表，订单编号，总价
                //生成订单号
                DateTime now = DateTime.Now;
                //获取拼接形式的，精确到毫秒
                string a = now.ToString("yyyyMMddHHmmssff");
                string u = String.Format("{0:0000}", uid);
                string ddh = a + u;
                order.UserID = uid;                 //用户id
                order.OrderNumber = ddh;            //订单编号
                order.CreateTime = DateTime.Now;    //订单创建时间
                order.OrderState = 0;               //订单状态
                order.ReceName = rece_name;         //收货人姓名
                order.ReceTel = rece_tel;           //收货人手机号
                order.ReceAddr = rece_area;         //收货地址
                order.UserVisible = 0;              //订单状态 默认 0 
                //根据订单主表，新建从表，填充从表数据
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    //保存地址信息
                    var new_addr = new tb_ReceAddress();
                    new_addr.UserID = uid;
                    new_addr.ReceName = rece_name;
                    new_addr.ReceAddr = rece_area;
                    new_addr.ReceTel = rece_tel;
                    var u_Addr = db.tb_ReceAddress.Where(x => x.UserID == uid);
                    if(u_Addr.Count() < 10)
                    {
                        db.tb_ReceAddress.Add(new_addr);
                    }
                    else
                    {
                        var old_Addr = db.tb_ReceAddress.FirstOrDefault(x => x.UserID == uid);
                        old_Addr.ReceName = rece_name;
                        old_Addr.ReceAddr = rece_area;
                        old_Addr.ReceTel = rece_tel;
                    }


                    //保存订单基础信息
                    db.tb_Order.Add(order);
                    db.SaveChanges();
                    //根据订单编号查询主表信息
                    var selorder = db.tb_Order.FirstOrDefault(x => x.OrderNumber == ddh);
                    //读取购物车id数组
                    foreach (var item in idArray)
                    {
                        //读取购物车数据
                        var sp = db.tb_ShopCart.FirstOrDefault(x => x.ShopID == item);
                        //查询商品表
                        var good = db.tb_GoodsInfo.FirstOrDefault(x => x.GoodID == sp.GoodID);
                        //减少库存
                        if(good.GoodStock > sp.GoodNum)
                        {
                            good.GoodStock -= sp.GoodNum;
                        }
                        else
                        {
                            good.GoodStock = 0;
                        }
                        good.GoodMsales += sp.GoodNum;  //增加销量
                        //填充从表数据
                        tb_OrderSp orderSp = new tb_OrderSp();
                        orderSp.OrderID = selorder.OrderID;
                        orderSp.GoodID = sp.GoodID;
                        orderSp.GoodName = good.GoodName;
                        orderSp.GoodImg = good.GoodMainImg;
                        orderSp.GoodPrice = good.GoodPrice;
                        orderSp.GoodNums = sp.GoodNum;
                        orderSp.GoodSum = sp.GoodNum * sp.GoodPrice;
                        //保存商品名，保存总价
                        oNames += good.GoodName + "&";
                        oMonery += sp.GoodNum * good.GoodPrice;
                        //保存从表
                        db.tb_OrderSp.Add(orderSp);
                        //删除购物车表数据
                        db.tb_ShopCart.Remove(sp);
                        db.SaveChanges();
                    }
                    selorder.GoodNames = oNames;
                    var Freight = oMonery >= 49 ? 0 : 8;
                    selorder.GoodFreight = Freight;
                    selorder.OrderMoney = (oMonery += Freight).ToString();
                    db.SaveChanges();
                }

                return Content("true");
            }

        }


        //订单页   udata 代表订单号  spname 代表商品名   state 订单状态
        public ActionResult Order(string udata,string spname,string state)
        {
            //查询主表数据
            if (Session["userid"] != null)
            {
                var uid = int.Parse(Session["userid"].ToString());
                //查询订单数据
                List<tb_Order> order = new List<tb_Order>();     //订单主表数据
                List<tb_OrderSp> formorder = new List<tb_OrderSp>();//订单从表数据
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (state != null && state.Replace(" ", "").Length > 0 && state != "5")
                    {
                        int sta = int.Parse(state);
                        if(sta > 4)
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null && spname == null)
                        {
                            //如果都为空，则默认全部检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderState == sta && x.UserVisible == 0).ToList();
                        }
                        else if (spname == null)
                        {
                            udata = udata.Replace(" ", "");
                            //如果商品名为空，则代表 订单号检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderNumber.Contains(udata) && x.OrderState == sta && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null)
                        {
                            //如果订单号为空，则代表 商品名检索
                            //由于商品信息存放于订单从表，因此这里罗辑需要重新编写。
                            //此方法有缺陷。 //缺陷1：输入的信息不能为空，否则会出现查询错误  //解决方案：在搜索前进行字符串长度判断
                            //更新：主表追加商品名，查询商品名可以直接查询主表了
                            spname = spname.Replace(" ", "");
                            //判断去除空格后的长度
                            if (spname.Replace(" ", "").Length > 0)
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.GoodNames.Contains(spname) && x.OrderState == sta && x.UserVisible == 0).ToList();
                            }
                            else
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.OrderState == sta && x.UserVisible == 0).ToList();
                            }
                        }
                        else
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                    }
                    else
                    {
                        if (udata == null && spname == null)
                        {
                            //如果都为空，则默认全部检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                        else if (spname == null)
                        {
                            udata = udata.Replace(" ", "");
                            //如果商品名为空，则代表 订单号检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderNumber.Contains(udata) && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null)
                        {
                            spname = spname.Replace(" ", "");
                            if (spname.Replace(" ", "").Length > 0)
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.GoodNames.Contains(spname) && x.UserVisible == 0).ToList();
                            }
                            else
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                            }
                        }
                        else
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                    }
                    foreach(var item in order)
                    {
                        //var sp = db.tb_OrderSp.FirstOrDefault(x => x.OrderID == item.OrderID);    //通过该方法，只能返回一个值，一个！！
                        //formorder.Add(sp);    //这种方式只能添加一条数据，但是需要添加多条数据,所以需要foreach循环

                        var spInfo = db.tb_OrderSp.Where(x => x.OrderID == item.OrderID).ToList();
                        foreach (var spItem in spInfo)
                        {
                            //订单从表，用于向List<tb_OrderSp>填充数据
                            tb_OrderSp orderSPInfo = new tb_OrderSp();//这行不能放foreach外边，不然都是最后一条数据，，
                            //将List拆分为一条条的数据
                            orderSPInfo.DataID = spItem.DataID;
                            orderSPInfo.OrderID = spItem.OrderID;
                            orderSPInfo.GoodID = spItem.GoodID;
                            orderSPInfo.GoodName = spItem.GoodName;
                            orderSPInfo.GoodImg = spItem.GoodImg;
                            orderSPInfo.GoodType = spItem.GoodType;
                            orderSPInfo.GoodSort = spItem.GoodSort;
                            orderSPInfo.GoodPrice = spItem.GoodPrice;
                            orderSPInfo.GoodNums = spItem.GoodNums;
                            orderSPInfo.GoodSum = spItem.GoodSum;
                            formorder.Add(orderSPInfo);
                        }
                    }
                }
                ViewBag.uOrder = order;
                ViewBag.formOrder = formorder;
            }
            else
            {
                return Content("<script>window.location.href = \"/FlowerUI/Index\"</script>");
            }
            return View();
        }
        //订单页·异步加载   udata 代表订单号  spname 代表商品名   state 订单状态
        public ActionResult OrderAjax(string udata, string spname, string state)
        {
            //查询主表数据
            if (Session["userid"] != null)
            {
                var uid = int.Parse(Session["userid"].ToString());
                //查询订单数据
                List<tb_Order> order = new List<tb_Order>();     //订单主表数据
                List<tb_OrderSp> formorder = new List<tb_OrderSp>();//订单从表数据
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    if (state != null && state.Replace(" ", "").Length > 0 && state != "5")
                    {
                        int sta = int.Parse(state);
                        if (sta > 4)
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null && spname == null)
                        {
                            //如果都为空，则默认全部检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderState == sta && x.UserVisible == 0).ToList();
                        }
                        else if (spname == null)
                        {
                            udata = udata.Replace(" ", "");
                            //如果商品名为空，则代表 订单号检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderNumber.Contains(udata) && x.OrderState == sta && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null)
                        {
                            spname = spname.Replace(" ", "");
                            if (spname.Replace(" ", "").Length > 0)
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.GoodNames.Contains(spname) && x.OrderState == sta && x.UserVisible == 0).ToList();
                            }
                            else
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.OrderState == sta && x.UserVisible == 0).ToList();
                            }
                        }
                        else
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                    }
                    else
                    {
                        if (udata == null && spname == null)
                        {
                            //如果都为空，则默认全部检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                        else if (spname == null)
                        {
                            udata = udata.Replace(" ", "");
                            //如果商品名为空，则代表 订单号检索
                            order = db.tb_Order.Where(x => x.UserID == uid && x.OrderNumber.Contains(udata) && x.UserVisible == 0).ToList();
                        }
                        else if (udata == null)
                        {
                            spname = spname.Replace(" ", "");
                            if (spname.Replace(" ", "").Length > 0)
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.GoodNames.Contains(spname) && x.UserVisible == 0).ToList();
                            }
                            else
                            {
                                order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                            }
                        }
                        else
                        {
                            order = db.tb_Order.Where(x => x.UserID == uid && x.UserVisible == 0).ToList();
                        }
                    }
                    foreach (var item in order)
                    {
                        var spInfo = db.tb_OrderSp.Where(x => x.OrderID == item.OrderID).ToList();
                        foreach (var spItem in spInfo)
                        {
                            //订单从表，用于向List<tb_OrderSp>填充数据
                            tb_OrderSp orderSPInfo = new tb_OrderSp();//这行不能放foreach外边，不然都是最后一条数据，，
                            //将List拆分为一条条的数据
                            orderSPInfo.DataID = spItem.DataID;
                            orderSPInfo.OrderID = spItem.OrderID;
                            orderSPInfo.GoodID = spItem.GoodID;
                            orderSPInfo.GoodName = spItem.GoodName;
                            orderSPInfo.GoodImg = spItem.GoodImg;
                            orderSPInfo.GoodType = spItem.GoodType;
                            orderSPInfo.GoodSort = spItem.GoodSort;
                            orderSPInfo.GoodPrice = spItem.GoodPrice;
                            orderSPInfo.GoodNums = spItem.GoodNums;
                            orderSPInfo.GoodSum = spItem.GoodSum;
                            //新的数据行填充至数据源List<T>中
                            formorder.Add(orderSPInfo);
                        }
                    }
                }
                ViewBag.uOrder = order;
                ViewBag.formOrder = formorder;
            }
            else
            {
                return Content("<script>window.location.href = \"/FlowerUI/Index\"</script>");
            }
            return PartialView("_OrderAjax");
        }
        //订单页·付款功能
        public ActionResult OrderPay(int oid,int pay)
        {
            if (Session["userid"] != null)
            {
                var uid = int.Parse(Session["userid"].ToString());
                using(FlowerDBEntities db= new FlowerDBEntities())
                {
                    var user = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    //查询支付密码是否正确
                    if (pay.ToString() != user.UserPayPwd)
                    {
                        return Content("payerror");
                    }
                    else { 
                        //查询订单状态
                        var order = db.tb_Order.FirstOrDefault(x => x.OrderID == oid);
                        //创建记录表
                        tb_ConsumeData con_data = new tb_ConsumeData();
                        con_data.UserID = uid; 
                        con_data.ConMoney = decimal.Parse(order.OrderMoney);  
                        con_data.ConRemarks = "订单付款"; 
                        con_data.ConType = 0;   
                        con_data.OrderNumber = order.OrderNumber; 
                        con_data.ConTime = DateTime.Now; 
                        db.tb_ConsumeData.Add(con_data);
                        //修改订单状态·付款时间
                        order.OrderState = 1;       
                        order.PaymentTime = DateTime.Now; 
                        db.SaveChanges();
                        return Content("true");
                    }
                }
            }
            else
            {
                return Content("<script>window.location.href = \"/FlowerUI/Index\"</script>");
            }
        }
        //订单页·收货功能
        public ActionResult OrderCon(int oid)
        {
            if (Session["userid"] == null)
            {
                return Content("<script>alert('登录状态超时，请重新登录。');window.location.href = \"/FlowerUI/Login\"</script>");
            }
            else
            {
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    var order = db.tb_Order.FirstOrDefault(x => x.OrderID == oid);
                    order.OrderState = 3;
                    order.ReceiveTime = DateTime.Now;
                    db.SaveChanges();
                }
                return Content("<script>alert('收货成功，请尽快评价。');window.location.href = \"/FlowerUI/Order\"</script>");
            }
        }
        //订单页·删除功能
        public ActionResult OrderDel(int oid)
        {
            if (Session["userid"] == null)
            {
                return Content("notlogin");
            }
            else
            {
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    var order = db.tb_Order.FirstOrDefault(x => x.OrderID == oid);
                    order.UserVisible = 1;
                    db.SaveChanges();
                }
                return Content("true");
            }
        }
        //订单页·手机端
        public ActionResult OrderMove(int id)
        {
            tb_Order order = new tb_Order();
            List<tb_OrderSp> forder = new List<tb_OrderSp>();//订单从表数据
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                order = db.tb_Order.FirstOrDefault(x => x.OrderID == id);
                var spInfo = db.tb_OrderSp.Where(x => x.OrderID == id).ToList();
                foreach (var spItem in spInfo)
                {
                    //订单从表，用于向List<tb_OrderSp>填充数据
                    tb_OrderSp orderSPInfo = new tb_OrderSp();
                    orderSPInfo.DataID = spItem.DataID;
                    orderSPInfo.OrderID = spItem.OrderID;
                    orderSPInfo.GoodID = spItem.GoodID;
                    orderSPInfo.GoodName = spItem.GoodName;
                    orderSPInfo.GoodImg = spItem.GoodImg;
                    orderSPInfo.GoodType = spItem.GoodType;
                    orderSPInfo.GoodSort = spItem.GoodSort;
                    orderSPInfo.GoodPrice = spItem.GoodPrice;
                    orderSPInfo.GoodNums = spItem.GoodNums;
                    orderSPInfo.GoodSum = spItem.GoodSum;
                    //新的数据行填充至数据源List<T>中
                    forder.Add(orderSPInfo);
                }
            }
            ViewBag.mOrder = order;
            ViewBag.fOrder = forder;
            return View();
        }
        //订单页·评价界面
        public ActionResult OrderDataAjax(string selectid)
        {
            int did;
            List<tb_OrderSp> spList = new List<tb_OrderSp>();
            if (selectid != null && selectid != "")
            {
                did = int.Parse(selectid);
                using (FlowerDBEntities db = new FlowerDBEntities())
                {
                    //查询从表数据
                    spList = db.tb_OrderSp.Where(x => x.OrderID == did).ToList();
                }
            }
            return PartialView("_OrderDataAjax", spList);
        }
        //订单页·评价功能
        public ActionResult OrderGoodPL(int oID,int[] cgID,string[] cgPL)
        {
            if (Session["userid"] == null)
            {
                return Content("notlogin");
            }
            else
            {
                var uid = int.Parse(Session["userid"].ToString());
                using(FlowerDBEntities db = new FlowerDBEntities())
                {
                    var user = db.tb_UserInfo.FirstOrDefault(x => x.UserID == uid);
                    var order = db.tb_Order.FirstOrDefault(x => x.OrderID == oID);
                    for (int i = 0; i < cgID.Length; i++)
                    {
                        tb_Comments comm = new tb_Comments();
                        comm.GoodID = cgID[i];
                        comm.GoodSort = " ";
                        comm.UserID = uid;
                        comm.UserName = user.UserSex == "女" ? user.UserName.Substring(0, 2) + " 女士" : user.UserName.Substring(0, 2) + " 先生";
                        comm.CommText = PlTextFiter(cgPL[i]);
                        comm.CommTime = DateTime.Now;
                        db.tb_Comments.Add(comm);
                        db.SaveChanges();
                    }
                    order.OrderState = 4;
                    db.SaveChanges();

                }
                return Content("true");
            }
        }
        //评论的敏感字过滤
        public string PlTextFiter(string pltext)
        {
            pltext = pltext.Replace("智障", "我爹");
            pltext = pltext.Replace("傻瓜", "我爹");
            pltext = pltext.Replace("政治", "**");
            pltext = pltext.Replace("色情", "**");
            pltext = pltext.Replace("脏话", "**");
            pltext = pltext.Replace("反动", "**");
            pltext = pltext.Replace("暴力", "**");
            pltext = pltext.Replace("侮辱", "**");
            pltext = pltext.Replace("废物", "我爹");
            pltext = pltext.Replace("孤儿", "我爹");
            pltext = pltext.Replace("沙雕", "我爹");
            pltext = pltext.Replace("杂种", "我爹");
            pltext = pltext.Replace("SB", "我爹");
            pltext = pltext.Replace("nm", "我妈");
            pltext = pltext.Replace("尼玛", "我妈");
            pltext = pltext.Replace("你妈", "我妈");
            pltext = pltext.Replace("妮马", "我妈");
            pltext = pltext.Replace("你马", "我妈");
            pltext = pltext.Replace("泥马", "我妈");
            pltext = pltext.Replace("泥吗", "我妈");
            pltext = pltext.Replace("尼美", "我妈");
            pltext = pltext.Replace("你妹", "我妈");
            pltext = pltext.Replace("妮妹", "我妈");
            pltext = pltext.Replace("泥煤", "我妈");
            pltext = pltext.Replace("你姐", "我妈");
            pltext = pltext.Replace("尼姐", "我妈");
            pltext = pltext.Replace("NM", "我妈");
            pltext = pltext.Replace("nM", "我妈");
            pltext = pltext.Replace("Nm", "我妈");
            pltext = pltext.Replace("nm", "我妈");
            pltext = pltext.Replace("吃屎", "让我吃屎");
            pltext = pltext.Replace("你爹", "你儿子");
            pltext = pltext.Replace("差评", "好评");
            pltext = pltext.Replace("垃圾", "很不错");
            pltext = pltext.Replace("辣鸡", "很好");
            pltext = pltext.Replace("腊鸡", "完美");
            pltext = pltext.Replace("恶心", "高兴");
            pltext = pltext.Replace("fw", "好人");
            pltext = pltext.Replace("zz", "我爹");
            pltext = pltext.Replace("沙雕", "**");
            pltext = pltext.Replace("暴乱", "**");
            pltext = pltext.Replace("杂种", "我爹");
            return pltext;
        }

        //搜索页
        public ActionResult Search(string spname,int? page=1)
        {
            List<tb_GoodsInfo> goods = new List<tb_GoodsInfo>();
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                if (spname != null && spname != "")
                {
                    spname = spname.Replace(" ", "");
                    goods = db.tb_GoodsInfo.Where(x => x.GoodName.Contains(spname) && x.GoodState != 2).ToList();

                }
                else
                {
                    goods = db.tb_GoodsInfo.Where(x => x.GoodState != 2).ToList();
                }
                if(goods.Count == 0)
                {
                    goods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains(spname) && x.GoodState != 2).ToList();
                }
            }
            const int pageItems = 10;     
            int currentPage = (page ?? 1); 
            IPagedList<tb_GoodsInfo> pageBooks = goods.ToPagedList(currentPage, pageItems);

            MyPageList vgood = new MyPageList();
            vgood.goodlist = pageBooks;
            return View(vgood);
        }
        //搜索页·局部刷新
        public ActionResult SearchAjax(string spname, int? page = 1)
        {
            List<tb_GoodsInfo> goods = new List<tb_GoodsInfo>();
            using (FlowerDBEntities db = new FlowerDBEntities())
            {
                if (spname != null && spname != "")
                {
                    spname = spname.Replace(" ", "");
                    goods = db.tb_GoodsInfo.Where(x => x.GoodName.Contains(spname) && x.GoodState != 2).ToList();

                }
                else
                {
                    goods = db.tb_GoodsInfo.Where(x => x.GoodState != 2).ToList();
                }
                if (goods.Count == 0)
                {
                    goods = db.tb_GoodsInfo.Where(x => x.GoodType.Contains(spname) && x.GoodState != 2).ToList();
                }
            }

            const int pageItems = 10;         
            int currentPage = (page ?? 1);    
            IPagedList<tb_GoodsInfo> pageBooks = goods.ToPagedList(currentPage, pageItems);

            MyPageList vgood = new MyPageList();
            vgood.goodlist = pageBooks;
            return PartialView("_SearchGood", vgood);
        }

    }
}