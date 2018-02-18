using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DiscountsForIC {
	public class BoolToEnableConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return ((List<string>)value).Count > 1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}
