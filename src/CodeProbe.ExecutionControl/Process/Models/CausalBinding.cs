using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Process.Models
{
    public class CausalBinding<T>
    {
        public CausalBinding(T activity, IEnumerable<T> input, IEnumerable<T> output)
        {
            if (activity == null)
                throw new ArgumentNullException("'activity' cannot be NULL.");
            Activity = activity;
            Input = new List<T>(input==null?new T[0]:input);
            Output = new List<T>(output==null?new T[0]:output);
        }

        public T Activity { get; protected set; }

        public IEnumerable<T> Input { get; protected set; }

        public IEnumerable<T> Output { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (GetType() != obj.GetType())
                return false;
            else
                return ((CausalBinding<T>)obj).Activity.Equals(Activity) &&
                    ((CausalBinding<T>)obj).Input.Join(Input,p=>p,p=>p,(k,p)=>p).Count()==Input.Count() &&
                    ((CausalBinding<T>)obj).Output.Join(Output, p => p, p => p, (k, p) => p).Count() == Output.Count();
        }

        public override int GetHashCode()
        {
            return Activity.GetHashCode() ^
                Input.Select(p=>p.GetHashCode()).Aggregate((a,b)=>a^b) ^
                Output.Select(p=>p.GetHashCode()).Aggregate((a,b)=>a^b);
        }

        public override string ToString()
        {
            return string.Format("{{{0},{{{1}}},{{{2}}}}}", Activity,string.Join(",",Input.Select(p => p.ToString())),string.Join(",",Output.Select(p=>p.ToString())));
        }
    }
}
