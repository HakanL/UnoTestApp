//#define LOG_FOCUS_EVENTS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using DMXCore.DMXCore100.Extensions;
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.ViewModels;
using DMXCore.DMXCore100.Views;
using Uno.Extensions.Specialized;

namespace DMXCore.DMXCore100.Services;

public class MenuFocusManager : IMenuFocusManager
{
    private static readonly Dictionary<int, string> aliveInvokations = new();

    public class NavigationStackItem
    {
        public bool IsDialog { get; set; }

        public bool IsBlockingView => ViewModel is IBlockingView;

        public object View { get; set; }

        public object ViewModel { get; set; }

        public IDisposable FocusContext { get; set; }

        public void DisposeItem(ILogger log)
        {
            if (ViewModel is BaseViewModel baseViewModel)
            {
                aliveInvokations.Remove(baseViewModel.InvokationId);
            }

            foreach (var kvp in aliveInvokations)
            {
                log.LogTrace("MFM-Alive instance: id {InvokationId} of type {InstanceType}", kvp.Key, kvp.Value);
            }

            (View as IDisposable)?.Dispose();
            (ViewModel as IDisposable)?.Dispose();
            FocusContext?.Dispose();
        }
    }

    public class FocusableItemsContext : IDisposable
    {
        public class ListItem
        {
            public DependencyObject FocusItem { get; set; }

            public int? ListViewIndex { get; set; }

            public bool IsBackButton { get; set; }

            public ListItem()
            {
            }

            public ListItem(DependencyObject item, int? listViewIndex = null)
            {
                FocusItem = item;
                ListViewIndex = listViewIndex;
            }

            public ListItem(int listViewIndex)
            {
                ListViewIndex = listViewIndex;
            }
        }

        private MenuFocusManager parent;

        public IList<ListItem> FocusableItems { get; set; }

        public int? CurrentFocusedIndex { get; set; }

        public bool IsPrimarySecondary => PrimaryButton != null || SecondaryButton != null;

        public DependencyObject PrimaryButton { get; set; }

        public DependencyObject SecondaryButton { get; set; }

        public bool HasItemsSelector { get; set; }

        public Action DisposeAction { get; set; }

        public FocusableItemsContext(MenuFocusManager parent)
        {
            this.parent = parent;
            FocusableItems = new List<ListItem>();
        }

        public void Dispose()
        {
            DisposeAction?.Invoke();
            this.parent?.ContextChanged(this);
            this.parent = null;
        }

        public void ResetCurrentFocusedIndex(int? newIndex = null)
        {
            if (newIndex.HasValue)
            {
                CurrentFocusedIndex = newIndex;
            }
            else
            {
                if (FocusableItems == null || !FocusableItems.Any())
                    CurrentFocusedIndex = null;
                else
                {
                    for (int i = 0; i < FocusableItems.Count; i++)
                    {
                        if (FocusableItems[i].IsBackButton)
                        {
                            CurrentFocusedIndex = i;
                            return;
                        }
                    }

                    // Default
                    CurrentFocusedIndex = 0;
                }
            }
        }
    }

    private readonly object lockObject = new();
    private readonly ILogger log;
    private readonly IScheduler scheduler;
    private object currentViewModel;
    private object currentPage;
    private readonly List<FocusableItemsContext> focusableItemsContexts = new();
    private readonly Stack<NavigationStackItem> navigationStack = new();
    private bool isInDialog;
    private bool focusMovementDisabled;
    private readonly Stopwatch watchNav = new();
    private HashSet<IDisposable> unusedInstances = [];

    public object CurrentViewModel => this.currentViewModel;

    public bool FocusMovementDisabled
    {
        get => this.focusMovementDisabled;
        set
        {
            this.focusMovementDisabled = value;

#if DEBUG
            this.log.LogTrace("Focus Movement Disabled: {FocusMovementDisabled}", value);
#endif

            if (value)
            {
                // Unfocus
                var obj = Microsoft.UI.Xaml.Input.FocusManager.FindFirstFocusableElement(null);
                if (obj is FrameworkElement frameworkElement)
                {
                    _ = Unfocus(frameworkElement);
                }
            }
        }
    }

    public bool IsInDialog => this.isInDialog;

    public MenuFocusManager(ILogger<MenuFocusManager> logger, IScheduler scheduler)
    {
        this.log = logger;
        this.scheduler = scheduler;

        this.focusableItemsContexts.Add(new FocusableItemsContext(this));
    }

    private void ContextChanged(FocusableItemsContext context)
    {
        this.focusableItemsContexts.Remove(context);

        SetFocus();
    }

