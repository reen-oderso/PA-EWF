using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class Basisjahr
    {
        //Navigation Properties und Foreign Keys
        [Key, ForeignKey("Netz")]
        public int NetzId { get; set; }
        public virtual Netz Netz { get; set; }

        [Display(Name = "Basisjahr Beginn")]
        [DataType(DataType.Date)]
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:dd.mm.yyyy}", NullDisplayText = "n/a")]
        public DateTime? StartDate { get; set; }

        [Display (Name ="Basisjahr Ende")]
        [DataType(DataType.Date)]
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:dd.mm.yyyy}", NullDisplayText = "n/a")]
        public DateTime? EndDate { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double Netzkosten { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double KAdnb { get; set; }

        public double Effizienzwert { get; set; }

        public double Verteilungsfaktor { get; set; }
        [Display(Name = "Restwert der Netzanlagen")]
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double RestwertNetzanlagen { get; set; }

        [Display(Name = "Restwert der Regelanlagen")]
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double RestwertRegelanlagen { get; set; }

        [Display(Name = "Versorgte Fläche (km²)")]
        public double VersorgteFlaeche { get; set; }

        [Display(Name = "Ausspeisepunkte (Anzahl)")]
        public int AnzahlAusspeisepunkte { get; set; }

        [Display(Name = "Jahreshöchstlast (m³/h")]
        public double Jahreshoechstlast { get; set; }
    }
}