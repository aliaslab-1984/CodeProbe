using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeProbe.ExecutionControl.Process
{
    public abstract class AbstractBusinessActivity
    {
        private List<Tuple<string, string>> _descriptor;
        
        private void Traverse_ConvertObject(ref List<Tuple<string, string>> tmp, string path, object obj, Dictionary<object,string> traversed)
        {            
            if (typeof(ValueType).IsAssignableFrom(obj.GetType()) || obj.GetType() == typeof(string))
            {
                tmp.Add(new Tuple<string, string>(path, obj.ToString()));
            }
            else if(traversed.ContainsKey(obj))
            {
                tmp.Add(new Tuple<string, string>(path, "ref>"+traversed[obj]));
            }
            else
            {
                traversed.Add(obj, path);
                foreach (PropertyInfo pinfo in obj.GetType().GetProperties())
                {
                    if (typeof(IEnumerable).IsAssignableFrom(pinfo.PropertyType) && pinfo.PropertyType != typeof(string))
                    {
                        int i = 0;
                        foreach (object item in (IEnumerable)pinfo.GetValue(obj, null))
                        {
                            Traverse_ConvertObject(ref tmp, string.Join("/",path,pinfo.Name,"[" + i + "]"), item, traversed);
                            i++;
                        }
                    }
                    else
                    {
                        Traverse_ConvertObject(ref tmp, string.Join("/",path,pinfo.Name), pinfo.GetValue(obj, null), traversed);
                    }
                }
            }
        }

        protected internal List<Tuple<string, string>> ConvertToObjectDictionary(object value)
        {
            List<Tuple<string, string>> tmp = new List<Tuple<string, string>>();
            Traverse_ConvertObject(ref tmp, "", value, new Dictionary<object, string>());

            return tmp;
        }

        protected internal bool CheckObjectDictionaryInclusion(List<Tuple<string, string>> container, List<Tuple<string, string>> contained)
        {            
            //TODO: controlla per array[*]
            return contained.Count == container.Join(
                contained,
                kv => string.Join("=",kv.Item1,kv.Item2),
                kv => string.Join("=", kv.Item1, kv.Item2), 
                (kv,p)=>p).Count();
        }

        public virtual bool CheckStateCompatibility(object state)
        {
            return CheckObjectDictionaryInclusion(ConvertToObjectDictionary(state), _descriptor);
        }

        public abstract void Start();
        public abstract void Enter();
        public abstract void Exit();
        public abstract IDisposable Wrap();
        public abstract void End();
    }
}
