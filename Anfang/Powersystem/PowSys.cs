using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anfang.Powersystem;
using System.Collections.ObjectModel;
using Anfang.LogicDevices;

namespace Anfang.Powersystem
{
    public class PowSys : ObservableCollection<PowSysElementBase>
    {
        public PowSys() { }
        public PowSysElementBase FindByID(int id)
        {
            PowSysElementBase result = new PowSysElementBase();
            foreach (var item in this)
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
