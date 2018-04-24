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
	public partial class MainWindow : Window {



		public MainWindow() {

			InitializeComponent();

		}

		


		



		//private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
		//	if (e.Source is TabControl) {
		//		if (TabControlSearch.SelectedIndex == 0) {
		//			if (ListViewSearchResults.SelectedItems.Count > 0)
		//				UpdateListViewDiscounts(ListViewSearchResults);
		//			else
		//				ClearListViewDiscounts();
		//		} else if (TabControlSearch.SelectedIndex == 1) {
		//			if ((DatePickerDateBegin.SelectedDate != null &&
		//				ComboBoxSelectDateEnd.IsChecked == true) ||
		//				(ComboBoxSelectDateEnd.IsChecked == false &&
		//				DatePickerEnd.SelectedDate != null &&
		//				DatePickerDateBegin.SelectedDate != null))
		//				UpdateListViewDiscounts(ButtonSearchByDate);
		//			else
		//				ClearListViewDiscounts();
		//		}
		//	}
		//}

		//private void ClearListViewDiscounts() {
		//	if (NeedToReturnToEditDiscounts())
		//		return;

		//	List<Button> buttonsToDisable = new List<Button>() {
		//		ButtonSelectAllDiscounts,
		//		ButtonAdd,
		//		ButtonApplyChanges,
		//		ButtonDelete,
		//		ButtonExportToExcel
		//	};

		//	foreach (Button button in buttonsToDisable)
		//		button.IsEnabled = false;

		//	ItemsDiscount.Clear();
		//	isDiscountsChanged = false;
		//}


	}
}
