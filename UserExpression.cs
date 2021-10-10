using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using org.mariuszgromada.math.mxparser;
using ZedGraph;

namespace goldenratio
{
    class UserExpression
    {
        public double a { get; set; }
        public double b { get; set; }
        public double precision { get; set; }
        public string userExpression { get; set; }
        public List<PointPairList> minPointProgressList { get; set; }
        PointPairList list;
        int step = 1;
        double d = (Math.Sqrt(5) + 1) / 2;

        public Task<PointPairList> getGraphPoints()
        {
            PointPairList pointList = new PointPairList();
            return Task.Run(() => {
                for (double pointX = a; pointX <= b; pointX += step)
                {
                    double pointY = getPointY(pointX);
                    pointList.Add(new PointPair(pointX, pointY));
                }
                return pointList;
            });
        }
        public double getPointY(double pointX)
        {
            Argument x = new Argument("x = " + pointX);
            Expression expression = new Expression(userExpression, x);
            double pointY = expression.calculate();
            return pointY;
        }
        public Task<PointPairList> getMinPointCoords()
        {
            return Task.Run(() =>
            {
                double leftX = b - (b - a) / d;
                //double leftX = a + d;
                double leftY = getPointY(a);
                double rightX = a + (b - a) / d;
                //double rightX = b - d;
                double rightY = getPointY(b);
                Debug.WriteLine(String.Format("leftX={0}, leftY={1}, rightX={2}, rightY={3}, a={4}, b={5}", leftX, leftY, rightX, rightY, a, b));

                while (Math.Abs(rightY - leftY) > precision)
                {
                    if (leftY < rightY)
                    {
                        b = rightX;
                        rightX = leftX;
                        rightY = leftY;
                        leftX = b - (b - a) / d;
                        leftY = getPointY(leftX);
                        list = new PointPairList();
                        list.Add(new PointPair(leftX, leftY));
                        minPointProgressList.Add(list);
                    }
                    else
                    {
                        a = leftX;
                        leftX = rightX;
                        leftY = rightY;
                        rightX = a + (b - a) / d;
                        rightY = getPointY(rightX);
                        list = new PointPairList();
                        list.Add(new PointPair(leftX, leftY));
                        minPointProgressList.Add(list);
                    }

                    list = new PointPairList();
                    list.Add(new PointPair(leftX, leftY));
                    minPointProgressList.Add(list);
                    Debug.WriteLine(String.Format("leftX={0}, leftY={1}, rightX={2}, rightY={3}, a={4}, b={5}", leftX, leftY, rightX, rightY, a, b));

                }
                
                PointPairList minPointList = new PointPairList();
                minPointList.Add(new PointPair((b + a) / 2, getPointY((b + a) / 2)));
                return minPointList;
            });
        }
    }
}
