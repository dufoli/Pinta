// 
// MainWindow.cs
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
using Gdk;
using Gtk;
using Pinta.Core;

namespace Pinta
{
	public partial class MainWindow : Gtk.Window
	{
		DialogHandlers dialog_handler;

		ProgressDialog progress_dialog;

		public MainWindow () : base(Gtk.WindowType.Toplevel)
		{
			Build ();
			
			// Initialize interface things
			PintaCore.Actions.AccelGroup = new AccelGroup ();
			this.AddAccelGroup (PintaCore.Actions.AccelGroup);
			
			progress_dialog = new ProgressDialog ();
			
			PintaCore.Initialize (tooltoolbar, label5, drawingarea1, history_treeview, this, progress_dialog, GtkScrolledWindow);
			colorpalettewidget1.Initialize ();
			
			PintaCore.Chrome.StatusBarTextChanged += new EventHandler<TextChangedEventArgs> (Chrome_StatusBarTextChanged);
			CreateToolBox ();
			
			PintaCore.Actions.CreateMainMenu (menubar1);
			PintaCore.Actions.CreateToolBar (toolbar1);
			PintaCore.Actions.Layers.CreateLayerWindowToolBar (toolbar4);
			PintaCore.Actions.Edit.CreateHistoryWindowToolBar (toolbar2);
			
			Gtk.Image i = new Gtk.Image (PintaCore.Resources.GetIcon ("StatusBar.CursorXY.png"));
			i.Show ();
			
			statusbar1.Add (i);
			Gtk.Box.BoxChild box = (Gtk.Box.BoxChild)statusbar1[i];
			box.Position = 2;
			box.Fill = false;
			box.Expand = false;
			
			this.Icon = PintaCore.Resources.GetIcon ("Pinta.png");
			
			dialog_handler = new DialogHandlers (this);
			
			// Create a blank document
			Layer background = PintaCore.Layers.AddNewLayer ("Background");
			
			using (Cairo.Context g = new Cairo.Context (background.Surface)) {
				g.SetSourceRGB (255, 255, 255);
				g.Paint ();
			}
			
			PintaCore.Workspace.Filename = "Untitled1";
			PintaCore.History.PushNewItem (new BaseHistoryItem ("gtk-new", "New Image"));
			PintaCore.Workspace.IsDirty = false;
			PintaCore.Workspace.Invalidate ();
			
			//History
			history_treeview.Model = PintaCore.History.ListStore;
			history_treeview.HeadersVisible = false;
			history_treeview.Selection.Mode = SelectionMode.Single;
			history_treeview.Selection.SelectFunction = HistoryItemSelected;
			
			Gtk.TreeViewColumn icon_column = new Gtk.TreeViewColumn ();
			Gtk.CellRendererPixbuf icon_cell = new Gtk.CellRendererPixbuf ();
			icon_column.PackStart (icon_cell, true);
			
			Gtk.TreeViewColumn text_column = new Gtk.TreeViewColumn ();
			Gtk.CellRendererText text_cell = new Gtk.CellRendererText ();
			text_column.PackStart (text_cell, true);
			
			text_column.SetCellDataFunc (text_cell, new Gtk.TreeCellDataFunc (HistoryRenderText));
			icon_column.SetCellDataFunc (icon_cell, new Gtk.TreeCellDataFunc (HistoryRenderIcon));
			
			history_treeview.AppendColumn (icon_column);
			history_treeview.AppendColumn (text_column);
			
			PintaCore.History.HistoryItemAdded += new EventHandler<HistoryItemAddedEventArgs> (OnHistoryItemsChanged);
			PintaCore.History.ActionUndone += new EventHandler (OnHistoryItemsChanged);
			PintaCore.History.ActionRedone += new EventHandler (OnHistoryItemsChanged);
			
			PintaCore.Actions.View.ZoomToWindow.Activated += new EventHandler (ZoomToWindow_Activated);
			DeleteEvent += new DeleteEventHandler (MainWindow_DeleteEvent);
			
			PintaCore.LivePreview.RenderUpdated += LivePreview_RenderUpdated;
			
			WindowAction.Visible = false;
			
			hruler = new HRuler ();
			hruler.Metric = MetricType.Pixels;
			table1.Attach (hruler, 1, 2, 0, 1, AttachOptions.Shrink | AttachOptions.Fill, AttachOptions.Shrink | AttachOptions.Fill, 0, 0);
			
			vruler = new VRuler ();
			vruler.Metric = MetricType.Pixels;
			table1.Attach (vruler, 0, 1, 1, 2, AttachOptions.Shrink | AttachOptions.Fill, AttachOptions.Shrink | AttachOptions.Fill, 0, 0);

			GtkScrolledWindow.Hadjustment.ValueChanged += delegate {
				UpdateRulerRange ();
			};
			GtkScrolledWindow.Vadjustment.ValueChanged += delegate {
				UpdateRulerRange ();
			};

			UpdateRulerRange ();
			
			PintaCore.Actions.View.ZoomComboBox.ComboBox.Changed += HandlePintaCoreActionsViewZoomComboBoxComboBoxChanged;
			
			if (Platform.GetOS () == Platform.OS.Mac) {
				try {
					//enable the global key handler for keyboard shortcuts
					IgeMacMenu.GlobalKeyHandlerEnabled = true;
					
					//Tell the IGE library to use your GTK menu as the Mac main menu
					IgeMacMenu.MenuBar = menubar1;
					/*
					//tell IGE which menu item should be used for the app menu's quit item
					IgeMacMenu.QuitMenuItem = yourQuitMenuItem;
					*/					
					//add a new group to the app menu, and add some items to it
					var appGroup = IgeMacMenu.AddAppMenuGroup ();
					MenuItem aboutItem = (MenuItem)PintaCore.Actions.Help.About.CreateMenuItem ();
					appGroup.AddMenuItem (aboutItem, Mono.Unix.Catalog.GetString ("About"));
					
					menubar1.Hide ();
				} catch {
					// If things don't work out, just use a normal menu.
				}
			}
		}

