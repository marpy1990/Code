using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forecast
{
    class PdInfo
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
        SubPredictor pd;

        List<PdInfo> Set=new List<PdInfo>();

        int nPd = 0;

        double mre = 0;

        double last = 0;

        List<double> list;

        public Predictor(List<double> list)
        {
            Random ran=new Random();
            this.list = list;
            //WNN
            for (int i = 0; i < 3; ++i)
            {
                WNN p = new WNN(list, ran.Next(5, 20));
                Set.Add(new PdInfo(p, 0));
            }

            
            for (int i = 0; i < 3; ++i)
            {
                INV p = new INV(list, ran.Next(10,30));
                Set.Add(new PdInfo(p, 0));
            }
            

            //ES
            
            for (int i = 0; i < 3; ++i)
            {
                ES p = new ES(list, ran.NextDouble(), ran.Next(5, 20));
                Set.Add(new PdInfo(p, 0));
            }

            pd = Set[ran.Next(0, 9)].pd;
        }

        public void Add(double item)
        {
            Random ran=new Random();

            foreach (PdInfo pdInf in Set)
            {
                if (Math.Abs(item - pdInf.pd.Last()) / (item+1) < 0.1)
                {
                    pdInf.score += 1;
                }
                else
                {
                    pdInf.score -= 1;
                }

                if (pdInf.score > 3)
                {
                    pd = pdInf.pd;
                }
                else if (pdInf.score < -3)
                {
                    
                    if (pdInf.pd is WNN)
                    {
                        SubPredictor s=new WNN(list,ran.Next(5,20));
                        pdInf.pd=s;
                        pdInf.score = 0;
                    }
                    else if (pdInf.pd is INV)
                    {
                        SubPredictor s = new INV(list, ran.Next(10,30));
                        pdInf.pd = s;
                        pdInf.score = 0;
                    }
                    else if (pdInf.pd is ES)
                    {
                        SubPredictor s = new ES(list, ran.NextDouble(), ran.Next(5, 20));
                        pdInf.pd = s;
                        pdInf.score = 0;
                    }

                    if (pdInf.pd == pd)
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
                        pd = Set[index].pd;
                    }
                }

                pdInf.pd.Add(item);
            }

            mre = (mre * nPd + Math.Abs(last - item) / (item+1)) / (++nPd);
        }

        public double Next()
        {
            last=pd.Next();
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

    class SubPredictor
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

        public WNN(List<double> list,int k)
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
        protected double value=0;

        public INV(List<double> list, double sum)
            : base(list)
        {
            this.sum = sum;
        }

        public override void Add(double item)
        {
            list.Add(item);
            value=item;
        }

        public override double Next()
        {
            last = sum - value;
            return last;
        }
    }
}
