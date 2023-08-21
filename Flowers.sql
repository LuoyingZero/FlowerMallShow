create database FlowerDB
on
(
name='FlowerDB',
filename='E:\�ҵ��ļ�\__ѧϰ����\FlowerDB.mdf'
)
go

use FlowerDB
go
--�������̨��¼���жϵ��˺�
create table tb_FlowerAdmin
(
AdminID int primary key identity(1,1) not null,--id
AdminLoginName nvarchar(50) not null,	--��¼�û������˺š� �ֻ��Ż�����
AdminLoginPwd nvarchar(20) not null,	--��¼����
AdminName nvarchar(20),	--����
AdminTel nvarchar(20),	--�ֻ���
AdminSex nvarchar(5),	--�Ա�
AdminImg nvarchar(50) default('/Images/UserHead/headimg_def1.jpg'),--�û�ͷ��

)

--�û���
create table tb_UserInfo
(
UserID int primary key identity(1,1) not null,--id
UserLoginName nvarchar(50) not null,	--��¼�û������˺š� �ֻ��Ż�����
UserLoginPwd nvarchar(20) not null,		--��¼����
UserPayPwd nvarchar(6),	--֧������
UserName nvarchar(20),	--�ǳ�	��ʾ���ǳƣ��ɱ����û��˺���˽
UserTel nvarchar(11),	--�ֻ���
UserSex nvarchar(5),	--�Ա�
UserMark int default(0),--����
UserImg nvarchar(50) default('/Images/UserHead/headimg_def1.jpg'),--�û�ͷ��
UserCollection int default(0),	--�ղ���
UserShopCort int default(0),	--���ﳵ����
UserDatetime datetime,		--ע��ʱ��
UserState int default(0),	--�û��˺���� 0-���� | 1-��ʱ�޷���½ | 2-��ֹ��¼-����
--������Ϊ0�ͺ��Ӹ����ݣ������Ϊ0�ʹ�������ʱ��
SealTime datetime,
)

--��Ʒ��
create table tb_GoodsInfo(
GoodID int primary key identity(1,1) not null,--��Ʒid
GoodName nvarchar(50),		--��Ʒ����
GoodType nvarchar(50),		--��Ʒ���ͣ������ͨ��ƴ���ַ���  ���֣��ɻ����ܱ�
GoodSort nvarchar(50),		--��Ʒ���࣬1�裬10��
FlowerLanguage nvarchar(200),	--����
GoodMainImg nvarchar(200),		--��Ʒչʾͼ��
GoodDataImg nvarchar(200),		--��Ʒ����ͼ
GoodStock int default(200),		--��Ʒ���
GoodState int default(0),		--��Ʒ״̬| 0-���� | 1-ȱ�ٿ�� | 2-��ʱ�¼�
GoodComments int default(0),	--��Ʒ������������ʾ��Ŀ
GoodMsales int default(0),		--��Ʒ������
GoodCollection int default(0),	--�ղ���
GoodPrice decimal(10,2),		--��Ʒ����
GoodFreight int default(8),		--��Ʒ�˷�
)

--�û��ջ���ַ��һ���û������ж���ջ���ַ
create table tb_ReceAddress(
ReceID int primary key identity(1,1) not null,--�ջ���ַid
UserID int foreign key references tb_UserInfo(UserID),--�û�id���û������ж���ջ���ַ
ReceName nvarchar(20),	--�ջ�������
ReceAddr nvarchar(50),	--�ջ��˵�ַ
ReceTel nvarchar(11),	--�ջ����ֻ���
)

--�û��ղر�
create table tb_Collection(
CollecID int  primary key identity(1,1) not null,		--�ղر�id
UserID int foreign key references tb_UserInfo(UserID),	--�û�id���û������ж���ղ�
GoodID int ,--��Ʒid��������Ʒ���ܻ��¼ܣ��������ﲻҪ�����
GoodName nvarchar(20),		--��Ʒ��
GoodPrice decimal(10,2),	--��Ʒ����
GoodImg nvarchar(20),		--��ƷͼƬ������ȡ��һ�ţ�����չʾ
GoodState int default(0),	--��Ʒ״̬�� 0 ���� |  1 ���¼� 
)


