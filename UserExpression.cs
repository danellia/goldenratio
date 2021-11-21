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
        int step = 1;
        double d = (Math.Sqrt(5) - 1) / 2;

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
        public Task<List<PointPairList>> getMinPointCoords()
        {
            return Task.Run(() =>
            {
                double leftX, rightX;
                List<PointPairList> minPointProgressList = new List<PointPairList>();
                while (true)
                {
                    leftX = b - (b - a) * d;
                    rightX = a + (b - a) * d;
                    if (getPointY(leftX) >= getPointY(rightX))
                    {
                        a = leftX;
                        addPointPair(leftX, minPointProgressList);
                    }
                    else
                    {
                        b = rightX;
                        addPointPair(rightX, minPointProgressList);
                    }
                    if (Math.Abs(b - a) < precision)
                        break;
                }
                addPointPair(round((b + a) / 2), minPointProgressList);
                return minPointProgressList;
            });
        }
        private void addPointPair(double coord, List<PointPairList> progressList)
        {
            PointPairList list = new PointPairList();
            list.Add(new PointPair(coord, getPointY(coord)));
            progressList.Add(list);
            //Debug.WriteLine("addpair" + list[0].X + " " + list[0].Y + " " + progressList[progressList.Count - 1][0].X);
        }
        private double round(double coord)
        {
            return Math.Round(coord, precision.ToString().Length - 2);
        }
    }
}
