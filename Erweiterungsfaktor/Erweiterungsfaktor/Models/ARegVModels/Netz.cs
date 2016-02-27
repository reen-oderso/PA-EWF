using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class Netz
    {
        [Key]
        public int NetzId { get; set; }
        public int NetzNr { get; set; }
        public string NetzName { get; set; }

        //Navigation Properties und Foreign Keys
        [ForeignKey("Regulierungsperiode")]
        public int RegPId { get; set; }
        public Regulierungsperiode Regulierungsperiode { get; set; }
        [ForeignKey("Netzbetreiber")]
        public int BNR { get; set; }
        public Netzbetreiber Netzbetreiber { get; set; }
        public virtual List<EOG> EOGs { get; set;}
        public virtual Basisjahr Basisjahr { get; set; }

    }
}