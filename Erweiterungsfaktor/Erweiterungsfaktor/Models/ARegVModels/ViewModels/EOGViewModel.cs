using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Erweiterungsfaktor.Models
{
    public class EOGViewModel
    {
        public int RegPNumber { get; set; }
        public int RegPMax { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:d}", NullDisplayText = "n/a", HtmlEncode = true)]
        public DateTime? RegPStart { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = true, DataFormatString = "{0:d}", NullDisplayText = "n/a", HtmlEncode = true)]
        public DateTime? RegPEnd { get; set; }

        public List<Netzbetreiber> Netzbetreiber { get; set; }


    }
}