//
// CloneStampTool.cs
//
// Author:
//       Olivier Dufour <olivier (dot) duff (at) gmail (dot) com>
//
// Copyright (c) 2010
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
using System.Collections.Generic;

namespace Pinta.Core
{
	public class CloneStampTool : BaseBrushTool
	{
		private class StaticData
		{
			public Point takeFrom;
			public Cairo.PointD lastMoved;
			public bool updateSrcPreview;
			public WeakReference wr;
		}

		public override string Name {
			get { return "Clone Stamp"; }
		}
		public override string Icon {
			get { return "Tools.CloneStamp.png"; }
		}

		private StaticData staticData;

		private StaticData Data
		{
			get {
				if (staticData == null) {
					staticData = new StaticData ();
				}

				return staticData;
			}
		}

		private Layer takeFromLayer;

		private bool switchedTo = false;
		private Rectangle undoRegion = Rectangle.Zero;
		private Region savedRegion;
		//private RenderArgs ra;
		private bool mouseUp = true;
		private List<Rectangle> historyRects;
		private bool antialiasing;
		private Region clipRegion;
		bool active;

		//private BrushPreviewRenderer rendererDst;
		//private BrushPreviewRenderer rendererSrc;

		// private bool added by MK for "clone source" cursor transition
		private bool mouseDownSettingCloneSource;

		//private Cursor cursorMouseDown, cursorMouseUp, cursorMouseDownSetSource;

		/*protected override void OnMouseEnter ()
		{
			//this.rendererDst.Visible = true;
			base.OnMouseEnter ();
		}

		protected override void OnMouseLeave ()
		{
			//this.rendererDst.Visible = false;
			base.OnMouseLeave ();
		}*/

		/*protected override void OnPulse ()
		{
			double time = (double)new SystemLayer.Timing ().GetTickCount ();
			double sin = Math.Sin (time / 300.0);
			int alpha = (int)Math.Ceiling ((((sin + 1.0) / 2.0) * 224.0) + 31.0);
			this.rendererSrc.BrushAlpha = alpha;
			base.OnPulse ();
		}*/

		protected override void OnActivated ()
		{
			base.OnActivated ();

			//cursorMouseDown = new Cursor (PdnResources.GetResourceStream ("Cursors.GenericToolCursorMouseDown.cur"));
			//cursorMouseDownSetSource = new Cursor (PdnResources.GetResourceStream ("Cursors.CloneStampToolCursorSetSource.cur"));
			//cursorMouseUp = new Cursor (PdnResources.GetResourceStream ("Cursors.CloneStampToolCursor.cur"));
			//this.Cursor = cursorMouseUp;

			//this.rendererDst = new BrushPreviewRenderer (this.RendererList);
			//this.RendererList.Add (this.rendererDst, false);

			/*this.rendererSrc = new BrushPreviewRenderer (this.RendererList);
			this.rendererSrc.BrushLocation = Data.takeFrom;
			this.rendererSrc.BrushSize = BrushWidth / 2f;
			this.rendererSrc.Visible = (Data.takeFrom != Point.Empty);
			this.RendererList.Add (this.rendererSrc, false);
			 */
			if (PintaCore.Layers.CurrentLayer != null) {
				switchedTo = true;
				historyRects = new List<Rectangle> ();

				if (Data.wr != null && Data.wr.IsAlive) {
					takeFromLayer = (Layer)Data.wr.Target;
				} else {
					takeFromLayer = null;
				}
			}

			//AppEnvironment.PenInfoChanged += new EventHandler (Environment_PenInfoChanged);
		}

		protected override void OnDeactivated ()
		{
			if (!this.mouseUp) {
				StaticData sd = Data;
				Cairo.PointD lastXY = new Cairo.PointD (0.0, 0.0);

				if (sd != null) {
					lastXY = sd.lastMoved;
				}

				OnMouseUp (1 , lastXY);//left
			}

			//AppEnvironment.PenInfoChanged -= new EventHandler (Environment_PenInfoChanged);

			/*this.RendererList.Remove (this.rendererDst);
			this.rendererDst.Dispose ();
			this.rendererDst = null;

			this.RendererList.Remove (this.rendererSrc);
			this.rendererSrc.Dispose ();
			this.rendererSrc = null;

			if (cursorMouseDown != null) {
				cursorMouseDown.Dispose ();
				cursorMouseDown = null;
			}

			if (cursorMouseUp != null) {
				cursorMouseUp.Dispose ();
				cursorMouseUp = null;
			}

			if (cursorMouseDownSetSource != null) {
				cursorMouseDownSetSource.Dispose ();
				cursorMouseDownSetSource = null;
			}
			 */
			base.OnDeactivated ();
		}

