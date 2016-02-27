using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Erweiterungsfaktor.Models;
using Erweiterungsfaktor.DataAccess;

namespace Erweiterungsfaktor.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NetzbetreiberController : Controller
    {
        private UnitOfWork db = new UnitOfWork();

        // GET: Admin/Netzbetreibers
        public ActionResult Index()
        {
            return View(db.Netzbetreiber.Get().ToList());
        }

        // GET: Admin/Netzbetreibers/Details/5
        public ActionResult Details(int? id, int? rpId, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netzbetreiber netzbetreiber = db.Netzbetreiber.GetByID(id);
            if (netzbetreiber == null)
            {
                return HttpNotFound();
            }
            //Wenn keine RegPId angegeben wurde, liefere die zum aktuellen Zeitpunkt aktuelle RegP, sonst die angegebene RegP.
            Regulierungsperiode rp;
            if (rpId == null)
            {
                //Hole aktuelle RegP, falls keine rpId angegeben wurde
                rp = db.Regulierungsperioden.Get(
                    o => o.StartDate <= DateTime.Now && 
                    o.EndDate >= DateTime.Now).FirstOrDefault();
            }
            else
            {
                //Hole angegebene RP
                rp = db.Regulierungsperioden.GetByID(rpId);
            }
            if (rp != null)
            {
                //Es wurde eine RP gefunden
                EOGViewModel vm = new EOGViewModel()
                {
                    RegPNumber = rp.RegPId,
                    RegPStart = rp.StartDate,
                    RegPEnd = rp.EndDate,
                    RegPMax = db.Regulierungsperioden.dbSet.Max(r => r.RegPId),
                    Netzbetreiber = new List<Netzbetreiber>()
                };
                //Nur die Netze übernehmen, die in der aktuellen RP vorhanden sind
                netzbetreiber.Netze = (from n in netzbetreiber.Netze
                                       where n.RegPId == rp.RegPId
                                       select n).ToList();
                vm.Netzbetreiber.Add(netzbetreiber);
                ViewBag.ReturnURL = returnURL;
                return View(vm);
            }
            //Es wurde keine RP gefunden
            return HttpNotFound();
        }

        // GET: Admin/Netzbetreibers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Netzbetreibers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "BNR,Name,Rechtsform,StrasseHausNr,PLZOrt,VereinfachtesVerfahren")]
            Netzbetreiber netzbetreiber)
        {
            if (ModelState.IsValid)
            {
                Netzbetreiber nb = db.Netzbetreiber.GetByID(netzbetreiber.BNR);
                if (nb != null)
                {
                    ViewBag.ErrorText = "Die angegebene BNR ist bereits vergeben.";
                    return View(netzbetreiber);
                }
                if ((netzbetreiber.BNR < 12000000) || (netzbetreiber.BNR > 12009999))
                {
                    ViewBag.ErrorText = "Die angegebene BNR ist ungültig.";
                    return View(netzbetreiber);
                }
                //BNR ist gültig und noch nicht vergeben
                db.Netzbetreiber.Insert(netzbetreiber);
                db.Save();
                return RedirectToAction("Index");
            }

            return View(netzbetreiber);
        }

        // GET: Admin/Netzbetreibers/Edit/5
        public ActionResult Edit(int? id, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netzbetreiber netzbetreiber = db.Netzbetreiber.GetByID(id);
            if (netzbetreiber == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(netzbetreiber);
        }

        // POST: Admin/Netzbetreibers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "BNR,Name,Rechtsform,StrasseHausNr,PLZOrt,VereinfachtesVerfahren")]
            Netzbetreiber netzbetreiber)
        {
            if (ModelState.IsValid)
            {
                db.Netzbetreiber.Update(netzbetreiber);
                db.Save();
                return RedirectToAction("Index");
            }
            return View(netzbetreiber);
        }

        // GET: Admin/Netzbetreibers/Delete/5
        public ActionResult Delete(int? id, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netzbetreiber netzbetreiber = db.Netzbetreiber.GetByID(id);
            if (netzbetreiber == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(netzbetreiber);
        }

        // POST: Admin/Netzbetreibers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Netzbetreiber netzbetreiber = db.Netzbetreiber.GetByID(id);
            db.Netzbetreiber.Delete(netzbetreiber);
            db.Save();
            return RedirectToAction("Index");
        }