--���ﳵ��
--�˱����ڼ�¼ĳλ�ͻ������й��ﳵ����Ʒ��Ϣ��ÿ����Ʒ����һ�����ݣ�
create table tb_ShopCart(
ShopID int primary key identity(1,1) not null,			--���ﳵid
UserID int foreign key references tb_UserInfo(UserID),	--�û�id���û����Խ������Ʒ���빺�ﳵ
GoodID int ,--��Ʒid��������Ʒ���ܻ��¼ܣ��������ﲻҪ�����
GoodName nvarchar(20),		--��Ʒ��
GoodPrice decimal(10,2),	--��Ʒ����
GoodSort nvarchar(50),		--��Ʒ���࣬1�裬10��
GoodInfo nvarchar(200),		--��Ʒ����
GoodNum int default(1),		--���빺�ﳵ����Ʒ����
GoodImg nvarchar(50),		--��ƷͼƬ������ȡ��һ�ţ�����չʾ
GoodState int default(0),	--��Ʒ״̬�� 0 ���� |  1 ���¼� 
)

--������
create table tb_Order(
OrderID int primary key identity(1,1) not null,	--����ID���û����ɼ�
OrderNumber nvarchar(20),	--������ţ�ͨ���㷨���ɵ�XΪ��ţ��û��ɼ�����ͨ���������������Ϣ
UserID int,		--�����������û�id�����ڶ���������Ҫ���ݣ����Բ�ʹ�����
ReceName nvarchar(20),	--�ջ�������
ReceAddr nvarchar(50),	--�ջ��˵�ַ
ReceTel nvarchar(11),	--�ջ����ֻ���
GoodFreight int default(0),	--��Ʒ�˷�
GoodNames nvarchar(500),	--��Ʒ��Ϣ
GoodDiscount nvarchar(50),	--��Ʒ�ۿ� 
OrderMoney nvarchar(20),	--�����ܽ�� ���㹫ʽ�� ���� * ���� + �ʷ� - �ۿ�
OrderState int default(0),	--����״̬ δ����Ѹ�������������ѷ��������ջ��������ջ��������ۣ����������
CreateTime datetime,		--��������ʱ��
PaymentTime datetime,		--����ʱ��
DeliverTime datetime,		--����ʱ��
ReceiveTime datetime,		--ȷ���ջ�ʱ��
UserVisible int default(0)	--�û��Ƿ�ɼ� 0-���� | 1-��ɾ��(�û����ɼ�)
)
--���������
create table tb_OrderSp(
DataID int primary key identity(1,1) not null,	
OrderID int foreign key references tb_Order(OrderID),	--����id���û����Խ������Ʒ���빺�ﳵ
GoodID int ,--��Ʒid������δ����Ʒ�����¼ܣ�����ͬ����ʹ�����
GoodName nvarchar(50),		--��Ʒ����
GoodImg nvarchar(200),		--��ƷͼƬ
GoodType nvarchar(50),		--��Ʒ���ͣ������ͨ��ƴ���ַ���  ���֣��ɻ����ܱ�
GoodSort nvarchar(50),		--��Ʒ���࣬1�裬10��
GoodPrice decimal(10,2),	--��Ʒ����
GoodNums int,				--��Ʒ����
GoodSum decimal(10,2)		--��Ʒ�ܼ�
)


--���Ѽ�¼��
create table tb_ConsumeData(
ConID int primary key identity(1,1) not null,	--��¼ID
UserID int,		--�û�id��
ConMoney decimal(10,2),		--���ѽ��
ConRemarks nvarchar(50),	--��ע��Ϣ
ConType int default(0),		--��¼����   0-���� | 1-�˿��
OrderNumber nvarchar(20),	--�������
ConTime datetime,			--��¼ʱ��
)

--��Ʒ���۱�
create table tb_Comments(
CommID int primary key identity(1,1) not null,	--����ID
GoodID int foreign key references tb_GoodsInfo(GoodID),--��ƷID
GoodSort nvarchar(50),		--��Ʒ���࣬1�裬10��
UserID int foreign key references tb_UserInfo(UserID),--�û�id
UserName nvarchar(20),		--�û��ǳ�  
CommText nvarchar(100),		--��������
CommTime date,				--����ʱ��
CommImg nvarchar(20),		--����ͼƬ
ManagerComm nvarchar(50),	--��һظ�
Fabulous int default(0),	--������
)


