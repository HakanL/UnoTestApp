using System;
using System.Collections.Generic;
using System.Text;

namespace DMXCore.DMXCore100.Services;

public interface IMenuFocusManager
{
    void Activate();

    void SetCurrentPage(object page);

    void SetCloseDialog();

    void UpdateCurrentPage();

    Task<string> DisplayDialog(object sender, INavigator navigator, string title, string content, string route = null, DialogAction[]? buttons = default);

    Task<bool> DisplayConfirmYesNo(object sender, INavigator navigator, string title, string content);

    void FocusItemTapped(DependencyObject item);

    void FocusItemHeld(DependencyObject item);

    MenuFocusManager.FocusableItemsContext PushFocusableList(IEnumerable<DependencyObject> focusableItems, int? focusIndex = null);

    void ClearBackStack();

    void NavigateBack();

    void NavigateDialog(CancellationToken? cancellationToken);

    bool IsInDialog { get; }

    void ClickButton(ButtonBase button);

    void FocusNext();

    void FocusPrevious();

    void SetFocusIndex(int index);

    int FocusableItemCount { get; }

    bool FocusMovementDisabled { get; set; }

    object CurrentViewModel { get; }

    void GetKeyboardAction(Windows.System.VirtualKey key, Action<string> added, Action deleted, string? filter = null);

    void StartNavigating();

    void TrackViewModel(object value);
}