#region Beziehung-CRUD
        // GET: Admin/Netzbetreiber/ChangeRelationship?id=4?bnr=5
        public ActionResult ChangeRelationship(string id, int? bnr, string returnURL)
        {
            //Ist die Betriebsnummer gültig
            if (id == null || bnr < 12000000 || bnr > 12009999 || returnURL == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserNetzbetreiberRelationship rs = 
                db.UserNetzbetreiberRelationships.Get(m => m.Id == id && m.BNR == bnr).First();
            //Wurde eine Relationship gefunden
            if (rs == null)
            {
                return HttpNotFound();
            }
            //Ist die RS bestätigt, dann setze falsch
            if (rs.Confirmed == true)
            {
                rs.Confirmed = false;
            }
            //Ist die RS nicht bestätigt, dann setze wahr
            else
            {
                rs.Confirmed = true;
            }
            //DB updaten
            db.UserNetzbetreiberRelationships.Update(rs);
            db.Save();
            //Zur Quell-URL zurückkehren
            return Redirect(returnURL);
        }

        // GET: Admin/Netzbetreiber/CreateRelationship/12001234
        public ActionResult CreateRelationship(int? bnr, string returnURL)
        {
            if (bnr == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netzbetreiber nb = db.Netzbetreiber.GetByID(bnr);
            if (nb == null)
            {
                return HttpNotFound();
            }
            //Erzeuge ViewModel
            EditRelationshipViewModel vm = new EditRelationshipViewModel()
            {
                BNR = nb.BNR,
                Netzbetreiber = nb,
                Confirmed = false,
                UserList = new List<SelectListItem>()
            };
            //Füge Liste der Nutzer dem ViewModel hinzu
            foreach (ApplicationUser user in db.Users.Get())
            {
                vm.UserList.Add(new SelectListItem() { Text = user.UserName, Value = user.Id });
            }
            ViewBag.returnURL = returnURL;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRelationship(EditRelationshipViewModel vm)
        {
            if (ModelState.IsValid)
            {
                UserNetzbetreiberRelationship rs = db.UserNetzbetreiberRelationships.GetByID(vm.Id, vm.BNR);
                //Prüfen ob RS schon vorhanden
                if (rs != null)
                {
                    //Beziehung bereits vorhanden
                    //Füge Liste der Nutzer dem ViewModel hinzu
                    vm.UserList = new List<SelectListItem>();
                    foreach (ApplicationUser user in db.Users.Get())
                    {
                        vm.UserList.Add(new SelectListItem() { Text = user.UserName, Value = user.Id });
                    }
                    //Anzeigen, dass RS schon vorhanden
                    ModelState.AddModelError("", "Die Verknüpfung existiert bereits.");
                    return View(vm);
                }
                //RS anlegen
                rs = new UserNetzbetreiberRelationship()
                {
                    Id = vm.Id,
                    BNR = vm.BNR,
                    Confirmed = vm.Confirmed
                };
                db.UserNetzbetreiberRelationships.Insert(rs);
                db.Save();
                return RedirectToAction("Details", "Netzbetreiber", new { Id = vm.BNR });
            }
            //Füge Liste der Nutzer dem ViewModel hinzu
            vm.UserList = new List<SelectListItem>();
            foreach (ApplicationUser user in db.Users.Get())
            {
                vm.UserList.Add(new SelectListItem() { Text = user.UserName, Value = user.Id });
            }
            //Gib Eingaben zurück
            return View(vm);
        }

        public ActionResult DeleteRelationship(string id, int? bnr, string returnURL)
        {
            if (id == null || bnr < 12000000 || bnr > 12009999)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserNetzbetreiberRelationship rs = 
                db.UserNetzbetreiberRelationships.Get(m => m.Id == id && m.BNR == bnr).First();
            if (rs == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(rs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRelationship(string id, int? bnr)
        {
            UserNetzbetreiberRelationship rs = db.UserNetzbetreiberRelationships.GetByID(id, bnr);
            db.UserNetzbetreiberRelationships.Delete(rs);
            db.Save();
            return RedirectToAction("Details", "User", new { Id = id });
        }
#endregion
#region Netze-CRUD
        public ActionResult CreateGrid(int? id, int? rpId)
        {
            if (id != null && rpId != null )
            {
                GridViewModel grid = new GridViewModel()
                {
                    BNR = id.Value,
                    RegPId = rpId.Value 
                };
                return View(grid);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        // POST: Admin/Netzbetreiber/CreateGrid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGrid(GridViewModel vm)
        {
            if (ModelState.IsValid)
            {
                //Daten holen
                Regulierungsperiode rp = db.Regulierungsperioden.GetByID(vm.RegPId);
                if (rp==null )
                {
                    return HttpNotFound();
                }
                Netzbetreiber nb = db.Netzbetreiber.GetByID(vm.BNR);
                if (nb == null)
                {
                    return HttpNotFound();
                }
                //Neues Netz erzeugen
                Netz netz = new Netz()
                {
                    BNR = vm.BNR,
                    NetzNr = vm.NetzNr,
                    NetzName = vm.NetzName,
                    Netzbetreiber = nb,
                    RegPId = vm.RegPId,
                    Regulierungsperiode = rp,
                    EOGs = new List<EOG>(),
                    Basisjahr = new Basisjahr()
                };
                //Werte fürs Basisjahr zuordnen
                netz.Basisjahr.Netz = netz;
                netz.Basisjahr.Netzkosten = vm.Netzkosten;
                netz.Basisjahr.KAdnb = vm.KAdnb;
                netz.Basisjahr.Effizienzwert = vm.Effizienzwert;
                netz.Basisjahr.Verteilungsfaktor = vm.Verteilungsfaktor;
                netz.Basisjahr.RestwertNetzanlagen = vm.RestwertNetzanlagen;
                netz.Basisjahr.RestwertRegelanlagen = vm.RestwertRegelanlagen;
                netz.Basisjahr.VersorgteFlaeche = vm.VersorgteFlaeche;
                netz.Basisjahr.AnzahlAusspeisepunkte = vm.AnzahlAusspeisepunkte;
                netz.Basisjahr.Jahreshoechstlast = vm.Jahreshoechstlast;
                netz.Basisjahr.StartDate = vm.StartDate;
                netz.Basisjahr.EndDate = vm.EndDate;
                //EOGs erzeugen
                int dauerRp = (rp.EndDate.Year - rp.StartDate.Year+1);
                for (int i= 1; i <= dauerRp; i++)
                {
                    netz.EOGs.Add(new EOG(netz.Basisjahr, i));
                    netz.EOGs.ElementAt(i - 1).Netz = netz;
                    netz.EOGs.ElementAt(i - 1).StartDate = rp.StartDate.AddYears(i - 1);
                }
                db.Netze.Insert(netz);
                db.Save();
                return RedirectToAction("Details", new { id = vm.BNR });
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult EditGrid(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netz netz = db.Netze.GetByID(id);
            db.Netze.Include(e => netz.Basisjahr);
            if (netz == null)
            {
                return HttpNotFound();
            }
            GridViewModel vm = new GridViewModel(netz);
            return View(vm);
        }
        // POST: Admin/Netzbetreiber/EditGrid/4
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGrid(GridViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Netz netz = db.Netze.GetByID(vm.NetzId);
                Regulierungsperiode rp = db.Regulierungsperioden.GetByID(netz.RegPId);
                if (netz == null)
                {
                    return HttpNotFound();
                }
                if (netz.Basisjahr.Netzkosten != vm.Netzkosten || 
                    netz.Basisjahr.KAdnb != vm.KAdnb || 
                    netz.Basisjahr.Verteilungsfaktor != vm.Verteilungsfaktor || 
                    netz.Basisjahr.Effizienzwert != vm.Effizienzwert)
                {
                    //Die EOGs müssen neu berechnet und ersetzt werden, da sich relevante Werte geändert haben
                    netz.Basisjahr.Netzkosten = vm.Netzkosten;
                    netz.Basisjahr.KAdnb = vm.KAdnb;
                    netz.Basisjahr.Effizienzwert = vm.Effizienzwert;
                    netz.Basisjahr.Verteilungsfaktor = vm.Verteilungsfaktor;

                    int dauerRp = (rp.EndDate.Year - rp.StartDate.Year);
                    for (int i = 0; i <= dauerRp; i++)
                    {
                        EOG eog = netz.EOGs.ElementAt(i);
                        eog.Update(netz.Basisjahr, i+1);
                        db.EOGs.Update(eog);
                    }
                }
                netz.NetzNr = vm.NetzNr;
                netz.NetzName = vm.NetzName;
                netz.Basisjahr.RestwertNetzanlagen = vm.RestwertNetzanlagen;
                netz.Basisjahr.RestwertRegelanlagen = vm.RestwertRegelanlagen;
                netz.Basisjahr.StartDate = vm.StartDate;
                netz.Basisjahr.EndDate = vm.EndDate;
                netz.Basisjahr.VersorgteFlaeche = vm.VersorgteFlaeche;
                netz.Basisjahr.AnzahlAusspeisepunkte = vm.AnzahlAusspeisepunkte;
                netz.Basisjahr.Jahreshoechstlast = vm.Jahreshoechstlast;
                db.Netze.Update(netz);
                db.Save();

                return RedirectToAction("Details", new { id = netz.BNR });
            }
            return View(vm);
        }
        // GET: Admin/Netzbetreiber/DeleteGrid/4
        public ActionResult DeleteGrid(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Netz netz = db.Netze.GetByID(id);
            //Damit auch der Name des Netzbetreibers, dessen Netz gelöscht werden soll, 
            //angezeigt wird, muss auch der Netzbetreiber geladen werden. 
            //Der View selbst nutzt kein LazyLoading mehr.
            db.Netze.Include(m => m.Netzbetreiber).Load();
            if (netz == null)
            {
                return HttpNotFound();
            }
            return View(netz);
        }
        // POST: Admin/Netzbetreiber/DeleteGrid/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGrid(Netz netz)
        {
            if (ModelState.IsValid)
            {
                // Da Entität Basisjahr für Netz optional ist, ist Lösch-Kaskadierung nicht aktiviert.
                // Basisjahr muss deshalb vorher gelöscht werden.
                db.Basisjahre.Delete(db.Basisjahre.GetByID(netz.NetzId));
                db.Netze.Delete(netz);
                db.Save();
                return RedirectToAction("Details", new { id = netz.BNR });
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
#endregion
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
