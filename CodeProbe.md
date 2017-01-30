# CodeProbe

## CodeProbe.Configuration

xml .config format example:
	
	<configSections>
    	<section name="codeProbe" type="CodeProbe.Configuration.CodeProbeConfigurationSection, CodeProbe" />
  	</configSections>

  	<codeProbe>
    	<reporting>
    	</reporting>
	    <remoteReporting>
	    </remoteReporting>
	    <healthchecks>
	    </healthchecks>
    	<sensing>
      		<reservoir type="CodeProbe.Sensing.SlidingWindowReservoir`1, CodeProbe">
	        	<args>
	          		<add name="size" value="100" />
	        	</args>
      		</reservoir>
		</sensing>
  	</codeProbe>


The library configuration is divided in four sections:
- _reporting_ configures the reporting part of the library if present
- _healthckecks_ configures the healthchecks part of the library if present
- _remoteReporting_ configures the remote reporting part of the library if present
- _sensing_ configures the sensing part of the library

In the base case, the _sensing_ section only is present. It configures the default reservoir which is used to calculate the statistics of timers and hystograms. Normally is a _SlidingWindowReservoir_ of 100 samples.

### Implementing your own configuration parser

If it's necessary to implement a factory able to read a configuration section of your own, the abstract class _ConfigurationResolverBase_ provides a group of protected methods usefull for creating object by reflection.

**NOTE: the library uses conventions on the position of the arguments fr the classes instantiated by reflection. As a basse rule, implmenting the abstract classes provided by the library which have contructors with parameters, keepthe same ordering in the derived classes constructors.**

## CodeProbe.Sensing

### Description

The library allows the use of software probes to evaluate specific applcation parameters at runtime. The probes aare identifed by a name composed by convention in the following way:

**context.probe name**

Where **context** may be:

- **machine**: indicate a machine value (eg.: memory, % cpu)
- **system**: value relative to components or operations of the library CodeProbe
- **application**: value relative to the monitored application

**probe name** can be everything given that is unique. Something like *classname*  and *description of the monitored value* is advised.

The probes can be created and used on their own, stand-alone or used by the factory singleton which keeps them available throughout the application and the reporting components.

### Use

In order to use the library the base factory must be initialized:

	ProbeManager.Init();

Once initialized, is possible to sue the factory to referentiate (and contextually create) the probes by name, by its instance accessible with the static method Ask().

    ProbeManager.Ask().Clear(".*");

    ProbeManager.Ask().Gauge("gauge_test", () => 1);
    ProbeManager.Ask().Histogram("histogram_test").Update(1);
    ProbeManager.Ask().Counter("counter_test").Increment();
    for (int i = 0; i < 10; i++)
    {
        ProbeManager.Ask().Timer("test_timer").Time(() => Thread.Sleep(100));
        ProbeManager.Ask().Meter("meter_test").Mark();
    }

The factory sets also some default probes:

   	this.SystemGauge("dateTime", () => DateTime.Now.ToFileTimeUtc());

    this.MachineGauge("process.ram", () =>
    {
        using (PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName))
        {
            return ramCounter.NextValue();
        }
    });
    this.MachineGauge("process.cpu", () =>
    {
        using (PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName))
        {
            return cpuCounter.NextValue();
        }
    });

## Probes

The probes allows to monitor a range of indicators, of which the following types are available:

### GaugeProbe

This probe allow to get every time it is read the istantaneous vaue of a property or calculated value.

### CounterProbe

This probe is a counter which can be incremented or decremented.

### MeterProbe

This probe is used to get the frequency of an event. The meter must be updated by the method *.Mark()*, on the occurrence of the monitored event.

### HistogramProbe

This probes allows to caluclate statistics on a value, keeping a pool of samples on which average, median ecc can be executes.
The insretion of a sample happens by the use of the method _.Update(val)_, and the samples are stored in a _reservoir_, with proteries depending on the concrete type (of fixed dimension, on fixed stored time, dynamic dimension).

### TimerProbe

This probe allows to get statistics on the execution time of an operation or section of code, giving also the frequency of happening.
Conceptually it's a combination between an _HistogramProbe_ of times and a _Meter_ of happenings.