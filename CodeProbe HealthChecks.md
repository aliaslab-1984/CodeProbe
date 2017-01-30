#CodeProbe HealthChecks

## Configuration

xml .config format:

Complete configuration example (it must be inside the main configuration tag _codeProbe_):

	<healthchecks>
    	<checks>
	    	<add name="test_hc" type="Test.CodeProbe.HealthChecks.utility.TestHealtCheck, Test.CodeProbe.HealthChecks" />
	    </checks>
	    <checkAllPolicy type="CodeProbe.HealthChecks.Policy.ZeroNullCheckAllPolicy, CodeProbe.HealthChecks"/>
	</healthchecks>

It is possible to set the default implementation of _ICheckAllPolicy_, and the list of healthchecks used by the application.

h2. CodeProbe.HealthChecks

Contains the abstract base types of the library and the factory which allows to easily create an get the probes.

The probes can be created and used on their own, stand-alone or used by the factory singleton which keeps them available throughout the application and the reporting components.

	HealthCheckManager.Ask().GetAll();
	HealthCheckManager.Ask().AddHealthCheck(test);
	HealthCheckManager.Ask().CheckAll(policy);

## Use

In order to use the library the base factory must be initialized:

	HealthCheckManager.Init();

Once initialized, is possible to sue the factory to referentiate (and contextually create) the probes by name, by its instance accessible with the static method Ask().

	HealthCheckManager.Ask().GetAll();
	HealthCheckManager.Ask().AddHealthCheck(test);
	HealthCheckManager.Ask().CheckAll(policy);
	HealthCheckManager.Ask().GetAll(filter,severity);

### Anatomy of an HealthCkeck

The healthChecks are classes that implements the abstract base class _AbstractHealthCheck_:

    public abstract class AbstractHealthCheck
    {
        public AbstractHealthCheck(string name)
        {
            Name = name;
        }

        public string Name { get; protected set; }
        public abstract int Severity { get; }
        public abstract bool? Check();
    }

Every implementation execute its control in the method _Check_ which must be the simplier possible (for performance considerations), manage every exception and thread-safe.

The method must return:
- true: when the check is positivie
- false: when the check is negativie
- null: when the check can't be executed or the results are inconclusive.

The name must be unique and is used to identify the probe inside the factory.

_Severity_ is an internal value which gives an idea of the impact of a check failure on the global system. The higher the value is the worst the problem is.

### Anatomy of a CheckAllPolicy

A _check all policy_, is a class that implements +ICheckAllPolicy+ and is used to evaluet the global system health based on the single results of a group of probes.
The policy must evaluate:

- the number of *positive* results
- the number of *negative* results
- the number of *inconclusive* results
- the *severity* of the various probes