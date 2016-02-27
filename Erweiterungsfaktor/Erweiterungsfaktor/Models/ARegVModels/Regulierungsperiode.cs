using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class Regulierungsperiode
    {
        [Key]
        public int RegPId { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Number { get; set; }

        //Navigation Properties
        public virtual List<Netz> Netze { get; set; }
    }
}