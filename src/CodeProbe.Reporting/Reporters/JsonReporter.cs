using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CodeProbe.Reporting.Reporters
{
    /// <summary>
    /// Reporters that wirtes an associative array of key-value pairs containg the name-value tuples of a sample.
    /// The file name is designed by the savePath value, wich can contain a date value and a uniqe resetId, using the classic format string arguments:
    /// {0:format>}-> datetime
    /// {1}-> resetId
    /// </summary>
    public class JsonReporter : AbstractReporter
    {
        protected static string _resetId;
        
        protected string _savePath;

        static JsonReporter()
        {
            _resetId = DateTime.Now.ToFileTimeUtc().ToString();
        }

        public JsonReporter(string name, AbstractSampler sampler, string savePath):
            base(name,sampler)
        {
            _savePath = savePath;

            _sampler.SampleTaken += OnSampleTaken;
            _sampler.SamplingError += OnSamplingError;
        }
        
        protected void OnSampleTaken(AbstractSampler sampler, Dictionary<string, object> sample)
        {
            if (_started)
            {
                string tmpPath = string.Format(_savePath, DateTime.Now, _resetId);

                if (!Directory.Exists(Path.GetDirectoryName(tmpPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(tmpPath));
                
                File.AppendAllText(tmpPath, JsonConvert.SerializeObject(sample)+","+Environment.NewLine);
            }
            else
            {
                _logger.Debug("The reporter is not started, skipping sample.");
            }
        }

        protected void OnSamplingError(AbstractSampler sampler, Exception e)
        {
            _logger.Debug(string.Format("Error sampling data. started={0}",_started),e);
        }

        public override void Dispose()
        {
            _sampler.SampleTaken -= OnSampleTaken;
            _sampler.SamplingError -= OnSamplingError;

            base.Dispose();
        }
    }
}
