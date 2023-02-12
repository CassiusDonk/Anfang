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
    class AND : BaseLogic
    {
        public AND(string label)
        {
            this.label = label;
        }

        public override void NotifyInputBoolListChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change. This uses BoolList input.
            if (input_bool_list.Contains(true))
            {
                output = true;
            }
            else
            {
                output = false;
            }
        }
    }
}
