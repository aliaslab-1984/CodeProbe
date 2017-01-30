using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CodeProbe.Reporting.Remote
{
    /// <summary>
    /// Remote reporting service interface.
    /// </summary>
    [ServiceContract]
    public interface IProbeService
    {
        /// <summary>
        /// The method will erspond with HTTP 200 on success, with HTTP error otherwise.
        /// </summary>
        [WebGet(UriTemplate="probes/is-alive")]
        [OperationContract()]
        void IsAlive();

        /// <summary>
        /// Get a sample from the probes matching the filter regex. On managed error return HTTP 520.
        /// </summary>
        /// <param name="filter">Regex filter</param>
        /// <returns>SampleResult list</returns>
        [WebGet(UriTemplate = "probes/probe?filter={filter}")]
        [OperationContract()]
        List<SampleResult> Probe(string filter);

        /// <summary>
        /// Get a collection of healthchecks results, from the checks mathcing the regex filter. On managed error return HTTP 520.
        /// </summary>
        /// <param name="filter">Regex filter</param>
        /// <returns>HealthCheckResult list</returns>
        [WebGet(UriTemplate = "probes/health?filter={filter}")]
        [OperationContract()]
        List<HealthCheckResult> HealthReport(string filter);

        /// <summary>
        /// Perform a CheckAll operation on the HealtChecks, using the configured CheckAllPolicy. On managed error return HTTP 530.
        /// </summary>
        /// <returns>true on success, false otherwise.</returns>
        [WebGet(UriTemplate = "probes/is-healthy")]
        [OperationContract()]
        bool IsHealthy();
    }
}
