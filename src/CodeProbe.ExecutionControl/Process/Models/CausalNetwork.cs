using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Process.Models
{
    public class CausalNetwork<T> : IProcessModel
    {
        #region temp
        public IEnumerable<CausalBinding<T>> Bindings { get; protected set; }

        public CausalNetwork()
            :this(new CausalBinding<T>[0])
        {}

        public CausalNetwork(IEnumerable<CausalBinding<T>> bindings)
        {
            Bindings = new List<CausalBinding<T>>(bindings);
        }

        #endregion

        #region IProcessModel

        private List<IProcessActivity> _acts = new List<IProcessActivity>();

        private IEnumerable<IEnumerable<IProcessActivity>> Combinations(IEnumerable<IProcessActivity> elements, int k)
        {
            return k == 0 ? new[] { new IProcessActivity[0] } :
              elements.SelectMany((e, i) =>
                Combinations(elements.Skip(i + 1),k - 1).Select(c => (new[] { e }).Concat(c)));
        }

        public void AddActivity(IProcessActivity activity)
        {
            if (!_acts.Contains(activity))
                _acts.Add(activity);
            else
                throw new ArgumentException(string.Format("Duplicate activity insertion illegal. Id={0}", activity.Id));
        }
        
        public void AddAndSplit(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            ((List<CausalBinding<IProcessActivity>>)Bindings)
                .Add(new CausalBinding<IProcessActivity>(activity,input,output));
        }

        public void AddOrSplit(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            output = output == null ? new IProcessActivity[0] : output;
            for (int i = 1; i < output.Count(); i++)
            {
                foreach (IEnumerable<IProcessActivity> item in Combinations(output,i))
                {
                    ((List<CausalBinding<IProcessActivity>>)Bindings)
                        .Add(new CausalBinding<IProcessActivity>(activity, input, item));
                }
            }
        }

        public void AddXOrSplit(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            output = output == null ? new IProcessActivity[0] : output;
            foreach (IProcessActivity item in output)
            {
                ((List<CausalBinding<IProcessActivity>>)Bindings)
                    .Add(new CausalBinding<IProcessActivity>(activity, input, new IProcessActivity[] { item }));
            }
        }

        public void AddAndJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            ((List<CausalBinding<IProcessActivity>>)Bindings)
                .Add(new CausalBinding<IProcessActivity>(activity, input, output));
        }

        public void AddOrJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            input = input == null ? new IProcessActivity[0] : input;
            for (int i = 1; i < input.Count(); i++)
            {
                foreach (IEnumerable<IProcessActivity> item in Combinations(input, i))
                {
                    ((List<CausalBinding<IProcessActivity>>)Bindings)
                        .Add(new CausalBinding<IProcessActivity>(activity, item, output));
                }
            }
        }

        public void AddXOrJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output)
        {
            input = input == null ? new IProcessActivity[0] : input;
            foreach (IProcessActivity item in input)
            {
                ((List<CausalBinding<IProcessActivity>>)Bindings)
                    .Add(new CausalBinding<IProcessActivity>(activity, new IProcessActivity[] { item }, output));
            }
        }
        #endregion
    }
}
