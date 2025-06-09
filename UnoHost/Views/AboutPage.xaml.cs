using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.UnoHost.Extensions;
using DMXCore.DMXCore100.ViewModels;

namespace DMXCore.DMXCore100.Views;

public sealed partial class AboutPage : Page, IFocusableItemsProvider, ICanSetFocus
{
    public AboutPage()
    {
        this.InitializeComponent();

        ScrollbarTweak.Tweak(this, RepeaterScrollViewer);
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
            AnimationDesired = false,
            VerticalAlignmentRatio = 0.5F
        });
    }
}

public class AboutItemListTemplateSelector : DataTemplateSelector
{
    public DataTemplate Normal { get; set; }

    public DataTemplate Header { get; set; }

    protected override DataTemplate SelectTemplateCore(object item) =>
        item switch
        {
            AboutHeaderItem => Header,
            AboutItem => Normal,
            _ => null
        };
}
