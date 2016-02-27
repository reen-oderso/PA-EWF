using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class GridViewModel
    {
        public int NetzId { get; set; }
        [Display(Name = "Netznummer")]
        public int NetzNr { get; set; }
        [Display(Name = "Netznamme")]
        public string NetzName { get; set; }
        public int BNR { get; set; }
        public int RegPId { get; set; }

        [Display(Name = "Basisjahr Beginn")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Basisjahr Ende")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public double Netzkosten { get; set; }
        public double KAdnb { get; set; }

        [Range(0.6, 1.00,
            ErrorMessage = "Effizienzwert muss zwischen 0,6 und 1 liegen")]
        public double Effizienzwert { get; set; }
        [Range(0.01, 1.00,
            ErrorMessage = "Verteilungsfaktor muss zwischen 0,6 und 1 liegen")]
        public double Verteilungsfaktor { get; set; }
        [Display(Name = "Restwert der Netzanlagen")]
        public double RestwertNetzanlagen { get; set; }
        [Display(Name = "Restwert der Regelanlagen")]
        public double RestwertRegelanlagen { get; set; }
        [Display(Name = "Versorgte Fläche")]
        public double VersorgteFlaeche { get; set; }
        [Display(Name = "Anzahl Ausspeisepunkte")]
        public int AnzahlAusspeisepunkte { get; set; }
        [Display(Name = "Jahreshöchstlast")]
        public double Jahreshoechstlast { get; set; }

        //Parameterloser Konstruktor
        public GridViewModel()
        {

        }
        //Konstruktor für Datenübernahme von Netz-/Basisjahr-Objekt
        public GridViewModel(Netz netz)
        {
            NetzId = netz.NetzId;
            NetzNr = netz.NetzNr;
            NetzName = netz.NetzName;
            BNR = netz.BNR;
            RegPId = netz.RegPId;
            StartDate = netz.Basisjahr.StartDate;
            EndDate = netz.Basisjahr.EndDate;
            Netzkosten = netz.Basisjahr.Netzkosten;
            KAdnb = netz.Basisjahr.KAdnb;
            Effizienzwert = netz.Basisjahr.Effizienzwert;
            Verteilungsfaktor = netz.Basisjahr.Verteilungsfaktor;
            RestwertNetzanlagen = netz.Basisjahr.RestwertNetzanlagen;
            RestwertRegelanlagen = netz.Basisjahr.RestwertRegelanlagen;
            VersorgteFlaeche = netz.Basisjahr.VersorgteFlaeche;
            AnzahlAusspeisepunkte = netz.Basisjahr.AnzahlAusspeisepunkte;
            Jahreshoechstlast = netz.Basisjahr.Jahreshoechstlast;
        }
    }
}