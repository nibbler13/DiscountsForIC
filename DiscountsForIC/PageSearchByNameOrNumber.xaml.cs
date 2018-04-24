using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
	/// Логика взаимодействия для PageSearchByNameOrNumber.xaml
	/// </summary>
	public partial class PageSearchByNameOrNumber : Page {
		public ObservableCollection<ItemIC> ItemsIC { get; set; } = new ObservableCollection<ItemIC>();
		private List<ItemFilial> itemsFilial;

		private ListSortDirection sortDirectionListViewSearch;
		private GridViewColumnHeader sortColumnListViewSearch;

		private bool isSearchCompleted = false;

		public PageSearchByNameOrNumber(List<ItemFilial> itemsFilial) {
			InitializeComponent();
			this.itemsFilial = itemsFilial;

			TextBlockSelectedFilials.Text = TextBlockSelectedFilials.Text + string.Join(", ", itemsFilial.Select(x => x.ShortName).ToArray());

			DataContext = this;
			ItemsIC.CollectionChanged +=
				(s, e) => { AutoResizeGridViewColumns(ListViewSearchResults.View as GridView); };

			TextBoxSearch.Focus();
			ItemsIC.Add(new ItemIC() { SHORTNAME = "Введите текст и нажмите кнопку поиск" });
		}


		private void ListViewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			//UpdateListViewDiscounts(sender);
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



		private void ListViewHeader_Click(object sender, RoutedEventArgs e) {
			SortListViewColumn(sender, e, ref sortColumnListViewSearch, ref sortDirectionListViewSearch);
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


		private void ButtonSelectAllIC_Click(object sender, RoutedEventArgs e) {
			ListViewSearchResults.SelectAll();
		}


		private async void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			string enteredText = TextBoxSearch.Text;

			if (string.IsNullOrEmpty(enteredText) ||
				string.IsNullOrWhiteSpace(enteredText)) {
				MessageBox.Show(
					"Необходимо ввести текст в поле для ввода", string.Empty,
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			ItemsIC.Clear();

			Cursor = Cursors.Wait;

			List<ItemIC> resultItems = new List<ItemIC>();
			bool displayClosedContracts = CheckBoxUseClosedContracts.IsChecked == true ? true : false;

			await Task.Run(() => {
				resultItems = SystemDataHandle.SearchForIC(enteredText, itemsFilial, displayClosedContracts);
			});


			ButtonSelectAllIC.IsEnabled = resultItems.Count > 0;
			isSearchCompleted = resultItems.Count > 0;

			if (resultItems.Count == 0)
				resultItems.Add(new ItemIC() { SHORTNAME = "По указаному запросу ничего не найдено" });

			resultItems.ForEach(ItemsIC.Add);

			Cursor = Cursors.Arrow;
		}


		private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				ButtonSearch_Click(sender, new RoutedEventArgs());
		}

		private void ButtonBack_Click(object sender, RoutedEventArgs e) {
			NavigationService.GoBack();
		}

		private void ButtonNext_Click(object sender, RoutedEventArgs e) {
			string message = string.Empty;

			if (!isSearchCompleted)
				message = "Необходимо выполнить поиск и выбрать один или несколько договоров";
			else if (ListViewSearchResults.SelectedItems.Count == 0)
				message = "Необходимо выбрать один или несколько договоров для продолжения";

			if (!string.IsNullOrEmpty(message)) {
				MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}
			
			List<ItemIC> itemsIc = new List<ItemIC>();

			foreach (ItemIC item in ListViewSearchResults.SelectedItems)
				itemsIc.Add(item);

			PageViewDiscounts pageViewDiscounts = new PageViewDiscounts(itemsIc);
			NavigationService.Navigate(pageViewDiscounts);
		}

		private void CheckBoxUseClosedContracts_Checked(object sender, RoutedEventArgs e) {
			UpdateList();
		}

		private void CheckBoxUseClosedContracts_Unchecked(object sender, RoutedEventArgs e) {
			UpdateList();
		}

		private void UpdateList() {
			if (string.IsNullOrEmpty(TextBoxSearch.Text))
				return;

			ButtonSearch_Click(null, null);
		}
	}
}
