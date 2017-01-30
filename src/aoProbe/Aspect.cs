using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace aoProbe
{
    public class Aspect
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj.GetType() != GetType())
                return false;
            return ((Aspect)obj).Name==Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public string Name { get; set; }

        protected Func<Dictionary<string,object>,object> _action;

        public Aspect(Func<Dictionary<string, object>, object> action)
        {
            _action = action;
        }

        public int ErrorCount { get; set; }
        public int OkCount { get; set; }

        public double OkTime { get; set; }
        public double ErrTime { get; set; }

        public Func<object> GetInstance(object expr)
        {
            return () => Exec(expr);
        }

        protected object Exec(object expr)
        {
            double pr = 0.9;
            Stopwatch wt = Stopwatch.StartNew();
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>();

                foreach (PropertyInfo item in expr.GetType().GetProperties())
                {
                    args.Add(item.Name, item.GetValue(expr, null));
                }

                object result = _action(args);

                OkCount++;
                wt.Stop();
                OkTime=(1-pr)*OkTime+pr*wt.ElapsedMilliseconds;
                return result;
            }
            catch (Exception e)
            {
                ErrorCount++;
                wt.Stop();
                ErrTime = (1 - pr) * ErrTime + pr * wt.ElapsedMilliseconds;
                throw e;
            }
        }
    }
}
