# CodeProbe.Reporting.Remote

## Use

In order to use the library the base factory must be initialized (after the one of sensing, reporting and healthcheck):

	ProbeManager.Init();
	ReportingManager.Init();
	HealthCheckManager.Init();
	RemoteReportingManager.Init();

Once initilalized the factory provides references to the list of _ProbeServiceExtractor_ and _ProbeServiceAggregator_ by name, from its instance accessible with the static method _Ask()_.
	
	RemoteReportingManager.Ask().GetProbeServiceExtractor(name)
	RemoteReportingManager.Ask().GetProbeServiceAggregator(name)

The applications which want to use this functionality must create a WCF service with a codebase class deriving from _ProbeServiceBase_ or _ProbeServiceAggregatorBase_, which are abstract classes that implement the remote reporting service.

## Configuration

xml .config format

Complete configuration example (it must be inside the main configuration tag _codeProbe_):

	<remoteReporting remoteExtractorNamePrefix="remote" probeServiceBindingName="ProbeServiceWebHttpBinding">
    	<probeServiceAggregators>
        	<add name="Test.CodeProbe.Reporting.Remote.AggregateTestService">
          		<endpoints>
            		<add name="i0" endpoint="http://localhost:28404/TestService.svc/test" severity="123" />
          		</endpoints>
          		<checkAllPolicy type="CodeProbe.HealthChecks.Policy.ZeroNullCheckAllPolicy, CodeProbe.HealthChecks"/>
        	</add>
      	</probeServiceAggregators>
      	<probeServices>
        	<add name="Test.CodeProbe.Reporting.Remote.TestService">
          		<sampler name="test_remote_sampler" type="CodeProbe.Reporting.Samplers.BasicSampler, CodeProbe.Reporting">
		            <stats>
		              <add type="CodeProbe.Reporting.Statistics.Gauge.ConstantStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Counter.ConstantStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Meter.ConstantStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Histogram.LinearAvgStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Histogram.LinearMinStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Histogram.LinearMaxStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Timer.ConstantRatioStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Timer.LinearAvgStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Timer.LinearMinStatistic, CodeProbe.Reporting" />
		              <add type="CodeProbe.Reporting.Statistics.Timer.LinearMaxStatistic, CodeProbe.Reporting" />
		            </stats>
          		</sampler>
          		<checkAllPolicy type="CodeProbe.HealthChecks.Policy.ZeroNullCheckAllPolicy, CodeProbe.HealthChecks" />
        	</add>
      	</probeServices>
	</remoteReporting>

In the section is possible to set:

- _remoteExtractorNamePrefix_ default prefix to identify the probeService reporters.
- _probeServiceBindingName_ binding configuration name for all the binding configuration relative to remote reporting services (_webHttpBinding_ e _basicHttpBinding_)
- the list of _probeServices_, everyone with its own _sampler_ (and statistics).
- the list of _probeServiceAggregators_, with its own endpoint (binding basicHttpBinding) and severity

## ProbeService

The service expose a ws interface with REST binding, with the following specification:

- **probes/is-alive**

The method will respond with http 200 if the server is fine, otherwise (eg.: iis pooldown or machine down) will not respond and a client timeout will kick in.

- **probes/probe?filter=<filter>**

The method will respond with http 200 and a dictionary of key-value pairs with some performance indicators filtered by the regular expression set by the **filter** parameter. On managed error http 520 will be returned. In every other case the http error deemed right by iis will be returned.

- **probes/health**

The method will respond with http 200 and a dictionary of objects that describes the health state of the application composed of **name**, **check outcome** (true, false or null, to indicate success, failure or inconcludence of the check), **severity** (the higher the worse). On managed error http 520 will be returned.In every other case the http error deemed right by iis will be returned.

- **probes/is-healthy**

The method will respond with http 200 and a **true** value if every check is ok (true). In case the checks outcomes show malfunctioning, an http 530 and a **false** value will be returned. In every other case the http error deemed right by iis will be returned.

## ProbeServiceAggregator

The service is an implemenattion of _ProbeService_. The difference is that the produced data are from other _ProbeService_s (other _ProbeServiceAggregator_s also).