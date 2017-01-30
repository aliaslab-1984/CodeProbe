using CodeProbe.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.CodeProbe.HealthChecks.utility
{
    public class TestHealtCheckFail : AbstractHealthCheck
    {
        public TestHealtCheckFail(string name)
            :base(name)
        {

        }

        public override int Severity
        {
            get { return 10; }
        }

        public override bool? Check()
        {
            return false;
        }
    }
}
