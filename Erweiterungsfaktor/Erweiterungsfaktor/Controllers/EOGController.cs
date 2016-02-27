using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Erweiterungsfaktor.DataAccess;
using Erweiterungsfaktor.Models;
using Microsoft.AspNet.Identity;

namespace Erweiterungsfaktor.Controllers
{
    [Authorize]
    public class EOGController : Controller
    {
        private UnitOfWork db = new UnitOfWork();

        // GET: EOG
        public ActionResult Index(int? id)
        {
            Regulierungsperiode rp;
            if (id == null)
            {
                //Hole aktuelle RegP
                rp = db.Regulierungsperioden.Get(
                    o => o.StartDate <= DateTime.Now && 
                    o.EndDate >= DateTime.Now).FirstOrDefault();
            }
            else
            {
                //Hole angegebene RP
                rp = db.Regulierungsperioden.GetByID(id );
            }
            //Es wurde eine RP gefunden
            if (rp != null)
            {
                EOGViewModel vm = new EOGViewModel()
                {
                    RegPNumber = rp.RegPId,
                    RegPStart=rp.StartDate,
                    RegPEnd =rp.EndDate,
                    RegPMax = db.Regulierungsperioden.dbSet.Max(r => r.RegPId ),
                    Netzbetreiber = new List<Netzbetreiber>()
                };
                //Das automatische Nachladen von Daten wird deaktiviert
                db.Users.context.Configuration.LazyLoadingEnabled = false;

                ApplicationUser user = db.Users.GetByID(User.Identity.GetUserId());

                //Lade die verknüpften und bestätigten NBs des Nutzers sowie alle Netze, EOGs etc.
                db.Users.context.Entry(user)
                    .Collection(u => u.NetzbetreiberRelationships)  //integriere die Beziehungen des Nutzers
                    .Query()                                        //filtere die Beziehungen
                    .Where (rs => rs.Confirmed == true)
                    .Include(rs => rs.Netzbetreiber.Netze           //integriere die Netzbetreiber und Netze der einzelnen Beziehungen
                        .Select(n => n.EOGs))                       //integriere alle EOGs der Netze
                    .Load();                                        //lade die Daten aus der DB

                //Nur falls der Nutzer bestätigte NBs hat
                if (user.NetzbetreiberRelationships != null)
                {
                    //Filtere alle Netzbetreiber nach vorhandenen Netzen in der Periode
                    List<Netzbetreiber> nbs = (from rs in user.NetzbetreiberRelationships
                                               select rs.Netzbetreiber).ToList();
                    foreach (Netzbetreiber nb in nbs)
                    {
                        List<Netz> netze = (from n in nb.Netze
                                            where n.RegPId == rp.RegPId
                                            select n).ToList();

                        if (netze.Count == 0)
                        {
                            continue;
                        }
                        nb.Netze = netze;
                        vm.Netzbetreiber.Add(nb);
                    }
                    //Das automatische Nachladen wird wieder aktiviert
                    db.Users.context.Configuration.LazyLoadingEnabled = true;
                    return View(vm);
                }
                //Das automatische Nachladen wird wieder aktiviert
                db.Users.context.Configuration.LazyLoadingEnabled = true;
                //Hier ein Redirect zur Beantragung ohne bestätigten NB
                return View(vm);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        // GET: EOG/CreateEWF
        public ActionResult CreateEWF(int eogId,int netzId)
        {
            if (eogId!=0 && netzId != 0)
            {
                Netz netz = db.Netze.GetByID(netzId);
                if (netz == null)
                {
                    return HttpNotFound();
                }
                //View-Model mit erstellen
                EWFViewModel vm = new EWFViewModel()
                {
                    EOGId = eogId,
                    NetzId = netzId,
                    VersorgteFlaeche0 = netz.Basisjahr.VersorgteFlaeche,
                    AnzahlAusspeisepunkte0 = netz.Basisjahr.AnzahlAusspeisepunkte,
                    Jahreshoechstlast0 = netz.Basisjahr.Jahreshoechstlast,
                    RestwertNetzanlagen = netz.Basisjahr.RestwertNetzanlagen,
                    RestwertRegelanlagen = netz.Basisjahr.RestwertRegelanlagen
                     
                };
                ViewBag.Title = "Antrag stellen";
                return View(vm);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: EOG/CreateEWF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEWF(
            [Bind(Include = "EOGId, NetzId, VersorgteFlaechet, AnzahlAusspeisepunktet, Jahreshoechstlastt")]
            EWFViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Netz netz = db.Netze.GetByID(vm.NetzId);
                if (netz == null)
                {
                    return HttpNotFound();
                }
                vm.VersorgteFlaeche0 = netz.Basisjahr.VersorgteFlaeche;
                vm.AnzahlAusspeisepunkte0 = netz.Basisjahr.AnzahlAusspeisepunkte;
                vm.Jahreshoechstlast0 = netz.Basisjahr.Jahreshoechstlast;
                vm.RestwertNetzanlagen = netz.Basisjahr.RestwertNetzanlagen;
                vm.RestwertRegelanlagen = netz.Basisjahr.RestwertRegelanlagen;
                vm.EOGs = netz.EOGs;
                //EOG, für die Antrag gestellt werden soll, holen
                EOG eog = (from n in vm.EOGs
                           where n.EOGId == vm.EOGId
                           select n).First();
                //EWF für das View-Model in EOG einsetzen (wird im VM berechnet)
                eog.EFt = vm.EWFGesamt;
                //der View EWFConfirm wird für Antrag stellen und Antrag bearbeiten verwendet
                ViewBag.Title = "Antrag stellen";
                ViewBag.SendText = "Wollen Sie den Antrag mit diesen Daten verbindlich an die Bundesnetzagentur übermitteln?";
                return View("EWFConfirm",vm);
            }
            return View(vm);
        }

        // POST: EOG/ConfirmEWF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmEWF(
            [Bind(Include = "EOGId, NetzId, VersorgteFlaechet, AnzahlAusspeisepunktet, Jahreshoechstlastt")]
            EWFViewModel vm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = db.Users.GetByID(User.Identity.GetUserId());
                Basisjahr bj = db.Basisjahre.GetByID(vm.NetzId);
                EOG eog = db.EOGs.GetByID(vm.NetzId, vm.EOGId);
                if (user == null || bj == null || eog == null)
                {
                    return HttpNotFound();
                }
                //Antrag erstellen
                AntragEWF ewf = new AntragEWF()
                {
                    VersorgteFlaeche = vm.VersorgteFlaechet,
                    AnzahlAusspeisepunkte = vm.AnzahlAusspeisepunktet,
                    Jahreshoechstlast = vm.Jahreshoechstlastt,
                    User = user,
                    Date = DateTime.Now,
                    EOG = eog
                };
                //AntragEWF-Entity speichern
                db.EWFs.Insert(ewf);
                //EWF für die gespeicherte EOG einsetzen (wird in AntragEWF berechnet)
                eog.EFt = ewf.EFt(bj);
                //EOG-Entity speichern 
                db.EOGs.Update(eog);
                db.Save();
                return RedirectToAction("Index");
            }
            return View(vm);
            
        }

        // GET: EOG/EditEWF
        public ActionResult EditEWF(int EOGId, int NetzId)
        {
            if (EOGId == 0 || NetzId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Lade Basisjahr der EOG
            Basisjahr bj = db.Basisjahre.GetByID(NetzId);
            if (bj == null)
            {
                return HttpNotFound();
            }
            //Lade alle Anträge der EOG
            List<AntragEWF> antraege = db.EWFs.Get(e => e.NetzId == NetzId && e.EOGId == EOGId).ToList();
            if (antraege.Count == 0)
            {
                return HttpNotFound();
            }
            //Hole den neuesten Antrag
            AntragEWF ewf = antraege.Where(e => e.Date == antraege.Max(x => x.Date)).FirstOrDefault ();
            if (ewf == null || bj == null)
            {
                return HttpNotFound();
            }
            EWFViewModel vm = new EWFViewModel()
            {
                EOGId = ewf.EOGId,
                NetzId = ewf.NetzId,
                VersorgteFlaechet = ewf.VersorgteFlaeche,
                AnzahlAusspeisepunktet = ewf.AnzahlAusspeisepunkte,
                Jahreshoechstlastt = ewf.Jahreshoechstlast,
                VersorgteFlaeche0 = bj.VersorgteFlaeche,
                AnzahlAusspeisepunkte0 = bj.AnzahlAusspeisepunkte,
                Jahreshoechstlast0 = bj.Jahreshoechstlast,
                RestwertNetzanlagen = bj.RestwertNetzanlagen,
                RestwertRegelanlagen = bj.RestwertRegelanlagen
            };
            ViewBag.Title = "Antrag bearbeiten";
            return View(vm);
        }

        // POST: EOG/EditEWF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEWF(
            [Bind(Include = "EOGId, NetzId, VersorgteFlaechet, AnzahlAusspeisepunktet, Jahreshoechstlastt")]
            EWFViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Netz netz = db.Netze.GetByID(vm.NetzId);
                if (netz == null)
                {
                    return HttpNotFound();
                }
                vm.VersorgteFlaeche0 = netz.Basisjahr.VersorgteFlaeche;
                vm.AnzahlAusspeisepunkte0 = netz.Basisjahr.AnzahlAusspeisepunkte;
                vm.Jahreshoechstlast0 = netz.Basisjahr.Jahreshoechstlast;
                vm.RestwertNetzanlagen = netz.Basisjahr.RestwertNetzanlagen;
                vm.RestwertRegelanlagen = netz.Basisjahr.RestwertRegelanlagen;
                vm.EOGs = netz.EOGs;
                //EOG, für die Antrag gestellt werden soll, holen
                EOG eog = (from n in vm.EOGs
                           where n.EOGId == vm.EOGId
                           select n).FirstOrDefault();
                //EWF für das View-Model in EOG einsetzen (wird im VM berechnet)
                eog.EFt = vm.EWFGesamt;
                //der View EWFConfirm wird für Antrag stellen und Antrag bearbeiten verwendet
                ViewBag.SendText = "Wollen Sie Ihren Antrag mit diesen Daten abändern und an die Bundesnetzagentur übermitteln?";
                ViewBag.Title = "Antrag bearbeiten";
                return View("EWFConfirm", vm);
            }
            return View(vm);
        }

