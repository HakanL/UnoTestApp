using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using DMXCore.DMXCore100.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.UI.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace DMXCore.DMXCore100.Views
{
    [ContentProperty(Name = "AdditionalContent")]
    public sealed partial class FooterControl : UserControl
    {
        private readonly ILogger log;
        public BaseViewModel? ViewModel => DataContext as BaseViewModel;

        private Visibility adminVisibility = Visibility.Visible;

        public FooterControl()
        {
            this.log = App.Host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<FooterControl>();

            this.InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is BaseViewModel viewModel)
                {
                    // Update VM
                    viewModel.AdminOptionVisibleInControl = AdminVisibility == Visibility.Visible;
                }
            };
        }

        public Button BackButton => ReturnButton;

        public FontIcon BackButtonIcon { get; set; }

        public Visibility AdminVisibility
        {
            get => this.adminVisibility;
            set
            {
                this.adminVisibility = value;
                if (ViewModel != null)
                    ViewModel.AdminOptionVisibleInControl = value == Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets or sets additional content for the UserControl
        /// </summary>
        public object AdditionalContent
        {
            get { return (object)GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register("AdditionalContent", typeof(object), typeof(FooterControl),
              new PropertyMetadata(null));

        private void Clock_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Microsoft.UI.Input.HoldingState.Started)
            {
                ViewModel?.ClockHeldCommand?.Execute(null);
            }
        }

        private void Clock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
#if DEBUG
            // Output the visual tree graph
            System.Diagnostics.Debug.WriteLine(this.TreeGraph());
#endif

            ViewModel?.ClockHeldCommand?.Execute(null);
        }
    }
}
