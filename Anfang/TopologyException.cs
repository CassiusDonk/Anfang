using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang
{
    [Serializable]
    public class TopologyException : Exception
    {
        public TopologyException()
        {
        }
        public TopologyException (string message) : base (message)
        {
        }
        public TopologyException(string message, Exception inner) : base (message, inner)
        {
        }
    }
}
