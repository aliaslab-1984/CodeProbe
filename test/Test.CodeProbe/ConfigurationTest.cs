using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProbe;
using CodeProbe.Sensing;
using CodeProbe.Reporting;
using CodeProbe.HealthChecks;
using CodeProbe.Reporting.Remote;

namespace Test.CodeProbe
{
    /// <summary>
    /// Summary description for ConfigurationTest
    /// </summary>
    [TestClass]
    public class ConfigurationTest
    {
        /// <summary>
        /// Test library globals configuration.
        /// </summary>
        [TestCategory("Configuration")]
        [TestMethod]
        public void InitTest()
        {
            ProbeManager.Init();
            HealthCheckManager.Init();
            ReportingManager.Init();
            RemoteReportingManager.Init();

            AbstractReservoir<long> result = ProbeManager.Ask().GetDefaultReservoir<long>();
            ICheckAllPolicy policy = HealthCheckManager.Ask().GetDefaultCheckAllPolicy();

            Assert.IsNotNull(result);
            Assert.IsNotNull(policy);
        }
    }
}
