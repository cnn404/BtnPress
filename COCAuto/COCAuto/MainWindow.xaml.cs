using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace COCAuto
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string subPic = @"..\..\Assets\enemy_num\3.png";
            //string parPic = @"C:\Users\JimmyBright\Desktop\搜鱼.png";


            //string subPic = @"C:\Users\JimmyBright\Desktop\2.bmp";
            //string parPic = @"C:\Users\JimmyBright\Desktop\22_black.bmp";
            //System.Drawing.Rectangle rect = new System.Drawing.Rectangle(40, 85, 165, 20);
            ////rect = System.Drawing.Rectangle.Empty;
            //var point = FindText(subPic, parPic, rect, 0, 0.8, true);


            //var point1 = FindColor(parPic, "#412a50");
            var dic = new Dictionary<int, string>();
            var rec = new System.Drawing.Rectangle(40, 85, 165, 20);
            //var rec = new System.Drawing.Rectangle(407, 259, 121, 27);

            var myPic = @"C:\Users\JimmyBright\Desktop\228234.bmp";
            for (int i = 0; i < 10; i++)
            {
                dic.Add(i, string.Format(@"..\..\Assets\enemy_num\enemy_coin\{0}.bmp", i));
            }
            var num = FindNumbers(dic, myPic, rec, 10, 0.85);
            var tt = FindText(dic[0], myPic, rec, 10, 0.65, true);
        }
        struct NumBody
        {
            public int num;//数字
            public int matchNum;//匹配的个数
            public int matchSum;
            public double matchRate;//匹配度
            public System.Drawing.Point point;
            public List<System.Drawing.Point> bodyCollectionPoint;//该数字所有像素在大图中的坐标
        }
        #region 找图

        /// <summary>
        /// 查找图片，不能镂空
        /// </summary>
        /// <param name="subPic"></param>
        /// <param name="parPic"></param>
        /// <param name="searchRect">如果为empty，则默认查找整个图像</param>
        /// <param name="errorRange">容错，单个色值范围内视为正确0~255</param>
        /// <param name="matchRate">图片匹配度，默认90%</param>
        /// <param name="isFindAll">是否查找所有相似的图片</param>
        /// <returns>返回查找到的图片的中心点坐标</returns>
        List<System.Drawing.Point> FindPicture(string subPic, string parPic, System.Drawing.Rectangle searchRect, byte errorRange, double matchRate = 0.9, bool isFindAll = false)
        {
            List<System.Drawing.Point> ListPoint = new List<System.Drawing.Point>();
            var subBitmap = new Bitmap(subPic);
            var parBitmap = new Bitmap(parPic);
            int subWidth = subBitmap.Width;
            int subHeight = subBitmap.Height;
            int parWidth = parBitmap.Width;
            int parHeight = parBitmap.Height;
            if (searchRect.IsEmpty)
            {
                searchRect = new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height);
            }

            var searchLeftTop = searchRect.Location;
            var searchSize = searchRect.Size;
            System.Drawing.Color startPixelColor = subBitmap.GetPixel(0, 0);
            var subData = subBitmap.LockBits(new System.Drawing.Rectangle(0, 0, subBitmap.Width, subBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var parData = parBitmap.LockBits(new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var byteArrarySub = new byte[subData.Stride * subData.Height];
            var byteArraryPar = new byte[parData.Stride * parData.Height];
            Marshal.Copy(subData.Scan0, byteArrarySub, 0, subData.Stride * subData.Height);
            Marshal.Copy(parData.Scan0, byteArraryPar, 0, parData.Stride * parData.Height);

            var iMax = searchLeftTop.Y + searchSize.Height - subData.Height;//行
            var jMax = searchLeftTop.X + searchSize.Width - subData.Width;//列

            int smallOffsetX = 0, smallOffsetY = 0;
            int smallStartX = 0, smallStartY = 0;
            int pointX = -1; int pointY = -1;
            for (int i = searchLeftTop.Y; i < iMax; i++)
            {
                for (int j = searchLeftTop.X; j < jMax; j++)
                {
                    //大图x，y坐标处的颜色值
                    int x = j, y = i;
                    int parIndex = i * parWidth * 4 + j * 4;
                    var colorBig = System.Drawing.Color.FromArgb(byteArraryPar[parIndex + 3], byteArraryPar[parIndex + 2], byteArraryPar[parIndex + 1], byteArraryPar[parIndex]);
                    ;
                    if (ColorAEqualColorB(colorBig, startPixelColor, errorRange))
                    {
                        smallStartX = x - smallOffsetX;//待找的图X坐标
                        smallStartY = y - smallOffsetY;//待找的图Y坐标
                        int sum = 0;//所有需要比对的有效点
                        int matchNum = 0;//成功匹配的点
                        for (int m = 0; m < subHeight; m++)
                        {
                            for (int n = 0; n < subWidth; n++)
                            {
                                int x1 = n, y1 = m;
                                int subIndex = m * subWidth * 4 + n * 4;
                                var color = System.Drawing.Color.FromArgb(byteArrarySub[subIndex + 3], byteArrarySub[subIndex + 2], byteArrarySub[subIndex + 1], byteArrarySub[subIndex]);

                                sum++;
                                int x2 = smallStartX + x1, y2 = smallStartY + y1;
                                int parReleativeIndex = y2 * parWidth * 4 + x2 * 4;//比对大图对应的像素点的颜色
                                var colorPixel = System.Drawing.Color.FromArgb(byteArraryPar[parReleativeIndex + 3], byteArraryPar[parReleativeIndex + 2], byteArraryPar[parReleativeIndex + 1], byteArraryPar[parReleativeIndex]);
                                if (ColorAEqualColorB(colorPixel, color, errorRange))
                                {
                                    matchNum++;
                                }
                            }
                        }
                        if ((double)matchNum / sum >= matchRate)
                        {
                            Console.WriteLine((double)matchNum / sum);
                            pointX = smallStartX + (int)(subWidth / 2.0);
                            pointY = smallStartY + (int)(subHeight / 2.0);
                            var point = new System.Drawing.Point(pointX, pointY);
                            if (!ListContainsPoint(ListPoint, point, 10))
                            {
                                ListPoint.Add(point);
                            }
                            if (!isFindAll)
                            {
                                goto FIND_END;
                            }
                        }
                    }
                    //小图x1,y1坐标处的颜色值
                }
            }
        FIND_END:
            subBitmap.UnlockBits(subData);
            parBitmap.UnlockBits(parData);
            subBitmap.Dispose();
            parBitmap.Dispose();
            GC.Collect();
            return ListPoint;
        }
        #endregion

        #region 找字
        //struct TextFindBody
        //{
        //    public System.Drawing.Point TextPoint;
        //    public List<System.Drawing.Point> ListPointsOnBigPic;
        //}
        /// <summary>
        /// 找文字，镂空的图片文字
        /// </summary>
        /// <param name="subPic"></param>
        /// <param name="parPic"></param>
        /// <param name="searchRect"></param>
        /// <param name="errorRange"></param>
        /// <param name="matchRate"></param>
        /// <param name="isFindAll"></param>
        /// <returns></returns>
        List<NumBody> FindText(string subPic, string parPic, System.Drawing.Rectangle searchRect, byte errorRange, double matchRate = 0.9, bool isFindAll = false)
        {

            List<NumBody> ListPoint = new List<NumBody>();
            var subBitmap = new Bitmap(subPic);
            var parBitmap = new Bitmap(parPic);
            int subWidth = subBitmap.Width;
            int subHeight = subBitmap.Height;
            int parWidth = parBitmap.Width;
            int parHeight = parBitmap.Height;
            var bgColor = subBitmap.GetPixel(0, 0);//背景红色
            if (searchRect.IsEmpty)
            {
                searchRect = new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height);
            }
            var searchLeftTop = searchRect.Location;
            var searchSize = searchRect.Size;
            var subData = subBitmap.LockBits(new System.Drawing.Rectangle(0, 0, subBitmap.Width, subBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var parData = parBitmap.LockBits(new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var byteArrarySub = new byte[subData.Stride * subData.Height];
            var byteArraryPar = new byte[parData.Stride * parData.Height];
            Marshal.Copy(subData.Scan0, byteArrarySub, 0, subData.Stride * subData.Height);
            Marshal.Copy(parData.Scan0, byteArraryPar, 0, parData.Stride * parData.Height);
            var iMax = searchLeftTop.Y + searchSize.Height - subData.Height;//行
            var jMax = searchLeftTop.X + searchSize.Width - subData.Width;//列
            System.Drawing.Color startPixelColor = System.Drawing.Color.FromArgb(0, 0, 0);
            int smallOffsetX = 0, smallOffsetY = 0;
            int smallStartX = 0, smallStartY = 0;
            int pointX = -1; int pointY = -1;


            for (int m = 0; m < subHeight; m++)
            {
                for (int n = 0; n < subWidth; n++)
                {
                    smallOffsetX = n;
                    smallOffsetY = m;
                    int subIndex = m * subWidth * 4 + n * 4;
                    var color = System.Drawing.Color.FromArgb(byteArrarySub[subIndex + 3], byteArrarySub[subIndex + 2], byteArrarySub[subIndex + 1], byteArrarySub[subIndex]);
                    if (!ColorAEqualColorB(color, bgColor, errorRange))
                    {
                        startPixelColor = color;
                        goto END;
                    }
                }
            }

        END:
            for (int i = searchLeftTop.Y; i < iMax; i++)
            {
                for (int j = searchLeftTop.X; j < jMax; j++)
                {
                    //大图x，y坐标处的颜色值
                    int x = j, y = i;
                    int parIndex = i * parWidth * 4 + j * 4;
                    var colorBig = System.Drawing.Color.FromArgb(byteArraryPar[parIndex + 3], byteArraryPar[parIndex + 2], byteArraryPar[parIndex + 1], byteArraryPar[parIndex]);
                    ;

                    List<System.Drawing.Point> myListPoint = new List<System.Drawing.Point>();
                    if (ColorAEqualColorB(colorBig, startPixelColor, errorRange))
                    {
                        smallStartX = x - smallOffsetX;//待找的图X坐标
                        smallStartY = y - smallOffsetY;//待找的图Y坐标
                        int sum = 0;//所有需要比对的有效点
                        int matchNum = 0;//成功匹配的点
                        for (int m = 0; m < subHeight; m++)
                        {
                            for (int n = 0; n < subWidth; n++)
                            {
                                int x1 = n, y1 = m;
                                int subIndex = m * subWidth * 4 + n * 4;
                                var color = System.Drawing.Color.FromArgb(byteArrarySub[subIndex + 3], byteArrarySub[subIndex + 2], byteArrarySub[subIndex + 1], byteArrarySub[subIndex]);
                                if (color != bgColor)
                                {
                                    sum++;
                                    int x2 = smallStartX + x1, y2 = smallStartY + y1;
                                    int parReleativeIndex = y2 * parWidth * 4 + x2 * 4;//比对大图对应的像素点的颜色
                                    var colorPixel = System.Drawing.Color.FromArgb(byteArraryPar[parReleativeIndex + 3], byteArraryPar[parReleativeIndex + 2], byteArraryPar[parReleativeIndex + 1], byteArraryPar[parReleativeIndex]);
                                    if (ColorAEqualColorB(colorPixel, color, errorRange))
                                    {
                                        matchNum++;
                                    }
                                    myListPoint.Add(new System.Drawing.Point(x2, y2));
                                }
                            }
                        }

                        double rate = (double)matchNum / sum;
                        if (rate>= matchRate)
                        {
                            Console.WriteLine((double)matchNum / sum);
                            pointX = smallStartX + (int)(subWidth / 2.0);
                            pointY = smallStartY + (int)(subHeight / 2.0);
                            var point = new System.Drawing.Point(pointX, pointY);
                            if (!ListTextBodyContainsPoint(ListPoint, point, 1))
                            {
                                ListPoint.Add(new NumBody() { point = point, matchNum = matchNum,matchSum=sum, matchRate = rate, bodyCollectionPoint = myListPoint });
                            }
                            SearchNumbersByMatchNum(ref ListPoint);
                            if (!isFindAll)
                            {
                                goto FIND_END;
                            }
                        }
                    }
                    //小图x1,y1坐标处的颜色值
                }
            }
        FIND_END:
            subBitmap.UnlockBits(subData);
            parBitmap.UnlockBits(parData);
            subBitmap.Dispose();
            parBitmap.Dispose();
            GC.Collect();
            return ListPoint;
        }
        bool ColorAEqualColorB(System.Drawing.Color colorA, System.Drawing.Color colorB, byte errorRange = 10)
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;
          
        }
        bool ListContainsPoint(List<System.Drawing.Point> listPoint, System.Drawing.Point point, double errorRange = 10)
        {
            bool isExist = false;
            foreach (var item in listPoint)
            {
                if (item.X <= point.X + errorRange && item.X >= point.X - errorRange && item.Y <= point.Y + errorRange && item.Y >= point.Y - errorRange)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
        bool ListTextBodyContainsPoint(List<NumBody> listPoint, System.Drawing.Point point, double errorRange = 10)
        {
            bool isExist = false;
            foreach (var item in listPoint)
            {

                if (item.point.X <= point.X + errorRange && item.point.X >= point.X - errorRange && item.point.Y <= point.Y + errorRange && item.point.Y >= point.Y - errorRange)
                {
                    isExist = true;
                }
            }
            return isExist;
        }
        #endregion

        #region 找色

        /// <summary>
        /// 查找颜色
        /// </summary>
        /// <param name="parPic"></param>
        /// <param name="searchColor">#412a50</param>
        /// <returns></returns>
        System.Drawing.Point FindColor(string parPic, string searchColor, System.Drawing.Rectangle searchRect, byte errorRange = 10)
        {
            var colorX = System.Drawing.ColorTranslator.FromHtml(searchColor);
            var parBitmap = new Bitmap(parPic);
            var parData = parBitmap.LockBits(new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var byteArraryPar = new byte[parData.Stride * parData.Height];
            Marshal.Copy(parData.Scan0, byteArraryPar, 0, parData.Stride * parData.Height);
            if (searchRect.IsEmpty)
            {
                searchRect = new System.Drawing.Rectangle(0, 0, parBitmap.Width, parBitmap.Height);
            }
            var searchLeftTop = searchRect.Location;
            var searchSize = searchRect.Size;
            var iMax = searchLeftTop.Y + searchSize.Height;//行
            var jMax = searchLeftTop.X + searchSize.Width;//列
            int pointX = -1; int pointY = -1;
            for (int m = searchRect.Y; m < iMax; m++)
            {
                for (int n = searchRect.X; n < jMax; n++)
                {
                    int index = m * parBitmap.Width * 4 + n * 4;
                    var color = System.Drawing.Color.FromArgb(byteArraryPar[index + 3], byteArraryPar[index + 2], byteArraryPar[index + 1], byteArraryPar[index]);
                    if (ColorAEqualColorB(color, colorX, errorRange))
                    {
                        pointX = n;
                        pointY = m;
                        goto END;
                    }
                }
            }
        END:
            parBitmap.UnlockBits(parData);
            return new System.Drawing.Point(pointX, pointY);
        }
        #endregion


        #region 查找数字
        
        /// <summary>
        /// 在指定区域里面查找数字
        /// </summary>
        /// <param name="numDic"></param>
        /// <param name="parPic"></param>
        /// <param name="searchRect"></param>
        /// <param name="errorRange"></param>
        /// <returns></returns>
        int FindNumbers(Dictionary<int, string> numDic, string parPic, System.Drawing.Rectangle searchRect, byte errorRange=8, double matchRate = 0.9)
        {
            //同一个区域找到多个相同的图片
            List<NumBody> ListBody = new List<NumBody>();
            foreach (var item in numDic)
            {
                var listPoint = FindText(item.Value, parPic, searchRect, errorRange, matchRate, true);
                foreach (var point in listPoint)
                {
                    ListBody.Add(new NumBody() { num = item.Key,matchNum=point.matchNum,matchSum=point.matchSum, matchRate=point.matchRate, point = point.point, bodyCollectionPoint = point.bodyCollectionPoint });
                }
            }

            SearchNumbersByMatchNum(ref ListBody);
            var myList = from body in ListBody orderby body.point.X ascending select body;
            string number = "0";
            foreach (var item in myList)
            {
                number += item.num;
            }
            int num = Int32.Parse(number);
            return num;
        }
       /// <summary>
       /// 搜索同一个数字的时候，出现重叠的地方，用匹配度去过滤掉匹配度低的
       /// 比如同样是1，在控制匹配度允许下，一个（83,95）和（84,95）这两个点明显是同一个数字
       /// 此时谁的匹配度低过滤掉谁
       /// </summary>
       /// <param name="ListBody"></param>
        void SearchNumbersByMatchNum(ref List<NumBody> ListBody)
        {
            bool isValid = true;
            for (int i = 0; i < ListBody.Count; i++)
            {
                var body = ListBody[i];
                
                for (int j = i; j < ListBody.Count; j++)
                {

                    var bodyX = ListBody[j];
                    if (!bodyX.Equals(body))
                    {
                        int sameNum = 0;
                        foreach (var item in body.bodyCollectionPoint)
                        {
                            if (bodyX.bodyCollectionPoint.Contains(item))
                            {
                                sameNum++;
                            }
                        }
                        if (sameNum >= 1)//有1个以上点重合，表面图像重叠，删除像素点数少的图像
                        {
                            isValid = false;

                            //如果某个数字100%匹配，那就不用比较了，这个数字肯定是对的
                            double maxRate = 1;
                            if (bodyX.matchRate >= maxRate)
                            {
                                ListBody.Remove(body);
                            }
                            else if (body.matchRate>=maxRate)
                            {
                                ListBody.Remove(bodyX);
                            }
                            else
                            {
                                if (bodyX.matchNum >= body.matchNum)//图像包含的所有像素个数
                                {
                                    ListBody.Remove(body);
                                }
                                else
                                {
                                    ListBody.Remove(bodyX);
                                }
                            }
                            SearchNumbersByMatchNum(ref ListBody);
                        }
                    }
                }
            }
            if (isValid)
            {
                return;
            }
        }

        #endregion

    }
}
