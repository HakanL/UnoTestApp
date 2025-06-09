using System;

namespace DMXCore.DMXCore100.Services;

public interface IFocusableItemsProvider
{
    IEnumerable<DependencyObject> FocusableItems { get; }
}
