using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anfang.Powersystem;
using System.Collections.ObjectModel;
using MathNet.Numerics;


namespace Anfang
{
    public class ResultsDisplay
    {
        public int id { get; set; }
        public bool voltage { get; set; }
        public int side { get; set; }
        public bool magnitude { get; set; }
        public bool invert { get; set; }
        public string results { get; set; }
        public ResultsDisplay()
        {
        
        }
        public void BuildResults(ObservableCollection<PowSysElementBase> powersystem)
        {
            PowSysElementBase element = FindByID(powersystem, id);
            Complex32 A = new Complex32();
            Complex32 B = new Complex32();
            Complex32 C = new Complex32();
            if (voltage == false)
            {
                if (side == 1)
                {
                    A = element.currents_side1[0];
                    B = element.currents_side1[1];
                    C = element.currents_side1[2];
                }
                if (side == 2)
                {
                    A = element.currents_side2[0];
                    B = element.currents_side2[1];
                    C = element.currents_side2[2];
                }
            }
            else
            {

            }
            if (invert)
            {
                A = -A;
                B = -B;
                C = -B;
            }
            if (magnitude)
            {
                A = new Complex32(A.Magnitude, RadsToDegs(A.Phase));
                B = new Complex32(B.Magnitude, RadsToDegs(B.Phase));
                C = new Complex32(C.Magnitude, RadsToDegs(C.Phase));
                float RadsToDegs(float rads)
                {
                    float degs = rads * 180 / MathF.PI;
                    return degs;
                }
            }

            if (voltage)
            {

            }
            else
            {
                string newLine = Environment.NewLine;
                results = $"{element.type}{id}, side = {side}, amps: A={A}; B={B}; C={C} {newLine}";
            }
        }

        public PowSysElementBase FindByID(ObservableCollection<PowSysElementBase> powersystem, int id)
        {
            PowSysElementBase result = new PowSysElementBase();
            foreach (var item in powersystem)
            {
                if (item.id == id)
                {
                    result = item;
                    return result;
                }
            }
            return result;
        }
    }
}
