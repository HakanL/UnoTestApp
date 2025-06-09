using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100.UnoHost.Extensions;

internal class ScrollbarTweak
{
    private bool scrollbarsHooked;
    private readonly FrameworkElement scrollContainer;

    private ScrollbarTweak(Page page, FrameworkElement scrollContainer)
    {
        this.scrollContainer = scrollContainer ?? throw new ArgumentNullException(nameof(scrollContainer));
        page.LayoutUpdated += Page_LayoutUpdated;
    }

    private void Page_LayoutUpdated(object? sender, object e)
    {
#if HAS_UNO
        if (!this.scrollbarsHooked)
        {
            var sv = this.scrollContainer;

            var vsb = (Control)sv.FindName("VerticalScrollBar");

            if (vsb != null)
            {
                vsb.LayoutUpdated += new EventHandler<object>((object _, object _) =>
                {
                    VisualStateManager.GoToState(vsb, "MouseIndicator", true);
                    VisualStateManager.GoToState(vsb, "Expanded", true);
                });

                this.scrollbarsHooked = true;
            }
        }
#endif
    }

    public static void Tweak(Page page, FrameworkElement scrollContainer)
    {
        new ScrollbarTweak(page, scrollContainer);
    }
}
