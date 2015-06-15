using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Data;

namespace CustomLoading
{
    #region Converters
    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var sent = (int)value;
            return sent == 0 ? 22 : sent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    public partial class LoadingUserControl : UserControl
    {
        #region Properties
        public string LoadingText
        {
            get { return (string)GetValue(LoadingTextProperty); }
            set { SetValue(LoadingTextProperty, value); }
        }

        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register("LoadingText", typeof(string), typeof(LoadingUserControl), null);

        public Color FirstColor
        {
            get { return (Color)GetValue(FirstColorProperty); }
            set { SetValue(FirstColorProperty, value);  }
        }

        public static readonly DependencyProperty FirstColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(LoadingUserControl),null);

        public Color SecondColor
        {
            get { return (Color)GetValue(SecondColorProperty); }
            set { SetValue(SecondColorProperty, value); }
        }
        public string Letter
        {
            get { return (string)GetValue(LetterProperty); }
            set { SetValue(LetterProperty, value);  }
        }

        public static readonly DependencyProperty LetterProperty =
            DependencyProperty.Register("Letter", typeof(string), typeof(LoadingUserControl), null);

        public static readonly DependencyProperty SecondColorProperty =
            DependencyProperty.Register("SecondColor", typeof(Color), typeof(LoadingUserControl),null);

        public Color TextColor
        {
            get { return ( Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof( Color), typeof(LoadingUserControl),null);

        public SolidColorBrush ConvertedTextColor { get { return new SolidColorBrush(TextColor); } }
        
        
        private LinearGradientBrush _linear = new LinearGradientBrush();

        public LinearGradientBrush Linear
        {
            get { return _linear; }
            set { _linear = value; }
        }
        private DispatcherTimer DPT = new DispatcherTimer() { Interval = new TimeSpan(1000000) };
        private double inter = 0;

        #endregion
        public LoadingUserControl()
        {
            InitializeComponent();
            // Initialize the gradient brush
            Linear.StartPoint = new Point(0, 0);
            Linear.EndPoint = new Point(1, 1);

            this.Loaded += LoadingUserControl_Loaded;
        }

        private void LoadingUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Linear.GradientStops.Add(new GradientStop() { Offset = 1.0, Color = SecondColor });
            Linear.GradientStops.Add(new GradientStop() { Offset = 0.0, Color = FirstColor });

            // Initialize the timer
            DPT.Tick += (c, r) =>
            {
                if (inter >= 1.1) inter = 0;
                Linear.GradientStops.RemoveAt(1);
                Linear.GradientStops.Add(new GradientStop() { Offset = inter, Color = FirstColor });
                inter += 0.1;
            };
            DPT.Start();
            this.DataContext = this;
        }
    }
}