    public FocusableItemsContext PushFocusableList(IEnumerable<DependencyObject> newFocusableItems, int? focusIndex = null)
    {
        var newCtx = new FocusableItemsContext(this)
        {
            FocusableItems = newFocusableItems.Select(x => new FocusableItemsContext.ListItem(x)).ToList()
        };
        newCtx.ResetCurrentFocusedIndex(focusIndex);

        this.focusableItemsContexts.Add(newCtx);

        SetFocus();

        return newCtx;
    }

    public FocusableItemsContext PushFocusableList(IEnumerable<FocusableItemsContext.ListItem> newFocusableItems, Action disposeAction, int? focusIndex = null)
    {
        var newCtx = new FocusableItemsContext(this)
        {
            FocusableItems = newFocusableItems.ToList(),
            DisposeAction = disposeAction
        };
        newCtx.ResetCurrentFocusedIndex(focusIndex);

        this.focusableItemsContexts.Add(newCtx);

        SetFocus();

        return newCtx;
    }

    public FocusableItemsContext PushPrimarySeconday(DependencyObject primaryButton, DependencyObject secondaryButton, bool addButtonsAsFocusableItems, bool focusOnPrimary = true)
    {
        var newCtx = new FocusableItemsContext(this)
        {
            PrimaryButton = primaryButton,
            SecondaryButton = secondaryButton
        };

        if (addButtonsAsFocusableItems)
        {
            var focusableItems = new List<FocusableItemsContext.ListItem>();
            if (primaryButton != null)
                focusableItems.Add(new FocusableItemsContext.ListItem(primaryButton));
            if (secondaryButton != null)
                focusableItems.Add(new FocusableItemsContext.ListItem(secondaryButton));

            newCtx.FocusableItems = focusableItems;
        }

        newCtx.ResetCurrentFocusedIndex(focusOnPrimary ? 0 : 1);

        this.focusableItemsContexts.Add(newCtx);

        SetFocus();

        return newCtx;
    }

    public IDisposable PushNothing()
    {
        var newCtx = new FocusableItemsContext(this)
        {
            HasItemsSelector = true
        };
        newCtx.ResetCurrentFocusedIndex();

        this.focusableItemsContexts.Add(newCtx);

        return newCtx;
    }

    public void Activate()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var coreWindow = Windows.UI.Core.CoreWindow.GetForCurrentThread();

            if (coreWindow != null)
            {
                // Doesn't work on WinSDK
                coreWindow.KeyDown += (s, e) =>
                {
                    Windows.System.VirtualKey key = e.VirtualKey;
                    if (e.VirtualKey == Windows.System.VirtualKey.None && e.KeyStatus.ScanCode == 187)
                        key = Windows.System.VirtualKey.Add;

                    switch (key)
                    {
                        case Windows.System.VirtualKey.Add:
                            // + Focus next
                            FocusNext();
                            e.Handled = true;
                            break;

                        case Windows.System.VirtualKey.Subtract:
                            // - Focus previous
                            FocusPrevious();
                            e.Handled = true;
                            break;

                        case Windows.System.VirtualKey.Insert:
                        case Windows.System.VirtualKey.Multiply:
                            // * Select
                            this.scheduler.ScheduleAsync((s, c) => SelectorPressedShort());
                            e.Handled = true;
                            break;

                        case Windows.System.VirtualKey.Divide:
                            // / Select Long
                            this.scheduler.ScheduleAsync((s, c) => SelectorPressedLong());
                            e.Handled = true;
                            break;
                    }
                };
            }
        }

        // Catch focus changes from touches
