﻿using System;
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
using System.Threading;
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
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public ObservableCollection<ItemIC> ItemsIC { get; set; } = new ObservableCollection<ItemIC>();
		public ObservableCollection<ItemDiscount> ItemsDiscount { get; set; } = new ObservableCollection<ItemDiscount>();

		private ListSortDirection sortDirectionListViewSearch;
		private GridViewColumnHeader sortColumnListViewSearch;
		private ListSortDirection sortDirectionListViewDiscounts;
		private GridViewColumnHeader sortColumnListViewDiscounts;

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


		public MainWindow() {
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo("ru-RU");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

			InitializeComponent();
			DataContext = this;
			ItemsIC.CollectionChanged += 
				(s, e) => { AutoResizeGridViewColumns(ListViewSearchResults.View as GridView); };
			ItemsDiscount.CollectionChanged += 
				(s, e) => { AutoResizeGridViewColumns(ListViewDiscounts.View as GridView); };
			Closing += MainWindow_Closing;
			TextBoxSearch.Focus();
			ItemsIC.Add(new ItemIC() { SHORTNAME = "Введите текст и нажмите кнопку поиск" });

			ItemsDiscount.CollectionChanged += ItemsDiscount_CollectionChanged;
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e) {
			if (NeedToReturnToEditDiscounts()) {
				e.Cancel = true;
				return;
			}

			SystemDataHandle.CloseConnection();
		}

		private bool NeedToReturnToEditDiscounts() {
			if (isDiscountsChanged) {
				MessageBoxResult messageBoxResult = MessageBox.Show(
					this, "Имеются изменения в текущих данных о скидках. Желаете сохранить изменения?",
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



		private void ListViewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			UpdateListViewDiscounts(sender);
		}

		private async void UpdateListViewDiscounts(object sender) {
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

			if (sender is ListView) {
				int selectedItems = ListViewSearchResults.SelectedItems.Count;
				ButtonAdd.IsEnabled = selectedItems > 0;
				if (selectedItems == 0)
					return;

				System.Collections.IList selectedItemsIc = ListViewSearchResults.SelectedItems;

				await Task.Run(() => {
					discounts = SystemDataHandle.SelectDiscount(selectedItemsIc);
				});
			} else if (sender is Button) {
				DateTime dateBegin = DatePickerDateBegin.SelectedDate.Value;
				DateTime? dateEnd =
					ComboBoxSelectDateEnd.IsChecked == false ?
					DatePickerEnd.SelectedDate : null;

				await Task.Run(() => {
					discounts = SystemDataHandle.SelectDiscoutByDates(dateBegin, dateEnd);
				});
			}

			if (discounts.Count == 0) {
				if (sender is ListView)
					ButtonAdd_Click(ButtonAdd, new RoutedEventArgs());
				else if (sender is Button)
					MessageBox.Show(this, "Нет данных за выбранный диапазон дат", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}

			ButtonExportToExcel.IsEnabled = discounts.Count > 0;
			ButtonSelectAllDiscounts.IsEnabled = discounts.Count > 0;

			discounts.ForEach(ItemsDiscount.Add);
			CheckBox_Click(null, null);
			Cursor = Cursors.Arrow;
		}

		private void ListViewDiscounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (ListViewDiscounts.SelectedItems.Count == 1 && (ListViewDiscounts.SelectedItem as ItemDiscount).BZ_ADID is null) {
				ButtonDelete.IsEnabled = false;
				return;
			}

			ButtonDelete.IsEnabled = ListViewDiscounts.SelectedItems.Count > 0;
		}

		
		private void ListViewHeader_Click(object sender, RoutedEventArgs e) {
			if (sender == ListViewSearchResults)
				SortListViewColumn(sender, e, ref sortColumnListViewSearch, ref sortDirectionListViewSearch);
			else if (sender == ListViewDiscounts)
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

		
		private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				ButtonSearch_Click(sender, new RoutedEventArgs());
		}

		private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { 
			TextBox senderBox = (TextBox)sender;
			senderBox.Text = senderBox.Text.TrimStart('0').Replace(" ", "").Replace(",", "").Replace("'", "").Replace(".", "");
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

		
		private async void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			string enteredText = TextBoxSearch.Text;

			if (string.IsNullOrEmpty(enteredText) ||
				string.IsNullOrWhiteSpace(enteredText)) {
				MessageBox.Show(this, "Необходимо ввести текст в поле для ввода", string.Empty,
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			ItemsIC.Clear();

			Cursor = Cursors.Wait;

			List<ItemIC> resultItems = new List<ItemIC>();

			await Task.Run(() => {
				resultItems = SystemDataHandle.SearchForIC(enteredText);
			});
			
			
			ButtonSelectAllIC.IsEnabled = resultItems.Count > 0;

			if (resultItems.Count == 0) 
				resultItems.Add(new ItemIC() { SHORTNAME = "По указаному запросу ничего не найдено" });

			resultItems.ForEach(ItemsIC.Add);

			Cursor = Cursors.Arrow;
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			string preview = "Добавление новой записи, выберите договор из списка";
			List<string> selectedContracts = new List<string>() { preview };

			foreach (ItemIC item in ListViewSearchResults.SelectedItems)
				selectedContracts.Add(GetContractPreview(item));

			ItemsDiscount.Add(new ItemDiscount(preview){ Contract = selectedContracts });
			CheckBox_Click(null, null);
		}

		private async void ButtonDelete_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(this,"Вы уверены, что хотите удалить выбранные записи?", "Удаление",
				MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
				return;

			List<ItemDiscount> itemsToDelete = new List<ItemDiscount>();

			foreach (ItemDiscount item in ListViewDiscounts.SelectedItems)
				itemsToDelete.Add(item);

			Cursor = Cursors.Wait;

			await Task.Run(() => {
				SystemDataHandle.DeleteDiscounts(itemsToDelete);
			});

			foreach (ItemDiscount item in itemsToDelete)
				ItemsDiscount.Remove(item);

			Cursor = Cursors.Arrow;
		}

		private async void ButtonApplyChanges_Click(object sender, RoutedEventArgs e) {
			foreach (ItemDiscount itemDiscount in ItemsDiscount) {
				if (!itemDiscount.IsChanged || itemDiscount.BZ_ADID != null)
					continue;
				
				if (!itemDiscount.ContractPreview.Contains(" / ")) {
					MessageBox.Show(this, "Имеются добавленные новые записи, у которых не выбран договор", 
						"Отмена", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				bool isFound = false;
				foreach (ItemIC itemIC in ListViewSearchResults.SelectedItems) {
					if (itemDiscount.ContractPreview.Equals(GetContractPreview(itemIC))) {
						itemDiscount.JID = itemIC.JID;
						itemDiscount.AGRID = itemIC.AGRID;
						itemDiscount.FILIAL = itemIC.FILIAL;
						isFound = true;
						break;
					}
				}

				if (!isFound) {
					MessageBox.Show(this, "Не удалось определить данные договора для строки: " + itemDiscount.ContractPreview,
						"Ошибка обработки", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			Cursor = Cursors.Wait;

			await Task.Run(() => {
				SystemDataHandle.UpdateOrInsertDiscount(ItemsDiscount.Where(i => i.IsChanged).ToList());
			});

			Cursor = Cursors.Arrow;

			isDiscountsChanged = false;
			((Button)sender).IsEnabled = false;
			ListViewSearchResults_SelectionChanged(ListViewSearchResults, null);
		}

		private string GetContractPreview(ItemIC item) {
			return item.SHORTNAME + " / " + item.JNAME + " / " + item.AGNUM;
		}

		private void ButtonSelectAllIC_Click(object sender, RoutedEventArgs e) {
			ListViewSearchResults.SelectAll();
		}

		private void ButtonSelectAllDiscounts_Click(object sender, RoutedEventArgs e) {
			ListViewDiscounts.SelectAll();
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
				MessageBox.Show(this, result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Cursor = Cursors.Arrow;
		}

		private void ComboBoxSelectDateEnd_Checked(object sender, RoutedEventArgs e) {
			SetDatePickerEndEnable();
		}

		private void ComboBoxSelectDateEnd_Unchecked(object sender, RoutedEventArgs e) {
			SetDatePickerEndEnable();
		}

		public void SetDatePickerEndEnable() {
			if (DatePickerEnd == null)
				return;

			DatePickerEnd.IsEnabled =
				ComboBoxSelectDateEnd.IsChecked != null &&
				ComboBoxSelectDateEnd.IsChecked == false;
		}

		private void ButtonSearchByDate_Click(object sender, RoutedEventArgs e) {
			if (DatePickerDateBegin.SelectedDate == null ||
				(ComboBoxSelectDateEnd.IsChecked == false && DatePickerEnd.SelectedDate == null)) {
				MessageBox.Show(this, "Выберите корректный диапазон дат", "", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			UpdateListViewDiscounts(sender);
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.Source is TabControl) {
				if (TabControlSearch.SelectedIndex == 0) {
					if (ListViewSearchResults.SelectedItems.Count > 0)
						UpdateListViewDiscounts(ListViewSearchResults);
					else
						ClearListViewDiscounts();
				} else if (TabControlSearch.SelectedIndex == 1) {
					if ((DatePickerDateBegin.SelectedDate != null &&
						ComboBoxSelectDateEnd.IsChecked == true) ||
						(ComboBoxSelectDateEnd.IsChecked == false &&
						DatePickerEnd.SelectedDate != null &&
						DatePickerDateBegin.SelectedDate != null))
						UpdateListViewDiscounts(ButtonSearchByDate);
					else
						ClearListViewDiscounts();
				}
			}
		}

		private void ClearListViewDiscounts() {
			if (NeedToReturnToEditDiscounts())
				return;

			List<Button> buttonsToDisable = new List<Button>() {
				ButtonSelectAllDiscounts,
				ButtonAdd,
				ButtonApplyChanges,
				ButtonDelete,
				ButtonExportToExcel
			};

			foreach (Button button in buttonsToDisable)
				button.IsEnabled = false;

			ItemsDiscount.Clear();
			isDiscountsChanged = false;
		}
	}
}
