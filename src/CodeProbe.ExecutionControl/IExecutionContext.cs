using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public interface IExecutionContext : IDisposable
    {
        IExecutionContext SetCtxValue(string name, object value);
        object GetCtxValue(string name);
        T GetCtxValue<T>(string name);
        List<string> GetCtxKeys();
        Dictionary<string, object> GetDump();
        IExecutionContext Merge(Dictionary<string, object> context);
    }
}
