// 
// TabStrip.cs
//  
// Author:
//       dufoli <>
// 
// Copyright (c) 2010 dufoli
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
using Pinta.Core;
using System.Collections.Generic;

namespace Pinta.Gui.Widgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TabStrip : Gtk.DrawingArea
	{
		private const int SideSize = 10;
		private const int BorderSize = 8;
		private List<ImageSurface> thumbnails;
		int offset;
		int selectedIndex;
		bool leftTriangle;
		bool rightTriangle;
		
		public int SelectedIndex {
			get {
				return selectedIndex;
			}
			set {
				if (selectedIndex != value) {
					selectedIndex = value;
					this.GdkWindow.Invalidate ();
					OnChanged ();
				}
			}
		}

		public int Offset {
			get {
				return offset;
			}
			set {
				if (offset != value && value >= 0 && value < thumbnails.Count) {
					offset = value;
					this.GdkWindow.Invalidate ();
				}
			}
		}

		public TabStrip ()
		{
			Events = ((Gdk.EventMask)(16134));
			
			thumbnails = new List<ImageSurface> ();
			offset = 0;
			selectedIndex = 0;
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Effects.Artistic.OilPainting.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			thumbnails.Add (new ImageSurface ("/home/dufoli/src/Pinta/Pinta.Resources/Resources/Menu.Adjustments.Posterize.png"));
			
			//FOR testing add a static list of image.
		}
		
		public void AddThumbnail (ImageSurface surf)
		{
			//TODO reduce flatImage to thumbnail
			thumbnails.Add (surf);
			//TODO selected and move offset if needed
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);
			
			int rectWidth;
			int rectHeight;
			
			rightTriangle = false;
			GdkWindow.GetSize (out rectWidth, out rectHeight);
			
			using (Cairo.Context g = Gdk.CairoHelper.Create (GdkWindow)) {
				if (offset > 0) {
					rightTriangle = true;
					g.MoveTo (rectWidth -1, rectHeight/2);
					g.LineTo (rectWidth -6, rectHeight/2 + 5);
					g.LineTo (rectWidth -6, rectHeight/2 - 5);
					g.LineTo (rectWidth -1, rectHeight/2);
					g.ClosePath ();
					g.LineWidth = 1;
					g.LineCap = LineCap.Square;
					g.Color = new Color (0, 0, 0);
					g.StrokePreserve ();
					g.Fill();
					//TODO draw gradient thumbnail
				}
				else
					leftTriangle = false;
				
				double pos = rectWidth - SideSize;
				for (int i = offset; i< thumbnails.Count ; i++) {
					ImageSurface thumbnail = thumbnails[i];
					
					if (pos - thumbnail.Width - SideSize < 0) {
						leftTriangle = true;
						g.MoveTo (1, rectHeight/2);
						g.LineTo (6, rectHeight/2 + 5);
						g.LineTo (6, rectHeight/2 - 5);
						g.LineTo (1, rectHeight/2);
						g.ClosePath ();
						g.LineWidth = 1;
						g.LineCap = LineCap.Square;
						g.Color = new Color (0, 0, 0);
						g.StrokePreserve ();
						g.Fill();
						//todo draw image gradient of thumbnails[offset -1]
						break;
					}
					
					if (i == selectedIndex) {
						g.MoveTo (pos + BorderSize/2, 0.0);
						g.LineTo (pos + BorderSize/2, rectHeight);
						g.LineTo (pos - BorderSize/2 - thumbnail.Width, rectHeight);
						g.LineTo (pos - BorderSize/2 - thumbnail.Width, 0.0);
						g.LineTo (pos + BorderSize/2, 0.0);
						g.Color = new Color (0, 0.10, 0.85, 0.35);
						g.Fill ();
						//TODO Stroke
					}
					
					g.MoveTo (pos, rectHeight - BorderSize/2);
					g.LineTo (pos, rectHeight - BorderSize/2 - thumbnail.Height);
					g.LineTo (pos - thumbnail.Width, rectHeight - BorderSize/2 - thumbnail.Height);
					g.LineTo (pos - thumbnail.Width, rectHeight - BorderSize/2);
					g.LineTo (pos, rectHeight - BorderSize/2);
					g.Color = new Color (0,0,0);
					g.Stroke ();
					
					g.SetSourceSurface (thumbnail, (int)pos - thumbnail.Width, rectHeight - BorderSize/2 - thumbnail.Height);
					g.Rectangle (pos - thumbnail.Width, rectHeight - BorderSize/2 - thumbnail.Height, thumbnail.Width, thumbnail.Height);
					g.Fill ();
					
					pos -= thumbnail.Width + BorderSize;
				}
			}
			return true;
		}
		
		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			requisition.Height = 65;
			requisition.Width = 400;
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if (evnt.Button == 1) { //left
				if (evnt.X < SideSize && leftTriangle) //todo test is triangle is active and clamp offset
					Offset++;
				else if (evnt.X > this.Allocation.Width - SideSize && rightTriangle)//todo test is triangle is active
					Offset--;
				else {
					int r = PointToOffset(evnt.X);
					if (r != -1) {
						SelectedIndex = r;
					}
				}
			}
			return base.OnButtonPressEvent (evnt);
		}

		int PointToOffset (double X)
		{
			int rectWidth;
			int rectHeight;
			GdkWindow.GetSize (out rectWidth, out rectHeight);
			
			int size = rectWidth - SideSize + BorderSize/2;
			for (int i = offset; i< thumbnails.Count ; i++) {
				ImageSurface thumbnail = thumbnails[i];
				size -= thumbnail.Width + BorderSize;
				if (X > size)
					return i;
			}
			return -1;
		}

		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return base.OnKeyPressEvent (evnt);
			
			//TODO
			//handle left and right arrow
			//handle supr to delete current
		}
		
		#region Public Events
		public event EventHandler Changed;
		
		protected virtual void OnChanged ()
		{
			if (Changed != null) {
				Changed (this, EventArgs.Empty);
			}
		}
		#endregion
	}
}
