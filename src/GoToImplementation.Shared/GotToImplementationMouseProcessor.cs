using GoToImplementation.Shared.Extensions;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;

using System;
using System.Windows;
using System.Windows.Input;

namespace GoToImplementation.Shared;

/// <summary>
/// Mouse processor that handles mouse events for the "Go To Implementation" feature.
/// </summary>
public class GotToImplementationMouseProcessor : MouseProcessorBase
{
    // Command group GUID for "Go To Implementation" command.
    public readonly Guid GoToImplementationCommandGroup = new("B61E1A20-8C13-49A9-A727-A0EC091647DD");

    // Command ID for "Go To Implementation" command.
    public const uint GoToImplementationCommand = 512;

    private readonly IWpfTextView _view;
    private readonly ITextStructureNavigator _navigator;
    private readonly IOleCommandTarget _commandTarget;
    private readonly ModifierKeysState _state;

    private Point? _mouseDownPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="GotToImplementationMouseProcessor"/> class.
    /// </summary>
    /// <param name="view">The view to process mouse events for.</param>
    /// <param name="commandTarget">Command target used for executing commands.</param>
    /// <param name="navigator">Text structure navigator used for navigating text.</param>
    /// <param name="state">Modifier keys state used for tracking modifier keys.</param>
    public GotToImplementationMouseProcessor(IWpfTextView view, IOleCommandTarget commandTarget, ITextStructureNavigator navigator, ModifierKeysState state)
    {
        _view = view;
        _commandTarget = commandTarget;
        _navigator = navigator;
        _state = state;

        // Update highlight when modifier keys state changes.
        _state.ModifierKeysStateChanged += OnModifierKeyStateChanged;

        // Clear highlight when mouse leaves the view or loses focus.
        _view.LostAggregateFocus += (sender, args) => HighlightSpan(null);
        _view.VisualElement.MouseLeave += (sender, args) => HighlightSpan(null);
    }

    /// <inheritdoc />
    public override void PostprocessMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        // Only record mouse down position if Ctrl+Alt keys are pressed.
        if (_state.ModifierKeys != ModifierKeys.CtrlAlt)
        {
            return;
        }

        _mouseDownPosition = e.GetPositionRelativeToView(_view);
    }

    /// <inheritdoc />
    public override void PreprocessMouseUp(MouseButtonEventArgs e)
    {
        // Only process mouse up event if mouse down position was recorded and Ctrl+Alt keys are pressed.
        var mouseDownPosition = GetAndClearMouseDownPosition();
        if (!mouseDownPosition.HasValue || _state.ModifierKeys != ModifierKeys.CtrlAlt)
        {
            return;
        }

        // Make sure mouse was not dragged.
        var position = e.GetPositionRelativeToView(_view);
        if (WasDragged(mouseDownPosition.Value, position))
        {
            return;
        }

        // Move caret to the click position, because the Go To Implementation command acts on the current caret position.
        if (!MoveCaretToClickPosition(mouseDownPosition.Value))
        {
            return;
        }

        // Clear any text selection.
        _view.Selection.Clear();

        // Execute Go To Implementation command.
        ExecuteCommand(GoToImplementationCommandGroup, GoToImplementationCommand);

        // Clear modifier key state.
        //_state.ModifierKey = ModifierKeyState.None;

        e.Handled = true;
    }

    /// <inheritdoc />
    public override void PreprocessMouseLeave(MouseEventArgs e)
    {
        // Clear mouse down position if mouse leaves the view.
        _mouseDownPosition = null;
    }

    /// <inheritdoc />
    public override void PreprocessMouseMove(MouseEventArgs e)
    {
        var position = e.GetPositionRelativeToView(_view);

        // Clear highlight if mouse was dragged.
        if (_mouseDownPosition.HasValue && WasDragged(_mouseDownPosition.Value, position))
        {
            _mouseDownPosition = null;
            HighlightSpan(null);
            return;
        }

        // Do not highlight if mouse button is down, Ctrl+Alt keys are not pressed or not over a significant word.
        if (e.LeftButton != MouseButtonState.Released || _state.ModifierKeys != ModifierKeys.CtrlAlt || !TryGetSignificantWordExtent(position, out var extent))
        {
            return;
        }
        HighlightSpan(extent?.Span);
    }

    private void OnModifierKeyStateChanged(object sender, EventArgs e)
    {
        var position = Mouse.GetPosition(_view.VisualElement);
        if (_state.ModifierKeys != ModifierKeys.CtrlAlt)
        {
            HighlightSpan(null);
        }
        else if (TryGetSignificantWordExtent(position, out var extent))
        {
            HighlightSpan(extent?.Span);
        }
    }

    private void HighlightSpan(SnapshotSpan? span)
    {
        var classifier = UnderlineClassifierProvider.GetClassifierForView(_view);
        if (classifier == null)
        {
            return;
        }

        Mouse.OverrideCursor = span.HasValue ? Cursors.Hand : null;
        classifier.SetUnderlineSpan(span);
    }

    private Point? GetAndClearMouseDownPosition()
    {
        var position = _mouseDownPosition;
        _mouseDownPosition = null;
        return position;
    }

    private bool WasDragged(Point originalPosition, Point currentPosition)
    {
        return Math.Abs(originalPosition.X - currentPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
               Math.Abs(originalPosition.Y - currentPosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
    }

    private bool TryGetSignificantWordExtent(Point position, out TextExtent? extent)
    {
        extent = null;

        try
        {
            var line = _view.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y);
            if (line == null)
            {
                return false;
            }

            var bufferPosition = line.GetBufferPositionFromXCoordinate(position.X);
            if (!bufferPosition.HasValue)
            {
                return false;
            }

            extent = _navigator.GetExtentOfWord(bufferPosition.Value);
            if (extent?.IsSignificant != true)
            {
                extent = null;
            }

            return extent != null;
        }
        catch
        {
            return false;
        }
    }

    private bool MoveCaretToClickPosition(Point position)
    {
        var line = _view.TextViewLines.GetTextViewLineContainingYCoordinate(position.Y);
        if (line == null)
        {
            return false;
        }

        _view.Caret.MoveTo(line, position.X);

        return true;
    }

    private void ExecuteCommand(Guid cmdGroup, uint cmdId)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            _commandTarget.Exec(ref cmdGroup, cmdId, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
        }
        catch
        {
        }
    }
}