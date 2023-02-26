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
using System.Timers;
using System.Globalization;
using System.Windows.Threading;
using System.Reflection;

namespace Anfang
{
    public class CustomObservable : ObservableCollection<Branch>
    {
        public CustomObservable()
        { }

        public void UpdateBranch(Branch branch, Branch branch_new)
        {
            Branch branch_old = this[IndexOf(branch)];

            this.Remove(branch);

            PropertyInfo[] properties_new = branch_new.GetType().GetProperties();
            PropertyInfo[] properties_old = branch_old.GetType().GetProperties();
            int i = 0;

            foreach (var property in properties_new)
            {
                if (property.Name != "Current")
                {
                    object new_property_value = property.GetValue(branch_new);
                    properties_old[i].SetValue(branch_old, new_property_value);
                }
                else
                {
                    // keep old current values
                }
                i++;
            }
            this.Add(branch);
        }

        public void UpdateProperty(Branch branch, string property_name, object property_new, bool force_update_event)
        {
            if (force_update_event == true)
            {
                Branch branch_old = this[IndexOf(branch)];

                this.Remove(branch);

                Type br = branch.GetType();

                PropertyInfo property_new_info = br.GetProperty(property_name);
                PropertyInfo[] properties_old = branch_old.GetType().GetProperties();

                int i = 0;

                foreach (var property in properties_old)
                {
                    if (property.Name == property_new_info.Name)
                    {
                        properties_old[i].SetValue(branch_old, property_new);
                    }
                    i++;
                }

                this.Add(branch_old);

                IEnumerable<Branch> sorted = this.OrderBy(branch => branch.Number);

                foreach (var item in sorted)
                {
                    this.Add(item);
                    this.Remove(item);
                }
            }
            else
            {
                Branch branch_old = this[IndexOf(branch)];

                Type br = branch.GetType();

                PropertyInfo property_new_info = br.GetProperty(property_name);
                PropertyInfo[] properties_old = branch_old.GetType().GetProperties();

                int i = 0;

                foreach (var property in properties_old)
                {
                    if (property.Name == property_new_info.Name)
                    {
                        properties_old[i].SetValue(branch_old, property_new);
                    }
                    i++;
                }
            }
        }

        public object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return Activator.CreateInstance(type);
            }
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
