using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using System;
using System.Collections.Generic;

namespace GoToImplementation.Shared;

/// <summary>
/// A tagger that underlines text in the editor.
/// </summary>
public class UnderlineClassifier : ITagger<ClassificationTag>
{
    private readonly IClassificationType _classificationType;

    private SnapshotSpan? _underlineSpan = null;

    /// <summary>
    /// Occurs when the tags for a span have changed.
    /// </summary>
    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    /// <summary>
    /// Gets the current underline span.
    /// </summary>
    public SnapshotSpan? CurrentUnderlineSpan => _underlineSpan;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnderlineClassifier"/> class.
    /// </summary>
    /// <param name="classificationType">The classification type to use for the underline.</param>
    public UnderlineClassifier(IClassificationType classificationType)
    {
        _classificationType = classificationType;
    }

    /// <summary>
    /// Sets the span to be underlined.
    /// </summary>
    /// <param name="span">Span to be underlined.</param>
    public void SetUnderlineSpan(SnapshotSpan? span)
    {
        var oldSpan = _underlineSpan;
        _underlineSpan = span;

        // If there's no old span to remove the underline from, nor a new span to underline, do nothing.
        if (!oldSpan.HasValue && !_underlineSpan.HasValue)
        {
            return;
        }

        // Nothing to do if the old span is the same as the new span.
        if (oldSpan.HasValue && _underlineSpan.HasValue && oldSpan == _underlineSpan)
        {
            return;
        }

        // If there's no new span to underline, only raise an event for the old span.
        if (!_underlineSpan.HasValue)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(oldSpan.Value));
            return;
        }

        SnapshotSpan updateSpan = _underlineSpan.Value;

        // If there's an old span, expand the span to include the old span.
        if (oldSpan.HasValue)
        {
            updateSpan = new SnapshotSpan(updateSpan.Snapshot,
                Span.FromBounds(Math.Min(updateSpan.Start, oldSpan.Value.Start),
                                Math.Max(updateSpan.End, oldSpan.Value.End)));
        }

        TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(updateSpan));
    }

    /// <inheritdoc />
    public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        if (!_underlineSpan.HasValue || spans.Count == 0)
        {
            yield break;
        }

        // Return underline tag if the underline span intersects with the requested spans.
        SnapshotSpan request = new(spans[0].Start, spans[spans.Count - 1].End);
        SnapshotSpan underline = _underlineSpan.Value.TranslateTo(request.Snapshot, SpanTrackingMode.EdgeInclusive);
        if (underline.IntersectsWith(request))
        {
            yield return new TagSpan<ClassificationTag>(underline, new ClassificationTag(_classificationType));
        }
    }
}