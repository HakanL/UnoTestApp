using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.ViewModels;
using Windows.UI.WebUI;

namespace DMXCore.DMXCore100.Views;

public sealed partial class HomePage : Page, IFocusableItemsProvider, INotifyPropertyChanged
{
    private readonly ILogger log;
    private readonly Services.IMenuFocusManager menuFocusManager;
    private readonly IScheduler scheduler;

    public HomeViewModel? ViewModel => DataContext as HomeViewModel;

    public event PropertyChangedEventHandler PropertyChanged;

    public void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public HomePage()
    {
        this.log = App.Host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<HomePage>();
        this.scheduler = App.Host.Services.GetRequiredService<IScheduler>();
        this.menuFocusManager = App.Host.Services.GetRequiredService<Services.IMenuFocusManager>();

        this.InitializeComponent();

        DataContextChanged += (s, e) =>
        {
            RaisePropertyChanged(nameof(ViewModel));
        };
    }

    public IEnumerable<DependencyObject> FocusableItems
    {
        get
        {
            return new List<DependencyObject>
                {
                    Footer.BackButton
                };
        }
    }

    private void Logo_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ViewModel?.SetPlayDetailsVisible(true);
    }

    private void PlayerDetails_Tapped(object sender, TappedRoutedEventArgs e)
    {
        this.menuFocusManager.FocusItemTapped(Footer.BackButton);
    }
}
