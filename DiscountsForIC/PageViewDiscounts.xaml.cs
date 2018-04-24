using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiscountsForIC {
	/// <summary>
	/// Логика взаимодействия для PageViewDiscounts.xaml
	/// </summary>
	public partial class PageViewDiscounts : Page, INotifyPropertyChanged {
		public ObservableCollection<ItemDiscount> ItemsDiscount { get; set; } = new ObservableCollection<ItemDiscount>();

		private ListSortDirection sortDirectionListViewDiscounts;
		private GridViewColumnHeader sortColumnListViewDiscounts;

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		private bool isDiscountsChanged = false;
		
		private bool comboBoxEndlessIsChecked = false;
		public bool ComboBoxEndlessIsChecked {
			get {
				return comboBoxEndlessIsChecked;
			}
			set {
				if (value != comboBoxEndlessIsChecked) {
					comboBoxEndlessIsChecked = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool comboBoxAmountRelationIsChecked = false;
		public bool ComboBoxAmountRelationIsChecked {
			get {
				return comboBoxAmountRelationIsChecked;
			}
			set {
				if (value != comboBoxAmountRelationIsChecked) {
					comboBoxAmountRelationIsChecked = value;
					NotifyPropertyChanged();
				}
			}
		}

		private List<ItemIC> itemsIC;
		private SearchType searchType;

		private string sqlQuerySelectDiscountsByDate;
		private Dictionary<string, string> sqlQueryParameters;
		private List<ItemFilial> itemsFilial;
		private bool showClosedContracts;

		public enum SearchType {
			ByNameOrNumber,
			ByDate
		}




		private PageViewDiscounts() {
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			InitializeComponent();

			ItemsDiscount.CollectionChanged += ItemsDiscount_CollectionChanged;
			ItemsDiscount.CollectionChanged += (s, e) => {
				AutoResizeGridViewColumns(ListViewDiscounts.View as GridView);
			};

			DataContext = this;
		}

		public PageViewDiscounts(List<ItemIC> itemsIC) : this() {
			this.itemsIC = itemsIC;
			searchType = SearchType.ByNameOrNumber;

			Loaded += (s, e) => {
				UpdateListViewDiscounts();
			};
		}

		public PageViewDiscounts(string sqlQuerySelectDiscountsByDate, Dictionary<string, string> sqlQueryParameters, List<ItemFilial> itemsFilial, bool showClosedContracts) : this() {
			this.sqlQuerySelectDiscountsByDate = sqlQuerySelectDiscountsByDate;
			this.sqlQueryParameters = sqlQueryParameters;
			this.itemsFilial = itemsFilial;
			this.showClosedContracts = showClosedContracts;
			searchType = SearchType.ByDate;

			Loaded += (s, e) => {
				UpdateListViewDiscounts();
			};
		}



		private void NavigationService_Navigating(object sender, NavigatingCancelEventArgs e) {
			if (NeedToReturnToEditDiscounts()) {
				e.Cancel = true;
				return;
			}

			SystemDataHandle.CloseConnection();
		}

		private void AutoResizeGridViewColumns(GridView view) {
			if (view == null || view.Columns.Count < 1) return;

			// Simulates column auto sizing
			foreach (var column in view.Columns) {
				// Forcing change
				if (double.IsNaN(column.Width))
					column.Width = 1;

				column.Width = double.NaN;
			}
		}


		private bool NeedToReturnToEditDiscounts() {
			if (isDiscountsChanged) {
				MessageBoxResult messageBoxResult = MessageBox.Show(
					"Имеются изменения в текущих данных о скидках. Желаете сохранить изменения?",
					"Сохранение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

				if (messageBoxResult == MessageBoxResult.Cancel)
					return true;

				if (messageBoxResult == MessageBoxResult.Yes)
					ButtonApplyChanges_Click(ButtonApplyChanges, new RoutedEventArgs());
			}

			return false;
		}

		private void ItemsDiscount_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (ItemDiscount item in e.OldItems) {
					//remove from db
					item.PropertyChanged -= ItemDiscount_PropertyChanged;
				}
			} else if (e.Action == NotifyCollectionChangedAction.Add) {
				foreach (ItemDiscount item in e.NewItems) {
					item.PropertyChanged += ItemDiscount_PropertyChanged;
				}
			}
		}

		private void ItemDiscount_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			isDiscountsChanged = true;
			ButtonApplyChanges.IsEnabled = true;
		}


		private async void UpdateListViewDiscounts() {
			if (NeedToReturnToEditDiscounts())
				return;

			ItemsDiscount.Clear();
			isDiscountsChanged = false;
			ButtonAdd.IsEnabled = false;
			ButtonApplyChanges.IsEnabled = false;
			ButtonExportToExcel.IsEnabled = false;
			ComboBoxEndlessIsChecked = false;
			ComboBoxAmountRelationIsChecked = false;

			List<ItemDiscount> discounts = new List<ItemDiscount>();
			Cursor = Cursors.Wait;
			
			switch (searchType) {
				case SearchType.ByNameOrNumber:
					await Task.Run(() => {
						discounts = SystemDataHandle.SelectDiscount(itemsIC);
					});

					ButtonAdd.IsEnabled = true;

					break;
				case SearchType.ByDate:
					await Task.Run(() => {
						discounts = SystemDataHandle.SelectDiscoutByDates(
							sqlQuerySelectDiscountsByDate, 
							sqlQueryParameters, 
							itemsFilial, 
							showClosedContracts);
					});
					break;
				default:
					return;
			}

			if (discounts.Count == 0) {
				switch (searchType) {
					case SearchType.ByNameOrNumber:
						ButtonAdd_Click(ButtonAdd, new RoutedEventArgs());
						break;
					case SearchType.ByDate:
						MessageBox.Show("Нет данных за выбранный диапазон дат", "", MessageBoxButton.OK, MessageBoxImage.Information);
						break;
					default:
						break;
				}
			}

			ButtonExportToExcel.IsEnabled = discounts.Count > 0;
			ButtonSelectAllDiscounts.IsEnabled = discounts.Count > 0;

			discounts.ForEach(ItemsDiscount.Add);
			CheckBox_Click(null, null);
			Cursor = Cursors.Arrow;
		}





		private void ListViewDiscounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonDelete.IsEnabled = ListViewDiscounts.SelectedItems.Count > 0;
		}




		private void ListViewHeader_Click(object sender, RoutedEventArgs e) {
			SortListViewColumn(sender, e, ref sortColumnListViewDiscounts, ref sortDirectionListViewDiscounts);
		}

		private void SortListViewColumn(object sender, RoutedEventArgs e,
			ref GridViewColumnHeader columnHeader, ref ListSortDirection sortDirection) {
			GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
			if (column == null)
				return;

			if (columnHeader == column)
				sortDirection = sortDirection == ListSortDirection.Ascending ?
												 ListSortDirection.Descending :
												 ListSortDirection.Ascending;
			else {
				if (columnHeader != null) {
					columnHeader.Column.HeaderTemplate = null;
					columnHeader.Column.Width = columnHeader.ActualWidth - 20;
				}

				columnHeader = column;
				sortDirection = ListSortDirection.Ascending;
				column.Column.Width = column.ActualWidth + 20;
			}

			if (sortDirection == ListSortDirection.Ascending)
				column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
			else
				column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;

			string header = string.Empty;

			if (columnHeader.Column.DisplayMemberBinding is Binding b)
				header = b.Path.Path;

			ICollectionView resultDataView = CollectionViewSource.GetDefaultView((sender as ListView).ItemsSource);
			resultDataView.SortDescriptions.Clear();
			resultDataView.SortDescriptions.Add(new SortDescription(header, sortDirection));
		}



		private async void ButtonApplyChanges_Click(object sender, RoutedEventArgs e) {
			foreach (ItemDiscount itemDiscount in ItemsDiscount) {
				if (!itemDiscount.IsChanged || itemDiscount.BZ_ADID != null)
					continue;

				if (!itemDiscount.ContractPreview.Contains(" / ")) {
					MessageBox.Show(
						"Имеются новые записи, у которых не выбран договор",
						"Отмена", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (itemDiscount.DISCOUNT < 1) {
					MessageBox.Show(
						"Имеются записи, у которых не задан размер скидки", "Отмена", 
						MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				bool isFound = false;
				foreach (ItemIC itemIC in itemsIC) {
					if (itemDiscount.ContractPreview.Equals(itemIC.GetContractPreview())) {
						itemDiscount.JID = itemIC.JID;
						itemDiscount.AGRID = itemIC.AGRID;
						itemDiscount.FILIAL = itemIC.FILIAL;
						isFound = true;
						break;
					}
				}

				if (!isFound) {
					MessageBox.Show(
						"Не удалось определить данные договора для строки: " + itemDiscount.ContractPreview,
						"Ошибка обработки", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			Cursor = Cursors.Wait;

			await Task.Run(() => {
				SystemDataHandle.UpdateOrInsertDiscount(ItemsDiscount.Where(i => i.IsChanged).ToList());
			});

			foreach (ItemDiscount item in ItemsDiscount)
				item.IsNewAdded = false;

			Cursor = Cursors.Arrow;

			isDiscountsChanged = false;
			((Button)sender).IsEnabled = false;

			UpdateListViewDiscounts();
		}


		private void ButtonSelectAllDiscounts_Click(object sender, RoutedEventArgs e) {
			ListViewDiscounts.SelectAll();
		}


		private async void ButtonDelete_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
				"Вы уверены, что хотите удалить выбранные записи?", "Удаление",
				MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
				return;

			List<ItemDiscount> itemsToDeleteFromDb = new List<ItemDiscount>();
			List<ItemDiscount> itemsToDeleteFromListView = new List<ItemDiscount>();

			foreach (ItemDiscount item in ListViewDiscounts.SelectedItems) {
				if (item.IsNewAdded) {
					itemsToDeleteFromListView.Add(item);
					continue;
				}

				itemsToDeleteFromDb.Add(item);
			}

			Cursor = Cursors.Wait;

			await Task.Run(() => {
				SystemDataHandle.DeleteDiscounts(itemsToDeleteFromDb);
			});

			itemsToDeleteFromListView.AddRange(itemsToDeleteFromDb);
			foreach (ItemDiscount item in itemsToDeleteFromListView)
				ItemsDiscount.Remove(item);

			if (ItemsDiscount.Count == 0) {
				isDiscountsChanged = false;
				ButtonApplyChanges.IsEnabled = false;
			}

			Cursor = Cursors.Arrow;
		}


		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			string preview = "Добавление новой записи, выберите договор из списка";
			List<string> selectedContracts = new List<string>() { preview };

			foreach (ItemIC item in itemsIC)
				selectedContracts.Add(item.GetContractPreview());

			ItemsDiscount.Add(new ItemDiscount(preview) { Contract = selectedContracts, IsNewAdded = true });
			CheckBox_Click(null, null);
		}



		private void CheckBoxEndless_Click(object sender, RoutedEventArgs e) {
			foreach (ItemDiscount item in ItemsDiscount)
				item.ENDLESS = ((CheckBox)sender).IsChecked == true;
		}

		private void CheckBoxAmountRelation_Click(object sender, RoutedEventArgs e) {
			foreach (ItemDiscount item in ItemsDiscount)
				item.AMOUNTRELATION = ((CheckBox)sender).IsChecked == true;
		}

		private void CheckBox_Click(object sender, RoutedEventArgs e) {
			ComboBoxEndlessIsChecked = ItemsDiscount.Where(i => i.ENDLESS == false).ToList().Count == 0;
			ComboBoxAmountRelationIsChecked = ItemsDiscount.Where(i => i.AMOUNTRELATION == false).ToList().Count == 0;
		}



		private async void ButtonExportToExcel_Click(object sender, RoutedEventArgs e) {
			Cursor = Cursors.Wait;

			string result = string.Empty;
			await Task.Run(() => {
				result = NpoiExcel.WriteItemsDiscountToExcel(ItemsDiscount.ToList());
			});

			if (File.Exists(result)) {
				Process.Start(result);
			} else {
				MessageBox.Show(result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Cursor = Cursors.Arrow;
		}


		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e) {
			if (e.DataObject.GetDataPresent(typeof(String))) {
				String text = (String)e.DataObject.GetData(typeof(String));
				if (!IsTextAllowed(text)) {
					e.CancelCommand();
				}
			} else {
				e.CancelCommand();
			}
		}

		private static bool IsTextAllowed(string text) {
			Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
			return !regex.IsMatch(text);
		}


		private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			TextBox senderBox = (TextBox)sender;
			senderBox.Text = senderBox.Text.TrimStart('0').Replace(" ", "").Replace(",", "").Replace("'", "").Replace(".", "");
		}

		private void ButtonBack_Click(object sender, RoutedEventArgs e) {
			if (NeedToReturnToEditDiscounts())
				return;

			NavigationService.GoBack();
		}

		private void ButtonToStart_Click(object sender, RoutedEventArgs e) {
			if (NeedToReturnToEditDiscounts())
				return;

			while (NavigationService.CanGoBack)
				NavigationService.GoBack();
		}
	}
}
