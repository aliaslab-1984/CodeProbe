using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Process.Models
{
    public class CausalNetworkEvaluator : IProcessEvaluator, IEqualityComparer<Tuple<IProcessActivity, IProcessActivity>>
    {
        #region IEqualityComparer
        public bool Equals(Tuple<IProcessActivity, IProcessActivity> x, Tuple<IProcessActivity, IProcessActivity> y)
        {
            return (x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2)) || (x.Item1.Equals(y.Item2) && x.Item2.Equals(y.Item1));
        }

        public int GetHashCode(Tuple<IProcessActivity, IProcessActivity> obj)
        {
            return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
        }
        #endregion

        #region private methods

        protected bool TestPath(List<CausalBinding<IProcessActivity>> seq, out List<Tuple<IProcessActivity, IProcessActivity>> obb)
        {
            if (seq[0].Input.Count() != 0)
            {
                obb = null;
                return false;
            }
            else
            {
                obb = new List<Tuple<IProcessActivity, IProcessActivity>>();
                foreach (IProcessActivity item in seq[0].Output)
                {
                    obb.Add(new Tuple<IProcessActivity, IProcessActivity>(seq[0].Activity, item));
                }
                seq = seq.GetRange(1, seq.Count - 1);
            }
            
            foreach (CausalBinding<IProcessActivity> bnd in seq)
            {
                List<Tuple<IProcessActivity, IProcessActivity>> inp = bnd.Input.Select(p => new Tuple<IProcessActivity, IProcessActivity>(bnd.Activity, p)).ToList();
                List<Tuple<IProcessActivity, IProcessActivity>> outp = bnd.Output.Select(p => new Tuple<IProcessActivity, IProcessActivity>(bnd.Activity, p)).ToList();

                if (obb.Intersect(inp, this).Count() != inp.Count())
                {
                    return false;
                }
                else
                {
                    obb = obb.Except(inp, this).ToList();
                    obb = obb.Union(outp, this).ToList();
                }
            }

            return true;
        }

        protected bool BF_AC_Prune(List<Tuple<IProcessActivity, List<CausalBinding<IProcessActivity>>>> maybe, bool forward)
        {
            for (int i = 0; i < maybe.Count - 1; i++)
            {
                if (forward) maybe[i].Item2.RemoveAll(p => p.Output.Intersect(maybe.GetRange(i + 1, maybe.Count - i - 1).Select(t => t.Item1)).Count() == 0);
                maybe[i + 1].Item2.RemoveAll(p => p.Input.Intersect(maybe.GetRange(0, i + 1).Select(t => t.Item1)).Count() == 0);

                if (maybe[i].Item2.Count == 0 || maybe[i + 1].Item2.Count == 0)
                    return false;
            }
            return true;
        }

        protected bool BFExplorePossibilityAndValidity(List<List<CausalBinding<IProcessActivity>>> grph, List<CausalBinding<IProcessActivity>> acc, ref bool valid)
        {
            if (grph.Count == 0)
            {
                return true;
            }
            else
            {
                List<Tuple<IProcessActivity, IProcessActivity>> tmp;
                foreach (CausalBinding<IProcessActivity> item in grph[0])
                {
                    acc.Add(item);
                    if (TestPath(acc, out tmp))
                    {
                        valid = tmp.Count == 0;
                        return BFExplorePossibilityAndValidity(grph.GetRange(1, grph.Count - 1), acc, ref valid);
                    }
                    else
                    {
                        acc.RemoveAt(acc.Count - 1);
                    }
                }
                return false;
            }
        }
        #endregion

        public bool IsPossibleSequence(IProcessModel model, IEnumerable<IProcessActivity> sequence)
        {
            bool valid = false;
            List<Tuple<IProcessActivity, List<CausalBinding<IProcessActivity>>>> maybe = 
                sequence.Select(p => new Tuple<IProcessActivity, List<CausalBinding<IProcessActivity>>>(p, ((CausalNetwork<IProcessActivity>)model).Bindings.Where(t => t.Activity.Equals(p)).ToList())).ToList();

            if (!BF_AC_Prune(maybe, false))
                return false;
            else
                return BFExplorePossibilityAndValidity(maybe.Select(kv => kv.Item2).ToList(), new List<CausalBinding<IProcessActivity>>(), ref valid);
        }

        public bool IsValidSequence(IProcessModel model, IEnumerable<IProcessActivity> sequence)
        {
            bool valid = false;
            List<Tuple<IProcessActivity, List<CausalBinding<IProcessActivity>>>> maybe = 
                sequence.Select(p => new Tuple<IProcessActivity, List<CausalBinding<IProcessActivity>>>(p, ((CausalNetwork<IProcessActivity>)model).Bindings.Where(t => t.Activity.Equals(p)).ToList())).ToList();

            if (!BF_AC_Prune(maybe, true))
                return false;
            else
                return BFExplorePossibilityAndValidity(maybe.Select(kv => kv.Item2).ToList(), new List<CausalBinding<IProcessActivity>>(), ref valid) && valid;
        }
    }
}
