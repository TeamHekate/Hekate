using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace Frontend.Controls;

public class HexTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);
    
    protected override void OnTextInput(TextInputEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Text) && !e.Text.All(IsHexCharacter))
            e.Handled = true;

        base.OnTextInput(e);
    }

    private static bool IsHexCharacter(char character)
    {
        return char.IsDigit(character) ||
               character is >= 'A' and <= 'F' ||
               character is >= 'a' and <= 'f';
    }
}