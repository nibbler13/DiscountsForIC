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
		}

		public string BZ_ADID { get; set; } = string.Empty;
		public string JNAME { get; set; } = string.Empty;
		public string AGNUM { get; set; } = string.Empty;
		public string SHORTNAME { get; set; } = string.Empty;
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

		private int? discount = null;
		public int? DISCOUNT {
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
	}
}
