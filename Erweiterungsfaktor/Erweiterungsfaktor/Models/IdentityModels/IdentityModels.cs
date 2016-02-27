using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Erweiterungsfaktor.Models
{
    // Nutzer-Account, wird für Authentication genutzt
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Beachten Sie, dass der "authenticationType" mit dem in "CookieAuthenticationOptions.AuthenticationType" definierten Typ übereinstimmen muss.
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Benutzerdefinierte Benutzeransprüche hier hinzufügen
            return userIdentity;
        }
        //Der Übersichtlichkeit halber sind hier Eigenschaften der IdentityUser-Klasse wiedergegeben. Nicht benutzte Eigenschaften sind auskommentiert.

        //      E-Mail-Adresse
        [Display(Name = "Mail")]
        [DataType(DataType.EmailAddress )]
        public override string Email { get; set; }

        //     E-Mail verifiziert
        [Display(Name = "bestätigt")]
        public override bool EmailConfirmed { get; set; }

        //     Account gesperrt
        [Display(Name = "gesperrt")]
        public override bool LockoutEnabled { get; set; }

        //     Ablaufdatum der Account-Sperre
        [Display(Name = "Gesperrt bis")]
        [DataType(DataType.Date)]
        public override DateTime? LockoutEndDateUtc { get; set; }

        //     Salted/Hashed Password
        //public override string PasswordHash { get; set; }

        //     Telefonnummer, für Account-Wiederherstellung
        //public override string PhoneNumber { get; set; }

        //     Telefonnummer bestätigt
        //public override bool PhoneNumberConfirmed { get; set; }

        //     Der Benutzername für den Login und Fremdschlüssel für die Klasse Netzbetreiber
        [Display(Name ="Benutzername")]
        public override string UserName { get; set; }
        
        //Navigation Properties
        public virtual List<UserNetzbetreiberRelationship> NetzbetreiberRelationships { get; set; }
        public virtual List<AntragEWF> EWFs { get; set; }
    }
}