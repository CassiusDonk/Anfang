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
    public class Comparator : INotifyPropertyChanged
    {
        Complex32 input_old = new Complex32();
        public Complex32 input
        {
            get
            {
                return input_old;
            }
            set
            {
                if (value != this.input_old)
                {
                    this.input_old = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Complex32 triplevel = new Complex32();
        public bool output = false;
        public Comparator()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        { // Updates output value on input change.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (input.Magnitude >= triplevel.Magnitude)
            {
                output = true;
            }
            else
            {
                output = false;
            }
        }

    }
}
