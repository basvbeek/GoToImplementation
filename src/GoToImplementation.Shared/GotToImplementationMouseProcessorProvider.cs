using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;

namespace GoToImplementation.Shared;

/// <summary>
/// Provides a mouse processor for handling "Go To Implementation" functionality.
/// </summary>
[Export(typeof(IMouseProcessorProvider))]
[ContentType("code")]
[Name("GoToImplementation")]
[TextViewRole(PredefinedTextViewRoles.Document)]
[TextViewRole(PredefinedTextViewRoles.EmbeddedPeekTextView)]
[Order(Before = "WordSelection")]
public class GotToImplementationMouseProcessorProvider : IMouseProcessorProvider
{
    [Import]
    private readonly ITextStructureNavigatorSelectorService _navigatorService;

    [Import]
    private readonly SVsServiceProvider _globalServiceProvider;

    /// <inheritdoc />
    public IMouseProcessor GetAssociatedProcessor(IWpfTextView view)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_globalServiceProvider.GetService(typeof(SUIHostCommandDispatcher)) is not IOleCommandTarget shellCommandDispatcher)
        {
            return null;
        }

        return new GotToImplementationMouseProcessor(view, shellCommandDispatcher, _navigatorService.GetTextStructureNavigator(view.TextBuffer), ModifierKeysState.GetStateForView(view));
    }
}