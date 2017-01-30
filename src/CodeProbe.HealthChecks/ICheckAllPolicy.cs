using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.HealthChecks
{
    /// <summary>
    /// Interface used to determine if a collection of healthchecks globally fails or succed.
    /// </summary>
    public interface ICheckAllPolicy
    {
        /// <summary>
        /// Enforce the policy.
        /// </summary>
        /// <param name="checks">List of checks results, with severity and outcome.</param>
        /// <returns>true if the collection globally succed. False otherwise.</returns>
        bool CheckAll(List<Tuple<int,bool?>> checks);
    }
}
