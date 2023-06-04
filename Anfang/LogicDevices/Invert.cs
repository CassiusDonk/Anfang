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
    public class Invert : BaseLogic
    {
        public Invert(string label)
        {
            this.label = label;
            if (input_bool == true)
            {
                output = false;
            }
            if (input_bool == false)
            {
                output = true;
            }
        }

        public override void UpdateOutput()
        {
            if (input_bool == true)
            {
                output = false;
            }
            if (input_bool == false)
            {
                output = true;
            }
        }
    }
}
