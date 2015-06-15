using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace GTVWin8.Converters
{
    public class MuteStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var nRet = (double)value;
            if (nRet == 0)
                return "ms-appx:///Images/BottomMenu/Volume.png";
            else
                return "ms-appx:///Images/BottomMenu/VolumeMute.png";
              
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
