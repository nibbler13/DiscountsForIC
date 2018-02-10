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
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public ObservableCollection<ItemIC> ItemsIC { get; set; } = new ObservableCollection<ItemIC>();
		public ObservableCollection<ItemDiscount> ItemsDiscount { get; set; } = new ObservableCollection<ItemDiscount>();
		private ListSortDirection sortDirectionListViewSearch;
		private GridViewColumnHeader sortColumnListViewSearch;
		private ListSortDirection sortDirectionListViewDiscounts;
		private GridViewColumnHeader sortColumnListViewDiscounts;

		public MainWindow() {
			InitializeComponent();
			DataContext = this;
			ItemsIC.CollectionChanged += 
				(s, e) => { AutoResizeGridViewColumns(ListViewSearchResults.View as GridView); };
			ItemsDiscount.CollectionChanged += 
				(s, e) => { AutoResizeGridViewColumns(ListViewDiscounts.View as GridView); };
			Closed += (s, e) => { SystemDataHandle.CloseConnection(); };
			TextBoxSearch.Focus();
			ItemsIC.Add(new ItemIC() { JNAME = "Введите текст и нажмите кнопку поиск" });
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

		private void ButtonSearch_Click(object sender, RoutedEventArgs e) {
			string enteredText = TextBoxSearch.Text;

			if (string.IsNullOrEmpty(enteredText) ||
				string.IsNullOrWhiteSpace(enteredText)) {
				MessageBox.Show(this, "Необходимо ввести текст в поле для ввода", string.Empty, 
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			ItemsIC.Clear();
			SystemDataHandle.SearchForIC(enteredText).ForEach(ItemsIC.Add);
		}

		private void ListViewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ItemsDiscount.Clear();

			if (ListViewSearchResults.SelectedItems.Count == 0) {
				ItemsDiscount.Add(new ItemDiscount() {
					FILIAL = "Выберите договор для просмотра информации о скидках"
				});
				return;
			}
			
			SystemDataHandle.SelectDiscount(ListViewSearchResults.SelectedItems).ForEach(ItemsDiscount.Add);
		}

		private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter)
				ButtonSearch_Click(sender, new RoutedEventArgs());
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

			Binding b = columnHeader.Column.DisplayMemberBinding as Binding;
			if (b != null)
				header = b.Path.Path;

			ICollectionView resultDataView = CollectionViewSource.GetDefaultView((sender as ListView).ItemsSource);
			resultDataView.SortDescriptions.Clear();
			resultDataView.SortDescriptions.Add(new SortDescription(header, sortDirection));
		}

		private void ListViewDiscounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}
	}
}
