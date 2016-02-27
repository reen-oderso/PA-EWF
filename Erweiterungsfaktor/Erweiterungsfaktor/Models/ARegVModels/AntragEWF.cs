using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class AntragEWF
    {
        [Key]
        public int EWFId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTime Date { get; set; }

        [Display(Name = "Versorgte Fläche (km²)")]
        public double VersorgteFlaeche { get; set; }

        [Display(Name = "Ausspeisepunkte (Anzahl)")]
        public int AnzahlAusspeisepunkte { get; set; }

        [Display(Name = "Jahreshöchstlast (m³/h")]
        public double Jahreshoechstlast { get; set; }

        //Navigation Properties und Foreign Keys
        [ForeignKey("EOG"), Column(Order = 0)]
        public int NetzId { get; set; }
        
        [ForeignKey("EOG"), Column(Order = 1)] 
        public int EOGId { get; set; }
        
        public virtual EOG EOG { get; set; }
        
        [ForeignKey("User")]
        public string Id { get; set; }
        public ApplicationUser User { get; set; }

        //Methode zur Berechnung des Faktors, der sich aus dem Antrag ergibt.
        public double EFt(Basisjahr bj)
        {
            if (bj.VersorgteFlaeche == 0 || bj.AnzahlAusspeisepunkte == 0 || bj.Jahreshoechstlast == 0 || bj.RestwertNetzanlagen + bj.RestwertRegelanlagen == 0)
            {
                return 1d;
            }
            double GewichtungNetzanlagen = Math.Round(bj.RestwertNetzanlagen / (bj.RestwertNetzanlagen + bj.RestwertRegelanlagen), 2);
            double GewichtungRegelanlagen = Math.Round(bj.RestwertRegelanlagen / (bj.RestwertNetzanlagen + bj.RestwertRegelanlagen), 2);
            double EWFNetzanlagen = Math.Round((Math.Max(1d, (VersorgteFlaeche / bj.VersorgteFlaeche)) * 0.5) + (Math.Max(1d, (AnzahlAusspeisepunkte / bj.AnzahlAusspeisepunkte)) * 0.5), 4);
            double EWFRegelanlagen = Math.Round(Math.Max(1d, Jahreshoechstlast / bj.Jahreshoechstlast), 4);
            return Math.Round(Math.Max(GewichtungNetzanlagen * EWFNetzanlagen + GewichtungRegelanlagen * EWFRegelanlagen, 1d), 4);
        }
    }
}