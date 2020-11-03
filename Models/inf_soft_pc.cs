using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Sample.Models
{
    public class inf_soft_pc : inf_software
    {
        public int id_soft_pc { get; set; }
        public int id_pc { get; set; }
        public int count { get; set;}
    }
}
