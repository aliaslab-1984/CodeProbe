using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aoProbe
{
    public static class ExecManager
    {
        private static List<Aspect> _aspects = new List<Aspect>();

        public static void Register(Aspect asp)
        {
            if (!_aspects.Contains(asp))
                _aspects.Add(asp);
        }

        public static Func<object> GetInstance(string name, object expr)
        {
            return _aspects.First(p => p.Name==name).GetInstance(expr);
        }

        public static Func<object> GetInstanceForProcess(int proc,string name, object expr)
        {
            Func<object> t = GetInstance(name, expr);
            return ()=>{
                int id=proc;
                return t();
            };
        }

        public static Func<object> GetInstanceForProcessOfType(int proc,ProcessType type, string name, object expr)
        {
            Func<object> t = GetInstance(name, expr);
            return () =>
            {
                int id = proc;
                type.ExecAction(name);
                return t();
            };
        }
    }
}
