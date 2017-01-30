using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Process
{
    public interface IProcessEvaluator
    {
        bool IsPossibleSequence(IProcessModel model, IEnumerable<IProcessActivity> sequence);
        bool IsValidSequence(IProcessModel model, IEnumerable<IProcessActivity> sequence);
    }
}
