// 
// WorkspaceManager.cs
//  
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
// 
// Copyright (c) 2010 Jonathan Pobst
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Cairo;

namespace Pinta.Core
{
	public class Document
	{
		public Document () {
			IsDirty = false;
			HasFile = false;
		}

		public bool HasFile { get; set; }

		private string pathname;

		public string Pathname {
			get { return (pathname != null) ? pathname : string.Empty; }
			set { pathname = value; }
		}

		public string Filename {
			get {
				return System.IO.Path.GetFileName (Pathname);
			}

			set {
				if (value != null) {
					Pathname = System.IO.Path.Combine (Pathname, value);
				}
			}
		}

		public bool IsDirty { get; set; }
	}


	public class WorkspaceManager
	{
		private Gdk.Size canvas_size;
		private Gtk.Viewport viewport;
		
		public Gdk.Size ImageSize { get; set; }

		public Gdk.Size CanvasSize {
			get { return canvas_size; }
			set {
				if (canvas_size.Width != value.Width || canvas_size.Height != value.Height) {
					canvas_size = value;
					OnCanvasSizeChanged ();
				}
			}
		}
		
		public PointD Offset {
			get { return new PointD ((PintaCore.Chrome.DrawingArea.Allocation.Width - canvas_size.Width) / 2, (PintaCore.Chrome.DrawingArea.Allocation.Height - CanvasSize.Height) / 2); }
		}

		public Gdk.Rectangle ViewRectangle {
			get {
				Gdk.Rectangle rect  = new Gdk.Rectangle (0, 0, 0, 0);

				if (PintaCore.Workspace.Offset.X > 0) {
					rect.X = - (int)(PintaCore.Workspace.Offset.X / PintaCore.Workspace.Scale);
					rect.Width = (int)(PintaCore.Workspace.ImageSize.Width / PintaCore.Workspace.Scale);
				}
				else {
					rect.X = (int)(viewport.Hadjustment.Value / PintaCore.Workspace.Scale);
					rect.Width = (int)((viewport.Hadjustment.PageSize) / PintaCore.Workspace.Scale);
				}
				if (PintaCore.Workspace.Offset.Y > 0) {
					rect.Y = - (int)(PintaCore.Workspace.Offset.Y / PintaCore.Workspace.Scale);
					rect.Height = (int)(PintaCore.Workspace.ImageSize.Height / PintaCore.Workspace.Scale);
				}
				else {
					rect.Y = (int)(viewport.Vadjustment.Value / PintaCore.Workspace.Scale);
					rect.Height = (int)((viewport.Vadjustment.PageSize) / PintaCore.Workspace.Scale);
				}
				return rect;
			}
		}
		
		public Document Document { get; set; }
		
		public WorkspaceManager ()
		{
			ActiveDocument = Document = new Document ();
			CanvasSize = new Gdk.Size (800, 600);
			ImageSize = new Gdk.Size (800, 600);
		}

		public void Initialize (Gtk.Viewport viewport)
		{
			this.viewport = viewport;
		}

		public double Scale {
			get { return (double)CanvasSize.Width / (double)ImageSize.Width; }
			set {
				if (Scale != value) {
					int new_x = (int)(ImageSize.Width * value);
					int new_y = (int)((new_x * ImageSize.Height) / ImageSize.Width);

					CanvasSize = new Gdk.Size (new_x, new_y);
					Invalidate ();
				}
			}
		}
		
		public void Invalidate ()
		{
			OnCanvasInvalidated (new CanvasInvalidatedEventArgs ());
		}
			
		public void Invalidate (Gdk.Rectangle rect)
		{
			rect = new Gdk.Rectangle ((int)((rect.X) * Scale + Offset.X), (int)((rect.Y) * Scale + Offset.Y), (int)(rect.Width * Scale), (int)(rect.Height * Scale));
			OnCanvasInvalidated (new CanvasInvalidatedEventArgs (rect));
		}
		
		public void ZoomIn ()
		{
			double zoom;

			if (!double.TryParse (PintaCore.Actions.View.ZoomComboBox.ComboBox.ActiveText.Trim ('%'), out zoom))
				zoom = Scale * 100;

			zoom = Math.Min (zoom, 3600);

			int i = 0;

			foreach (object item in (PintaCore.Actions.View.ZoomComboBox.ComboBox.Model as Gtk.ListStore)) {
				if (((object[])item)[0].ToString () == "Window" || int.Parse (((object[])item)[0].ToString ().Trim ('%')) <= zoom) {
					PintaCore.Actions.View.ZoomComboBox.ComboBox.Active = i - 1;
					return;
				}
				
				i++;
			}
		}
		
		public void ZoomOut ()
		{
			double zoom;
			
			if (!double.TryParse (PintaCore.Actions.View.ZoomComboBox.ComboBox.ActiveText.Trim ('%'), out zoom))
				zoom = Scale * 100;
				
			zoom = Math.Min (zoom, 3600);
			
			int i = 0;

			foreach (object item in (PintaCore.Actions.View.ZoomComboBox.ComboBox.Model as Gtk.ListStore)) {
				if (((object[])item)[0].ToString () == "Window")
					return;

				if (int.Parse (((object[])item)[0].ToString ().Trim ('%')) < zoom) {
					PintaCore.Actions.View.ZoomComboBox.ComboBox.Active = i;
					return;
				}

				i++;
			}
		}

