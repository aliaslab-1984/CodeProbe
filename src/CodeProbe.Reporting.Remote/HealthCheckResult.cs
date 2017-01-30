using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CodeProbe.Reporting.Remote
{
    [DataContract]
    public class HealthCheckResult
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "value")]
        public bool? Value { get; set; }
        [DataMember(Name="severity")]
        public int Severity { get; set; }

        [DataMember(Name = "results")]
        public List<HealthCheckResult> Results { get; set; }
    }
}
