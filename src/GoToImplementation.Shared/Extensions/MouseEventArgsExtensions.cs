using Microsoft.VisualStudio.Text.Editor;

using System.Windows;
using System.Windows.Input;

namespace GoToImplementation.Shared.Extensions;

public static class MouseEventArgsExtensions
{
    public static Point GetPositionRelativeToView(this MouseEventArgs args, IWpfTextView view) 
        => GetPositionRelativeToView(args, view.VisualElement, view);

    public static Point GetPositionRelativeToView(this MouseEventArgs args, IInputElement inputElement, ITextView view)
    {
        var position = args.GetPosition(inputElement);
        return new Point(position.X + view.ViewportLeft, position.Y + view.ViewportTop);
    }
}