        // GET: EOG/Delete/5
        public ActionResult DeleteEWF(int eogId, int netzId)
        {
            if (eogId == 0 || netzId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EOG eog = db.EOGs.GetByID (netzId, eogId);
            if (eog == null)
            {
                return HttpNotFound();
            }
            return View(eog);
        }

        // POST: EOG/Delete/5
        [HttpPost, ActionName("DeleteEWF")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEWFConfirmed(int eogId, int netzId)
        {
            if (eogId == 0 || netzId == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.GetByID(User.Identity.GetUserId());
            Basisjahr bj = db.Basisjahre.GetByID(netzId);
            EOG eog = db.EOGs.GetByID(netzId, eogId);
            if (user == null || bj == null || eog == null)
            {
                return HttpNotFound();
            }
            //Der gestellte Antrag wird nicht gelöscht, sondern geupdatet mit Parameter-Werten = 0.
            //So kann die Antragshistorie später immernoch rekonstruiert werden.
            AntragEWF ewf = new AntragEWF()
            {
                VersorgteFlaeche = 0d,
                AnzahlAusspeisepunkte = 0,
                Jahreshoechstlast = 0d,
                User = user,
                Date = DateTime.Now,
                EOG = eog
            };
            db.EWFs.Insert(ewf);
            eog.EFt = 1d;
            db.EOGs.Update(eog);
            db.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}