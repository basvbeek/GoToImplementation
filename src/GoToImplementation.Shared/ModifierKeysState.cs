using Microsoft.VisualStudio.Text.Editor;

using System;

namespace GoToImplementation.Shared;

/// <summary>
/// Represents the state of the modifier keys for a given view.
/// </summary>
public class ModifierKeysState
{
    private ModifierKeys _modifierKeys = ModifierKeys.None;

    /// <summary>
    /// Occurs when the state of the modifier keys changes.
    /// </summary>
    public event EventHandler<EventArgs> ModifierKeysStateChanged;

    /// <summary>
    /// Gets the state of the modifier keys for the specified view.
    /// </summary>
    /// <param name="view">View to get the state for.</param>
    /// <returns>The state of the modifier keys for the specified view.</returns>
    public static ModifierKeysState GetStateForView(ITextView view)
    {
        return view.Properties.GetOrCreateSingletonProperty(typeof(ModifierKeysState), () => new ModifierKeysState());
    }

    /// <summary>
    /// Gets or sets the state of the modifier keys.
    /// </summary>
    public ModifierKeys ModifierKeys
    {
        get => _modifierKeys;
        set
        {
            var previousModifierKeys = _modifierKeys;
            _modifierKeys = value;

            // Trigger event if state of modifier keys has changed.
            if (_modifierKeys != previousModifierKeys)
            {
                ModifierKeysStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
