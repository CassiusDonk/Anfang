using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Anfang.LogicDevices
{
    public class BaseLogicV2
    {
        public string Label { get; set; }
        public string ExtendedLabel;

        //  Inputs
        public List<string> InputLinks = new List<string>();
        public bool InputsAreBool = new bool();
        public bool InputCountFixed = new bool();
        public int InputCount = new int();
        public ObservableCollection<Complex32> InputsComplex32 = new ObservableCollection<Complex32>();
        public ObservableCollection<bool> InputsBool = new ObservableCollection<bool>();

        //  Outputs
        public List<string> OutputLinks = new List<string>();
        public bool OutputsAreBool = new bool();
        public Complex32 OutputComplex32 = new Complex32();
        public bool OutputBool = new bool();
        public bool IsOutputOfFunction = new bool();

        //  Simulation time data
        public int SimTime = new int();
        public int SimTimeStep = new int();
        public int InternalTime = new int();

        public BaseLogicV2()
        {
            InputsComplex32.CollectionChanged += InputsComplex32_CollectionChanged;
            InputsBool.CollectionChanged += InputsBool_CollectionChanged;
            IsOutputOfFunction = false;
        }

        public BaseLogicV2(List<string> InputLinks)
        {
            InputsComplex32.CollectionChanged += InputsComplex32_CollectionChanged;
            InputsBool.CollectionChanged += InputsBool_CollectionChanged;
            IsOutputOfFunction = false;
            this.InputLinks = InputLinks;
        }

        public void BuildExtendedLabels()
        {
            string type = "Undefined";
            if (this.OutputsAreBool == true) { type = "Дискрет"; }
            else { type = "Числовой"; }
            this.ExtendedLabel = $"{this.Label}, ({type})";
        }

        //  This enforces the ammount of inputs to be always equal to the allowed number if it is specified.
        private void InputsBool_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (InputCountFixed & InputsBool.Count() > InputCount)
            {
                InputsBool.RemoveAt(0);
            }
        }

        //  This enforces the ammount of inputs to be always equal to the allowed number if it is specified.
        private void InputsComplex32_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (InputCountFixed & InputsComplex32.Count() > InputCount)
            {
                InputsComplex32.RemoveAt(0);
            }
        }

        public virtual void ProcessInputs() { }

        public virtual List<string> ConvertToString()
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
            result.Add(IsOutputOfFunction.ToString());
            result.Add("LogicDeviceEnd");
            return result;
        }
    }
}
