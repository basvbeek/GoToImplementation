using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

using System.ComponentModel.Composition;
using System.Windows.Media;

namespace GoToImplementation.Shared;

/// <summary>
/// Defines the format for the underline classification.
/// </summary>
[Export(typeof(EditorFormatDefinition))]
[ClassificationType(ClassificationTypeNames = ClassificationTypeName)]
[Name("UnderlineClassificationFormat")]
[UserVisible(true)]
[Order(After = Priority.High)]
public class UnderlineFormatDefinition : ClassificationFormatDefinition
{
    /// <summary>
    /// The name of the classification type for underlining text.
    /// </summary>
    public const string ClassificationTypeName = "UnderlineClassification";

    /// <summary>
    /// Initializes a new instance of the <see cref="UnderlineFormatDefinition"/> class.
    /// </summary>
    public UnderlineFormatDefinition()
    {
        DisplayName = "Underline";
        TextDecorations = System.Windows.TextDecorations.Underline;
        ForegroundColor = Color.FromRgb(0x56, 0x9c, 0xd6);
    }
}