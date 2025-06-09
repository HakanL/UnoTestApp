using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.UnoHost.Extensions;
using DMXCore.DMXCore100.ViewModels;

namespace DMXCore.DMXCore100.Views;

public sealed partial class MenuPageSmall : Page, IFocusableItemsProvider, ICanSetFocus
{
    private readonly ILogger log;
    private readonly Services.IMenuFocusManager menuFocusManager;

    public MenuSmallViewModel? ViewModel => DataContext as MenuSmallViewModel;

    public MenuPageSmall()
    {
        this.log = App.Host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<MenuPageSmall>();
        this.InitializeComponent();

        this.menuFocusManager = App.Host.Services.GetRequiredService<Services.IMenuFocusManager>();

        ScrollbarTweak.Tweak(this, ListContainer);
    }

    public IEnumerable<DependencyObject> FocusableItems
    {
        get
        {
            return new List<DependencyObject>
                {
                    Footer.BackButton,
                    ListContainer
                };
        }
    }

    public void SetFocus(DependencyObject item)
    {
        (item as UIElement)?.StartBringIntoView(new BringIntoViewOptions
        {
            AnimationDesired = false
        });
    }

    private void Card_Tapped(object sender, RoutedEventArgs e)
    {
        if (sender is Card card)
        {
            this.log.LogTrace("Tap on card {CardHeader}", card.HeaderContent);
        }

        var listViewItem = Extensions.UiControlHelper.FindParent<ListViewItem>(sender as DependencyObject);

        if (listViewItem != null)
        {
            this.menuFocusManager.FocusItemTapped(listViewItem);
        }
    }

    private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var listViewItem = Extensions.UiControlHelper.FindParent<ListViewItem>(sender as DependencyObject);

        if (listViewItem != null)
        {
            this.menuFocusManager.FocusItemHeld(listViewItem);
        }
    }

    private void Card_Holding(object sender, HoldingRoutedEventArgs e)
    {
        if (e.HoldingState == Microsoft.UI.Input.HoldingState.Started)
        {
            var listViewItem = Extensions.UiControlHelper.FindParent<ListViewItem>(sender as DependencyObject);
            if (listViewItem != null)
            {
                this.menuFocusManager.FocusItemHeld(listViewItem);
            }
        }
    }
}
