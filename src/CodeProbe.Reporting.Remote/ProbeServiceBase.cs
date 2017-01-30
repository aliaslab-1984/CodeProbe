using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using CodeProbe.Sensing;
using CodeProbe.HealthChecks;
using CodeProbe.Reporting.Reporters;
using CodeProbe.Reporting.Extractors;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CodeProbe.Reporting.Remote
{
    /// <summary>
    /// Default implementation of a ProbeService.
    /// </summary>
    public abstract class ProbeServiceBase : IProbeService
    {
        protected ILog _logger;

        protected ICheckAllPolicy _checkAllPolicy;
        protected DirectSampleExtractor _extractor;

        protected virtual string AuditName { get { return GetType().FullName; } }

        public ProbeServiceBase()
        {
            _logger = LogManager.GetLogger(AuditName);

            Tuple<DirectSampleExtractor, ICheckAllPolicy> tmp=RemoteReportingManager.Ask().GetProbeServiceExtractor(GetType().FullName);

            _extractor = tmp.Item1;
            _checkAllPolicy = tmp.Item2;

            if (ProbeManager.Ask().SystemTimer(AuditName + ".IsActive") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".IsActive", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".Probe") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".Probe", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".HealthReport") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".HealthReport", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".IsHealthy") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".IsHealthy", ProbeManager.Ask().GetDefaultReservoir<long>());
        }

        public virtual void IsAlive()
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);
                _logger.InfoFormat("Executed {0}", MethodBase.GetCurrentMethod().Name);
            }
        }

        public virtual List<SampleResult> Probe(string filter)
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);

                WebOperationContext ctx = WebOperationContext.Current;

                List<SampleResult> result = new List<SampleResult>();

                try
                {
                    if (string.IsNullOrEmpty(filter))
                        filter = ".*";

                    _extractor.Extract(filter);
                    foreach (KeyValuePair<string, object> item in _extractor.Current)
                    {
                        result.Add(new SampleResult() { Name = item.Key, Value = item.Value });
                    }
                    _logger.InfoFormat("Executed {0}", MethodBase.GetCurrentMethod().Name);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Error executing method {0}", MethodBase.GetCurrentMethod().Name), e);
                    ctx.OutgoingResponse.StatusCode = (System.Net.HttpStatusCode)520;
                }

                return result;
            }
        }

        public virtual List<HealthCheckResult> HealthReport(string filter)
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);

                WebOperationContext ctx = WebOperationContext.Current;

                ConcurrentBag<HealthCheckResult> result = new ConcurrentBag<HealthCheckResult>();

                if (string.IsNullOrEmpty(filter))
                    filter = ".*";

                try
                {
                    Parallel.ForEach(HealthCheckManager.Ask().GetAll(filter), new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, p =>
                    {
                        try
                        {
                            bool? ch = p.Check();
                            result.Add(new HealthCheckResult() { Name = p.Name, Severity = p.Severity, Value = ch });
                        }
                        catch (Exception e)
                        {
                            _logger.Warn(string.Format("Error checking healthcheck: {0}", p.Name), e);
                            result.Add(new HealthCheckResult() { Name = p.Name, Severity = p.Severity, Value = null });
                        }
                    });
                    _logger.InfoFormat("Executed {0}", MethodBase.GetCurrentMethod().Name);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Error executing method {0}", MethodBase.GetCurrentMethod().Name), e);
                    ctx.OutgoingResponse.StatusCode = (System.Net.HttpStatusCode)520;
                }

                return result.ToList();
            }
        }

        public virtual bool IsHealthy()
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);
                WebOperationContext ctx = WebOperationContext.Current;

                bool result = false;

                try
                {
                    result = HealthCheckManager.Ask().CheckAll(_checkAllPolicy);

                    _logger.InfoFormat("Executed {0}", MethodBase.GetCurrentMethod().Name);
                }
                catch (Exception e)
                {
                    _logger.Error(string.Format("Error executing method {0}", MethodBase.GetCurrentMethod().Name), e);
                    ctx.OutgoingResponse.StatusCode = (System.Net.HttpStatusCode)520;
                }

                if (!result)
                {
                    ctx.OutgoingResponse.StatusCode = (System.Net.HttpStatusCode)530;
                }

                return result;
            }
        }
    }
}