#if LOG_FOCUS_EVENTS
        Microsoft.UI.Xaml.Input.FocusManager.GotFocus += (s, e) =>
        {
            string id = null;
            if (e.NewFocusedElement is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
            {
                id = menuItem.Name;
            }

            this.log.LogTrace("Object got focus: {ObjectType} from {Sender}, id {Id}", e.NewFocusedElement?.GetType().Name, s?.GetType().Name, id);
        };

        Microsoft.UI.Xaml.Input.FocusManager.LostFocus += (s, e) =>
        {
            string id = null;
            if (e.OldFocusedElement is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
            {
                id = menuItem.Name;
            }

            this.log.LogTrace("Object lost focus: {ObjectType} from {Sender}, id {Id}", e.OldFocusedElement?.GetType().Name, s?.GetType().Name, id);
        };

        Microsoft.UI.Xaml.Input.FocusManager.LosingFocus += (s, e) =>
        {
            string id = null;
            if (e.NewFocusedElement is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
            {
                id = menuItem.Name;
            }

            this.log.LogTrace("Object losing focus: {ObjectType} from {Sender} with state {FocusState}, id {Id}", e.NewFocusedElement?.GetType().Name, s?.GetType().Name, e.FocusState, id);
        };

        Microsoft.UI.Xaml.Input.FocusManager.GettingFocus += (s, e) =>
        {
            string id = null;
            if (e.NewFocusedElement is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
            {
                id = menuItem.Name;
            }

            this.log.LogTrace("Object getting focus: {ObjectType} from {Sender} with state {FocusState}, id {Id}", e.NewFocusedElement?.GetType().Name, s?.GetType().Name, e.FocusState, id);
        };
#endif
    }

    public void SetFocusIndex(int index)
    {
        var ctx = this.focusableItemsContexts.Last();

        if (index >= 0 && index < ctx.FocusableItems.Count)
            ctx.CurrentFocusedIndex = index;

        SetFocus();
    }

    public int FocusableItemCount
    {
        get
        {
            var ctx = this.focusableItemsContexts.Last();

            return ctx.FocusableItems.Count;
        }
    }

    public void FocusNext()
    {
        if (FocusMovementDisabled)
            return;

        var ctx = this.focusableItemsContexts.Last();

        if (ctx.HasItemsSelector || this.currentPage is IRotaryHandler)
        {
            (this.currentPage as IRotaryHandler)?.TurnCW();
            return;
        }

        if (this.currentViewModel is IRotaryHandler)
        {
            (this.currentViewModel as IRotaryHandler)?.TurnCW();
            return;
        }

        if (ctx.FocusableItems != null || ctx.FocusableItems.Any())
        {
            if (!ctx.CurrentFocusedIndex.HasValue)
                // Set to first item
                ctx.CurrentFocusedIndex = 0;
            else
            {
                int? lastIndex = ctx.CurrentFocusedIndex;

                while (ctx.CurrentFocusedIndex < ctx.FocusableItems.Count - 1)
                {
                    ctx.CurrentFocusedIndex += 1;

                    // Let's go to the next item
                    if (!CanSetFocusTo(ctx))
                    {
                        // Hidden
                        if (ctx.CurrentFocusedIndex == ctx.FocusableItems.Count - 1)
                        {
                            // No more items
                            ctx.CurrentFocusedIndex = lastIndex;
                            break;
                        }

                        // Let's go to the next item
                        continue;
                    }

                    break;
                }
            }
        }

        SetFocus();
    }

    public void FocusPrevious()
    {
        if (FocusMovementDisabled)
            return;

        var ctx = this.focusableItemsContexts.Last();

        if (ctx.HasItemsSelector || this.currentPage is IRotaryHandler)
        {
            (this.currentPage as IRotaryHandler)?.TurnCCW();
            return;
        }

        if (this.currentViewModel is IRotaryHandler)
        {
            (this.currentViewModel as IRotaryHandler)?.TurnCCW();
            return;
        }

        if (ctx.FocusableItems != null || ctx.FocusableItems.Any())
        {
            if (!ctx.CurrentFocusedIndex.HasValue)
                // Set to first item
                ctx.CurrentFocusedIndex = 0;
            else
            {
                int? lastIndex = ctx.CurrentFocusedIndex;

                while (ctx.CurrentFocusedIndex > 0)
                {
                    ctx.CurrentFocusedIndex -= 1;

                    if (!CanSetFocusTo(ctx))
                    {
                        // Hidden
                        if (ctx.CurrentFocusedIndex == 0)
                        {
                            // No more items
                            ctx.CurrentFocusedIndex = lastIndex;
                            break;
                        }

                        // Let's go to the next item
                        continue;
                    }

                    break;
                }
            }
        }

        SetFocus();
    }

    private bool CanSetFocusTo(FocusableItemsContext ctx)
    {
        // Check if it's visible
        var focusItem = GetFocusItem(ctx);

        if (focusItem is UIElement uiElement)
        {
            if (!uiElement.IsTabStop)
                return false;

            if (!UiControlHelper.IsVisible(uiElement))
                return false;

            if (focusItem is Control control)
            {
                if (!control.IsEnabled)
                    return false;
            }
        }

        return true;
    }

    private void GoToFirstFocusableItem(FocusableItemsContext ctx)
    {
        for (int i = 0; i < ctx.FocusableItems.Count; i++)
        {
            ctx.CurrentFocusedIndex = i;

            if (CanSetFocusTo(ctx))
                return;
        }

        ctx.CurrentFocusedIndex = null;
    }

    private void GoToLastFocusableItem(FocusableItemsContext ctx)
    {
        for (int i = ctx.FocusableItems.Count - 1; i >= 0; i--)
        {
            ctx.CurrentFocusedIndex = i;

            if (CanSetFocusTo(ctx))
                return;
        }

        ctx.CurrentFocusedIndex = null;
    }

    private async Task SelectorPressedShort(DependencyObject focusItem = null)
    {
        var watch = Stopwatch.StartNew();

        if (FocusMovementDisabled)
            return;

        string id = null;
        if (focusItem is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
        {
            id = menuItem.Name;
        }

        var ctx = this.focusableItemsContexts.Last();

        if (ctx.PrimaryButton != null && !ctx.FocusableItems.Any())
            focusItem = ctx.PrimaryButton;

        focusItem ??= GetFocusItem(ctx);

        this.log.LogTrace("Short press on item {ItemType}. Ctx.Count = {Count}, id {Id}", focusItem?.GetType().Name, this.focusableItemsContexts.Count, id);

        this.watchNav.Restart();

        if (!ctx.HasItemsSelector && focusItem is ButtonBase button)
        {
            ClickButton(button);
        }
        else
        {
            if (this.currentPage is IItemSelector itemSelector)
            {
                await itemSelector.SelectorPressedShort(focusItem);
            }

            if (this.currentViewModel is IItemSelector itemSelectorVM)
            {
                await itemSelectorVM.SelectorPressedShort(focusItem);
            }
        }

        this.log.LogTrace("SelectorPressedShort took {ElapsedMilliseconds} ms", watch.ElapsedMilliseconds);
    }

    private async Task SelectorPressedLong(DependencyObject focusItem = null)
    {
        this.log.LogTrace("Long press");

        if (FocusMovementDisabled)
            return;

        var ctx = this.focusableItemsContexts.Last();
        bool pressButton = false;

        focusItem ??= GetFocusItem(ctx);

        if (focusItem is Controls.ButtonSecondary buttonSecondary && buttonSecondary.SecondaryCommand != null)
        {
            buttonSecondary.SecondaryCommand.Execute(null);

            return;
        }

        if (ctx.IsPrimarySecondary)
        {
            focusItem = ctx.SecondaryButton;
            if (ctx.SecondaryButton == null && ctx.PrimaryButton != null && !ctx.FocusableItems.Any())
                // Only one button
                focusItem = ctx.PrimaryButton;

            pressButton = focusItem != null;
        }

        if (!ctx.HasItemsSelector && pressButton && focusItem is ButtonBase button)
        {
            ClickButton(button);
        }
        else
        {
            if (this.currentPage is IItemSelector itemSelector)
            {
                await itemSelector.SelectorPressedLong(focusItem);
            }

            if (this.currentViewModel is IItemSelector itemSelectorVM)
            {
                await itemSelectorVM.SelectorPressedLong(focusItem);
            }
        }
    }

    public void ClickButton(ButtonBase button)
    {
        if (button.Command != null)
        {
            button.Command.Execute(button.CommandParameter);
        }
        else
        {
            // Use reflection to invoke the click handler
            var method = button.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(button, new object[0]);
        }
    }

    public void SetCloseDialog()
    {
        this.log.LogTrace("SetCloseDialog");

        SetCurrentPage(null);
    }

    public void UpdateCurrentPage()
    {
        this.log.LogTrace("UpdateCurrentPage");

        UpdateViewModelOnContentDialog(this.currentPage);
    }

    private bool UpdateViewModelOnContentDialog(object? page)
    {
        object viewModel;
        var xamlRoot = (page as UIElement)?.XamlRoot;
        ContentDialog contentDialog = null;
        if (xamlRoot != null)
        {
            // Note: On WinUI this doesn't return a standard dialog
            var openPopups = VisualTreeHelper.GetOpenPopupsForXamlRoot(xamlRoot);

            var contentDialogPopups = openPopups.Select(x => x.Child).OfType<ContentDialog>().ToList();
            // Exact match
            contentDialog = contentDialogPopups.FirstOrDefault(x => x == (page as ContentDialog));
            // Any
            contentDialog ??= contentDialogPopups.FirstOrDefault();
        }

        if (this.isInDialog && contentDialog == null)
        {
            // This is odd, we shouldn't be in a dialog without finding the content dialog
        }

        if (contentDialog != null)
        {
            viewModel = (contentDialog as FrameworkElement)?.DataContext;

            // See if we already have this page at the top of the stack.
            if (this.navigationStack.TryPeek(out var navStack) && (navStack.View as ContentDialog) == contentDialog)
            {
                navStack.ViewModel = viewModel;

                FocusMovementDisabled = navStack.IsBlockingView;
            }
            else
            {
                navStack = new NavigationStackItem
                {
                    View = contentDialog,
                    ViewModel = viewModel,
                    IsDialog = true
                };
                this.navigationStack.Push(navStack);

                lock (this.lockObject)
                {
                    if (viewModel is IDisposable disposable)
                    {
                        this.unusedInstances.Remove(disposable);
                    }
                }

                CleanupUnusedInstances();
            }

            this.currentPage = contentDialog;
            this.currentViewModel = viewModel;

            if (viewModel is BaseViewModel baseViewModel)
            {
                aliveInvokations[baseViewModel.InvokationId] = baseViewModel.GetType().Name;
            }

            if (contentDialog is IItemSelector)
            {
                if (contentDialog is IFocusableItemsProvider focusableItemsProvider)
                {
                    var focusableItems = GetFocusableItems(focusableItemsProvider);

                    int focusIndex = 0;
                    if (contentDialog is IProvidesDefaultFocusItem providesDefaultFocusItem)
                    {
                        var defaultItem = providesDefaultFocusItem.DefaultFocusItem;

                        for (int i = 0; i < focusableItems.List.Count; i++)
                        {
                            if (focusableItems.List[i].FocusItem.Equals(defaultItem))
                            {
                                focusIndex = i;
                                break;
                            }
                        }
                    }

                    var contentDisposable = PushFocusableList(focusableItems.List, focusableItems.DisposeAction, focusIndex);

                    navStack.FocusContext = contentDisposable;
                }
                else
                {
                    navStack.FocusContext = PushNothing();
                }
            }
            else
            {
                // Find buttons
                var buttons = UiControlHelper.FindAllChildren<Button>(contentDialog).Where(x => x.Content != null && x.Content.ToString() != string.Empty && x.IsTabStop).ToList();

                if (buttons.Any())
                {
                    Button primaryButton = null;
                    Button secondaryButton = null;

                    if (contentDialog.DefaultButton == ContentDialogButton.Secondary)
                    {
                        primaryButton = buttons.FirstOrDefault(x => x.Name == Common.Consts.SecondaryButtonName);
                        secondaryButton = buttons.FirstOrDefault(x => x.Name == Common.Consts.PrimaryButtonName);
                    }
                    else
                    {
                        primaryButton = buttons.FirstOrDefault(x => x.Name == Common.Consts.PrimaryButtonName);
                        secondaryButton = buttons.FirstOrDefault(x => x.Name == Common.Consts.SecondaryButtonName);
                    }

                    if (contentDialog is IRotaryHandler || (buttons.Count <= 2 && (primaryButton != null || secondaryButton != null)))
                    {
                        // Special case where we'll map short click to Primary and long click to Secondary
                        if (buttons.Count > 2 && primaryButton != null && secondaryButton != null)
                            // If we have more buttons then we don't want to map short click to primary
                            primaryButton = null;

                        var contentDisposable = PushPrimarySeconday(
                            primaryButton,
                            secondaryButton,
                            !(contentDialog is IRotaryHandler || viewModel is IRotaryHandler));
                        navStack.FocusContext = contentDisposable;
                    }
                    else
                    {
                        // Unclear why we need this
                        /*                            // Remove primary/secondary buttons
                                                    if (primaryButton != null)
                                                        buttons.Remove(primaryButton);
                                                    if (secondaryButton != null)
                                                        buttons.Remove(secondaryButton);*/

                        var contentDisposable = PushFocusableList(buttons, 0);

                        contentDisposable.SecondaryButton = secondaryButton;

                        navStack.FocusContext = contentDisposable;
                    }
                }
            }

            (contentDialog as IFocusManagerReady)?.FocusManagerReady();

            return true;
        }

        return false;
    }

    public void SetCurrentPage(object? page)
    {
        if (page is Page pageType && pageType.DataContext is BaseViewModel baseViewModelType)
        {
            this.log.LogTrace("SetCurrentPage {PageType} with id {InvocationId}", page?.GetType().Name, baseViewModelType.InvokationId);
        }
        else
        {
            this.log.LogTrace("SetCurrentPage {PageType}", page?.GetType().Name);
        }

        if (this.watchNav.IsRunning)
        {
            this.watchNav.Stop();
            this.log.LogDebug("Navigation took {ElapsedMilliseconds} ms", this.watchNav.ElapsedMilliseconds);
        }

        object viewModel;

        if (page != null && page is not ContentDialog)
        {
            if (UpdateViewModelOnContentDialog(page))
                return;
        }

        viewModel = (page as FrameworkElement)?.DataContext;
        bool wasInDialog = false;
        if (page == null)
        {
            if (this.navigationStack.Count != 0)
            {
                // Remove the dialog reference
                if (this.navigationStack.TryPeek(out var stackItem) && stackItem.IsDialog)
                {
                    // Remove from stack
                    var last = this.navigationStack.Pop();
                    if (last.IsDialog)
                        wasInDialog = true;
                    if (last.IsBlockingView)
                        FocusMovementDisabled = false;

                    last.DisposeItem(this.log);
                }

                if (this.navigationStack.TryPeek(out stackItem))
                {
                    page = stackItem.View;
                    viewModel = stackItem.ViewModel;
                    this.isInDialog = stackItem.IsDialog;
                }
            }
        }
        else
        {
            this.navigationStack.TryPeek(out var last);

            if (last == null || (last.View != page && last.ViewModel != viewModel))
            {
                var navStack = new NavigationStackItem
                {
                    View = page,
                    ViewModel = viewModel,
                    IsDialog = page is ContentDialog
                };

                this.navigationStack.Push(navStack);

                lock (this.lockObject)
                {
                    if (viewModel is IDisposable disposable)
                    {
                        this.unusedInstances.Remove(disposable);
                    }
                }

                CleanupUnusedInstances();

                FocusMovementDisabled = navStack.IsBlockingView;
            }

            if (last?.View == page && last.ViewModel == null)
                last.ViewModel = viewModel;
        }

        this.currentPage = page;
        this.currentViewModel = viewModel;

        if (viewModel is BaseViewModel baseViewModel)
        {
            aliveInvokations[baseViewModel.InvokationId] = baseViewModel.GetType().Name;
        }

        if (page is INotifyOfNavigatingTo pageNotifyOfNavigatingTo)
        {
            pageNotifyOfNavigatingTo.NavigatingTo(wasInDialog);
        }
        if (viewModel is INotifyOfNavigatingTo viewModelNotifyOfNavigatingTo)
        {
            viewModelNotifyOfNavigatingTo.NavigatingTo(wasInDialog);
        }

        if (page is ContentDialog)
            return;

        var ctx = this.focusableItemsContexts.Last();

        if (!ctx.HasItemsSelector)
        {
            // Get list of focusable items
            if (page is IFocusableItemsProvider focusableItemsProvider)
            {
                var items = GetFocusableItems(focusableItemsProvider);
                ctx.FocusableItems = items.List;

                if (ctx.DisposeAction != null)
                {
                    ctx.DisposeAction();
                }

                ctx.DisposeAction = items.DisposeAction;

                ctx.ResetCurrentFocusedIndex();
            }
            else
            {
                ctx.FocusableItems = new List<FocusableItemsContext.ListItem>();
                ctx.CurrentFocusedIndex = null;
            }
        }

        (page as IFocusManagerReady)?.FocusManagerReady();

        SetFocus();
    }

    public void TrackViewModel(object value)
    {
        if (value is IDisposable disposable)
        {
            lock (this.lockObject)
            {
                this.unusedInstances.Add(disposable);
            }
        }
    }

    private void CleanupUnusedInstances()
    {
        lock (this.lockObject)
        {
            foreach (var instance in this.unusedInstances)
            {
                this.log.LogTrace("Disposing unused instance {InstanceType} with id {InvocationId}", instance.GetType().Name, (instance as BaseViewModel)?.InvokationId);

                try
                {
                    instance.Dispose();
                }
                catch (Exception ex)
                {
                    this.log.LogError(ex, "Failed to dispose unused instance: {Message}", ex.Message);
                }
            }

            this.unusedInstances.Clear();
        }
    }

    private (IList<FocusableItemsContext.ListItem> List, Action DisposeAction) GetFocusableItems(IFocusableItemsProvider focusableItemsProvider)
    {
        FocusableItemsContext.ListItem backButton = null;
        var disposeActions = new List<Action>();

        var focusableItems = new List<FocusableItemsContext.ListItem>();
        foreach (var item in focusableItemsProvider.FocusableItems)
        {
            if (item is ListView listView)
            {
                // Add the items
                disposeActions.Add(AddFocusItemsFromListView(focusableItems, listView));
            }
            else if (item is ItemsRepeater itemsRepeater)
            {
                // Add the items
                disposeActions.Add(AddFocusItemsFromItemsRepeater(focusableItems, itemsRepeater));
            }
            else
            {
                var listItem = new FocusableItemsContext.ListItem(item);
                focusableItems.Add(listItem);

                if (item is Button button && button.Tag is string buttonTag && buttonTag == "RETURN")
                {
                    backButton = listItem;
                    listItem.IsBackButton = true;
                }
            }
        }

        if (backButton != null)
        {
            // Remove from list
            focusableItems.Remove(backButton);

            focusableItems.Insert(0, backButton);
        }

        return (focusableItems, () =>
        {
            foreach (var item in disposeActions)
            {
                item();
            }
        }
        );
    }

    public void FocusItemTapped(DependencyObject item)
    {
        this.scheduler.ScheduleAsync((s, c) => SelectorPressedShort(item));
    }

    public void FocusItemHeld(DependencyObject item)
    {
        this.scheduler.ScheduleAsync((s, c) => SelectorPressedLong(item));
    }

    public async Task<string> DisplayDialog(object sender, INavigator navigator, string title, string content, string route = null, DialogAction[]? buttons = default)
    {
        try
        {
            var result = await navigator.ShowMessageDialogAsync<string>(this, route: route, title: title, content: content, buttons: buttons);

            this.log.LogDebug("Dialog closed, result = {Result}", result);

            return result;
        }
        finally
        {
            SetFocus();
        }
    }

    public async Task<bool> DisplayConfirmYesNo(object sender, INavigator navigator, string title, string content)
    {
        try
        {
            var result = await navigator.ShowMessageDialogAsync<bool>(this, title: title + " ", content: content + " ", defaultButtonIndex: 0, cancelButtonIndex: 0, buttons: new[]
                {
                    new DialogAction("No", Id: false),
                    new DialogAction("Yes", Id: true)
                });

            return result;
        }
        finally
        {
            SetFocus();
        }
    }

    private DependencyObject GetFocusItem(FocusableItemsContext ctx)
    {
        DependencyObject focusItem = null;

        if (ctx.CurrentFocusedIndex.HasValue)
        {
            if (ctx.CurrentFocusedIndex.Value >= 0 && ctx.CurrentFocusedIndex.Value < ctx.FocusableItems.Count)
            {
                var focusListItem = ctx.FocusableItems[ctx.CurrentFocusedIndex.Value];
                if (focusListItem.FocusItem != null)
                {
                    focusItem = focusListItem.FocusItem;
                }
                else
                {
                    this.log.LogTrace("We don't have a FocusItem for index {FocusIndex}", ctx.CurrentFocusedIndex);
                }
            }
        }

        return focusItem;
    }

    private async Task Unfocus(FrameworkElement control)
    {
        if (control == null)
            return;

        var controlParent = UiControlHelper.FindParent<UIElement>(control);
        if (controlParent != null)
        {
#if DEBUG
            this.log.LogTrace("Unfocus by setting focus to control: {ControlType}", controlParent.GetType().Name);
#endif

            bool oldValue = controlParent.IsTabStop;
            controlParent.IsTabStop = true;
            await Microsoft.UI.Xaml.Input.FocusManager.TryFocusAsync(controlParent, FocusState.Pointer);
            controlParent.IsTabStop = oldValue;
        }
    }

    private void SetFocus()
    {
        if (FocusMovementDisabled)
            return;

        var canSetFocus = this.currentPage as ICanSetFocus;

        var ctx = this.focusableItemsContexts.Last();

        var focusItem = GetFocusItem(ctx);

#if DEBUG
        this.log.LogTrace("Set focus to index {FocusIndex} (Type {ControlType} - {ControlName})",
            ctx.CurrentFocusedIndex, focusItem?.GetType().Name, (focusItem as Control)?.Name);
#endif

        if (focusItem != null)
        {
            if (focusItem is FrameworkElement control && control.FocusState != FocusState.Unfocused)
            {
#if DEBUG
                // Hack so the keyboard focus becomes visible
                this.log.LogTrace("First unfocus to correct the state");
#endif

                this.scheduler.ScheduleAsync(async (s, c) =>
                {
                    await Unfocus(control);

                    canSetFocus?.SetFocus(focusItem);

                    await Microsoft.UI.Xaml.Input.FocusManager.TryFocusAsync(focusItem, FocusState.Keyboard);
                });
            }
            else
            {
                this.scheduler.ScheduleAsync(async (s, c) =>
                {
                    canSetFocus?.SetFocus(focusItem);

                    var focusResult = await Microsoft.UI.Xaml.Input.FocusManager.TryFocusAsync(focusItem, FocusState.Keyboard);

                    if (!focusResult.Succeeded)
                    {
                        // Unfocus
                        await Unfocus(focusItem as FrameworkElement);
                    }
                });
            }
        }
    }

    private Action AddFocusItemsFromListView(List<FocusableItemsContext.ListItem> list, ListView source)
    {
        source.ContainerContentChanging += Source_ContainerContentChanging;

        for (int i = 0; i < source.Items.Count; i++)
        {
            var item = source.ContainerFromIndex(i);
            if (item != null)
            {
                list.Add(new FocusableItemsContext.ListItem(item, i));
            }
            else
            {
                // Will be populated in the ContainerContentChanging event
                list.Add(new FocusableItemsContext.ListItem(i));
            }
        }

        return () => source.ContainerContentChanging -= Source_ContainerContentChanging;
    }

    private Action AddFocusItemsFromItemsRepeater(List<FocusableItemsContext.ListItem> list, ItemsRepeater source)
    {
        source.ElementPrepared += Source_ElementPrepared;

        if (source.ItemsSource is IEnumerable enumerable)
        {
            int count = enumerable.Count();
            for (int i = 0; i < count; i++)
            {
                var element = source.TryGetElement(i);
                if (element != null)
                {
                    // Special case for Grid/ContentControl
                    if (element.TabIndex == -1)
                    {
                        // Find
                        var childWithTabStop = UiControlHelper.FindChild<UIElement>(element, predicate: x => (x as UIElement)?.IsTabStop == true);

                        if (childWithTabStop != null)
                            element = childWithTabStop;
                    }

                    list.Add(new FocusableItemsContext.ListItem(element, i));
                }
                else
                {
                    // Will be populated in the ElementPrepared event
                    list.Add(new FocusableItemsContext.ListItem(i));
                }
            }
        }

        return () => source.ElementPrepared -= Source_ElementPrepared;
    }

    private void Source_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        var ctx = this.focusableItemsContexts.Last();

        foreach (var item in ctx.FocusableItems.Where(x => x.ListViewIndex == args.Index))
        {
            // Found
            var element = args.Element;

            // Special case for Grid/ContentControl
            if (element.TabIndex == -1)
            {
                // Find
                var childWithTabStop = UiControlHelper.FindChild<UIElement>(element, predicate: x => (x as UIElement)?.IsTabStop == true);

                if (childWithTabStop != null)
                    element = childWithTabStop;
            }

            item.FocusItem = element;
        }
    }

    private void Source_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        var ctx = this.focusableItemsContexts.Last();

        foreach (var item in ctx.FocusableItems)
        {
            if (item.ListViewIndex == args.ItemIndex)
            {
                // Found
                item.FocusItem = args.ItemContainer;
                break;
            }
        }
    }

    public void ClearBackStack()
    {
        while (this.navigationStack.TryPop(out var navStack))
        {
            if (navStack.View is ContentDialog contentDialog)
            {
                // Close the dialog
                contentDialog.Hide();
            }

            if (navStack.IsBlockingView)
                FocusMovementDisabled = false;

            navStack.DisposeItem(this.log);
        }

        this.isInDialog = false;
    }

    public void NavigateBack()
    {
        if (this.isInDialog)
            // But not if we're in a dialog
            return;

        if (this.navigationStack.TryPop(out var navStack))
        {
            if (navStack.IsBlockingView)
                FocusMovementDisabled = false;

            // Remove last
            navStack.DisposeItem(this.log);
        }
    }

    public void NavigateDialog(CancellationToken? cancellationToken)
    {
        this.isInDialog = true;

        if (cancellationToken != null && cancellationToken.Value != CancellationToken.None)
        {
            cancellationToken.Value.Register(() =>
            {
                SetCloseDialog();
            });
        }
    }

    public void GetKeyboardAction(Windows.System.VirtualKey key, Action<string> added, Action deleted, string? filter = null)
    {
        string? addedString = null;
        switch (key)
        {
            case Windows.System.VirtualKey.Number0:
            case Windows.System.VirtualKey.NumberPad0:
                addedString = "0";
                break;

            case Windows.System.VirtualKey.Number1:
            case Windows.System.VirtualKey.NumberPad1:
                addedString = "1";
                break;

            case Windows.System.VirtualKey.Number2:
            case Windows.System.VirtualKey.NumberPad2:
                addedString = "2";
                break;

            case Windows.System.VirtualKey.Number3:
            case Windows.System.VirtualKey.NumberPad3:
                addedString = "3";
                break;

            case Windows.System.VirtualKey.Number4:
            case Windows.System.VirtualKey.NumberPad4:
                addedString = "4";
                break;

            case Windows.System.VirtualKey.Number5:
            case Windows.System.VirtualKey.NumberPad5:
                addedString = "5";
                break;

            case Windows.System.VirtualKey.Number6:
            case Windows.System.VirtualKey.NumberPad6:
                addedString = "6";
                break;

            case Windows.System.VirtualKey.Number7:
            case Windows.System.VirtualKey.NumberPad7:
                addedString = "7";
                break;

            case Windows.System.VirtualKey.Number8:
            case Windows.System.VirtualKey.NumberPad8:
                addedString = "8";
                break;

            case Windows.System.VirtualKey.Number9:
            case Windows.System.VirtualKey.NumberPad9:
                addedString = "9";
                break;

            case Windows.System.VirtualKey.Back:
            case Windows.System.VirtualKey.Delete:
                deleted();
                break;
        }

        if (!string.IsNullOrEmpty(addedString))
        {
            // Check against the filter
            if (string.IsNullOrEmpty(filter))
            {
                // No filter
                added(addedString);
            }
            else
            {
                if (filter.Contains(addedString))
                    added(addedString);
            }
        }
    }

    public void StartNavigating()
    {
        if (!this.watchNav.IsRunning)
            this.watchNav.Restart();
    }
}
