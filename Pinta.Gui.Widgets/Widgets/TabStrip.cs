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
using System.Collections.Generic;

namespace Pinta.Gui.Widgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TabStrip : Gtk.DrawingArea
	{
		private List<ImageSurface> thumbnails;
		int offset;
		int selectedIndex;

		public TabStrip ()
		{
			this.Build ();
			thumbnails = new List<ImageSurface> ();
			offset = 0;
			selectedIndex = 0;
			
			//FOR testing add a static list of image.
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);
			
			Rectangle rect = GdkWindow.GetBounds ();
			using (Cairo.Context g = CairoHelper.Create (GdkWindow)) {
				if (offset > 0) {
					g.MoveTo (1, -30);
					g.LineTo (6, -25);
					g.LineTo (6, -35);
					g.LineTo (1, -30);
					g.ClosePath ();
					g.Color = new Color (0,0,0);
					g.Fill();
					//todo draw image gradient of thumbnails[offset -1]
				}
				
				double pos = 10.0;
				for (int i = offset; i< thumbnails.Count ; i++) {
					ImageSurface thumbnail = thumbnails[i];
					
					if (10 + thumbnail.Width  > rect.Width -10) {
						//rectangle
						g.MoveTo (rect.Width -1, -30);
						g.LineTo (rect.Width -6, -25);
						g.LineTo (rect.Width -6, -35);
						g.LineTo (rect.Width -1, -30);
						g.ClosePath ();
						g.Color = new Color (0,0,0);
						g.Fill();
						//TODO draw gradient thumbnail
						break;
					}
					
					if (i == selectedIndex) {
						g.MoveTo (pos - 2.0, 0.0);
						g.LineTo (pos - 2.0, rect.Height);
						g.LineTo (pos + 2.0 + thumbnail.Width, rect.Height);
						g.LineTo (pos + 2.0 + thumbnail.Width, 0.0);
						g.LineTo (pos - 2.0, 0.0);
						g.Color = new Color (0, 0.10, 0.75, 0.75);
						g.FillPreserve ();
						//TODO Stroke
					}
					
					g.MoveTo (pos, rect.Height -2);
					g.LineTo (pos, rect.Height -2 - thumbnail.Height);
					g.LineTo (pos + thumbnail.Width, rect.Height -2 - thumbnail.Height);
					g.LineTo (pos + thumbnail.Width, rect.Height -2);
					g.LineTo (pos, rect.Height -2);
					g.Color = new Color (0,0,0);
					g.Stroke ();
					
					g.SetSource (thumbnail, 0.0, 0.0);
					g.Paint ();
					pos += thumbnail.Width + 4;
				}
			}
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			return base.OnButtonPressEvent (evnt);
			if (evnt.Button = 1) { //left
				if (evnt.X < 10.0)
					offset--;
				else if (evnt.X > this.Allocation.Width - 10.0)
					offset++;
				else
					selectedIndex = PointToOffset(evnt.X);
			}
		}

		void PointToOffset (double X)
		{
			int size = 10.0;
			for (int i = offset; i< thumbnails.Count ; i++) {
				ImageSurface thumbnail = thumbnails[i];
				size += thumbnail.Width;
				if (X < size)
					return i;
			}
		}

		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return base.OnKeyPressEvent (evnt);
			
			//TODO
			//handle left and right arrow
		}
	}
}