		/*protected override void OnKeyDown (KeyEventArgs e)
		{
			if (IsCtrlDown () && mouseUp) {
				Cursor = cursorMouseDownSetSource;
				mouseDownSettingCloneSource = true;
			}

			base.OnKeyDown (e);
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			// this isn't likely the best way to check to see if
			// the CTRL key has been let up.  If it's not, version
			// 2.1 can address the discrepancy.
			if (!IsCtrlDown () && mouseDownSettingCloneSource) {
				Cursor = cursorMouseUp;
				mouseDownSettingCloneSource = false;
			}

			base.OnKeyUp (e);
		}*/

		protected override void OnMouseUp (Gtk.DrawingArea canvas, Gtk.ButtonReleaseEventArgs args, Cairo.PointD point)
		{
			mouseUp = true;

			OnMouseUp (args.Event.Button, point);
		}

		protected void OnMouseUp (uint button, Cairo.PointD point)
		{
			if (!mouseDownSettingCloneSource) {
				//Cursor = cursorMouseUp;
			}

			if (button == 1) { //left
				active = false;
				//this.rendererDst.Visible = true;

				/*if (savedRegion != null) {
					//commented in pdn
					//RestoreRegion(this.savedRegion);
					PintaCore.Workspace.Invalidate (this.savedRegion.Clipbox);
					savedRegion.Dispose ();
					savedRegion = null;
					//Update ();
				}*/

				if ((Data.takeFrom.X == 0.0 && Data.takeFrom.Y == 0.0) || (Data.lastMoved.X == 0.0 && Data.lastMoved.Y == 0.0)) {
					return;
				}

				if (historyRects.Count > 0) {
					Region saveMeRegion;

					Rectangle[] rectsRO = this.historyRects.ToArray ();
					int rectsROLength = historyRects.Count;

					saveMeRegion = Utility.RectanglesToRegion(rectsRO);

					Region simplifiedRegion = Utility.SimplifyAndInflateRegion (saveMeRegion);
					//SaveRegion (simplifiedRegion, simplifiedRegion.GetBoundsInt ());

					historyRects = new List<Rectangle> ();

					SimpleHistoryItem ha = new SimpleHistoryItem (Icon, Name, PintaCore.Layers.CurrentLayer.Surface, PintaCore.Layers.CurrentLayerIndex);

					PintaCore.History.PushNewItem (ha);
					//this.ClearSavedMemory ();
				}
			}

		}

