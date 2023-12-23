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
    public class DiscrSignal : BaseLogic
    {
        
        public DiscrSignal(string label)
        {
            this.label = label;
            inputType = "Boolean";
            outputType = "Boolean";
        }

        public override void UpdateOutput()
        {
            if (input_bool == true)
            {
                output = true;
            }
            if (input_bool == false)
            {
                output = false;
            }
        }
    }
}
