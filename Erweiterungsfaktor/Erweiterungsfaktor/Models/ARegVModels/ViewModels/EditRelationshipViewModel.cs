using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Erweiterungsfaktor.Models
{
    public class EditRelationshipViewModel
    {
        public string Id { get; set; }
        [Range(12000000,12009999,ErrorMessage ="Die BNR muss zwischen 12000000 und 12009999 liegen")]
        public int BNR { get; set; }
        public bool Confirmed { get; set; }
        
        public ApplicationUser User { get; set; }
        public Netzbetreiber Netzbetreiber { get; set; }

        public List<SelectListItem> BNRList { get; set; }
        public List<SelectListItem> UserList { get; set; }
    }
}