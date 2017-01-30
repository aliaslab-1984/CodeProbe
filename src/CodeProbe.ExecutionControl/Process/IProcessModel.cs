using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Process
{
    public interface IProcessModel
    {
        void AddActivity(IProcessActivity activity);
        
        void AddAndSplit(IEnumerable<IProcessActivity> input,IProcessActivity activity, IEnumerable<IProcessActivity> output);
        void AddOrSplit(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output);
        void AddXOrSplit(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output);
        void AddAndJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output);
        void AddOrJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output);
        void AddXOrJoin(IEnumerable<IProcessActivity> input, IProcessActivity activity, IEnumerable<IProcessActivity> output);
    }
}