		private void MainWindow_DeleteEvent (object o, DeleteEventArgs args)
		{
			// leave window open so user can cancel quitting
			args.RetVal = true;
			
			PintaCore.Actions.File.Exit.Activate ();
		}

		private void ZoomToWindow_Activated (object sender, EventArgs e)
		{
			// The image is small enough to fit in the window
			if (PintaCore.Workspace.ImageFitsInWindow) {
				PintaCore.Actions.View.ActualSize.Activate ();
				return;
			}
			
			int image_x = PintaCore.Workspace.ImageSize.Width;
			int image_y = PintaCore.Workspace.ImageSize.Height;
			
			int window_x = GtkScrolledWindow.Children[0].Allocation.Width;
			int window_y = GtkScrolledWindow.Children[0].Allocation.Height;
			
			// The image is more constrained by width than height
			if ((double)image_x / (double)window_x >= (double)image_y / (double)window_y) {
				double ratio = (double)(window_x - 20) / (double)image_x;
				PintaCore.Workspace.Scale = ratio;
				PintaCore.Actions.View.SuspendZoomUpdate ();
				(PintaCore.Actions.View.ZoomComboBox.ComboBox as ComboBoxEntry).Entry.Text = string.Format ("{0}%", (int)(PintaCore.Workspace.Scale * 100));
				PintaCore.Actions.View.ResumeZoomUpdate ();
			} else {
				double ratio2 = (double)(window_y - 20) / (double)image_y;
				PintaCore.Workspace.Scale = ratio2;
				PintaCore.Actions.View.SuspendZoomUpdate ();
				(PintaCore.Actions.View.ZoomComboBox.ComboBox as ComboBoxEntry).Entry.Text = string.Format ("{0}%", (int)(PintaCore.Workspace.Scale * 100));
				PintaCore.Actions.View.ResumeZoomUpdate ();
			}
		}

		private void Chrome_StatusBarTextChanged (object sender, TextChangedEventArgs e)
		{
			label5.Text = e.Text;
		}

		#region History
		public bool HistoryItemSelected (TreeSelection selection, TreeModel model, TreePath path, bool path_currently_selected)
		{
			int current = path.Indices[0];
			if (!path_currently_selected) {
				while (PintaCore.History.Pointer < current) {
					PintaCore.History.Redo ();
				}
				while (PintaCore.History.Pointer > current) {
					PintaCore.History.Undo ();
				}
			}
			return true;
		}

