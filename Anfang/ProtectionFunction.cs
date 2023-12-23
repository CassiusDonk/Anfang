using Anfang.Powersystem;
using Anfang.LogicDevices;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Anfang
{
    public class ProtectionFunction
    {
        public int sim_time = 0;
        public int sim_time_step = 0;
        public string label { get; set; }
        public string description { get; set; }
        public List<BaseLogic> logicDevices = new List<BaseLogic>();
        public ProtectionFunction()
        {

        }
        public void ParseLogic()
        {
            for (int i = 0; i < logicDevices.Count(); i++)
            {
                if (logicDevices[i].label.Contains("ANALOG") | logicDevices[i].label.Contains("Analog") | logicDevices[i].label.Contains("analog"))
                {

                }
                if (logicDevices[i].label.Contains("AND") | logicDevices[i].label.Contains("And") | logicDevices[i].label.Contains("and"))
                {

                }
                if (logicDevices[i].label.Contains("COMP") | logicDevices[i].label.Contains("Comp") | logicDevices[i].label.Contains("comp"))
                {

                }
                if (logicDevices[i].label.Contains("DISCRETE") | logicDevices[i].label.Contains("Discrete") | logicDevices[i].label.Contains("discrete"))
                {

                }
                if (logicDevices[i].label.Contains("INVERT") | logicDevices[i].label.Contains("Invert") | logicDevices[i].label.Contains("invert"))
                {

                }
                if (logicDevices[i].label.Contains("OR") | logicDevices[i].label.Contains("Or") | logicDevices[i].label.Contains("or"))
                {

                }
                if (logicDevices[i].label.Contains("PHASOR") | logicDevices[i].label.Contains("Phasor") | logicDevices[i].label.Contains("phasor"))
                {

                }
                if (logicDevices[i].label.Contains("TIMER") | logicDevices[i].label.Contains("Timer") | logicDevices[i].label.Contains("timer"))
                {

                }
            }
        }
        public void ProcessLogic(List<AnalogSignal> InternalAnalogs, List<DiscrSignal> InternalDiscretes)
        {
            Type analog = GetType("Anfang.LogicDevices.AnalogSignal");
            Type discrete = GetType("Anfang.LogicDevices.DiscrSignal");
            Type timer = GetType("Anfang.LogicDevices.Timer");
            for (int i = 0; i < logicDevices.Count(); i++)
            {
                if (logicDevices[i].GetType() == analog)
                {
                    logicDevices[i].input_complex = InternalAnalogs.Find(x => x.label == logicDevices[i].label).output_complex;
                }
                if (logicDevices[i].GetType() == timer)
                {
                    logicDevices[i].sim_time = sim_time;
                    logicDevices[i].sim_time_step = sim_time_step;
                }
                for (int n = i + 1; n < logicDevices.Count(); n++)
                {
                    if (logicDevices[i].outputType == logicDevices[n].inputType)
                    {
                        if (logicDevices[i].outputType == "Complex32" & logicDevices[i].GetType() != analog)
                        {
                            logicDevices[n].input_complex_list.Add(logicDevices[i].output_complex);
                        }
                        if (logicDevices[i].outputType == "Boolean")
                        {
                            logicDevices[n].input_bool_list.Add(logicDevices[i].output);
                        }
                        logicDevices[n].UpdateOutput();
                    }
                }
                if (logicDevices[i].GetType() == discrete)
                {
                    if (InternalDiscretes.Find(x => x.label == logicDevices[i].label) != null)
                    {
                        InternalDiscretes.Add(logicDevices[i] as DiscrSignal);
                    }
                    else
                    {
                        InternalDiscretes.Find(x => x.label == logicDevices[i].label).output = logicDevices[i].output;
                    }
                }
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
