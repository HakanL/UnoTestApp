using System.Reactive.Linq;
using DMXCore.DMXCore100.Extensions;
using DMXCore.DMXCore100.ViewModels;

namespace DMXCore.DMXCore100.Views;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();

        PatchSplashContentPresenter(Splash);
    }

    private bool PatchSplashContentPresenter(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is ContentControl contentControl && contentControl.Name == "LoadingContentPresenter")
            {
                // Found

                contentControl.IsTabStop = false;

                return true;
            }

            bool result = PatchSplashContentPresenter(child);
            if (result)
                return true;
        }

        return false;
    }

    public ContentControl ContentControl => Splash;
}
