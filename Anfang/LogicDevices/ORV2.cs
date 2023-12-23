using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class ORV2 : BaseLogicV2
    {
        public ORV2()
        {
            InputsAreBool = true;
            InputCountFixed = false;
            OutputsAreBool = true;
        }

        public override void ProcessInputs()
        {
            if (InputsBool.Contains(true))
            {
                OutputBool = true;
            }
            else
            {
                OutputBool = false;
            }
        }
    }
}
