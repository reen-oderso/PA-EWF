using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class EWFViewModel
    {
        public int EOGId { get; set; }
        public int NetzId { get; set; }

        [Display(Name = "Versorgte Fläche (km²)")]
        public double VersorgteFlaeche0 { get; set; }
        [Display(Name = "Ausspeisepunkte (Anzahl)")]
        public int AnzahlAusspeisepunkte0 { get; set; }
        [Display(Name = "Jahreshöchstlast (m³/h")]
        public double Jahreshoechstlast0 { get; set; }

        public double VersorgteFlaechet { get; set; }
        public int AnzahlAusspeisepunktet { get; set; }
        public double Jahreshoechstlastt { get; set; }

        [Display(Name = "Restwert der Netzanlagen")]
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double RestwertNetzanlagen { get; set; }
        [Display(Name = "Restwert der Regelanlagen")]
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:C}")]
        public double RestwertRegelanlagen { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:P2}")]
        public double GewichtungNetzanlagen
        {
            get
            {
                if ((RestwertNetzanlagen + RestwertRegelanlagen) == 0)
                {
                    return 0d;
                }
                return Math.Round(RestwertNetzanlagen / (RestwertNetzanlagen + RestwertRegelanlagen), 2);
            }
        }
        [DisplayFormat(ConvertEmptyStringToNull = false, DataFormatString = "{0:P2}")]
        public double GewichtungRegelanlagen
        {
            get
            {
                if ((RestwertNetzanlagen + RestwertRegelanlagen) == 0)
                {
                    return 0d;
                }
                return Math.Round(RestwertRegelanlagen / (RestwertNetzanlagen + RestwertRegelanlagen), 2);
            }
        }

        [Display(Name = "EWF der Ebene Netzanlagen")]
        public double EWFNetzanlagen {
            get
            {
                if (VersorgteFlaechet ==0 || AnzahlAusspeisepunktet == 0)
                {
                    return 1d;
                }
                return Math.Round((Math.Max(1d, (VersorgteFlaechet / VersorgteFlaeche0)) * 0.5) + (Math.Max(1d,(AnzahlAusspeisepunktet / AnzahlAusspeisepunktet)) * 0.5), 4);
            }
        }
        [Display(Name = "EWF der Ebene Regelanlagen")]
        public double EWFRegelanlagen {
            get
            {
                if (Jahreshoechstlastt  == 0)
                {
                    return 1d;
                }
                return Math.Round(Math.Max(1d, Jahreshoechstlastt /Jahreshoechstlast0 ), 4);
            }
        }
        [Display(Name = "Gewichteter EWF des Gesamtnetzes")]
        public double EWFGesamt {
            get
            {
                return Math.Round(Math.Max(GewichtungNetzanlagen * EWFNetzanlagen + GewichtungRegelanlagen * EWFRegelanlagen,1d), 4); 
            }
        }

        public List<EOG> EOGs { get; set; }
    }
}