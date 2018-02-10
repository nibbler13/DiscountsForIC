using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscountsForIC {
    public class ItemDiscount {
		public string BZ_ADID { get; set; } = string.Empty;
		public string JID { get; set; } = string.Empty;
		public string AGRID { get; set; } = string.Empty;
		public string FILIAL { get; set; } = string.Empty;
		public bool ENDLESS { get; set; }
		public DateTime BEGINDATE { get; set; }
		public DateTime ENDDATE { get; set; }
		public bool AMOUNTRELATION { get; set; }
		public string STARTAMOUNT { get; set; } = string.Empty;
		public string FINISHAMOUNT { get; set; } = string.Empty;
		public string COMMENT { get; set; } = string.Empty;
		public string DISCOUNT { get; set; } = string.Empty;
	}
}