		private void HistoryRenderText (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BaseHistoryItem item = (BaseHistoryItem)model.GetValue (iter, 0);
			if (item.State == HistoryItemState.Undo) {
				(cell as Gtk.CellRendererText).Style = Pango.Style.Normal;
				(cell as Gtk.CellRendererText).Foreground = "black";
				(cell as Gtk.CellRendererText).Text = item.Text;
			} else if (item.State == HistoryItemState.Redo) {
				(cell as Gtk.CellRendererText).Style = Pango.Style.Oblique;
				(cell as Gtk.CellRendererText).Foreground = "gray";
				(cell as Gtk.CellRendererText).Text = item.Text;
			}
			
		}

		private void HistoryRenderIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			BaseHistoryItem item = (BaseHistoryItem)model.GetValue (iter, 0);
			(cell as Gtk.CellRendererPixbuf).Pixbuf = PintaCore.Resources.GetIcon (item.Icon);
		}

		private void OnHistoryItemsChanged (object o, EventArgs args)
		{
			if (PintaCore.History.Current != null) {
				history_treeview.Selection.SelectIter (PintaCore.History.Current.Id);
				history_treeview.ScrollToCell (history_treeview.Model.GetPath (PintaCore.History.Current.Id), history_treeview.Columns[1], true, (float)0.9, 0);
			}
			
		}
		#endregion

		private void CreateToolBox ()
		{
			// Create our tools
			PintaCore.Tools.AddTool (new RectangleSelectTool ());
			PintaCore.Tools.AddTool (new MoveSelectedTool ());
			PintaCore.Tools.AddTool (new LassoSelectTool ());
			PintaCore.Tools.AddTool (new MoveSelectionTool ());
			PintaCore.Tools.AddTool (new EllipseSelectTool ());
			PintaCore.Tools.AddTool (new ZoomTool ());
			PintaCore.Tools.AddTool (new MagicWandTool ());
			PintaCore.Tools.AddTool (new PanTool ());
			PintaCore.Tools.AddTool (new PaintBucketTool ());
			PintaCore.Tools.AddTool (new GradientTool ());
			
			BaseTool pb = new PaintBrushTool ();
			PintaCore.Tools.AddTool (pb);
			PintaCore.Tools.AddTool (new EraserTool ());
			PintaCore.Tools.SetCurrentTool (pb);
			
			PintaCore.Tools.AddTool (new PencilTool ());
			PintaCore.Tools.AddTool (new ColorPickerTool ());
			PintaCore.Tools.AddTool (new CloneStampTool ());
			PintaCore.Tools.AddTool (new RecolorTool ());
			PintaCore.Tools.AddTool (new TextTool ());
			PintaCore.Tools.AddTool (new LineCurveTool ());
			PintaCore.Tools.AddTool (new RectangleTool ());
			PintaCore.Tools.AddTool (new RoundedRectangleTool ());
			PintaCore.Tools.AddTool (new EllipseTool ());
			PintaCore.Tools.AddTool (new FreeformShapeTool ());
			
			bool even = true;
			
			foreach (var tool in PintaCore.Tools) {
				if (even)
					toolbox1.Insert (tool.ToolItem, toolbox1.NItems);
				else
					toolbox2.Insert (tool.ToolItem, toolbox2.NItems);
				
				even = !even;
			}
		}

		void LivePreview_RenderUpdated (object o, LivePreviewRenderUpdatedEventArgs args)
		{
			double scale = PintaCore.Workspace.Scale;
			var offset = PintaCore.Workspace.Offset;
			
			var bounds = args.Bounds;
			
			// Transform bounds (Image -> Canvas -> Window)
			
			// Calculate canvas bounds.
			double x1 = bounds.Left * scale;
			double y1 = bounds.Top * scale;
			double x2 = bounds.Right * scale;
			double y2 = bounds.Bottom * scale;
			
			// TODO Figure out why when scale > 1 that I need add on an
			// extra pixel of padding.
			// I must being doing something wrong here.
			if (scale > 1.0) {
				//x1 = (bounds.Left-1) * scale;
				y1 = (bounds.Top - 1) * scale;
				//x2 = (bounds.Right+1) * scale;
				//y2 = (bounds.Bottom+1) * scale;
			}
			
			// Calculate window bounds.
			x1 += offset.X;
			y1 += offset.Y;
			x2 += offset.X;
			y2 += offset.Y;
			
			// Convert to integer, carefull not to miss paritally covered
			// pixels by rounding incorrectly.
			int x = (int)Math.Floor (x1);
			int y = (int)Math.Floor (y1);
			int width = (int)Math.Ceiling (x2) - x;
			int height = (int)Math.Ceiling (y2) - y;
			
			// Tell GTK to expose the drawing area.			
			drawingarea1.QueueDrawArea (x, y, width, height);
		}

