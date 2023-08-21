create database FlowerDB
on
(
name='FlowerDB',
filename='E:\我的文件\__学习资料\FlowerDB.mdf'
)
go

use FlowerDB
go
--管理表·后台登录所判断的账号
create table tb_FlowerAdmin
(
AdminID int primary key identity(1,1) not null,--id
AdminLoginName nvarchar(50) not null,	--登录用户名【账号】 手机号或邮箱
AdminLoginPwd nvarchar(20) not null,	--登录密码
AdminName nvarchar(20),	--姓名
AdminTel nvarchar(20),	--手机号
AdminSex nvarchar(5),	--性别
AdminImg nvarchar(50) default('/Images/UserHead/headimg_def1.jpg'),--用户头像

)

--用户表
create table tb_UserInfo
(
UserID int primary key identity(1,1) not null,--id
UserLoginName nvarchar(50) not null,	--登录用户名【账号】 手机号或邮箱
UserLoginPwd nvarchar(20) not null,		--登录密码
UserPayPwd nvarchar(6),	--支付密码
UserName nvarchar(20),	--昵称	显示的昵称，可保护用户账号隐私
UserTel nvarchar(11),	--手机号
UserSex nvarchar(5),	--性别
UserMark int default(0),--积分
UserImg nvarchar(50) default('/Images/UserHead/headimg_def1.jpg'),--用户头像
UserCollection int default(0),	--收藏量
UserShopCort int default(0),	--购物车数量
UserDatetime datetime,		--注册时间
UserState int default(0),	--用户账号情况 0-正常 | 1-暂时无法登陆 | 2-禁止登录-永久
--如果情况为0就忽视该数据，如果不为0就代表解除的时间
SealTime datetime,
)

--商品表
create table tb_GoodsInfo(
GoodID int primary key identity(1,1) not null,--商品id
GoodName nvarchar(50),		--商品名称
GoodType nvarchar(50),		--商品类型，多个可通过拼接字符串  花种，成花，周边
GoodSort nvarchar(50),		--商品分类，1盆，10盆
FlowerLanguage nvarchar(200),	--花语
GoodMainImg nvarchar(200),		--商品展示图，
GoodDataImg nvarchar(200),		--商品详情图
GoodStock int default(200),		--商品库存
GoodState int default(0),		--商品状态| 0-正常 | 1-缺少库存 | 2-暂时下架
GoodComments int default(0),	--商品评论数，仅显示数目
GoodMsales int default(0),		--商品月销量
GoodCollection int default(0),	--收藏量
GoodPrice decimal(10,2),		--商品单价
GoodFreight int default(8),		--商品运费
)

--用户收货地址表·一个用户可以有多个收货地址
create table tb_ReceAddress(
ReceID int primary key identity(1,1) not null,--收货地址id
UserID int foreign key references tb_UserInfo(UserID),--用户id，用户可以有多个收货地址
ReceName nvarchar(20),	--收货人姓名
ReceAddr nvarchar(50),	--收货人地址
ReceTel nvarchar(11),	--收货人手机号
)

--用户收藏表
create table tb_Collection(
CollecID int  primary key identity(1,1) not null,		--收藏表id
UserID int foreign key references tb_UserInfo(UserID),	--用户id，用户可以有多个收藏
GoodID int ,--商品id，由于商品可能会下架，所以这里不要用外键
GoodName nvarchar(20),		--商品名
GoodPrice decimal(10,2),	--商品单价
GoodImg nvarchar(20),		--商品图片，仅获取第一张，用于展示
GoodState int default(0),	--商品状态， 0 正常 |  1 已下架 
)


--购物车表
--此表用于记录某位客户的所有购物车内商品信息（每个商品都是一条数据）
create table tb_ShopCart(
ShopID int primary key identity(1,1) not null,			--购物车id
UserID int foreign key references tb_UserInfo(UserID),	--用户id，用户可以将多个商品加入购物车
GoodID int ,--商品id，由于商品可能会下架，所以这里不要用外键
GoodName nvarchar(20),		--商品名
GoodPrice decimal(10,2),	--商品单价
GoodSort nvarchar(50),		--商品分类，1盆，10盆
GoodInfo nvarchar(200),		--商品介绍
GoodNum int default(1),		--加入购物车的商品数量
GoodImg nvarchar(50),		--商品图片，仅获取第一张，用于展示
GoodState int default(0),	--商品状态， 0 正常 |  1 已下架 
)

--订单表
create table tb_Order(
OrderID int primary key identity(1,1) not null,	--订单ID·用户不可见
OrderNumber nvarchar(20),	--订单编号，通过算法生成的X为编号，用户可见，可通过编号搜索订单信息
UserID int,		--创建订单的用户id。由于订单属于重要数据，所以不使用外键
ReceName nvarchar(20),	--收货人姓名
ReceAddr nvarchar(50),	--收货人地址
ReceTel nvarchar(11),	--收货人手机号
GoodFreight int default(0),	--商品运费
GoodNames nvarchar(500),	--商品信息
GoodDiscount nvarchar(50),	--商品折扣 
OrderMoney nvarchar(20),	--订单总金额 计算公式： 单价 * 数量 + 邮费 - 折扣
OrderState int default(0),	--订单状态 未付款，已付款（待发货），已发货（待收货），已收货（待评价），交易完成
CreateTime datetime,		--订单创建时间
PaymentTime datetime,		--付款时间
DeliverTime datetime,		--发货时间
ReceiveTime datetime,		--确认收货时间
UserVisible int default(0)	--用户是否可见 0-正常 | 1-已删除(用户不可见)
)
--订单详情表
create table tb_OrderSp(
DataID int primary key identity(1,1) not null,	
OrderID int foreign key references tb_Order(OrderID),	--主表id，用户可以将多个商品加入购物车
GoodID int ,--商品id，由于未来商品可能下架，所以同样不使用外键
GoodName nvarchar(50),		--商品名称
GoodImg nvarchar(200),		--商品图片
GoodType nvarchar(50),		--商品类型，多个可通过拼接字符串  花种，成花，周边
GoodSort nvarchar(50),		--商品分类，1盆，10盆
GoodPrice decimal(10,2),	--商品单价
GoodNums int,				--商品数量
GoodSum decimal(10,2)		--商品总价
)


