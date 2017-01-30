using CodeProbe.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.CodeProbe.HealthChecks.utility
{
    public class TestHealtCheck : AbstractHealthCheck
    {
        public TestHealtCheck(string name)
            :base(name)
        {

        }

        public override int Severity
        {
            get { return 10; }
        }

        public override bool? Check()
        {
            return true;
        }
    }
}
