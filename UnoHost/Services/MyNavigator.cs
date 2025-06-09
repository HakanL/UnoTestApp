using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100.Services;

public class MyNavigator : IMyNavigator, IInstance<IServiceProvider>
{
    private readonly INavigator navigator;
    private readonly IMenuFocusManager menuFocusManager;
    private readonly IMenuManager menuManager;

    public MyNavigator(INavigator navigator, IMenuFocusManager menuFocusManager, IMenuManager menuManager)
    {
        this.navigator = navigator;
        this.menuFocusManager = menuFocusManager;
        this.menuManager = menuManager;
    }

    public Route? Route => this.navigator.Route;

    IServiceProvider? IInstance<IServiceProvider>.Instance => (this.navigator as IInstance<IServiceProvider>).Instance;

    public Task<bool> CanNavigate(Route route)
    {
        return this.navigator.CanNavigate(route);
    }

    public async Task<NavigationResponse?> NavigateAsync(NavigationRequest request)
    {
        this.menuFocusManager.StartNavigating();

        switch (request.Route.Qualifier)
        {
            case Qualifiers.ClearBackStack:
                this.menuFocusManager.ClearBackStack();
                break;

            case Qualifiers.NavigateBack:
                this.menuFocusManager.NavigateBack();
                break;

            case Qualifiers.Dialog:
                this.menuFocusManager.NavigateDialog(request.Cancellation);
                break;
        }

        var response = await this.navigator.NavigateAsync(request);

        return response;
    }

    public async Task<NavigationResponse?> NavigateBackOrHomeAsync(object sender, object? resultData = null)
    {
        this.menuFocusManager.NavigateBack();

        NavigationResponse? response;
        if (resultData != null)
        {
            response = await this.navigator.NavigateBackWithResultAsync(sender, data: resultData);
        }
        else
        {
            response = await this.navigator.NavigateBackAsync(sender);
        }

        if (response == null)
        {
            return await this.menuManager.NavigateToStart(sender, this);
        }

        return response;
    }
}
