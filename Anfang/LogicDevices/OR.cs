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

        public override void NotifyInputBoolListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        public override void NotifySimTimeChanged([CallerMemberName] string propertyName = "")
        { // clears input_bool_list on new sim cycle. Have to feed sim_time before updating inputs!
            input_bool_list.Clear();
        }
    }
}
