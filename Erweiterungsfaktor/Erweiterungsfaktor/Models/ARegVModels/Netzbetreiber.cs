using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class Netzbetreiber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display (Name="BNR")]
        public int BNR { get; set; }
        [Display(Name="Name")]
        public string Name { get; set; }
        [Display(Name="Rechtsform")]
        public string Rechtsform { get; set; }
        [Display(Name = "Adresse")]
        public string StrasseHausNr { get; set; }
        [Display(Name = "PLZ Ort")]
        public string PLZOrt { get; set; }
        [Display(Name="Vereinfachtes Verfahren")]
        public Boolean VereinfachtesVerfahren { get; set; }
        //Navigation Properties
        public virtual List<UserNetzbetreiberRelationship> UserRelationships { get; set; }
        public virtual List<Netz> Netze { get; set; }
    }
}