--���Ĭ������
--����� // ��¼�˺� * ��¼���� * �û��ǳ� * �ֻ��� * �Ա�
insert tb_FlowerAdmin(AdminLoginName,AdminLoginPwd,AdminName,AdminTel,AdminSex) values('2457879996','admin123456','������','24578799960','��')

--�û��� // ��¼�˺� * ��¼���� * ֧������ * �ǳ� * �ֻ��� * ע��ʱ��
insert tb_UserInfo(UserLoginName,UserLoginPwd,UserPayPwd,UserName,UserTel,UserDatetime)
select '12333334444','123456a','987654','����','12345678910','2020-05-21 13:23:00' union
select '12344445555','123456a','987654','����','12345678910','2020-05-21 13:24:00' union
select '12344445556','123456a','987654','����','12345678910','2020-05-21 13:25:00' 

--��Ʒ�� // ��Ʒ�� * ��Ʒ���� * ��Ʒ����(��λ) * ���� * չʾͼ * ����ͼ * ���� * �˷�
insert tb_GoodsInfo(GoodName,GoodType,GoodSort,FlowerLanguage,GoodMainImg,GoodDataImg,GoodPrice,GoodFreight)
select '1��ɫ������','�ɻ�','1��','�����������ŵ�С�׻����ǵ��ĵ�׺��ǳ��ɫ��֦Ҷ�У�����ϸ�¡������覵�С��������ɢɢ�ľ���һ�������޼�ҹ���еĵ�㷱�ǡ�','/Images/GoodsInfo/һ��һ��.JPG','','23.5','8' union
select '2��ɫ������','�ɻ�','1��','�����������ŵ�С�׻����ǵ��ĵ�׺��ǳ��ɫ��֦Ҷ�У�����ϸ�¡������覵�С��������ɢɢ�ľ���һ�������޼�ҹ���еĵ�㷱�ǡ�','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '3������','�ɻ�','1��','�����㱻��Ϊʤ�������õ�������ͬʱ���������Ű��ı�׺������ף����','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '4���տ�','�ɻ�','1��','���տ����������͸�׷��������ˣ�Ҳ�����͸��㰮Ľ�Ķ��󣬱�ʾTA�����������������̫����������Զ�ػ�����ʹ��','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '5�ٺϻ�','�ɻ�','1��','���й��ٺϻ�������������úϡ���ͥ������ΰ��İ�֮���⣬������ף�������塣�ܵ��ٺϻ���ף�����˾����崿������Ը񣬼����˳谮��һ��','/Images/GoodsInfo/���İ���.JPG','','23.5','8' union
select '6޹�²�','�ɻ�','1��','޹�²ݾ��ǡ��㡱�Ĵ����С���֮���顱֮�ơ�޹�²ݵĻ����ǵȴ����飬޹�²���֮�������ܵ�ʱ����Ⱥ������������Ϊ޹�²ݻ�������������õ�Ԣ�⡣','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '7����ܰ','�ɻ�','1��','лл��İ������顢ĸ���Ұ��㡢��ܰ��ף�����Ȱ����㡢������۵�ĸ��������˼�ΰ����ʥ�����飬��ܰ��ĸ�ס�˼�','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '8��õ��','�ɻ�','1��','���飬���ã�ף����õ����������ﰮ���ͨ�����ԡ�','/Images/GoodsInfo/һ��һ��.JPG','','23.5','8' union
select '9���ӻ�','�ɻ�','1��','ϲ�ã�����İ���Լ���������ǰ���ļ��裬ƽ�����־á���ܰ�����׵�����£��̺��ģ������������͡�������������ʡ�','/Images/GoodsInfo/���Ĳ���.JPG','','23.5','8' union
select '10������','�ɻ�','1��','���������ʢ�ĵ������������ܵ����ֻ�ף�����������˾��д����������ˬ�ʵ����ʣ������İ��Ƚ��ʺ��������㡣','/Images/GoodsInfo/һ��һ��.JPG','','23.5','8'

--���ﳵ��
insert tb_ShopCart(UserID,GoodID)
select 1,2 union
select 1,3 union
select 1,5 

