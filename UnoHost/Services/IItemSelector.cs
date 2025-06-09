using System;
using System.Collections.Generic;
using System.Text;

namespace DMXCore.DMXCore100.Services;

public interface IItemSelector
{
    Task SelectorPressedShort(DependencyObject focusItem);

    Task SelectorPressedLong(DependencyObject focusItem);
}
