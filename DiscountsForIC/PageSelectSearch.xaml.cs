using System;
using System.Collections.Generic;
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
	/// Логика взаимодействия для PageSelectSearch.xaml
	/// </summary>
	public partial class PageSelectSearch : Page {
		public PageSelectSearch() {
			InitializeComponent();
		}

		private void ButtonSearchByNameOrNumber_Click(object sender, RoutedEventArgs e) {
			PageSelectFilial pageSelectFilial = new PageSelectFilial(PageViewDiscounts.SearchType.ByNameOrNumber);
			NavigationService.Navigate(pageSelectFilial);
		}

		private void ButtonSearchByDate_Click(object sender, RoutedEventArgs e) {
			PageSelectFilial pageSelectFilial = new PageSelectFilial(PageViewDiscounts.SearchType.ByDate);
			NavigationService.Navigate(pageSelectFilial);
		}

		private void ButtonBack_Click(object sender, RoutedEventArgs e) {
			NavigationService.GoBack();
		}
	}
}
