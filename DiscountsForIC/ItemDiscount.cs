using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscountsForIC {
    public class ItemDiscount : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			IsChanged = true;
		}

		public bool IsChanged { get; private set; } = false;

		public int? BZ_ADID { get; set; } = null;
		public int JID { get; set; } = 0;
		public int AGRID { get; set; } = 0;
		public int FILIAL { get; set; } = 0;

		public List<string> Contract { get; set; } = new List<string>();

		private string contractPreview = string.Empty;
		public string ContractPreview {
			get {
				return contractPreview;
			}
			set {
				if (value != contractPreview) {
					contractPreview = value;
					NotifyPropertyChanged();
				}
			}
		}
		
		private bool endless = true;
		public bool ENDLESS {
			get {
				return endless;
			}
			set {
				if (value != endless) {
					endless = value;
					NotifyPropertyChanged();
				}
			}
		}

		private DateTime beginDate = DateTime.Now;
		public DateTime BEGINDATE {
			get {
				return beginDate;
			}
			set {
				if (value != beginDate) {
					beginDate = value;
					NotifyPropertyChanged();
				}
			}
		}

		private DateTime endDate = DateTime.Now;
		public DateTime ENDDATE {
			get {
				return endDate;
			}
			set {
				if (value != endDate) {
					endDate = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool amountRelation = false;
		public bool AMOUNTRELATION {
			get {
				return amountRelation;
			}
			set {
				if (value != amountRelation) {
					amountRelation = value;
					NotifyPropertyChanged();
				}
			}
		}

		private int? startAmount = null;
		public int? STARTAMOUNT {
			get {
				return startAmount;
			}
			set {
				if (value != startAmount) {
					startAmount = value;
					NotifyPropertyChanged();
				}
			}
		}

		private int? finishAmount = null;
		public int? FINISHAMOUNT {
			get {
				return finishAmount;
			}
			set {
				if (value != finishAmount) {
					finishAmount = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string comment = string.Empty;
		public string COMMENT {
			get {
				return comment;
			}
			set {
				if (value != comment) {
					comment = value;
					NotifyPropertyChanged();
				}
			}
		}

		private int discount = 0;
		public int DISCOUNT {
			get {
				return discount;
			}
			set {
				if (value != discount) {
					discount = value;
					NotifyPropertyChanged();
				}
			}
		}

		public ItemDiscount(string contractPreview) {
			this.contractPreview = contractPreview;
		}

		public ItemDiscount(int? bz_adid, int jid, int agrid, int filial,
			string contractPreview, bool endless,  DateTime beginDate, DateTime endDate, 
			bool amountRelation, int? startAmount, int? finishAmount,
			string comment, int discount) {
			BZ_ADID = bz_adid;
			JID = jid;
			AGRID = agrid;
			FILIAL = filial;
			this.contractPreview = contractPreview;
			this.endless = endless;
			this.beginDate = beginDate;
			this.endDate = endDate;
			this.amountRelation = amountRelation;
			this.startAmount = startAmount;
			this.finishAmount = finishAmount;
			this.comment = comment;
			this.discount = discount;
		}

		public void ItemIsUpdated() {
			IsChanged = false;
		}
	}
}