		public void ZoomToRectangle (Rectangle rect)
		{
			double ratio;
			
			if (ImageSize.Width / rect.Width <= ImageSize.Height / rect.Height)
				ratio = ImageSize.Width / rect.Width;
			else
				ratio = ImageSize.Height / rect.Height;
			
			(PintaCore.Actions.View.ZoomComboBox.ComboBox as Gtk.ComboBoxEntry).Entry.Text = String.Format ("{0:F}%", ratio * 100.0);
			Gtk.Main.Iteration (); //Force update of scrollbar upper before recenter
			RecenterView (rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
		}
		
		public void RecenterView (double x, double y)
		{
			viewport.Hadjustment.Value = Utility.Clamp (x * Scale - viewport.Hadjustment.PageSize / 2 , viewport.Hadjustment.Lower, viewport.Hadjustment.Upper);
			viewport.Vadjustment.Value = Utility.Clamp (y * Scale - viewport.Vadjustment.PageSize / 2  , viewport.Vadjustment.Lower, viewport.Vadjustment.Upper);
		}
		
		public void ResizeImage (int width, int height)
		{
			if (ImageSize.Width == width && ImageSize.Height == height)
				return;
				
			PintaCore.Layers.FinishSelection ();
			
			ResizeHistoryItem hist = new ResizeHistoryItem (ImageSize.Width, ImageSize.Height);
			hist.TakeSnapshotOfImage ();

			ImageSize = new Gdk.Size (width, height);
			CanvasSize = new Gdk.Size (width, height);
			
			foreach (var layer in PintaCore.Layers)
				layer.Resize (width, height);
			
			PintaCore.History.PushNewItem (hist);
			
			PintaCore.Layers.ResetSelectionPath (true);
			PintaCore.Workspace.Invalidate ();
		}
		
		public void ResizeCanvas (int width, int height, Anchor anchor)
		{
			if (ImageSize.Width == width && ImageSize.Height == height)
				return;

			PintaCore.Layers.FinishSelection ();

			ResizeHistoryItem hist = new ResizeHistoryItem (ImageSize.Width, ImageSize.Height);
			hist.Icon = "Menu.Image.CanvasSize.png";
			hist.Text = "Resize Canvas";
			hist.TakeSnapshotOfImage ();

			ImageSize = new Gdk.Size (width, height);
			CanvasSize = new Gdk.Size (width, height);

			foreach (var layer in PintaCore.Layers)
				layer.ResizeCanvas (width, height, anchor);

			PintaCore.History.PushNewItem (hist);

			PintaCore.Layers.ResetSelectionPath (true);
			PintaCore.Workspace.Invalidate ();
		}
		
		public Cairo.PointD WindowPointToCanvas (double x, double y)
		{
			return new Cairo.PointD ((x - Offset.X) / PintaCore.Workspace.Scale, (y - Offset.Y) / PintaCore.Workspace.Scale);
		}

		public bool PointInCanvas (Cairo.PointD point)
		{
			if (point.X < 0 || point.Y < 0)
				return false;

			if (point.X >= PintaCore.Workspace.ImageSize.Width || point.Y >= PintaCore.Workspace.ImageSize.Height)
				return false;

			return true;
		}

		public Gdk.Rectangle ClampToImageSize (Gdk.Rectangle r)
		{
			int x = Utility.Clamp (r.X, 0, ImageSize.Width);
			int y = Utility.Clamp (r.Y, 0, ImageSize.Height);
			int width = Math.Min (r.Width, ImageSize.Width - x);
			int height = Math.Min (r.Height, ImageSize.Height - y);

			return new Gdk.Rectangle (x, y, width, height);
		}

		public Document ActiveDocument { get; set; }

		public string DocumentPath {
			get { return Document.Pathname; }
			set { Document.Pathname = value; }
		}

		public string Filename {
			get { return Document.Filename; }
			set {
				if (Document.Filename != value) {
					Document.Filename = value;
					ResetTitle ();
				}
			}
		}
		
		public bool IsDirty {
			get { return Document.IsDirty; }
			set {
				if (Document.IsDirty != value) {
					Document.IsDirty = value;
					ResetTitle ();
				}
			}
		}
		
		public bool CanvasFitsInWindow {
			get {
				int window_x = PintaCore.Chrome.DrawingArea.Allocation.Width;
				int window_y = PintaCore.Chrome.DrawingArea.Allocation.Height;

				if (CanvasSize.Width <= window_x && CanvasSize.Height <= window_y)
					return true;

				return false;
			}
		}

		public bool ImageFitsInWindow {
			get {
				int window_x = PintaCore.Chrome.DrawingArea.Allocation.Width;
				int window_y = PintaCore.Chrome.DrawingArea.Allocation.Height;

				if (ImageSize.Width <= window_x && ImageSize.Height <= window_y)
					return true;

				return false;
			}
		}
		
		public void ScrollCanvas (int dx, int dy)
		{
			viewport.Hadjustment.Value = Utility.Clamp (dx + viewport.Hadjustment.Value, viewport.Hadjustment.Lower, viewport.Hadjustment.Upper - viewport.Hadjustment.PageSize);
			viewport.Vadjustment.Value = Utility.Clamp (dy + viewport.Vadjustment.Value, viewport.Vadjustment.Lower, viewport.Vadjustment.Upper - viewport.Vadjustment.PageSize);
		}
		
		private void ResetTitle ()
		{
			PintaCore.Chrome.MainWindow.Title = string.Format ("{0}{1} - Pinta", Filename, IsDirty ? "*" : "");
		}

		#region Protected Methods
		protected void OnCanvasInvalidated (CanvasInvalidatedEventArgs e)
		{
			if (CanvasInvalidated != null)
				CanvasInvalidated (this, e);
		}

		protected void OnCanvasSizeChanged ()
		{
			if (CanvasSizeChanged != null)
				CanvasSizeChanged (this, EventArgs.Empty);
		}
		#endregion

		#region Public Events
		public event EventHandler<CanvasInvalidatedEventArgs> CanvasInvalidated;
		public event EventHandler CanvasSizeChanged;
		#endregion
	}
}
