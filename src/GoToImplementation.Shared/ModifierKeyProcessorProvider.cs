using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;

namespace GoToImplementation.Shared;

/// <summary>
/// Provides a key processor for tracking the state of the modifier keys..
/// </summary>
[Export(typeof(IKeyProcessorProvider))]
[TextViewRole(PredefinedTextViewRoles.Document)]
[TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
[ContentType("code")]
[Name("GoToImplementation")]
[Order(Before = "VisualStudioKeyboardProcessor")]
public class ModifierKeyProcessorProvider : IKeyProcessorProvider
{
    /// <inheritdoc />
    public KeyProcessor GetAssociatedProcessor(IWpfTextView view)
    {
        return view.Properties.GetOrCreateSingletonProperty(typeof(ModifierKeyProcessor), () => new ModifierKeyProcessor(ModifierKeysState.GetStateForView(view)));
    }
}
