using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Anfang.LogicDevices
{
    public class AnalogSignal : BaseLogic
    {
        public AnalogSignal(string label)
        {
            this.label = label;
        }

        public override void NotifyInputComplexChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change. This uses complex input.
            output_complex = input_complex;
        }
    }
}
