using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PP3.Models
{
    public class Patent
    {
        public int ID { get; set; }
        public string Name { get; set; } //+

        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; } //
        public string CPC { get; set; }      //
        public string Link { get; set; }     //+
        public string Country { get; set; }  //
        public string Autors { get; set; }   //
    }
}
