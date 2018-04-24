using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Логика взаимодействия для PageSelectFilial.xaml
	/// </summary>
	public partial class PageSelectFilial : Page {
		public ObservableCollection<ItemFilial> ItemsFilialAll { get; set; } = new ObservableCollection<ItemFilial>();
		public ObservableCollection<ItemFilial> ItemsFilialSelected { get; set; } = new ObservableCollection<ItemFilial>();

		private PageViewDiscounts.SearchType searchType;

		public PageSelectFilial(PageViewDiscounts.SearchType searchType) {
			InitializeComponent();

			this.searchType = searchType;
			
			ListViewFilialsAll.DataContext = this;
			ListViewFilialsSelected.DataContext = this;
			ItemsFilialAll.CollectionChanged += (s, e) => {
				ButtonFilialAllToSelected.IsEnabled = ItemsFilialAll.Count > 0;
			};

			ItemsFilialSelected.CollectionChanged += (s, e) => {
				ButtonFilialAllRemoveFromSelected.IsEnabled = ItemsFilialSelected.Count > 0;
			};

			GetFilials(null, null);
		}

		private async void GetFilials(object sender, RoutedEventArgs e) {
			List<ItemFilial> itemsFilial = null;

			await Task.Run(() => {
				itemsFilial = SystemDataHandle.GetFilials();
			});

			if (itemsFilial != null)
				itemsFilial.ForEach(ItemsFilialAll.Add);
		}

		private void ListViewFilialsAll_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonFilialToSelected.IsEnabled = ListViewFilialsAll.SelectedItems.Count > 0;
		}

		private void ListViewFilialsSelected_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ButtonFilialRemoveFromSelected.IsEnabled = ListViewFilialsSelected.SelectedItems.Count > 0;
		}

		private void ListViewFilialsAll_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ItemFilial itemFilial = ListViewFilialsAll.SelectedItem as ItemFilial;

			if (itemFilial is null)
				return;

			ItemsFilialSelected.Add(itemFilial);
			ItemsFilialAll.Remove(itemFilial);
		}

		private void ListViewFilialsSelected_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			ItemFilial itemFilial = ListViewFilialsSelected.SelectedItem as ItemFilial;

			if (itemFilial is null)
				return;

			ItemsFilialSelected.Remove(itemFilial);
			ItemsFilialAll.Add(itemFilial);
		}

		private void ButtonFilialToSelected_Click(object sender, RoutedEventArgs e) {
			List<ItemFilial> itemsToMove = new List<ItemFilial>();
			foreach (ItemFilial item in ListViewFilialsAll.SelectedItems)
				itemsToMove.Add(item);

			foreach (ItemFilial item in itemsToMove) {
				ItemsFilialSelected.Add(item);
				ItemsFilialAll.Remove(item);
			}
		}

		private void ButtonFilialRemoveFromSelected_Click(object sender, RoutedEventArgs e) {
			List<ItemFilial> itemsToMove = new List<ItemFilial>();
			foreach (ItemFilial item in ListViewFilialsSelected.SelectedItems)
				itemsToMove.Add(item);

			foreach (ItemFilial item in itemsToMove) {
				ItemsFilialSelected.Remove(item);
				ItemsFilialAll.Add(item);
			}
		}

		private void ButtonFilialAllToSelected_Click(object sender, RoutedEventArgs e) {
			foreach (ItemFilial item in ItemsFilialAll)
				ItemsFilialSelected.Add(item);

			ItemsFilialAll.Clear();
		}

		private void ButtonFilialAllRemoveFromSelected_Click(object sender, RoutedEventArgs e) {
			foreach (ItemFilial item in ItemsFilialSelected)
				ItemsFilialAll.Add(item);

			ItemsFilialSelected.Clear();
		}

		private void ButtonBack_Click(object sender, RoutedEventArgs e) {
			NavigationService.GoBack();
		}

		private void ButtonNext_Click(object sender, RoutedEventArgs e) {
			if (ItemsFilialSelected.Count == 0) {
				MessageBox.Show("Необходимо выбрать один или несколько филиалов для продолжения", "", 
					MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			List<ItemFilial> itemsFilial = ItemsFilialSelected.ToList();
			Page page;
			switch (searchType) {
				case PageViewDiscounts.SearchType.ByNameOrNumber:
					page = new PageSearchByNameOrNumber(itemsFilial);
					break;
				case PageViewDiscounts.SearchType.ByDate:
					page = new PageSearchByDate(itemsFilial);
					break;
				default:
					return;
			}

			NavigationService.Navigate(page);
		}
	}
}
