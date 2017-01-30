using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CodeProbe.Reporting.Remote
{
    [DataContract]
    public class SampleResult
    {
        [DataMember(Name="name")]
        public string Name { get; set; }
        [DataMember(Name="value")]
        public object Value { get; set; }

        [DataMember(Name = "samples")]
        public List<SampleResult> Samples { get; set; }
    }
}
