using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeProbe.Configuration
{
    /// <summary>
    /// Configuration reader base class, containing helper methods to allow class dynamic building by reflection.
    /// </summary>
    public abstract class ConfigurationResolverBase
    {
        #region protected methods

        /// <summary>
        /// Return the deafult value of a type, like default(T).
        /// </summary>
        /// <param name="type">Desired type</param>
        /// <returns>Default value</returns>
        protected object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        /// <summary>
        /// Build the default constructor argument list, given a type and a CodeProbe configuration element ArgumentCollection.
        /// </summary>
        /// <param name="type">Desired type</param>
        /// <param name="args">Arguments configuration</param>
        /// <returns>Array of arguments, to pass to the default constructor.</returns>
        protected object[] BuildArgsList(Type type, CodeProbeConfigurationSection.ArgumentElementCollection args)
        {
            Dictionary<ParameterInfo, object> result = new Dictionary<ParameterInfo, object>();

            foreach (ParameterInfo p in type.GetConstructors()[0].GetParameters())
            {
                result.Add(p, GetDefault(p.ParameterType));
            }
            if (args != null && args.Count > 0)
            {
                foreach (CodeProbeConfigurationSection.ArgumentElementCollection.ArgumentElement arg in args)
                {
                    ParameterInfo tmp = result.Keys.FirstOrDefault(k => k.Name == arg.Name);
                    if (tmp != null)
                    {
                        result[tmp] = Convert.ChangeType(arg.Value, tmp.ParameterType);
                    }
                }
            }

            return result.Values.ToArray();
        }

        #endregion
    }
}
