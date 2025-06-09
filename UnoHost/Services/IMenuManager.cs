using System;
using System.Collections.Generic;
using System.Text;
using DMXCore.DMXCore100.Models;

namespace DMXCore.DMXCore100.Services;

public interface IMenuManager
{
    Task<Menu> GetRootMenuItems(INavigator navigator);

    Task<NavigationResponse?> NavigateToStart(object sender, INavigator navigator);
}
