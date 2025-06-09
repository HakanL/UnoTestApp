using System;

namespace DMXCore.DMXCore100.Services;

public interface IProvidesDefaultFocusItem
{
    DependencyObject? DefaultFocusItem { get; }
}
