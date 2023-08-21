using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace FlowerMall
{
    /// <summary>
    /// yzCode 的摘要说明
    /// </summary>
    public class yzCode : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "image/gif";
            //1.设置生成验证码字符的源字符串
            string strWord = "QZdqsSWXwED34aAerC8tcyg9zR52KvVxuT643C24Y9hbFB8U7567NJn453jimI723kGHpLMP";
            Random rand = new Random();//随机数
            string numStr = "";
            for (int i = 0; i < 4; i++)
            {
                //2.根据随机数从源字符串中抽取字符，并合并到生成的验证码中
                int index = rand.Next(0, strWord.Length);
                numStr += strWord[index];
            }
            //3.将生成的验证码字符存储到session对象中
            context.Session["yzmcode"] = numStr.ToLower();
            //4.根据验证码字符串生成对应的图片，并将图片输出到前台
            CreateImage(context, numStr);//这是一个方法 
        }
        private void CreateImage(HttpContext context, string yzmStr)
        {
            //创建一个验证码图片
            int imgWidth = yzmStr.Length * 20;
            Bitmap img = new Bitmap(imgWidth, 35);
            //根据图片生成绘图对象
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.White);
            //随机的颜色与字体
            Random r = new Random();
            Color[] c = { Color.Red, Color.Black, Color.Green, Color.Blue, Color.Orange };
            string[] str = { "宋体", "黑体", "等线", "微软雅黑" };
            for (int i = 0; i < yzmStr.Length; i++)
            {
                //随机生成颜色与字体
                int cIndex = r.Next(c.Length);
                int sIndex = r.Next(str.Length);
                Font f = new Font(str[sIndex], 14, FontStyle.Bold);
                Brush b = new SolidBrush(c[cIndex]);
                int y = 8;
                if ((i + 1) % 2 == 0)
                {
                    y = 5;
                }
                g.DrawString(yzmStr.Substring(i, 1), f, b, 2 + (i * 15), y);
            }
            g.DrawRectangle(new Pen(Color.Gray), 0, 0, img.Width - 1, img.Height - 1);
            //输出图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            context.Response.BinaryWrite(ms.ToArray());
            //销毁对象
            g.Dispose();
            img.Dispose();
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}