using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class InvertV2 : BaseLogicV2
    {
        public InvertV2()
        {
            InputsAreBool = true;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
        }
        public override void ProcessInputs()
        {
            if (InputsBool.Count() > 0)
            {
                OutputBool = !InputsBool[0];
            }
        }
    }
}
