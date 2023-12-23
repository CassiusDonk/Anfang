using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using System.Collections.ObjectModel;

namespace Anfang.LogicDevices
{
    public class BaseLogicV2
    {
        public string Label { get; set; }

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
    }
}
