using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Common.Base
{
    public abstract  class BaseEntity 
    {
        public int Id { get;   set; }
        public bool IsDisable { get; set; } = false;


    }
}
