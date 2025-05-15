using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;

namespace Frontend.Controls;

public class CodeEditorBox : TextBox
{
    
    protected override Type StyleKeyOverride => typeof(TextBox);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            Text ??= "";
            var prev = Text[..CaretIndex];
            var next = Text[CaretIndex..];
            var currentColumn = prev.Split("\n").Last().Length;
            var sixFold = (currentColumn / 6 + 1) * 6;
            var amt = sixFold - currentColumn;
            Text = prev + new string(' ', amt) + next;
            Text = Text.Replace("\t", "");
            e.Handled = true;
            CaretIndex = CaretIndex + amt;
        }
        else
        {
            base.OnKeyDown(e);
        }
    }
}