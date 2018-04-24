using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountsForIC {
    public class ItemIC {
		public int FILIAL { get; set; } = -1;
		public string SHORTNAME { get; set; } = string.Empty;
		public string JNAME { get; set; } = string.Empty;
		public string AGNUM { get; set; } = string.Empty;
		public int JID { get; set; } = -1;
		public int AGRID { get; set; } = -1;
		public DateTime AGDATE { get; set; }
		public DateTime? EDATE { get; set; } = null;
		public bool ISCLOSE { get; set; } = false;

		public string ISCLOSED {
			get {
				if (AGRID == -1)
					return string.Empty;

				return ISCLOSE ? "Да" : "Нет";
			}
		}

		public string EDATESTR {
			get {
				return EDATE.HasValue ? EDATE.Value.ToShortDateString() : string.Empty;
			}
		}

		public string AGDATESTR {
			get {
				if (AGDATE == new DateTime())
					return string.Empty;

				return AGDATE.ToShortDateString();
			}
		}

		public string GetContractPreview() {
			return SHORTNAME + " / " + JNAME + " / " + AGNUM;
		}
	}
}
