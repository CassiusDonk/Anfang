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
    class OR : BaseLogic
    {
        public OR(string label)
        {
            this.label = label;
        }

        public override void NotifyInputBoolListChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change. This uses BoolList input.
            if (input_bool_list.Contains(false))
            {
                output = false;
            }
            else if (input_bool_list.Contains(true) & input_bool_list.Count > 0)
            {
                output = true;
            }
        }
    }
}
