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
	/// Логика взаимодействия для PageWelcome.xaml
	/// </summary>
	public partial class PageSelectOperation : Page {
		public PageSelectOperation() {
			InitializeComponent();
		}

		private void ButtonOprionEditRemoveReport_Click(object sender, RoutedEventArgs e) {
			PageSelectSearch pageSelectSearch = new PageSelectSearch();
			NavigationService.Navigate(pageSelectSearch);
		}

		private void ButtonAdd_Click(object sender, RoutedEventArgs e) {
			PageSelectFilial pageSelectFilial = new PageSelectFilial(PageViewDiscounts.SearchType.ByNameOrNumber);
			NavigationService.Navigate(pageSelectFilial);
		}
	}
}
