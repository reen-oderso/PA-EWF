using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class UserNetzbetreiberRelationship
    {
        [Key, ForeignKey("User")]
        [Column(Order = 1)]
        public string Id { get; set; }
        [Key, ForeignKey("Netzbetreiber")]
        [Column(Order = 2)]
        public int BNR { get; set; }

        public bool Confirmed { get; set; }
        //Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual Netzbetreiber Netzbetreiber { get; set; }
    }
}