--消费记录表
create table tb_ConsumeData(
ConID int primary key identity(1,1) not null,	--记录ID
UserID int,		--用户id。
ConMoney decimal(10,2),		--消费金额
ConRemarks nvarchar(50),	--备注信息
ConType int default(0),		--记录类型   0-消费 | 1-退款到账
OrderNumber nvarchar(20),	--订单编号
ConTime datetime,			--记录时间
)

--商品评论表
create table tb_Comments(
CommID int primary key identity(1,1) not null,	--评论ID
GoodID int foreign key references tb_GoodsInfo(GoodID),--商品ID
GoodSort nvarchar(50),		--商品分类，1盆，10盆
UserID int foreign key references tb_UserInfo(UserID),--用户id
UserName nvarchar(20),		--用户昵称  
CommText nvarchar(100),		--评论内容
CommTime date,				--评论时间
CommImg nvarchar(20),		--评论图片
ManagerComm nvarchar(50),	--店家回复
Fabulous int default(0),	--点赞量
)


--添加默认数据
--管理表 // 登录账号 * 登录密码 * 用户昵称 * 手机号 * 性别
insert tb_FlowerAdmin(AdminLoginName,AdminLoginPwd,AdminName,AdminTel,AdminSex) values('2457879996','admin123456','访月人','24578799960','男')

--用户表 // 登录账号 * 登录密码 * 支付密码 * 昵称 * 手机号 * 注册时间
insert tb_UserInfo(UserLoginName,UserLoginPwd,UserPayPwd,UserName,UserTel,UserDatetime)
select '12333334444','123456a','987654','张三','12345678910','2020-05-21 13:23:00' union
select '12344445555','123456a','987654','利达','12345678910','2020-05-21 13:24:00' union
select '12344445556','123456a','987654','乔治','12345678910','2020-05-21 13:25:00' 

--商品表 // 商品名 * 商品类型 * 商品分类(单位) * 花语 * 展示图 * 详情图 * 单价 * 运费
insert tb_GoodsInfo(GoodName,GoodType,GoodSort,FlowerLanguage,GoodMainImg,GoodDataImg,GoodPrice,GoodFreight)
select '1白色满天星','成花','1盆','满天星那素雅的小白花星星点点的点缀在浅绿色的枝叶中，玲珑细致、洁白无瑕的小花，松松散散的聚在一起，宛若无际夜空中的点点繁星。','/Images/GoodsInfo/一心一意.JPG','','23.5','8' union
select '2粉色满天星','成花','1盆','满天星那素雅的小白花星星点点的点缀在浅绿色的枝叶中，玲珑细致、洁白无瑕的小花，松松散散的聚在一起，宛若无际夜空中的点点繁星。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '3郁金香','成花','1盆','郁金香被视为胜利和美好的象征，同时它还代表着爱的表白和永恒的祝福。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '4向日葵','成花','1盆','向日葵不仅可以送给追求梦想的人，也可以送给你爱慕的对象，表示TA是你心中永不降落的太阳，是你永远守护的天使。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '5百合花','成花','1盆','在中国百合花具有象征百年好合、家庭美满、伟大的爱之含意，有深深祝福的意义。受到百合花的祝福的人具有清纯天真的性格，集众人宠爱于一身。','/Images/GoodsInfo/用心爱你.JPG','','23.5','8' union
select '6薰衣草','成花','1盆','薰衣草就是“香”的代表，有“花之精灵”之称。薰衣草的花语是等待爱情，薰衣草能之所以能受到时尚族群的青睐，是因为薰衣草还有许多浪漫美好的寓意。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '7康乃馨','成花','1盆','谢谢你的爱、真情、母亲我爱你、温馨的祝福、热爱着你、不求代价的母爱、亲情思念、伟大，神圣，慈祥，温馨的母亲、思念。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '8红玫瑰','成花','1盆','爱情，美好，祝福。玫瑰是用来表达爱情的通用语言。','/Images/GoodsInfo/一心一意.JPG','','23.5','8' union
select '9栀子花','成花','1盆','喜悦，永恒的爱与约定。不仅是爱情的寄予，平淡、持久、温馨、脱俗的外表下，蕴涵的，是美丽、坚韧、醇厚的生命本质。','/Images/GoodsInfo/初心不负.JPG','','23.5','8' union
select '10紫罗兰','成花','1盆','永恒的美，盛夏的清凉。凡是受到这种花祝福而诞生的人具有带给周遭的人爽朗的特质，纯纯的爱比较适合这样的你。','/Images/GoodsInfo/一心一意.JPG','','23.5','8'

--购物车表
insert tb_ShopCart(UserID,GoodID)
select 1,2 union
select 1,3 union
select 1,5 

