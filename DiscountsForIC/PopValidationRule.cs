using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DiscountsForIC {
	public class PopValidationRule : ValidationRule {
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			string enteredText = (string)value;

			if (int.TryParse(enteredText, out int popVal) &&
				(popVal < 0 || popVal > 100)) {
				MessageBox.Show("Размер скидки должен быть в диапазоне от 0 до 100", "", MessageBoxButton.OK, MessageBoxImage.Warning);
				return new ValidationResult(false, null);
			}

			return new ValidationResult(true, null);
		}
	}
}
