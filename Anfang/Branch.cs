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
using System.Reflection;
using System.Collections.ObjectModel;
using System.Timers;
using System.Globalization;
using System.Windows.Threading;

namespace Anfang
{
    public class Branch : INotifyPropertyChanged
    {
        private float Ohms_Act_Value;
        private float Ohms_React_Value;
        private float E_Act_Value;
        private float E_React_Value;
        public int Number { get; set; }
        public int Node1 { get; set; }
        public int Node2 { get; set; }
        public float Ohms_Act 
        {
            get
            {
                return Ohms_Act_Value;
            }
            set
            {
                if (value == 0 | (Enabled == true & IsBreaker == true))
                {
                    value = 0.000000001F;
                }
                if (value != this.Ohms_Act_Value)
                {
                    this.Ohms_Act_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public float Ohms_React
        {
            get
            {
                return Ohms_React_Value;
            }
            set
            {
                if (value == 0 | (Enabled == true & IsBreaker == true))
                {
                    value = 0.000000001F;
                }
                if (value != this.Ohms_React_Value)
                {
                    this.Ohms_React_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public float E_Act 
        {
            get
            {
                return E_Act_Value;
            }
            set
            {
                if (value != this.E_Act_Value)
                {
                    this.E_Act_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public float E_React 
        {
            get
            {
                return E_React_Value;
            }
            set
            {
                if (value != this.E_React_Value)
                {
                    this.E_React_Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Complex32 Ohms { get; set; }
        public Complex32 E { get; set; }
        public bool Direction { get; set; }
        public bool Reversed { get; set; }

        public bool Enabled
        {
            get
            {
                return Enabled_;
            }
            set
            {
                Enabled_ = value;
                NotifyTrip();
            }
        }

        private bool Enabled_;

        public bool IsBreaker { get; set; }

        public Complex32 Current { get; set; }

        public Complex32 Voltage_Node1 { get; set; }
        public Complex32 Voltage_Node2 { get; set; }
        public Complex32 Voltage_Drop { get; set; }

        public int id;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        { // updates complex values "E" and "Ohms" when their parents are changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "Ohms_Act" | propertyName == "Ohms_React")
            {
                Ohms = new Complex32(Ohms_Act, Ohms_React);
            }
            if (propertyName == "E_Act" | propertyName == "E_React")
            {
                E = new Complex32(E_Act, E_React);
            }
        }

        private void NotifyTrip()
        {
            if (Enabled_ == false)
            {
                Ohms_Act = 1000000000;
                Ohms_React = 1000000000;
            }
            if (Enabled_ == true)
            {
                Ohms_Act = 0;
                Ohms_React = 0;
            }
        }

        public Branch CrateCopy(Branch branch_original)
        {
            Branch branch_copy = new Branch();

            PropertyInfo[] properties_original = branch_original.GetType().GetProperties();
            PropertyInfo[] properties_copy = branch_copy.GetType().GetProperties();
            int i = 0;

            foreach (var property in properties_original)
            {
                object original_property_value = property.GetValue(branch_original);
                properties_copy[i].SetValue(branch_copy, original_property_value);
                i++;
            }

            return branch_copy;
        }
    }
}
