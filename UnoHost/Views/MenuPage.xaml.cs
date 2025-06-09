//#define TRACE_POINTER

using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.UnoHost.Extensions;
using DMXCore.DMXCore100.ViewModels;

namespace DMXCore.DMXCore100.Views;

public sealed partial class MenuPage : Page, IFocusableItemsProvider, ICanSetFocus
{
    private readonly ILogger log;
    private readonly Services.IMenuFocusManager menuFocusManager;
    private readonly Stopwatch renderDuration = new Stopwatch();

    public MenuViewModel? ViewModel => DataContext as MenuViewModel;

    public MenuPage()
    {
        this.log = App.Host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<MenuPage>();

        this.InitializeComponent();

        this.menuFocusManager = App.Host.Services.GetRequiredService<Services.IMenuFocusManager>();

        this.log.LogDebug("MenuPage instance created");

        DataContextChanged += (s, e) =>
        {
            this.log.LogTrace("Data context assigned = {DC}, with id {InvocationId}", DataContext?.GetType().Name, (DataContext as BaseViewModel)?.InvokationId);

            this.renderDuration.Restart();
        };

        ScrollbarTweak.Tweak(this, RepeaterScrollViewer);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        this.log.LogTrace("Render took {RenderDuration} ms", this.renderDuration.ElapsedMilliseconds);

        return base.ArrangeOverride(finalSize);
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

        if (sender is DependencyObject depObj)
        {
            this.menuFocusManager.FocusItemTapped(depObj);
        }
    }

    private void Card_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is DependencyObject depObj)
        {
            this.menuFocusManager.FocusItemHeld(depObj);
        }
    }

    private void Card_Holding(object sender, HoldingRoutedEventArgs e)
    {
#if TRACE_POINTER && DEBUG
        this.log.LogTrace("Card holding state: {State}", e.HoldingState);
#endif

        if (e.HoldingState == Microsoft.UI.Input.HoldingState.Started)
        {
            if (sender is DependencyObject depObj)
            {
                this.menuFocusManager.FocusItemHeld(depObj);
            }
        }
    }
}