		unsafe private void DrawACircle (Cairo.PointD pt, Cairo.ImageSurface srfSrc, Cairo.ImageSurface srfDst, Point difference, Rectangle rect)
		{
			float bw = BrushWidth / 2;
			double envAlpha = PintaCore.Palette.PrimaryColor.A / 255;

			rect.Intersect (new Rectangle (difference, new Size (srfSrc.Width, srfSrc.Height)));
			rect.Intersect (srfDst.GetBounds ());

			if (rect.Width == 0 || rect.Height == 0) {
				return;
			}

			// envAlpha = envAlpha^4
			envAlpha *= envAlpha;
			envAlpha *= envAlpha;

			for (int y = rect.Top; y < rect.Bottom; y++) {
				ColorBgra* srcRow = srfSrc.GetRowAddressUnchecked (y - difference.Y);
				ColorBgra* dstRow = srfDst.GetRowAddressUnchecked (y);

				for (int x = rect.Left; x < rect.Right; x++) {
					ColorBgra* srcPtr = unchecked(srcRow + x - difference.X);
					ColorBgra* dstPtr = unchecked(dstRow + x);
					double distFromRing = 0.5f + bw - pt.Distance (new Cairo.PointD (x, y));

					if (distFromRing > 0) {
						double alpha = antialiasing ? Utility.Clamp (distFromRing * envAlpha, 0, 1) : 1;
						alpha *= srcPtr->A / 255f;
						dstPtr->A = (byte)(255 - (255 - dstPtr->A) * (1 - alpha));

						if (0 == (alpha + (1 - alpha) * dstPtr->A / 255)) {
							dstPtr->Bgra = 0;
						} else {
							dstPtr->R = (byte)((srcPtr->R * alpha + dstPtr->R * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
							dstPtr->G = (byte)((srcPtr->G * alpha + dstPtr->G * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
							dstPtr->B = (byte)((srcPtr->B * alpha + dstPtr->B * (1 - alpha) * dstPtr->A / 255) / (alpha + (1 - alpha) * dstPtr->A / 255));
						}
					}
				}
			}

			rect.Inflate (1, 1);
			PintaCore.Workspace.Invalidate (rect);
		}

		private void DrawCloneLine (Cairo.PointD currentMouse, Cairo.PointD lastMoved, Point lastTakeFrom, Cairo.ImageSurface surfaceSource, Cairo.ImageSurface surfaceDest)
		{
			Rectangle[] rectSelRegions;
			Rectangle rectBrushArea;
			int ceilingPenWidth = (int)Math.Ceiling ((double)BrushWidth);

			if (mouseUp || switchedTo) {
				lastMoved = currentMouse;
				lastTakeFrom = Data.takeFrom;
				mouseUp = false;
				switchedTo = false;
			}

			Point difference = new Point ((int)(currentMouse.X - Data.takeFrom.X), (int)(currentMouse.Y - Data.takeFrom.Y));
			Cairo.PointD direction = new Cairo.PointD (currentMouse.X - lastMoved.X, currentMouse.Y - lastMoved.Y);
			double length = direction.Magnitude ();
			double bw = 1 + BrushWidth / 2.0;

			rectSelRegions = this.clipRegion.GetRegionScansReadOnlyInt ();

			Rectangle rect = Utility.PointsToRectangle (lastMoved, currentMouse);
			rect.Inflate (BrushWidth / 2 + 1, BrushWidth / 2 + 1);
			rect.Intersect (new Rectangle (difference, new Size (surfaceSource.Width, surfaceSource.Height)));
			rect.Intersect (surfaceDest.GetBounds ());

			if (rect.Width == 0 || rect.Height == 0) {
				return;
			}

			//SaveRegion (null, rect);
			historyRects.Add (rect);

			// Follow the line to draw the clone... line
			double fInc;

			try {
				fInc = Math.Sqrt (bw) / length;

			} catch (DivideByZeroException) {
				// See bug #1796
				return;
			}

			for (double f = 0; f < 1; f += fInc) {
				// Do intersects with each of the rectangles in a selection
				foreach (Rectangle rectSel in rectSelRegions) {
					Cairo.PointD p = new Cairo.PointD (currentMouse.X * (1 - f) + f * lastMoved.X, currentMouse.Y * (1 - f) + f * lastMoved.Y);

					rectBrushArea = new Rectangle ((int)(p.X - bw), (int)(p.Y - bw), (int)(bw * 2 + 1), (int)(bw * 2 + 1));

					Rectangle rectBrushArea2 = new Rectangle (rectBrushArea.X - difference.X, rectBrushArea.Y - difference.Y, rectBrushArea.Width, rectBrushArea.Height);

					if (rectBrushArea.IntersectsWith (rectSel)) {
						rectBrushArea.Intersect (rectSel);
						//SaveRegion (null, rectBrushArea);
						//SaveRegion (null, rectBrushArea2);
						DrawACircle (p, surfaceSource, surfaceDest, difference, rectBrushArea);
					}
				}
			}
		}

		protected override void OnMouseMove (object o, Gtk.MotionNotifyEventArgs args, Cairo.PointD point)
		{
			base.OnMouseMove (o, args, point);

			//this.rendererDst.BrushLocation = new Point (e.X, e.Y);
			//this.rendererDst.BrushSize = BrushWidth / 2f;

			OnMouseMove (point, args.Event.State);
		}
		
		protected void OnMouseMove (Cairo.PointD point, Gdk.ModifierType modifier)
		{
			if (!(PintaCore.Layers.CurrentLayer is Layer) || (takeFromLayer == null)) {
				return;
			}

			if (Data.updateSrcPreview) {
				Cairo.PointD difference = new Cairo.PointD (point.X - Data.lastMoved.X, point.Y - Data.lastMoved.Y);
				//this.rendererSrc.BrushLocation = new Point (Data.takeFrom.X + difference.X, Data.takeFrom.Y + difference.Y);
				//this.rendererSrc.BrushSize = BrushWidth / 2f;
			}

			if (active && ((Data.takeFrom.X != 0.0) || (Data.takeFrom.Y != 0.0)) && ((modifier & Gdk.ModifierType.ControlMask) == 0)) {
				Point lastTakeFrom = Point.Zero;

				lastTakeFrom = Data.takeFrom;
				if (Data.lastMoved.X != 0 || Data.lastMoved.Y != 0) {
					Data.takeFrom = new Point (Data.takeFrom.X + (int)point.X - (int)Data.lastMoved.X, Data.takeFrom.Y + (int)point.Y - (int)Data.lastMoved.Y);
				} else {
					Data.lastMoved = point;
				}

				Rectangle rect;

				if (BrushWidth != 1) {
					rect = new Rectangle (new Point ((int)(Data.takeFrom.X - BrushWidth / 2), (int)(Data.takeFrom.Y - BrushWidth / 2)), new Size (BrushWidth + 1, BrushWidth + 1));
				} else {
					rect = new Rectangle (new Point (((int)Data.takeFrom.X - BrushWidth), ((int)Data.takeFrom.Y - BrushWidth)), new Size (1 + (2 * BrushWidth), 1 + (2 * BrushWidth)));
				}

				Rectangle boundRect = new Rectangle (Data.takeFrom, new Size (1, 1));

				// If the takeFrom area escapes the boundary
				if (!PintaCore.Layers.CurrentLayer.Surface.GetBounds ().Contains (boundRect)) {
					Data.lastMoved = point;
					lastTakeFrom = Data.takeFrom;
				}

				/*if (this.savedRegion != null) {
					PintaCore.Workspace.Invalidate (savedRegion.GetBoundsInt ());
					this.savedRegion.Dispose ();
					this.savedRegion = null;
				}*/

				rect.Intersect (takeFromLayer.Surface.GetBounds ());

				if (rect.Width == 0 || rect.Height == 0) {
					return;
				}

				//this.savedRegion = new Region (rect);
				//SaveRegion (this.savedRegion, rect);

				// Draw that clone line
				Cairo.ImageSurface takeFromSurface;
				if (object.ReferenceEquals (takeFromLayer, PintaCore.Layers.CurrentLayer)) {
					takeFromSurface = PintaCore.Layers.ToolLayer.Surface;
				} else {
					takeFromSurface = takeFromLayer.Surface;
				}

				if (this.clipRegion == null) {
					this.clipRegion = Selection.CreateRegion ();
				}

				DrawCloneLine (point, Data.lastMoved, lastTakeFrom, takeFromSurface, PintaCore.Layers.CurrentLayer.Surface);

				//this.rendererSrc.BrushLocation = Data.takeFrom;

				PintaCore.Workspace.Invalidate (rect);
				//Update ();

				Data.lastMoved = point;
			}
		}

		/*protected override void OnSelectionChanged ()
		{
			if (this.clipRegion != null) {
				this.clipRegion.Dispose ();
				this.clipRegion = null;
			}

			base.OnSelectionChanged ();
		}*/

		protected override void OnMouseDown (Gtk.DrawingArea canvas, Gtk.ButtonPressEventArgs args, Cairo.PointD point)
		{
			base.OnMouseDown (canvas, args, point);

			if (!(PintaCore.Layers.CurrentLayer is Layer)) {
				return;
			}

			//Cursor = cursorMouseDown;

			if (args.Event.Button == 1) { // left
				active = true;
				//this.rendererDst.Visible = false;
				//using (Cairo.Context g = new Cairo.Context (PintaCore.Layers.CurrentLayer.Surface)
				//{
				if ((args.Event.State & Gdk.ModifierType.ControlMask) != 0) {
					Data.takeFrom = new Point ((int)point.X, (int)point.Y);

					/*this.rendererSrc.BrushLocation = new Point (e.X, e.Y);
					this.rendererSrc.BrushSize = BrushWidth / 2f;
					this.rendererSrc.Visible = true;*/
					Data.updateSrcPreview = false;

					Data.wr = new WeakReference (PintaCore.Layers.CurrentLayer);
					takeFromLayer = (Layer)(Data.wr.Target);
					Data.lastMoved = new Cairo.PointD (0.0, 0.0);
				} else {
					Data.updateSrcPreview = true;

					// Determine if there is something to work if, if there isn't return
					if (Data.takeFrom.X == 0.0 && Data.takeFrom.Y == 0.0) {
					} else if (!Data.wr.IsAlive || takeFromLayer == null) {
						Data.takeFrom = Point.Zero;
						Data.lastMoved = new Cairo.PointD (0.0, 0.0);
					// Make sure the layer is still there!
					} else if (takeFromLayer != null && !PintaCore.Layers.GetLayersToPaint ().Contains (takeFromLayer)) {
						Data.takeFrom = Point.Zero;
						Data.lastMoved = new Cairo.PointD (0.0, 0.0);
					} else {
						//this.antialiasing = AppEnvironment.AntiAliasing;
						OnMouseMove (point, args.Event.State);
					}
				}
				//}
			}
		}

		/*private void Environment_PenInfoChanged (object sender, EventArgs e)
		{
			this.rendererSrc.BrushSize = BrushWidth / 2f;
			this.rendererDst.BrushSize = BrushWidth / 2f;
		}*/

	}
}
