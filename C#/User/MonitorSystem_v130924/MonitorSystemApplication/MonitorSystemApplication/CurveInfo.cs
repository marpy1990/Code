using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;
using MonitorSystem;
using System.Drawing;
//using Forecast;

namespace MonitorSystemApplication
{
    class TimeDouble
    {
        public double value=0;
        public TimeDouble(double x)
        {
            value = x;
        }
    }

    class CurveInfo
    {
        public Monitor monitor;
        public PointPairList list;
        public LineItem curve;
        public Color color;
        public TimeDouble intrvalSeconds = new TimeDouble(1000);
        public double remainSeconds;
        public bool isPredict;
        public PointPairList trainList;
        public Predictor pd;
        public double totalValue = 0;
        public int nStat = 0;
        public int nSteps=5;

        public void AddValue(double value)
        {
            totalValue += value;
            nStat++;
        }

        public double PopMeanValue()
        {
            if (nStat == 0) return 0;
            else
            {
                double rst = totalValue / nStat;
                totalValue = 0;
                nStat = 0;
                return rst;
            }
        }

        public CurveInfo(Monitor monitor, PointPairList list, LineItem curve, Color color)
        {
            this.monitor = monitor;
            this.list = list;
            this.curve = curve;
            this.color = color;
            this.intrvalSeconds.value = 1000;
            this.remainSeconds = 1000;
            isPredict = false;
        }
        
        public CurveInfo(Monitor monitor, PointPairList list, LineItem curve, Color color, double interval)
        {
            this.monitor = monitor;
            this.list = list;
            this.curve = curve;
            this.color = color;
            this.intrvalSeconds.value = interval;
            this.remainSeconds = interval;
            isPredict = false;
        }
    }
}
