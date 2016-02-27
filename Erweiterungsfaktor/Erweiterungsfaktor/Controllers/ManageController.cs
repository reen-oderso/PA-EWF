using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Erweiterungsfaktor.Models;
using Erweiterungsfaktor.DataAccess;
using System.Net;

namespace Erweiterungsfaktor.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private UnitOfWork db = new UnitOfWork();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            //Statusnachricht für den View um Erfolg oder Misserfolg bei Änderungen anzuzeigen 
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ihr Kennwort wurde geändert."
                : message == ManageMessageId.SetPasswordSuccess ? "Ihr Kennwort wurde festgelegt."
                : message == ManageMessageId.ChangesSaved ? "Ihre Änderungen wurden gespeichert."
                : message == ManageMessageId.AddNBSuccess  ? "Verknüpfter Netzbetreiber hinzugefügt. Bitte warten Sie bis die Verknüpfung " +
                                                             "von der Bundesnetzagentur bestätigt wurde." 
                : "";
            ViewBag.ErrorMessage = 
                message == ManageMessageId.BNRNotFound ? "Die BNR wurde nicht gefunden."
                : message == ManageMessageId.RelationshipExists ? "Der Netzbetreiber ist bereits verknüpft."
                : "";
            
            string userId = User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);
            // Das ViewModel erzeugen
            IndexViewModel model = new IndexViewModel();
            model.HasPassword = HasPassword();
            model.PhoneNumber = await UserManager.GetPhoneNumberAsync(userId);
            model.BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId);
            model.UserName = user.UserName;
            model.Email = user.Email;
            model.EmailConfirmed = user.EmailConfirmed ? "bestätigt" : "nicht bestätigt";
            model.VerknüpfteNetzbetreiber = new System.Collections.Generic.List<UserNetzbetreiberRelationship>();
            // Die Netzbetreiber dem ViewModel hinzufügen
            model.VerknüpfteNetzbetreiber = user.NetzbetreiberRelationships;
            
            return View(model);
        }

        //
        // POST: /Manage/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include="UserName,Email")] IndexViewModel model)
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);
            if (user.UserName.Equals(model.UserName) || user.Email.Equals (model.Email))
            {
                if (!user.UserName.Equals(model.UserName))
                {
                    //Der User wurde verändert.
                    user.UserName = model.UserName;
                }
                if (!user.Email.Equals (model.Email))
                {
                    //E-Mail wurde geändert
                    user.Email = model.Email;
                    //Falls die EMail-Adresse verändert wurde, wird die Confirmed-Eigenschaft (wieder) auf falsch gesetzt
                    user.EmailConfirmed = false;
                }
                //Die DB updaten
                UserManager.Update(user);
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangesSaved });
            }
            else
            {
                //Es wurde keine Änderung vorgenommen, keine StatusMessage
                return RedirectToAction("Index", new { Message = "" });
            }
        }

        // GET: Manage/Details/12009999
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserNetzbetreiberRelationship rs = db.UserNetzbetreiberRelationships.GetByID(User.Identity.GetUserId(), id);
            if (!rs.Confirmed)
            {
                //Daten werden nur angezeigt, wenn NB bestätigt wurde
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(rs.Netzbetreiber);
        }

        //
        // GET: /Manage/ChangePassword 
        public ActionResult ChangePassword()
        {
            return View();
         }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Wurde dieser Punkt erreicht, ist ein Fehler aufgetreten. Formular erneut anzeigen.
            return View(model);
        }

        // POST: Admin/User/CreateRelationship/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRelationship( [Bind(Include = "BNR")] IndexViewModel model)
        {
            if (model.BNR !=null) // Es wurde eine BNR gepostet.
            {
                string userId = User.Identity.GetUserId();
                // Den User NICHT über den UserManager holen, da dieser einen eigenen DbContext hat und dann mehrere 
                // ChangeTracker existieren, die Probleme verursachen.
                ApplicationUser user = db.Users.GetByID(userId);
                Netzbetreiber nb = db.Netzbetreiber.GetByID(model.BNR);
                if (nb != null) // Der NB existiert, der verknüpft werden soll.  
                {
                    UserNetzbetreiberRelationship rs = db.UserNetzbetreiberRelationships.GetByID(user.Id, nb.BNR);
                    if (rs == null) 
                    {
                        // Die RS ist nicht bereits vorhanden
                        rs = new UserNetzbetreiberRelationship()
                        {
                            BNR = Convert.ToInt32(model.BNR),
                            Netzbetreiber = nb,
                            Id = user.Id,
                            User = user,
                            Confirmed = false
                        };
                        nb.UserRelationships.Add(rs);
                        user.NetzbetreiberRelationships.Add(rs);
                        db.UserNetzbetreiberRelationships.Insert(rs);
                        db.Save();
                        return RedirectToAction("Index", new { Message = ManageMessageId.AddNBSuccess });
                    }
                    else
                    {
                        // Die RS ist bereits vorhanden
                        return RedirectToAction("Index", new { Message = ManageMessageId.RelationshipExists});
                    }
                }
                // Die BNR gibt es nicht
                return RedirectToAction("Index", new { Message = ManageMessageId.BNRNotFound });
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteRelationship(int? bnr)
        {
            if (bnr < 12000000 || bnr > 12009999 || bnr==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserNetzbetreiberRelationship rs= db.UserNetzbetreiberRelationships.GetByID(User.Identity.GetUserId(), bnr);
            return View(rs);
        }

        [HttpPost, ActionName ("DeleteRelationship")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRelationshipConfirmed(int? bnr)
        {
            if (bnr < 12000000 || bnr > 12009999 || bnr == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            db.UserNetzbetreiberRelationships.Delete(User.Identity.GetUserId(), bnr );
            db.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
                db.Dispose();
                db = null;
            }

            base.Dispose(disposing);
        }

#region Hilfsprogramme
        // Wird für XSRF-Schutz beim Hinzufügen externer Anmeldungen verwendet.
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            ChangesSaved,
            AddNBSuccess,
            BNRNotFound,
            RelationshipExists
        }

#endregion
    }
}