﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Editor;

namespace ICSharpCode.AvalonEdit.AddIn
{
	/// <summary>
	/// Icon bar: contains breakpoints and other icons.
	/// </summary>
	public class IconBarMargin : AbstractMargin
	{
		readonly IconBarManager manager;
		
		public IconBarMargin(IconBarManager manager)
		{
			if (manager == null)
				throw new ArgumentNullException("manager");
			this.manager = manager;
		}
		
		#region OnTextViewChanged
		/// <inheritdoc/>
		protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
		{
			if (oldTextView != null) {
				oldTextView.VisualLinesChanged -= OnRedrawRequested;
				manager.RedrawRequested -= OnRedrawRequested;
			}
			base.OnTextViewChanged(oldTextView, newTextView);
			if (newTextView != null) {
				newTextView.VisualLinesChanged += OnRedrawRequested;
				manager.RedrawRequested += OnRedrawRequested;
			}
			InvalidateVisual();
		}
		
		void OnRedrawRequested(object sender, EventArgs e)
		{
			InvalidateVisual();
		}
		#endregion
		
		/// <inheritdoc/>
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			// accept clicks even when clicking on the background
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		
		/// <inheritdoc/>
		protected override Size MeasureOverride(Size availableSize)
		{
			return new Size(18, 0);
		}
		
		protected override void OnRender(DrawingContext drawingContext)
		{
			Size renderSize = this.RenderSize;
			drawingContext.DrawRectangle(SystemColors.ControlBrush, null,
			                             new Rect(0, 0, renderSize.Width, renderSize.Height));
			drawingContext.DrawLine(new Pen(SystemColors.ControlDarkBrush, 1),
			                        new Point(renderSize.Width - 0.5, 0),
			                        new Point(renderSize.Width - 0.5, renderSize.Height));
			
			TextView textView = this.TextView;
			if (textView != null && textView.VisualLinesValid) {
				// create a dictionary line number => first bookmark
				Dictionary<int, IBookmark> bookmarkDict = new Dictionary<int, IBookmark>();
				foreach (IBookmark bm in manager.Bookmarks) {
					int line = bm.LineNumber;
					if (!bookmarkDict.ContainsKey(line))
						bookmarkDict.Add(line, bm);
				}
				Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
				foreach (VisualLine line in textView.VisualLines) {
					int lineNumber = line.FirstDocumentLine.LineNumber;
					IBookmark bm;
					if (bookmarkDict.TryGetValue(lineNumber, out bm)) {
						Rect rect = new Rect(0, PixelSnapHelpers.Round(line.VisualTop - textView.VerticalOffset, pixelSize.Height), 16, 16);
						if (dragDropBookmark == bm && dragStarted)
							drawingContext.PushOpacity(0.5);
						drawingContext.DrawImage((bm.Image ?? BookmarkBase.DefaultBookmarkImage).ImageSource, rect);
						if (dragDropBookmark == bm && dragStarted)
							drawingContext.Pop();
					}
				}
				if (dragDropBookmark != null && dragStarted) {
					Rect rect = new Rect(0, PixelSnapHelpers.Round(dragDropCurrentPoint - 8, pixelSize.Height), 16, 16);
					drawingContext.DrawImage((dragDropBookmark.Image ?? BookmarkBase.DefaultBookmarkImage).ImageSource, rect);
				}
			}
		}
		
		IBookmark dragDropBookmark; // bookmark being dragged (!=null if drag'n'drop is active)
		double dragDropStartPoint;
		double dragDropCurrentPoint;
		bool dragStarted; // whether drag'n'drop operation has started (mouse was moved minimum distance)
		
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			CancelDragDrop();
			base.OnMouseDown(e);
			int line = GetLineFromMousePosition(e);
			if (!e.Handled && line > 0) {
				foreach (IBookmark bm in manager.Bookmarks) {
					if (bm.LineNumber == line) {
						bm.MouseDown(e);
						if (e.Handled)
							return;
						if (e.ChangedButton == MouseButton.Left && bm.CanDragDrop && CaptureMouse()) {
							StartDragDrop(bm, e);
							e.Handled = true;
							return;
						}
					}
				}
			}
			// don't allow selecting text through the IconBarMargin
			if (e.ChangedButton == MouseButton.Left)
				e.Handled = true;
		}
		
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			CancelDragDrop();
			base.OnLostMouseCapture(e);
		}
		
		void StartDragDrop(IBookmark bm, MouseEventArgs e)
		{
			dragDropBookmark = bm;
			dragDropStartPoint = dragDropCurrentPoint = e.GetPosition(this).Y;
			if (TextView != null)
				TextView.PreviewKeyDown += TextView_PreviewKeyDown;
		}
		
		void CancelDragDrop()
		{
			if (dragDropBookmark != null) {
				dragDropBookmark = null;
				dragStarted = false;
				if (TextView != null)
					TextView.PreviewKeyDown -= TextView_PreviewKeyDown;
				ReleaseMouseCapture();
				InvalidateVisual();
			}
		}
		
		void TextView_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			// any key press cancels drag'n'drop
			CancelDragDrop();
			if (e.Key == Key.Escape)
				e.Handled = true;
		}
		
		int GetLineFromMousePosition(MouseEventArgs e)
		{
			TextView textView = this.TextView;
			if (textView == null)
				return 0;
			VisualLine vl = textView.GetVisualLineFromVisualTop(e.GetPosition(textView).Y + textView.ScrollOffset.Y);
			if (vl == null)
				return 0;
			return vl.FirstDocumentLine.LineNumber;
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (dragDropBookmark != null) {
				dragDropCurrentPoint = e.GetPosition(this).Y;
				if (Math.Abs(dragDropCurrentPoint - dragDropStartPoint) > SystemParameters.MinimumVerticalDragDistance)
					dragStarted = true;
				InvalidateVisual();
			}
		}
		
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);
			int line = GetLineFromMousePosition(e);
			if (!e.Handled && dragDropBookmark != null) {
				if (dragStarted) {
					if (line != 0)
						dragDropBookmark.Drop(line);
					e.Handled = true;
				}
				CancelDragDrop();
			}
			if (!e.Handled && line != 0) {
				foreach (IBookmark bm in manager.Bookmarks) {
					if (bm.LineNumber == line) {
						bm.MouseUp(e);
						if (e.Handled)
							return;
					}
				}
				if (e.ChangedButton == MouseButton.Left && TextView != null) {
					// no bookmark on the line: create a new breakpoint
					ITextEditor textEditor = TextView.Services.GetService(typeof(ITextEditor)) as ITextEditor;
					if (textEditor != null) {
						ICSharpCode.SharpDevelop.Debugging.DebuggerService.ToggleBreakpointAt(textEditor, line);
					}
				}
			}
		}
	}
}