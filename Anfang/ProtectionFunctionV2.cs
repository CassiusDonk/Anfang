using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anfang.LogicDevices;

namespace Anfang
{
    public class ProtectionFunctionV2
    {
        public int SimTime = 0;
        public int SimTimeStep = 0;
        public string Label { get; set; }
        public string Description { get; set; }
        public List<BaseLogicV2> LogicDevices = new List<BaseLogicV2>();


        public ProtectionFunctionV2()
        {

        }
        public void ProcessLogic(List<AnalogSignalV2> InternalAnalogs, List<DiscrSignalV2> InternalDiscretes)
        {
            foreach (var logicDevice in LogicDevices)
            {
                if (logicDevice.GetType() == GetType("Anfang.LogicDevices.AnalogSignalV2"))
                {
                    if (logicDevice.IsOutputOfFunction)
                    {
                        foreach (var inputLink in logicDevice.InputLinks)
                        {
                            logicDevice.InputsComplex32.Add(LogicDevices.Find(x => x.Label == inputLink).OutputComplex32);
                        }
                    }
                    else
                    {
                        logicDevice.InputsComplex32.Add(InternalAnalogs.Find(x => x.Label == logicDevice.Label).OutputComplex32);
                    }
                }
                if (logicDevice.GetType() == GetType("Anfang.LogicDevices.DiscrSignalV2"))
                {
                    if (logicDevice.IsOutputOfFunction)
                    {
                        foreach (var inputLink in logicDevice.InputLinks)
                        {
                            logicDevice.InputsBool.Add(LogicDevices.Find(x => x.Label == inputLink).OutputBool);
                        }
                    }
                    else
                    {
                        logicDevice.InputsBool.Add(InternalDiscretes.Find(x => x.Label == logicDevice.Label).OutputBool);
                    }
                }
                if (logicDevice.GetType() != GetType("Anfang.LogicDevices.AnalogSignalV2") & logicDevice.GetType() != GetType("Anfang.LogicDevices.DiscrSignalV2"))
                {
                    foreach (var inputLink in logicDevice.InputLinks)
                    {
                        if (logicDevice.InputsAreBool)
                        {
                            logicDevice.InputsBool.Add(LogicDevices.Find(x => x.Label == inputLink).OutputBool);
                        }
                        else
                        {
                            logicDevice.InputsComplex32.Add(LogicDevices.Find(x => x.Label == inputLink).OutputComplex32);
                        }
                    }
                }
                if (logicDevice.GetType() == GetType("Anfang.LogicDevices.TimerV2"))
                {
                    logicDevice.SimTimeStep = SimTimeStep;
                    logicDevice.SimTime = SimTime;
                }
                logicDevice.ProcessInputs();
            }
        }
        public Type GetType(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return type;
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                {
                    return type;
                }
            }
            return type;
        }
    }
}
