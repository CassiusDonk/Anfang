using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using Anfang.Powersystem;
using System.Collections.ObjectModel;
using Anfang.LogicDevices;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anfang
{
    public class ProtectionDevice
    {
        public string label { get; set; }
        public string description { get; set; }
        public List<AnalogInputLink> analogInputLinks = new List<AnalogInputLink>();
        public List<DiscreteInputLink> discreteInputLinks = new List<DiscreteInputLink>();
        public List<BreakerLink> breakerLinks = new List<BreakerLink>();
        public List<ProtectionFunctionV2> protectionFunctions = new List<ProtectionFunctionV2>();
        public int sim_time = 0;
        public int sim_time_step = 0;
        public PowSys powersystem = new PowSys();
        public List<AnalogSignalV2> internalAnalogs = new List<AnalogSignalV2>();
        public List<DiscrSignalV2> internalDiscretes = new List<DiscrSignalV2>();

        public ProtectionDevice() { }
        public void UpdateInternalSignals(PowSys powersystem)  // Creates or updates analog inputs as internal analog signals.
        {
            internalAnalogs.Clear();
            internalDiscretes.Clear();
            foreach (var analogInputLink in analogInputLinks)
            {
                PowSysElementBase element = powersystem.FindByID(analogInputLink.id);
                AnalogSignalV2 AnalogFromModel = new AnalogSignalV2();
                AnalogFromModel.Label = $"Ток, фаза {analogInputLink.phase} - Элемент {analogInputLink.id}";
                if (analogInputLink.isVoltage == false)
                {
                    if (analogInputLink.side == 1)
                    {
                        if (element.currents_side1.Count() > 0)
                        {
                            if (analogInputLink.phase == "A") { AnalogFromModel.InputsComplex32.Add(element.currents_side1[0]); }
                            if (analogInputLink.phase == "B") { AnalogFromModel.InputsComplex32.Add(element.currents_side1[1]); }
                            if (analogInputLink.phase == "C") { AnalogFromModel.InputsComplex32.Add(element.currents_side1[2]); }
                            if (analogInputLink.phase == "N") { AnalogFromModel.InputsComplex32.Add(element.currents_side1[3]); }
                        }
                    }
                    if (analogInputLink.side == 2)
                    {
                        if (element.currents_side2.Count() > 0)
                        {
                            if (analogInputLink.phase == "A") { AnalogFromModel.InputsComplex32.Add(element.currents_side2[0]); }
                            if (analogInputLink.phase == "B") { AnalogFromModel.InputsComplex32.Add(element.currents_side2[1]); }
                            if (analogInputLink.phase == "C") { AnalogFromModel.InputsComplex32.Add(element.currents_side2[2]); }
                            if (analogInputLink.phase == "N") { AnalogFromModel.InputsComplex32.Add(element.currents_side2[3]); }
                        }
                    }
                }
                else
                {
                    if (analogInputLink.side == 1)
                    {
                        if (element.voltages_side1.Count() > 0)
                        {
                            if (analogInputLink.phase == "A") { AnalogFromModel.InputsComplex32.Add(element.voltages_side1[0]); }
                            if (analogInputLink.phase == "B") { AnalogFromModel.InputsComplex32.Add(element.voltages_side1[1]); }
                            if (analogInputLink.phase == "C") { AnalogFromModel.InputsComplex32.Add(element.voltages_side1[2]); }
                            if (analogInputLink.phase == "N") { AnalogFromModel.InputsComplex32.Add(element.voltages_side1[3]); }
                        }
                    }
                    if (analogInputLink.side == 2)
                    {
                        if (element.voltages_side2.Count > 0)
                        {
                            if (analogInputLink.phase == "A") { AnalogFromModel.InputsComplex32.Add(element.voltages_side2[0]); }
                            if (analogInputLink.phase == "B") { AnalogFromModel.InputsComplex32.Add(element.voltages_side2[1]); }
                            if (analogInputLink.phase == "C") { AnalogFromModel.InputsComplex32.Add(element.voltages_side2[2]); }
                            if (analogInputLink.phase == "N") { AnalogFromModel.InputsComplex32.Add(element.voltages_side2[3]); }
                        }
                    }
                }
                AnalogFromModel.ProcessInputs();
                internalAnalogs.Add(AnalogFromModel);
            }
            foreach (var discreteInputLink in discreteInputLinks)
            {
                PowSysElementBase element = powersystem.FindByID(discreteInputLink.id);
                DiscrSignalV2 DiscrFromModel = new DiscrSignalV2();
                DiscrFromModel.Label = $"Дискрет, фаза {discreteInputLink.phase} - Элемент {discreteInputLink.id}";
                bool PoleA = false;
                if (element.property1 == 1) { PoleA = true; }
                bool PoleB = false;
                if (element.property2 == 1) { PoleB = true; }
                bool PoleC = false;
                if (element.property3 == 1) { PoleC = true; }

                if (discreteInputLink.phase == "A") { DiscrFromModel.InputsBool.Add(PoleA); }
                if (discreteInputLink.phase == "B") { DiscrFromModel.InputsBool.Add(PoleB); }
                if (discreteInputLink.phase == "C") { DiscrFromModel.InputsBool.Add(PoleC); }
                DiscrFromModel.ProcessInputs();
                internalDiscretes.Add(DiscrFromModel);
            }
            foreach (var protectionFunction in protectionFunctions)
            {
                if (protectionFunction.LogicDevices.Find(x => x.IsOutputOfFunction == true) != null)
                {
                    BaseLogicV2 FunctionOutput = protectionFunction.LogicDevices.Find(x => x.IsOutputOfFunction == true);
                    if (FunctionOutput.OutputsAreBool)
                    {
                        internalDiscretes.Add(new DiscrSignalV2() { OutputBool = FunctionOutput.OutputBool, Label = $"Функция {protectionFunction.Label} - Выход {FunctionOutput.Label} (дискрет)" });
                    }
                    else
                    {
                        internalAnalogs.Add(new AnalogSignalV2() { OutputComplex32 = FunctionOutput.OutputComplex32, Label = $"Функция {protectionFunction.Label} - Выход {FunctionOutput.Label} (числовой)" });
                    }
                }
            }
        }
        public void ProcessProtectionFunctions()
        {
            foreach (var protectionFunction in protectionFunctions)
            {
                protectionFunction.SimTime = sim_time;
                protectionFunction.SimTimeStep = sim_time_step;
                protectionFunction.ProcessLogic(internalAnalogs, internalDiscretes);
                foreach (var breakerLink in breakerLinks)
                {
                    if (internalDiscretes.Find(x => x.Label == breakerLink.discreteOutput).OutputBool == true)
                    {
                        powersystem.FindByID(breakerLink.brerakerID).property1 = 0;
                        powersystem.FindByID(breakerLink.brerakerID).property2 = 0;
                        powersystem.FindByID(breakerLink.brerakerID).property3 = 0;
                        TripBreaker(protectionFunction.Label);
                    }
                }
            }
        }

        public void ResetDevices()
        {
            foreach (var protectionFunction in protectionFunctions)
            {
                foreach (var logicDevice in protectionFunction.LogicDevices)
                {
                    logicDevice.InternalTime = 0;
                }
            }
        }

        public event PropertyChangedEventHandler Trip;
        public void TripBreaker([CallerMemberName] String propertyName = "")
        {
            Trip?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler Start;
        public void ProtStart([CallerMemberName] String propertyName = "")
        {
            Start?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProtectionDevice CreateCopy(ProtectionDevice original)
        {
            ProtectionDevice copy = new ProtectionDevice();
            copy.label = original.label.ToString();
            copy.description = original.label.ToString();
            foreach (var item in original.analogInputLinks)
            {
                copy.analogInputLinks.Add(item);
            }
            foreach (var item in original.discreteInputLinks)
            {
                copy.discreteInputLinks.Add(item);
            }
            foreach (var item in original.breakerLinks)
            {
                copy.breakerLinks.Add(item);
            }
            foreach (var item in original.protectionFunctions)
            {
                copy.protectionFunctions.Add(item);
            }
            return copy;
        }

        public List<string> ConvertToString()
        {
            List<string> result = new List<string>();
            result.Add("ProtectionDeviceStart");
            result.Add(label);
            result.Add(description);

            result.Add("AnalogInputLinksStart");
            foreach (var analogInputLink in analogInputLinks)
            {
                result.Add(analogInputLink.ConvertToString());
            }
            result.Add("AnalogInputLinksEnd");

            result.Add("DiscreteInputLinksStart");
            foreach (var discreteInputLink in discreteInputLinks)
            {
                result.Add(discreteInputLink.ConvertToString());
            }
            result.Add("DiscreteInputLinksEnd");

            result.Add("BreakerLinksStart");
            foreach (var breakerLink in breakerLinks)
            {
                result.Add(breakerLink.ConvertToString());
            }
            result.Add("BreakerLinksEnd");

            result.Add("ProtectionFunctionsStart");
            foreach (var protectionFunction in protectionFunctions)
            {
                result.AddRange(protectionFunction.ConvertToString());
                protectionFunction.ConvertToString2();
            }
            result.Add("ProtectionFunctionsEnd");

            result.Add("ProtectionDeviceEnd");
            return result;
        }
        public List<string> ConvertToString2()
        {
            List<string> result = new List<string>();
            UniversalSaver universalSaver = new UniversalSaver();
            result.Add("ProtectionDeviceBegin");
            result.AddRange(universalSaver.DumpToText(this));
            foreach (var protectionFunction in protectionFunctions)
            {
                result.AddRange(protectionFunction.ConvertToString2());
            }
            return result;
        }
    }
}
