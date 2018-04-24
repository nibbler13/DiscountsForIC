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
	/// Логика взаимодействия для PageSearchByDate.xaml
	/// </summary>
	public partial class PageSearchByDate : Page {
		private List<ItemFilial> itemsFilial;

		public PageSearchByDate(List<ItemFilial> itemsFilial) {
			InitializeComponent();
			this.itemsFilial = itemsFilial;

			TextBlockSelectedFilials.Text = TextBlockSelectedFilials.Text + string.Join(", ", itemsFilial.Select(x => x.ShortName).ToArray());
		}

		private void CheckBoxDontSelectDateEnd_Checked(object sender, RoutedEventArgs e) {
			SetStackPanelDateEnable();
		}

		private void CheckBoxDontSelectDateEnd_Unchecked(object sender, RoutedEventArgs e) {
			SetStackPanelDateEnable();
		}

		private void SetStackPanelDateEnable() {
			if (StackPanelDatePick != null)
				StackPanelDatePick.IsEnabled = !CheckBoxDontSelectDateEnd.IsChecked ?? true;
		}

		private async void ButtonSearchByDate_Click(object sender, RoutedEventArgs e) {
			bool showErrorMessage = false;

			if (DatePickerDateBegin.SelectedDate == null)
				showErrorMessage = true;

			if (CheckBoxDontSelectDateEnd.IsChecked == false)
				if (RadioButtonDateSelect.IsChecked == true) {
					if (DatePickerEnd.SelectedDate == null)
						showErrorMessage = true;
					else if (DatePickerEnd.SelectedDate.Value < DatePickerDateBegin.SelectedDate.Value)
						showErrorMessage = true;
				}
			
			if (showErrorMessage) {
				MessageBox.Show("Выберите корректный диапазон дат", "", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			string sqlQuery = Properties.Settings.Default.MisSqlSelectDiscountsByDatesTemplate + Environment.NewLine;
			bool strongEqual = RadioButtonStrongEqual.IsChecked ?? false;
			bool dontSelectEndDate = CheckBoxDontSelectDateEnd.IsChecked ?? false;
			bool endDateSelected = RadioButtonDateSelect.IsChecked ?? false;

			if (dontSelectEndDate) {
				if (strongEqual) {
					sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginStrongEqual;
				} else {
					sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginWeakEqual;
				}
			} else {
				if (endDateSelected) {
					if (strongEqual) {
						sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginEndStrongEqual;
					} else {
						sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginEndWeakEqual;
					}
				} else {
					if (strongEqual) {
						sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginEndlesslStrongEqual;
					} else {
						sqlQuery += Properties.Settings.Default.MisSqlSelectDiscountsByDateBeginEndlesslWeakEqual;
					}
				}
			}

			DateTime? dateTimeEnd = null;
			if (!dontSelectEndDate && endDateSelected)
				dateTimeEnd = DatePickerEnd.SelectedDate.Value;

			Dictionary<string, string> parameters = new Dictionary<string, string>() {
				{ "@dateBegin", DatePickerDateBegin.SelectedDate.Value.ToShortDateString() }
			};

			if (dateTimeEnd.HasValue)
				parameters.Add("@dateEnd", dateTimeEnd.Value.ToShortDateString());

			bool showClosesContracts = CheckBoxShowClosedContracts.IsChecked ?? false;
			List<ItemDiscount> itemsDiscount = null;

			Cursor = Cursors.Wait;

			await Task.Run(() => {
				itemsDiscount = SystemDataHandle.SelectDiscoutByDates(sqlQuery, parameters, itemsFilial, showClosesContracts);
			});

			Cursor = Cursors.Arrow;

			if (itemsDiscount.Count == 0) {
				MessageBox.Show("Нет данных за выбранный диапазон дат", "", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}
			
			PageViewDiscounts pageViewDiscounts = new PageViewDiscounts(sqlQuery, parameters, itemsFilial, showClosesContracts);
			NavigationService.Navigate(pageViewDiscounts);
		}

		private void ButtonBack_Click(object sender, RoutedEventArgs e) {
			NavigationService.GoBack();
		}

		private void RadioButtonDateSelect_Checked(object sender, RoutedEventArgs e) {
			SetDatePickerEndEnable();
		}

		private void RadioButtonDateSelect_Unchecked(object sender, RoutedEventArgs e) {
			SetDatePickerEndEnable();
		}

		public void SetDatePickerEndEnable() {
			if (DatePickerEnd != null)
				DatePickerEnd.IsEnabled = RadioButtonDateSelect.IsChecked ?? false;
		}
	}
}
