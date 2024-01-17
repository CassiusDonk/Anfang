using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anfang.LogicDevices
{
    public class ComparatorV2 : BaseLogicV2
    {
        public float tripLevel = new float();
        public ComparatorV2()
        {
            InputsAreBool = false;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
        }
        public ComparatorV2(List<string> InputLinks, float tripLevel)
        {
            InputsAreBool = false;
            InputCountFixed = true;
            InputCount = 1;
            OutputsAreBool = true;
            this.InputLinks = InputLinks;
            this.tripLevel = tripLevel;
        }
        public override void ProcessInputs()
        {
            if (InputsComplex32.Count() > 0)
            {
                if (InputsComplex32[0].Magnitude >= tripLevel)
                {
                    OutputBool = true;
                }
                else
                {
                    OutputBool = false;
                }
            }
        }
        public override List<string> ConvertToString()
        {
            List<string> result = new List<string>();
            result.Add("LogicDeviceStart");
            result.Add(Label);
            result.Add(this.GetType().ToString());
            result.Add("InputLinksStart");
            foreach (var inputLink in InputLinks)
            {
                result.Add(inputLink);
            }
            result.Add("InputLinksEnd");
            result.Add(tripLevel.ToString());
            result.Add("LogicDeviceEnd");
            return result;
        }
    }
}
