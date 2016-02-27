using Erweiterungsfaktor.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.DataAccess
{
    public class EWFDbContext : IdentityDbContext<ApplicationUser>
    {
        //Variante 2, die ARegV-Models im EWFDbContext. IdentityDbContext erbt auch vom normalen DbContext.
        public DbSet<Netzbetreiber> Netzbetreiber { get; set; }
        public DbSet<Regulierungsperiode> Regulierungsperioden { get; set; }
        public DbSet<Netz> Netze { get; set; }
        public DbSet<Basisjahr> Basisjahre { get; set; }
        public DbSet<EOG> EOGs { get; set; }
        public DbSet<AntragEWF> EWFs { get; set; }
        public DbSet<UserNetzbetreiberRelationship> UserNetzbetreiberRelationships { get; set; }

        //Das Users DbSet wird schon von der vererbenden Klasse IdentityDbContext geerbt
        public override IDbSet<ApplicationUser> Users { get; set; }

        public EWFDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static EWFDbContext Create()
        {
            return new EWFDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Für Identity notwendig, da vererbende Klasse Konfigurationen für IdentityUserLogin und IdentityUserRoles enthält
            base.OnModelCreating(modelBuilder);
            //Keine Mehrzahl für Tabellen-Namen
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }


    }
    //Individueller Initializer erzeugt neue Db falls nicht vorhanden und erzeugt dann einen Standard-Nutzer
    public class EWFDbContextInitializer : CreateDatabaseIfNotExists<EWFDbContext>
    {
        protected override void Seed(EWFDbContext context)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            //Standard-Account erstellen
            ApplicationUser user = new ApplicationUser()
            {
                UserName = "webadmin",
                Email = "admin@mail.de",
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0
            };
            //Erzeugt den User
            manager.Create(user, "webadmin");
            //Erzeugt die Admin-Rolle
            context.Roles.Add(new IdentityRole()
            {
                Name = "Admin"
            });
            context.SaveChanges();
            //Fügt die Beziehung zwischen User und Role hinzu.
            manager.AddToRole(user.Id, "Admin");

            //Regulierungsperiode 
            Regulierungsperiode regP = new Regulierungsperiode()
            {
                Number = 1,
                StartDate = new DateTime(2009, 1, 1),
                EndDate = new DateTime(2012, 12, 31)
            };
            context.Regulierungsperioden.Add(regP);

            //Test-Netzbetreiber erstellen
            Netzbetreiber nb = new Netzbetreiber()
            {
                BNR = 12009999,
                Name = "Verteilernetzbetreiber",
                Rechtsform = "GmbH",
                PLZOrt = "99999 Musterstadt",
                StrasseHausNr = "Musterstraße 555",
                VereinfachtesVerfahren = false
            };
            //Beziehung zwischen Nutzer und Netzbetreiber erstellen
            UserNetzbetreiberRelationship rs = new UserNetzbetreiberRelationship()
            {
                Netzbetreiber=nb,
                BNR =nb.BNR,
                User=user,
                Id = user.Id,
                Confirmed =true 
            };
            //Navigation-Properties zuweisen
            nb.UserRelationships = new List<UserNetzbetreiberRelationship>();
            nb.UserRelationships.Add(rs);
            user.NetzbetreiberRelationships = new List<UserNetzbetreiberRelationship>();
            user.NetzbetreiberRelationships.Add(rs);
            //Fügt nb und rs dem ORM hinzu 
            context.Netzbetreiber.Add(nb);
            context.UserNetzbetreiberRelationships.Add(rs);
            
            //Netz für Test-Netzbetreiber
            Netz n = new Netz()
            {
                Netzbetreiber = nb,
                BNR=nb.BNR,
                RegPId = regP.RegPId,
                Regulierungsperiode = regP,
                NetzNr = 1,
                NetzName = "Region Ost",
                EOGs=new List<EOG> ()               
            };
            context.Netze.Add(n);
            //Basisjahr für Netz 1 des Test-Netzbetreiber 
            Basisjahr b = new Basisjahr()
            {
                NetzId=n.NetzId,
                Netz = n,

                Netzkosten = 79500587.45d,
                KAdnb = 4879325.78d,
                Effizienzwert = 0.92547878d,
                Verteilungsfaktor = 0.1d,

                RestwertNetzanlagen = 240567345.36d,
                RestwertRegelanlagen = 61894356.56d,

                StartDate = new DateTime(2006, 1, 1),
                EndDate = new DateTime(2006, 12, 31),

                VersorgteFlaeche= 345.34d,
                AnzahlAusspeisepunkte=74895,
                Jahreshoechstlast = 25326d
            };
            context.Basisjahre.Add(b);
            context.SaveChanges();
            //EOGs für Netz 1 des Test-Netzbetreiber 
            EOG eog = new EOG(b,1)
            {
                StartDate = new DateTime(2009, 1, 1),
                EndDate = new DateTime(2009, 12, 31),
                NetzId =n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b,2)
            {
                StartDate = new DateTime(2010, 1, 1),
                EndDate = new DateTime(2010, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b,3)
            {
                StartDate = new DateTime(2011, 1, 1),
                EndDate = new DateTime(2011, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b,4)
            {
                StartDate = new DateTime(2012, 1, 1),
                EndDate = new DateTime(2012, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            context.SaveChanges();
            //Weiteres Netz für Test-Netzbetreiber
            n = new Netz()
            {
                BNR = nb.BNR,
                Netzbetreiber = nb,
                RegPId = regP.RegPId,
                NetzNr = 2,
                NetzName = "Region West",
                Regulierungsperiode = regP,
                EOGs = new List<EOG>()
            };
            context.Netze.Add(n);
            //Basisjahr für Netz 2 des Test-Netzbetreiber 
            b = new Basisjahr()
            {
                NetzId = n.NetzId,
                Netz = n,
                Netzkosten = 25500587.45d,
                KAdnb = 2879325.78d,
                Effizienzwert = 0.897851d,
                Verteilungsfaktor = 0.1d,
                RestwertNetzanlagen = 112567345.36d,
                RestwertRegelanlagen = 19894356.56d,
                StartDate = new DateTime(2006, 1, 1),
                EndDate = new DateTime(2006, 12, 31),
                VersorgteFlaeche = 134.4d,
                AnzahlAusspeisepunkte = 24895,
                Jahreshoechstlast = 11389d
            };
            context.Basisjahre.Add(b);
            //EOGs für Netz 2 des Test-Netzbetreiber 
            eog = new EOG(b, 1)
            {
                StartDate = new DateTime(2009, 1, 1),
                EndDate = new DateTime(2009, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 2)
            {
                StartDate = new DateTime(2010, 1, 1),
                EndDate = new DateTime(2010, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 3)
            {
                StartDate = new DateTime(2011, 1, 1),
                EndDate = new DateTime(2011, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 4)
            {
                StartDate = new DateTime(2012, 1, 1),
                EndDate = new DateTime(2012, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            //Weitere Regulierungsperiode
            regP = new Regulierungsperiode()
            {
                Number = 2,
                StartDate = new DateTime(2013, 1, 1),
                EndDate = new DateTime(2017, 12, 31)
            };
            context.Regulierungsperioden.Add(regP);
            //Weiteres Netz für Test-Netzbetreiber
            n = new Netz()
            {
                BNR = nb.BNR,
                Netzbetreiber = nb,
                RegPId = regP.RegPId,
                Regulierungsperiode = regP,
                NetzNr = 1,
                NetzName = "Gesamtnetz",
                EOGs = new List<EOG>()
            };
            context.SaveChanges();
            //Basisjahr für Netz 1 des Test-Netzbetreiber 
            b = new Basisjahr()
            {
                NetzId = n.NetzId,
                Netz = n,
                Netzkosten = 101500587.45d,
                KAdnb = 5879325.78d,
                Effizienzwert = 0.93547878d,
                Verteilungsfaktor = 0.2d,
                RestwertNetzanlagen = 396567345.36d,
                RestwertRegelanlagen = 3894356.56d,
                StartDate = new DateTime(2010, 1, 1),
                EndDate = new DateTime(2010, 12, 31),
                VersorgteFlaeche = 480.4d,
                AnzahlAusspeisepunkte = 104895,
                Jahreshoechstlast = 38576d
            };
            context.Basisjahre.Add(b);
            //EOGs für Netz 2 des Test-Netzbetreiber 
            eog = new EOG(b, 1)
            {
                StartDate = new DateTime(2013, 1, 1),
                EndDate = new DateTime(2013, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 2)
            {
                StartDate = new DateTime(2014, 1, 1),
                EndDate = new DateTime(2014, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 3)
            {
                StartDate = new DateTime(2015, 1, 1),
                EndDate = new DateTime(2015, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 4)
            {
                StartDate = new DateTime(2016, 1, 1),
                EndDate = new DateTime(2016, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            eog = new EOG(b, 5)
            {
                StartDate = new DateTime(2017, 1, 1),
                EndDate = new DateTime(2017, 12, 31),
                NetzId = n.NetzId,
                Netz = n
            };
            n.EOGs.Add(eog);
            context.EOGs.Add(eog);
            //Weiterer Test-Netzbetreiber
            nb = new Netzbetreiber()
            {
                BNR = 12009998,
                Name = "Fernleitungsnetzbetreiber",
                Rechtsform = "AG",
                PLZOrt = "00001 Testheim",
                StrasseHausNr = "Teststraße 444",
                VereinfachtesVerfahren = false
            };
            context.Netzbetreiber.Add(nb);
            //Relationship für weiteren Test-Netzbetreiber
            rs = new UserNetzbetreiberRelationship()
            {
                Netzbetreiber = nb,
                BNR = nb.BNR,
                User = user,
                Id = user.Id,
                Confirmed = true
            };
            //Navigation-Properties zuweisen
                        
            context.UserNetzbetreiberRelationships.Add(rs);
            //Weiterer Test-User
            user = new ApplicationUser()
            {
                UserName = "testuser",
                Email = "test@test.de",
                EmailConfirmed = true,
                LockoutEnabled = false,
                AccessFailedCount = 0
            };
            //Erzeugt den User
            manager.Create(user, "test12");

            //Relationship für weiteren User
            rs = new UserNetzbetreiberRelationship()
            {
                Netzbetreiber = nb,
                BNR = nb.BNR,
                User = user,
                Id = user.Id,
                Confirmed = false
            };
            context.UserNetzbetreiberRelationships.Add(rs);
            //Weitere Regulierungsperiode
            regP = new Regulierungsperiode()
            {
                Number = 3,
                StartDate = new DateTime(2018, 1, 1),
                EndDate = new DateTime(2022, 12, 31)
            };
            context.Regulierungsperioden.Add(regP);
            context.SaveChanges();
        }
    }
}