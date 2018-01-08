using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DontTowMeBro.ViewModels
{
    [DataContract]
    public class FindMyCarViewModel
    {
        [DataMember]
       public List<string> dirList = new List<string>();
       
    }
}
