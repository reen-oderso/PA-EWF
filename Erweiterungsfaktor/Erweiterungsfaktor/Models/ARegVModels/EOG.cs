using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class EOG
    {
        [Key,Column(Order=1)]
        public int EOGId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double KAdnbt { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double KAvnbt { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double KAbt { get; set; }
        [Display(Name = "EOG")]
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double EOGSumme {
            get {return KAdnbt + (KAvnbt + KAbt) * EFt; }
        }
        public double EFt {get ; set; }

        [Display (Name ="Jahr")]
        [DisplayFormat(ConvertEmptyStringToNull =true,DataFormatString = "{0:yyy}", NullDisplayText = "n/a", HtmlEncode = false)]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:yyy}", NullDisplayText = "n/a",HtmlEncode =false)]
        public DateTime? EndDate { get; set; }

        //Navigation Properties und Foreign Keys
        [Key, ForeignKey("Netz"), Column(Order = 0)]
        public int NetzId { get; set; }
        public virtual Netz Netz { get; set; }

        public virtual List<AntragEWF> EWFs { get; set; }

        //Parameterloser Konstruktor
        public EOG()
        {
            EFt = 1;
        }

        //Konstruktor, der die Kostenanteile automatisch berechnet und zuweist.
        public EOG(Basisjahr bj, int index)
        {
            EOGId = index;
            KAdnbt = bj.KAdnb;
            KAvnbt = (bj.Netzkosten - bj.KAdnb) * bj.Effizienzwert;
            KAbt = (bj.Netzkosten - bj.KAdnb - KAvnbt) * (1 - bj.Verteilungsfaktor * index);
            EFt = 1;
        }
        //Methode um die EOG neu zu berechnen, anstatt alle Eigenschaften einzeln zuzuweisen
        public void Update(Basisjahr bj, int index)
        {
            EOGId = index;
            KAdnbt = bj.KAdnb;
            KAvnbt = (bj.Netzkosten - bj.KAdnb) * bj.Effizienzwert;
            KAbt = (bj.Netzkosten - bj.KAdnb - KAvnbt) * (1 - bj.Verteilungsfaktor * index);
        }
    }
}