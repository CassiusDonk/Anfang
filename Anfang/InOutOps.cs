using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Data;
using System.Collections.ObjectModel;

namespace Anfang
{
    public class InOutOps
    {
        public List<Branch> GetBranches_after(ObservableCollection<Branch> datagrid_collection, ObservableCollection<Branch> shockpointgrid_collection)
        { // builds a lsit of branches from both datagrids.
            List<Branch> branches = new List<Branch>();
            foreach (Branch branch in datagrid_collection)
            {
                branches.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms_Act = branch.Ohms_Act, Ohms_React = branch.Ohms_React, E_Act = branch.E_Act, E_React = branch.E_React });
            }
            if (shockpointgrid_collection.Count > 0)
            {
                foreach (Branch branch in shockpointgrid_collection)
                {
                    branches.Add(new Branch() { Number = branches.Count + 1, Node1 = branch.Node1, Node2 = 0, Ohms_Act = branch.Ohms_Act, Ohms_React = branch.Ohms_React, E_Act = 0, E_React = 0 });
                }
            }
            return branches;
        }
        public List<Branch> GetBranches_before(ObservableCollection<Branch> datagrid_collection)
        { // builds a list of branches from the main datagrid only.
            List<Branch> branches = new List<Branch>();
            foreach (Branch branch in datagrid_collection)
            {
                branches.Add(new Branch() { Number = branch.Number, Node1 = branch.Node1, Node2 = branch.Node2, Ohms_Act = branch.Ohms_Act, Ohms_React = branch.Ohms_React, E_Act = branch.E_Act, E_React = branch.E_React });
            }
            return branches;
        }
    }
}
