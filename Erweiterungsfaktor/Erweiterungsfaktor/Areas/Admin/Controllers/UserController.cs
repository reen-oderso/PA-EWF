using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Erweiterungsfaktor.Models;
using Erweiterungsfaktor.DataAccess;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace Erweiterungsfaktor.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private UnitOfWork db = new UnitOfWork();

        // GET: Admin/User
        public ActionResult Index()
        {
            List<ApplicationUser> users = db.Users.Get().ToList();
            return View(users);
        }

        // GET: Admin/User/Details/5
        public ActionResult Details(string id, string returnURL )
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.GetByID (id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(applicationUser);
        }

        // GET: Admin/User/Create
        public ActionResult Create()
        {
            //Neuer Nutzer kann über normale Registrieren-Funktion erzeugt werden.
            return RedirectToAction("Register","Account");
        }

        // GET: Admin/User/Edit/5
        public ActionResult Edit(string id, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.GetByID(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(applicationUser);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Email,EmailConfirmed,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Update(user);
                db.Save();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/User/Delete/5
        public ActionResult Delete(string id, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.GetByID(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnURL = returnURL;
            return View(applicationUser);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = db.Users.GetByID(id);
            db.Users.Delete(applicationUser);
            db.Save();
            return RedirectToAction("Index");
        }
        
        // GET: Admin/ChangePassword/8
        public ActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.GetByID(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ChangePasswordAdminViewModel model = new ChangePasswordAdminViewModel { Id = user.Id };
            return View(model);
        }

        // GET: Admin/ChangePassword/8
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordAdminViewModel model)
        {
            if (ModelState.IsValid && model.NewPassword.Equals(model.ConfirmPassword ))
            {
                ApplicationUserManager manager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string resetToken = await manager.GeneratePasswordResetTokenAsync(model.Id);
                IdentityResult result = await manager.ResetPasswordAsync(model.Id, resetToken, model.NewPassword);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(result); 
            }
            return View(model);
        }

        // GET: Admin/User/CreateRelationship/abc-56
        public ActionResult CreateRelationship(string id, string returnURL)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.GetByID(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            //Erzeuge ViewModel
            EditRelationshipViewModel vm = new EditRelationshipViewModel()
            {
                Id=user.Id,
                User=user,
                Confirmed=false,
                BNRList = new List<SelectListItem>()                
            };
            //Füge Liste der Netzbetreiber dem ViewModel hinzu
            foreach (Netzbetreiber nb in db.Netzbetreiber.Get())
            {
                vm.BNRList.Add(new SelectListItem() { Text = nb.BNR + " " + nb.Name + " " + nb.Rechtsform, Value = nb.BNR.ToString() });
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
                    //Füge Liste der Netzbetreiber dem ViewModel hinzu
                    vm.BNRList = new List<SelectListItem>();
                    foreach (Netzbetreiber nb in db.Netzbetreiber.Get())
                    {
                        vm.BNRList.Add(new SelectListItem() { Text = nb.BNR + " " + nb.Name + " " + nb.Rechtsform, Value = nb.BNR.ToString() });
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
                return RedirectToAction("Details", "User", new { Id = vm.Id });
            }
            //Füge Liste der Netzbetreiber dem ViewModel hinzu
            vm.BNRList = new List<SelectListItem>();
            foreach (Netzbetreiber nb in db.Netzbetreiber.Get())
            {
                vm.BNRList.Add(new SelectListItem() { Text = nb.BNR + " " + nb.Name + " " + nb.Rechtsform, Value = nb.BNR.ToString() });
            }
            return View(vm);
        }

        public ActionResult DeleteRelationship(string id, int? bnr, string returnURL)
        {
            if (id == null || bnr < 12000000 || bnr > 12009999)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserNetzbetreiberRelationship rs = db.UserNetzbetreiberRelationships.GetByID(id,bnr);
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

        #region Hilfsprogramme
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }  
}
#endregion