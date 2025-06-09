using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMXCore.DMXCore100.Controls;

public class ButtonSecondary : Button
{
    protected override void OnRightTapped(RightTappedRoutedEventArgs e)
    {
        base.OnRightTapped(e);

        SecondaryCommand?.Execute(null);
    }

    public ICommand SecondaryCommand { get; set; }
}
