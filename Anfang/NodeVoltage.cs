using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Anfang
{
    public class NodeVoltage
    {
        public int Node { get; set; }
        public Complex32 Voltage { get; set; }
    }
}
