using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100.Services;

public interface IMyNavigator : INavigator
{
    Task<NavigationResponse?> NavigateBackOrHomeAsync(object sender, object? resultData = null);
}
