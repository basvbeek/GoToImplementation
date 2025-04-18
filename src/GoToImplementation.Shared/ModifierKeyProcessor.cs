using Microsoft.VisualStudio.Text.Editor;

using System.Windows.Input;

namespace GoToImplementation.Shared;

/// <summary>
/// Key processor that tracks the state of the modifier keys.
/// </summary>
public class ModifierKeyProcessor : KeyProcessor
{
    private readonly ModifierKeysState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierKeyProcessor"/> class.
    /// </summary>
    /// <param name="state">The modifier keys state to update.</param>
    public ModifierKeyProcessor(ModifierKeysState state)
    {
        _state = state;
    }

    /// <inheritdoc />
    public override void PreviewKeyDown(KeyEventArgs args)
    {
        UpdateState(args);
    }

    /// <inheritdoc />
    public override void PreviewKeyUp(KeyEventArgs args)
    {
        UpdateState(args);
    }

    private void UpdateState(KeyEventArgs args)
    {
        ModifierKeys state = ModifierKeys.None;

        bool ctrlDown = (args.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0;
        bool shiftDown = (args.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0;
        bool altDown = (args.KeyboardDevice.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0;

        if (ctrlDown && shiftDown && altDown)
        {
            state = ModifierKeys.CtrlAltShift;
        }
        else if (ctrlDown && shiftDown)
        {
            state = ModifierKeys.CtrlShift;
        }
        else if (ctrlDown && altDown)
        {
            state = ModifierKeys.CtrlAlt;
        }
        else if (shiftDown && altDown)
        {
            state = ModifierKeys.ShiftAlt;
        }
        else
        {
            if (ctrlDown)
            {
                state = ModifierKeys.Ctrl;
            }

            if (shiftDown)
            {
                state = ModifierKeys.Shift;
            }

            if (altDown)
            {
                state = ModifierKeys.Alt;
            }
        }

        _state.ModifierKeys = state;
    }
}
