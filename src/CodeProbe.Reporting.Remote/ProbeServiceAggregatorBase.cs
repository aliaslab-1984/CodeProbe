using CodeProbe.HealthChecks;
using CodeProbe.Sensing;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace CodeProbe.Reporting.Remote
{
    /// <summary>
    /// Default implementation of a ProbeServiceAggregator. The name of the aggregated probes are name::endpoint.
    /// </summary>
    public abstract class ProbeServiceAggregatorBase : IProbeService
    {
        protected ILog _logger;
        protected List<RemoteReportingManager.ProbeServiceEndpointDescriptor> _endpoints = new List<RemoteReportingManager.ProbeServiceEndpointDescriptor>();
        protected ICheckAllPolicy _checkAllPolicy;

        protected virtual string AuditName { get { return GetType().FullName; } }

        public ProbeServiceAggregatorBase()
        {
            _logger = LogManager.GetLogger(AuditName);

            Tuple<List<RemoteReportingManager.ProbeServiceEndpointDescriptor>, ICheckAllPolicy> tmp = RemoteReportingManager.Ask().GetProbeServiceAggregator(GetType().FullName);

            _endpoints = tmp.Item1;
            _checkAllPolicy = tmp.Item2;

            if (ProbeManager.Ask().SystemTimer(AuditName + ".IsActive")==null)
                ProbeManager.Ask().SystemTimer(AuditName + ".IsActive", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".Probe") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".Probe", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".HealthReport") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".HealthReport", ProbeManager.Ask().GetDefaultReservoir<long>());
            if (ProbeManager.Ask().SystemTimer(AuditName + ".IsHealthy") == null)
                ProbeManager.Ask().SystemTimer(AuditName + ".IsHealthy", ProbeManager.Ask().GetDefaultReservoir<long>());
        }

        public void IsAlive()
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);
                ParallelLoopResult result = Parallel.ForEach(_endpoints,
                    new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    (item, loopState) =>
                    {
                        try
                        {
                            using (ChannelFactory<IProbeService> svcCh = new ChannelFactory<IProbeService>(new BasicHttpBinding(RemoteReportingManager.Ask().ProbeServiceBindingName), new EndpointAddress(item.Endpoint)))
                            {
                                IProbeService service = svcCh.CreateChannel();

                                service.IsAlive();
                            }
                        }
                        catch (Exception e)
                        {
                            loopState.Break();
                            _logger.Debug(string.Format("Error sampling."), e);
                            throw e;
                        }
                    }
                );

                if (!result.IsCompleted)
                    throw new Exception(string.Format("Loop stopped at: {0}", result.LowestBreakIteration));
                _logger.InfoFormat("Executed {0}", MethodBase.GetCurrentMethod().Name);
            }
        }

        public List<SampleResult> Probe(string filter)
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

                    ConcurrentBag<SampleResult> tmp = new ConcurrentBag<SampleResult>();

                    ParallelLoopResult plRes = Parallel.ForEach(_endpoints,
                        new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        (item, loopState) =>
                        {
                            try
                            {
                                using (ChannelFactory<IProbeService> svcCh = new ChannelFactory<IProbeService>(new BasicHttpBinding(RemoteReportingManager.Ask().ProbeServiceBindingName), new EndpointAddress(item.Endpoint)))
                                {
                                    IProbeService service = svcCh.CreateChannel();

                                    tmp.Add(new SampleResult() { Name = item.Name+"::"+item.Endpoint, Samples = service.Probe(filter), Value = -1 });
                                }
                            }
                            catch(WebFaultException e)
                            {
                                WebException ex = e.InnerException as WebException;
                                if (((HttpWebResponse)ex.Response).StatusCode == (System.Net.HttpStatusCode)520)
                                {
                                    tmp.Add(new SampleResult() { Name = item.Name + "::" + item.Endpoint, Samples = null, Value = -1 });
                                    _logger.Debug(string.Format("Remote error on {0}.",item.Endpoint), e);
                                }
                                else
                                {
                                    throw e;
                                }
                            }
                            catch (Exception e)
                            {
                                loopState.Break();
                                _logger.Debug(string.Format("Error sampling."), e);
                                throw e;
                            }
                        }
                    );

                    if (!plRes.IsCompleted)
                        throw new Exception(string.Format("Loop stopped at: {0}", plRes.LowestBreakIteration));

                    result = tmp.ToList();

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

        public List<HealthCheckResult> HealthReport(string filter)
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);

                WebOperationContext ctx = WebOperationContext.Current;

                List<HealthCheckResult> result = new List<HealthCheckResult>();

                if (string.IsNullOrEmpty(filter))
                    filter = ".*";

                try
                {
                    ConcurrentBag<HealthCheckResult> tmp = new ConcurrentBag<HealthCheckResult>();

                    ParallelLoopResult plRes = Parallel.ForEach(_endpoints,
                        new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        (item, loopState) =>
                        {
                            try
                            {
                                using (ChannelFactory<IProbeService> svcCh = new ChannelFactory<IProbeService>(new BasicHttpBinding(RemoteReportingManager.Ask().ProbeServiceBindingName), new EndpointAddress(item.Endpoint)))
                                {
                                    IProbeService service = svcCh.CreateChannel();

                                    HealthCheckResult v = new HealthCheckResult() { Name = item.Name+"::"+item.Endpoint, Results = service.HealthReport(filter) };
                                    v.Value = v.Results.All(p => p.Value == true);
                                    v.Severity = new int[] { item.Severity, v.Results.Max(p=>p.Severity) }.Max();

                                    tmp.Add(v);
                                }
                            }
                            catch (WebFaultException e)
                            {
                                WebException ex = e.InnerException as WebException;
                                if (((HttpWebResponse)ex.Response).StatusCode == (System.Net.HttpStatusCode)520)
                                {
                                    HealthCheckResult v = new HealthCheckResult() { Name = item.Name + "::" + item.Endpoint, Results = null };
                                    v.Value = v.Results.All(p => p.Value == true);
                                    v.Severity = item.Severity;

                                    tmp.Add(v);
                                    _logger.Debug(string.Format("Remote error on {0}.", item.Endpoint), e);
                                }
                                else
                                {
                                    throw e;
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Warn(string.Format("Error checking healthcheck: {0}", item.Name), e);
                                tmp.Add(new HealthCheckResult() { Name = item.Name + "::" + item.Endpoint, Severity=item.Severity, Results = null, Value = null });
                            }
                        }
                    );

                    if (!plRes.IsCompleted)
                        throw new Exception(string.Format("Loop stopped at: {0}", plRes.LowestBreakIteration));

                    result = tmp.ToList();

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

        public bool IsHealthy()
        {
            using (ProbeManager.Ask().SystemTimer(AuditName + "." + MethodBase.GetCurrentMethod().Name).Time())
            {
                _logger.InfoFormat("Called {0}", MethodBase.GetCurrentMethod().Name);
                WebOperationContext ctx = WebOperationContext.Current;

                bool result = false;

                try
                {
                    ConcurrentBag<Tuple<int, bool?>> tmp = new ConcurrentBag<Tuple<int, bool?>>();

                    ParallelLoopResult plRes = Parallel.ForEach(_endpoints,
                        new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        (item, loopState) =>
                        {
                            try
                            {
                                using (ChannelFactory<IProbeService> svcCh = new ChannelFactory<IProbeService>(new BasicHttpBinding(RemoteReportingManager.Ask().ProbeServiceBindingName), new EndpointAddress(item.Endpoint)))
                                {
                                    IProbeService service = svcCh.CreateChannel();

                                    tmp.Add(new Tuple<int,bool?>(item.Severity,service.IsHealthy()));
                                }
                            }
                            catch (ProtocolException e)
                            {
                                WebException ex = e.InnerException as WebException;
                                if (((HttpWebResponse)ex.Response).StatusCode == (System.Net.HttpStatusCode)530)
                                {
                                    tmp.Add(new Tuple<int, bool?>(item.Severity, false));
                                    _logger.Debug(string.Format("Remote error on {0}.", item.Endpoint), e);
                                }
                                else
                                {
                                    throw e;
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.Warn(string.Format("Error checking is-healthy: {0}", item.Name), e);
                                tmp.Add(new Tuple<int, bool?>(item.Severity, null));
                            }
                        }
                    );

                    if (!plRes.IsCompleted)
                        throw new Exception(string.Format("Loop stopped at: {0}", plRes.LowestBreakIteration));

                    result = _checkAllPolicy.CheckAll(tmp.ToList());

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
