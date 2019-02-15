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
		public bool IsNewAdded { get; set; } = false;

		public int? BZ_ADID { get; set; } = null;
		public int JID { get; set; } = -1;
		public int AGRID { get; set; } = -1;
		public int FILIAL { get; set; } = -1;

		public List<string> Contract { get; set; } = new List<string>();

		private string contractPreview = string.Empty;
		public string ContractPreview {
			get {
				Console.WriteLine("ContractPreview get: " + contractPreview);
				return contractPreview;
			}
			set {
				Console.WriteLine("ContractPreview set value: " + value);
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

		private float discount = 0;
		public float DISCOUNT {
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

		private string shortName;
		public string SHORTNAME {
			get {
				return shortName;
			} set {
				if (value != shortName) {
					shortName = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string jName;
		public string JNAME {
			get {
				return jName;
			} set {
				if (value != jName) {
					jName = value;
					NotifyPropertyChanged();
				}
			}
		}


		private string agNum;
		public string AGNUM {
			get {
				return agNum;
			} set {
				if (value != agNum) {
					agNum = value;
					NotifyPropertyChanged();
				}
			}
		}

		public ItemDiscount(string contractPreview) {
			ContractPreview = contractPreview;
		}

		public ItemDiscount(string shortName, string jName, string agNum) { 
			SHORTNAME = shortName;
			JNAME = jName;
			AGNUM = agNum;
			ContractPreview = SHORTNAME + " / " + JNAME + " / " + AGNUM;
		}

		public ItemDiscount(
			string shortName, string jName, string agNum, 
			int? bz_adid, int jid, int agrid, int filial,
			bool endless,  DateTime beginDate, DateTime endDate, 
			bool amountRelation, int? startAmount, int? finishAmount,
			string comment, float discount) : this(shortName, jName, agNum) {
			BZ_ADID = bz_adid;
			JID = jid;
			AGRID = agrid;
			FILIAL = filial;
			ENDLESS = endless;
			BEGINDATE = beginDate;
			ENDDATE = endDate;
			AMOUNTRELATION = amountRelation;
			STARTAMOUNT = startAmount;
			FINISHAMOUNT = finishAmount;
			COMMENT = comment;
			DISCOUNT = discount;
		}

		public void ItemIsUpdated() {
			IsChanged = false;
		}
	}
}
