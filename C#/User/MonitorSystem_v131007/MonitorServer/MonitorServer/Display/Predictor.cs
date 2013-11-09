using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorSystemApplication
{

    public class Predictor
    {
        public List<Unit> units = new List<Unit>();

        public int selector = 0;

        public bool auto = true;

        List<double> list;


        public Predictor(int step, List<double> lst)
        {
            list = lst;
            units.Add(new KNN(step, lst));
            units.Add(new ES(lst, 0.5, 10));
        }


        public void Add(double value)
        {
            list.Add(value);
        }

        public double Next(int index)
        {
            double rst=0;
            for (int i = 0; i < units.Count; ++i)
            {
                if (i == selector) rst = units[i].Next(index);
                else units[i].Next(index);
            }
            return rst;
        }

        public double MRE()
        {
            double minMRE=100000;
            int s = 0;
            
            if (auto)
            {
                for(int i=0 ;i<units.Count; ++i)
                {
                    double m=units[i].MRE();
                    if ( m< minMRE)
                    {
                        minMRE = m;
                        s = i;
                    }
                }
                selector = s;
            }
            return units[selector].MRE();
        }
    }

    public class Unit
    {
        public virtual void Add(double value)
        {
        }

        public virtual double Next(int index)
        {
            return 0;
        }

        public virtual double MRE()
        {
            return 0;
        }
    }

    public class KNN:Unit
    {
        public int step=10;

        public int dim=7;

        List<double> lst;

        Dictionary<int, double> dict=new Dictionary<int,double>();

        public KNN(int dim)
        {
            this.step = dim;
            this.dim=dim;
            lst=new List<double>();
        }

        public KNN(int dim, List<double> lst)
        {
            this.step = dim;
            this.dim=dim;
            this.lst=lst;
        }

        public void SetDim(int n)
        {
            dim=n;
        }

        public override void Add(double value)
        {
            lst.Add(value);
        }

        public override double Next(int index)
        {
            double rst=0;
            double minDst=-1;
            int tail = lst.Count-step-dim+1;
            for (int i = 0; i < tail; ++i)
            {
                double dst = 0;
                for (int j = 0; j < dim; ++j)
                {
                    dst += (lst[i + j] - lst[tail + j]) * (lst[i + j] - lst[tail + j]);
                }

                if (minDst == -1)
                {
                    minDst = dst;
                    rst = lst[i + dim + index - 1];
                }
                else if (dst < minDst)
                {
                    minDst = dst;
                    rst = lst[i + dim + index-1];
                }
            }
            if(minDst!=-1)
                dict.Add(lst.Count - 1 + index, rst);
            return rst;
        }

        public override double MRE()
        {
            double mre = 0;
            int n = 0;
            foreach (int index in dict.Keys)
            {
                if (index < lst.Count && index>lst.Count-10)
                {
                    mre += Math.Abs(dict[index] - lst[index]) / (lst[index]+0.01);
                    n++;
                }
            }
            if (n > 0)
                mre = mre / n;
            return mre;
        }
    }

    public class ES : Unit
    {
        public double s;
        public int p;

        List<double> list;

        Dictionary<int, double> dict = new Dictionary<int, double>();

        public ES(List<double> list, double s, int p)
        {
            this.s = s;
            this.p = p;
            this.list=list;
        }

        public override void Add(double item)
        {
            list.Add(item);
        }

        public override double Next(int index)
        {
            double result = 0;
            double tmp = 1;

            try
            {
                result += list[list.Count - 1];
                for (int i = 1; i < p; ++i)
                {
                    tmp *= 1 - s;
                    result += (tmp * list[list.Count - i - 1]);
                }
                result *= s;
                dict.Add(list.Count - 1 + index, result);
                return result;
            }
            catch
            {
                return 0;
            }
        }

        public override double MRE()
        {
            double mre = 0;
            int n = 0;
            foreach (int index in dict.Keys)
            {
                if (index < list.Count && index > list.Count - 10)
                {
                    mre += Math.Abs(dict[index] - list[index]) / (list[index]+0.01);
                    n++;
                }
            }
            if (n > 0)
                mre = mre / n;
            return mre;
        }
    }

    /*
    public class PdInfo
    {
        public SubPredictor pd;
        public double score;
        public PdInfo(SubPredictor pd, double score)
        {
            this.pd = pd;
            this.score = score;
        }
    }
    
    public class Predictor
    {
        public PdInfo ppd;

        List<PdInfo> Set = new List<PdInfo>();

        int nPd = 0;

        double mre = 0;

        double last = 0;

        List<double> list;

        public Predictor(List<double> list)
        {
            Random ran = new Random();
            this.list = list;
            //WNN
            for (int i = 0; i < 3; ++i)
            {
                WNN p = new WNN(list, ran.Next(5, 20));
                Set.Add(new PdInfo(p, 0));
            }


            for (int i = 0; i < 3; ++i)
            {
                INV p = new INV(list, ran.Next(10, 30));
                Set.Add(new PdInfo(p, 0));
            }


            //ES

            for (int i = 0; i < 3; ++i)
            {
                ES p = new ES(list, ran.NextDouble(), ran.Next(5, 20));
                Set.Add(new PdInfo(p, 0));
            }

            ppd = Set[ran.Next(0, 9)];
        }

        public void Add(double item)
        {
            Random ran = new Random();

            foreach (PdInfo pdInf in Set)
            {
                if (Math.Abs(item - pdInf.pd.Last()) / (item + 1) < 0.1)
                {
                    pdInf.score += 1;
                }
                else
                {
                    pdInf.score -= 1;
                }

                if (pdInf.score > 3)
                {
                    ppd = pdInf;
                }
                else if (pdInf.score < -3)
                {
                    if (pdInf.pd is WNN)
                    {
                        SubPredictor s = new WNN(list, ran.Next(5, 20));
                        pdInf.pd = s;
                        pdInf.score = 0;
                    }
                    else if (pdInf.pd is INV)
                    {
                        SubPredictor s = new INV(list, ran.Next(10, 30));
                        pdInf.pd = s;
                        pdInf.score = 0;
                    }
                    else if (pdInf.pd is ES)
                    {
                        SubPredictor s = new ES(list, ran.NextDouble(), ran.Next(5, 20));
                        pdInf.pd = s;
                        pdInf.score = 0;
                    }

                    if (pdInf == ppd)
                    {
                        int index = 0;
                        double score = 0;
                        for (int i = 0; i < Set.Count; ++i)
                        {
                            if (Set[i].score > score)
                            {
                                index = i;
                                score = Set[i].score;
                            }
                        }
                        ppd = Set[index];
                    }
                }

                pdInf.pd.Add(item);
            }

            mre = (mre * nPd + Math.Abs(last - item) / (item + 1)) / (++nPd);
        }

        public double Next()
        {
            last = ppd.pd.Next();
            return last;
        }

        public double MRE()
        {
            return mre;
        }

        public double Last()
        {
            return last;
        }

    }


    
    public class SubPredictor
    {
        protected List<double> list;

        protected double last;

        public SubPredictor(List<double> list)
        {
            this.list = list;
        }

        public virtual void Add(double item)
        {
        }

        public virtual double Next()
        {
            return 0;
        }

        public virtual double Last()
        {
            return last;
        }
    }

    class WNN : SubPredictor
    {
        protected int k;

        public WNN(List<double> list, int k)
            : base(list)
        {
            this.k = k;
        }

        public override void Add(double item)
        {
            list.Add(item);
        }

        public override double Next()
        {
            double sum = 0;
            for (int i = list.Count - 1, j = 0; j < k; ++j, --i)
            {
                sum += list[i];
            }
            last = sum / k;
            return last;
        }

    }

    class WNND : SubPredictor
    {
        private List<double> a = new List<double>(5);
        public int k = 5;
        public void setk(int new_k)
        {
            if (k < new_k)
            {
                List<double> temp = new List<double>(new_k);
                for (int i = 0; i < a.Count; i++)
                {
                    temp.Add(a[i]);
                }
                a.Clear();
                a = temp;
            }
            if (k > new_k)
            {
                a.RemoveRange(0, k - new_k);
                a.TrimExcess();
            }
            k = new_k;
        }
        public WNND(List<double> list, int k)
            : base(list)
        {
            this.setk(k);
        }

        public override void Add(double data)
        {
            if (a.Contains(data))
            {
                a.Remove(data);
                a.Add(data);
            }
            else
            {
                if (a.Count < a.Capacity)
                {
                    a.Add(data);
                }
                else
                {
                    a.RemoveAt(0);
                    a.Add(data);
                }
            }
        }

        public override double Next()
        {
            double next = 0.0;
            double sum = 0.0;
            for (int i = 0; i < a.Count; i++)
            {
                sum += a[i];
            }
            next = sum / a.Count;
            last = next;
            return next;
        }

    }

    class ES : SubPredictor
    {
        protected double s;
        protected int p;

        public ES(List<double> list, double s, int p)
            : base(list)
        {
            this.s = s;
            this.p = p;
        }

        public override void Add(double item)
        {
            list.Add(item);
        }

        public override double Next()
        {
            double result = 0;
            double tmp = 1;


            result += list[list.Count - 1];
            for (int i = 1; i < p; ++i)
            {
                tmp *= 1 - s;
                result += (tmp * list[list.Count - i - 1]);
            }
            result *= s;
            last = result;
            return result;
        }
    }

    class INV : SubPredictor
    {
        protected double sum;
        protected double value = 0;

        public INV(List<double> list, double sum)
            : base(list)
        {
            this.sum = sum;
        }

        public override void Add(double item)
        {
            list.Add(item);
            value = item;
        }

        public override double Next()
        {
            last = sum - value;
            return last;
        }
    }*/
}

