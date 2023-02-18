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
    public class Comparator : BaseLogic
    {
        public Comparator(string label)
        {
            this.label = label;
        }

        public override void NotifyInputComplexChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change. This uses a single bool input.
            output = false;
            if (input_complex.Magnitude >= triplevel.Magnitude)
            {
                output = true;
            }
        }

    }
}
