# CodeProbe
Library for code instrumentation with probes for performance auditing, healtcheck, context-aware logging, process validation.

The library has been inspired by codhale-metrics for java.

## Code Probe

CodeProbe is a framework for the instrumentation of the with audit probes. The objective of the framework is to provide to the developer a suite of simple and configurable tools to enable the runtime evaluation of the functioning of an application.
The framewoork is modular and divided in five modules.

## CodeProbe

It the base module which contains the classes that allow its configuration from .config files, the abstractions and utilities used by other modules.
The namespace **CodeProbe.Sensing** contains the audit probes to use to evaluate various metrics in an application.

## CodeProbe.ExecutionControl

The module contains the abstractions which allow to keep a distributed operation context between threads and remote wcf services also.

## CodeProbe.HealthChecks

The module contains the abstractions which allow to instrument the code with healtchecks.

## CodeProbe.Reporting

The module contains the abstractions which allow to extract and report the data gatered by the probes.

## CodeProbe.Reporting.Remote

The module contains the abstractions classes and services needed to promptly create pull services for the extraction and remote reporting of the sampled data.
