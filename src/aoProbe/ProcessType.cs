using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aoProbe
{
    public class ProcessType
    {
        protected Queue<string> _queue;

        public ProcessType(string[] ops)
        {
            _queue = new Queue<string>(ops);
        }

        public void ExecAction(string name)
        {
            if (_queue.Peek() != name)
                throw new Exception(string.Format("Wrong operation. Expected={0}, Actual={1}", _queue.Peek(),name));
            else
                _queue.Dequeue();
        }
    }
}
