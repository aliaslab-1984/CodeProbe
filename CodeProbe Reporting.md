# CodeProbe.Reporting

Contains the facilities used to get the sampled data.

## Use

In order to be used the base factory must be initialized (after the sensing one):

    ProbeManager.Init();
    ReportingManager.Init();

Once initialized the factory allows to address the _reporters_ and the _extractors_ by name, by its instance accessible with the static method _Ask()_.

    ReportngManager.Ask().Reporters
    ReportngManager.Ask().Extractors

## Configuration

xml .config format

Complete configuration example (it must be inside the main configuration tag _codeProbe_):

	<reporting>
      <extractors>
        <add name="test_ext" type="CodeProbe.Reporting.Extractors.TimedSampleExtractor, CodeProbe.Reporting">
          <args>
            <add name="period" value="60000" />
          </args>
        </add>
      </extractors>
      <reporters>
        <add name="test_rep" type="CodeProbe.Reporting.Reporters.JsonReporter, CodeProbe.Reporting" extractorRef="test_ext">
          <sampler name="test_sampler" type="CodeProbe.Reporting.Samplers.BasicSampler, CodeProbe.Reporting">
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
          <args>
            <add name="savePath" value="C:\Progetti\CodeProbe\test\Test.CodeProbe.Reporting.Remote\reports\{0:yyyyMMdd}_{1}.json" />
          </args>
        </add>
      </reporters>
    </reporting>


In the section is possible to set:

- a list of _extractors_, setting _name_, type and constructor arguments, that further the list of the abstract base class _AbstractSampleExtractor_.
- a list of _reporters_, ietting _name_, type and constructor arguments, that further the list of the abstract base class _AbstractReporter_, the configuration of its sampler and the name of the _extractor reference_.
- for each _reporter_ its _sampler_, setting _name_, type and the list of used statistics.
- _statistics_ for each _sampler_, setting type and constructor arguments, that further the list of the abstract base class _AbstractProbeStatistic_.

## Extractors

an _extractor_ is a calss derived by the abstract base class _AbstractSampleExtractor_. Its purpose is to give a unique method to order to its managed sampler to sample the values appling the configured statistics.

The libray gives three implememntation of _AbstractSampleExtractor_:

### BasicSampleExtractor

Orders every managed sampler to sample. The samplers will notify each connected reporter the sampled values.

### DirectSampleExtractor

Derived from  _BasicSampleExtractor_.
Orders to the unique managed sampler to sample. The obtained sample will be immediatly available through the _Current_ property of the extractor.

### TimedSampleExtractor

Derived from _BasicSampleExtractor_.
Orders every managed sampler to sample at fixed interval of time. The samplers will notify each connected reporter the sampled values.

## Reporters

A _Reporter_ is a class derived from the abstract base class _AbstractReporter_. Its purpose is to produce a readable report of the informtion obtained by the samplers.

The libray gives two implememntation of _AbstractReporter_:

### JsonReporter

Produces a .json file of which every row is an associative array of key-value pairs containing the sampled values.

### DictionaryReporter

Makes available through its property _CurrentSample_, an associative array of key-value pairs containing the sampled values.

## Samplers

A _sampler_ is a class derived from the base abstract class _AbstractSampler_. Its prpose is to give a unique way to get a sample using the configured statistics.

The values are extracted and notified to the subscribers as an associative array of key-value pairs, where the name has the format:

**probe name. statisitc**

Where:

- *probe name*: is the name of the probe
- *statistic*: its the description of the statistic calculated on the probe value

The libray gives one implememntation of _AbstractSampler_:

### BasicSampler

Performs a sample on every probe.

## Statistics

A _statistic_ is a classderived from the abstract base class _AbstractProbeStatistic_. Its purpose is to provide a way to calculate values from a raw sample extracted from a probe depending on the the type of the probe.

Every _statistic_ has a name, which is concatenated by the _sampler_ to the probe name.
The name has the following format:

**probe.calculation.type**

Where:

- *probe*: is a value specific for every type of probe (eg.: timer for TimerProbe, counter for CounterProbe)
- *calculation*: is a value specific for type of calculation (eg.: const_ms for constant value in milliseconds, lin_ms for linear in millisecond)
- *type*: is a value specific for type of value (eg.: min for minimum, ratio for frequency, avg for average)

Exists an abstract class for probe type which is the base for the statistics for probe types.
The statistics available for _single values_ are all constant, while for _pool values_ are _min_, _max_, _avg_ and _ratio_.