using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;

namespace GoToImplementation.Shared;

/// <summary>
/// Provides a tagger for underlining text in the editor.
/// </summary>
[Export(typeof(IViewTaggerProvider))]
[ContentType("text")]
[TagType(typeof(ClassificationTag))]
public class UnderlineClassifierProvider : IViewTaggerProvider
{
    private static IClassificationType _underlineClassification;

    [Import]
    private readonly IClassificationTypeRegistryService _cassificationRegistry;

    /// <summary>
    /// Exports the <see cref="ClassificationTypeDefinition"/> for the underline classification type.
    /// </summary>
    [Export(typeof(ClassificationTypeDefinition))]
    [Name("UnderlineClassification")]
    public static ClassificationTypeDefinition UnderlineClassificationType;

    /// <summary>
    /// Gets the <see cref="UnderlineClassifier"/> for the specified <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The <see cref="ITextView"/> to get the <see cref="UnderlineClassifier"/> for.</param>
    /// <returns>The <see cref="UnderlineClassifier"/> for the specified <paramref name="view"/>.</returns>
    /// <remarks>
    /// A new <see cref="UnderlineClassifier"/> is created and added to the view's properties if one does not already exist.
    /// </remarks>
    public static UnderlineClassifier GetClassifierForView(ITextView view)
    {
        return _underlineClassification == null ? null : view.Properties.GetOrCreateSingletonProperty(() => new UnderlineClassifier(_underlineClassification));
    }

    /// <inheritdoc />
    public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
    {
        _underlineClassification ??= _cassificationRegistry.GetClassificationType(UnderlineFormatDefinition.ClassificationTypeName);

        // Only provide highlighting on the top-level buffer.
        return textView.TextBuffer != buffer ? null : GetClassifierForView(textView) as ITagger<T>;
    }
}