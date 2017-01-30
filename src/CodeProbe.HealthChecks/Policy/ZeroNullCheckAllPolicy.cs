using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.HealthChecks.Policy
{
    /// <summary>
    /// Check policy that succed only if every check succed.
    /// </summary>
    public class ZeroNullCheckAllPolicy : ICheckAllPolicy
    {
        public bool CheckAll(List<Tuple<int, bool?>> checks)
        {
            return checks.All(p=>p.Item2==true);
        }
    }
}
