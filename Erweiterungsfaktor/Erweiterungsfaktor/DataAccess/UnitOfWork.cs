using Erweiterungsfaktor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.DataAccess
{
    public class UnitOfWork : IDisposable
    {
        private EWFDbContext context = new EWFDbContext();
        private Repository<ApplicationUser> _Users;
        private Repository<Netzbetreiber> _Netzbetreiber;
        private Repository<Regulierungsperiode> _Regulierungsperioden;
        private Repository<Netz> _Netz;
        private Repository<EOG> _EOGs;
        private Repository<Basisjahr> _Basisjahre;
        private Repository<AntragEWF> _EWFs;
        private Repository<UserNetzbetreiberRelationship> _UserNBRelationships;

        public Repository<ApplicationUser> Users
        {
            get
            {

                if (this._Users == null)
                {
                    this._Users = new Repository<ApplicationUser>(context);
                }
                return _Users;
            }
        }

        public Repository<Netzbetreiber> Netzbetreiber
        {
            get
            {

                if (this._Netzbetreiber  == null)
                {
                    this._Netzbetreiber = new Repository<Netzbetreiber>(context);
                }
                return _Netzbetreiber;
            }
        }

        public Repository<Regulierungsperiode> Regulierungsperioden
        {
            get
            {

                if (this._Regulierungsperioden == null)
                {
                    this._Regulierungsperioden = new Repository<Regulierungsperiode>(context);
                }
                return _Regulierungsperioden;
            }
        }

        public Repository<Netz> Netze
        {
            get
            {

                if (this._Netz == null)
                {
                    this._Netz = new Repository<Netz>(context);
                }
                return _Netz;
            }
        }

        public Repository<EOG> EOGs
        {
            get
            {

                if (this._EOGs == null)
                {
                    this._EOGs = new Repository<EOG>(context);
                }
                return _EOGs;
            }
        }

        public Repository<AntragEWF> EWFs
        {
            get
            {

                if (this._EWFs == null)
                {
                    this._EWFs = new Repository<AntragEWF>(context);
                }
                return _EWFs;
            }
        }

        public Repository<Basisjahr> Basisjahre
        {
            get
            {

                if (this._Basisjahre == null)
                {
                    this._Basisjahre = new Repository<Basisjahr>(context);
                }
                return _Basisjahre;
            }
        }

        public Repository<UserNetzbetreiberRelationship > UserNetzbetreiberRelationships
        {
            get
            {

                if (this._UserNBRelationships == null)
                {
                    this._UserNBRelationships = new Repository<UserNetzbetreiberRelationship>(context);
                }
                return _UserNBRelationships;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}