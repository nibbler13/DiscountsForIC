using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DiscountsForIC {
	class BoolToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			bool enabled = value.ToString().Equals("True");
			bool direct = parameter.ToString().Equals("direct");

			if ((enabled && direct) || (!enabled && !direct))
				return Visibility.Visible;

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}