		#region Drawing Area
		private void OnDrawingarea1MotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			Cairo.PointD point = PintaCore.Workspace.WindowPointToCanvas (args.Event.X, args.Event.Y);
			
			if (PintaCore.Workspace.PointInCanvas (point))
				CursorPositionLabel.Text = string.Format ("{0}, {1}", (int)point.X, (int)point.Y);
			
			hruler.Position = point.X;
			vruler.Position = point.Y;
		}
		#endregion

		#region rulers
		private HRuler hruler;
		private VRuler vruler;

		public void ShowRulers ()
		{
			hruler.Show ();
			vruler.Show ();
		}

		public void HideRulers ()
		{
			hruler.Hide ();
			vruler.Hide ();
		}

		public void ChangeRulersUnit (Gtk.MetricType metric)
		{
			hruler.Metric = metric;
			vruler.Metric = metric;
			switch (metric) {
			case Gtk.MetricType.Pixels:
				if (PintaCore.Actions.View.UnitComboBox.ComboBox.Active != 0)
					PintaCore.Actions.View.UnitComboBox.ComboBox.Active = 0;
				break;
			case Gtk.MetricType.Inches:
				if (PintaCore.Actions.View.UnitComboBox.ComboBox.Active != 1)
					PintaCore.Actions.View.UnitComboBox.ComboBox.Active = 1;
				break;
			case Gtk.MetricType.Centimeters:
				if (PintaCore.Actions.View.UnitComboBox.ComboBox.Active != 2)
					PintaCore.Actions.View.UnitComboBox.ComboBox.Active = 2;
				break;
				
			}
		}

		void HandlePintaCoreActionsViewZoomComboBoxComboBoxChanged (object sender, EventArgs e)
		{
			UpdateRulerRange ();
		}

		void UpdateRulerRange ()
		{
			Gtk.Main.Iteration (); //Force update of scrollbar upper before recenter

			Cairo.PointD lower = new Cairo.PointD (0, 0);
			Cairo.PointD upper = new Cairo.PointD (0, 0);

			if (PintaCore.Workspace.Offset.X > 0) {
				lower.X = - PintaCore.Workspace.Offset.X / PintaCore.Workspace.Scale;
				upper.X = PintaCore.Workspace.ImageSize.Width - lower.X;
			}
			else {
				lower.X = GtkScrolledWindow.Hadjustment.Value / PintaCore.Workspace.Scale;
				upper.X = (GtkScrolledWindow.Hadjustment.Value + GtkScrolledWindow.Hadjustment.PageSize) / PintaCore.Workspace.Scale;
			}
			if (PintaCore.Workspace.Offset.Y > 0) {
				lower.Y = - PintaCore.Workspace.Offset.Y / PintaCore.Workspace.Scale;
				upper.Y = PintaCore.Workspace.ImageSize.Height - lower.Y;
			}
			else {
				lower.Y = GtkScrolledWindow.Vadjustment.Value / PintaCore.Workspace.Scale;
				upper.Y = (GtkScrolledWindow.Vadjustment.Value + GtkScrolledWindow.Vadjustment.PageSize) / PintaCore.Workspace.Scale;
			}

			hruler.SetRange (lower.X, upper.X, 0, upper.X);
			vruler.SetRange (lower.Y, upper.Y, 0, upper.Y);
		}
		#endregion
		protected virtual void HandleScroll (object o, Gtk.ScrollChildArgs args)
		{
			UpdateRulerRange ();
		}
		
		
	}
}
