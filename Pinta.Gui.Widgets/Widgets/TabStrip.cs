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
		
		public int SelectedIndex {
			get {
				return selectedIndex;
			}
			set {
				if (selectedIndex != value) {
					selectedIndex = value;
					OnChanged ();
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
			
			GdkWindow.GetSize (out rectWidth, out rectHeight);
			
			using (Cairo.Context g = Gdk.CairoHelper.Create (GdkWindow)) {
				if (offset > 0) {
					//triangle
					g.MoveTo (1, -30);
					g.LineTo (6, -25);
					g.LineTo (6, -35);
					g.LineTo (1, -30);
					g.ClosePath ();
					g.Color = new Color (0,0,0);
					g.Fill();
					//todo draw image gradient of thumbnails[offset -1]
				}
				
				double pos = SideSize;
				for (int i = offset; i< thumbnails.Count ; i++) {
					ImageSurface thumbnail = thumbnails[i];
					
					if (SideSize + thumbnail.Width  > rectWidth - SideSize) {
						//triangle
						g.MoveTo (rectWidth -1, -30);
						g.LineTo (rectWidth -6, -25);
						g.LineTo (rectWidth -6, -35);
						g.LineTo (rectWidth -1, -30);
						g.ClosePath ();
						g.Color = new Color (0,0,0);
						g.Fill();
						//TODO draw gradient thumbnail
						break;
					}
					
					if (i == selectedIndex) {
						g.MoveTo (pos - BorderSize/2, 0.0);
						g.LineTo (pos - BorderSize/2, rectHeight);
						g.LineTo (pos + BorderSize/2 + thumbnail.Width, rectHeight);
						g.LineTo (pos + BorderSize/2 + thumbnail.Width, 0.0);
						g.LineTo (pos - BorderSize/2, 0.0);
						g.Color = new Color (0, 0.10, 0.75, 0.45);
						g.FillPreserve ();
						//TODO Stroke
					}
					
					g.MoveTo (pos, rectHeight - BorderSize/2);
					g.LineTo (pos, rectHeight - BorderSize/2 - thumbnail.Height);
					g.LineTo (pos + thumbnail.Width, rectHeight - BorderSize/2 - thumbnail.Height);
					g.LineTo (pos + thumbnail.Width, rectHeight - BorderSize/2);
					g.LineTo (pos, rectHeight - BorderSize/2);
					g.Color = new Color (0,0,0);
					g.Stroke ();
					
					//TODO add clip region
					//http://cairographics.org/FAQ/

					g.SetSourceSurface (thumbnail, (int)pos, rectHeight - BorderSize/2 - thumbnail.Height);
					g.Rectangle (pos, rectHeight - BorderSize/2 - thumbnail.Height, thumbnail.Width, thumbnail.Height);
					g.Fill ();
					
					pos += thumbnail.Width + BorderSize;
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
				if (evnt.X < SideSize) //todo test is triangle is active and clamp offset
					offset--;
				else if (evnt.X > this.Allocation.Width - SideSize)//todo test is triangle is active
					offset++;
				else {
					int r = PointToOffset(evnt.X);
					if (r != -1) {
						SelectedIndex = r;
						this.GdkWindow.Invalidate ();
					}
				}
			}
			return base.OnButtonPressEvent (evnt);
		}

		int PointToOffset (double X)
		{
			int size = SideSize - BorderSize/2;
			for (int i = offset; i< thumbnails.Count ; i++) {
				ImageSurface thumbnail = thumbnails[i];
				size += thumbnail.Width + BorderSize;
				if (X < size)
					return i;
			}
			return -1;
		}

		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return base.OnKeyPressEvent (evnt);
			
			//TODO
			//handle left and right arrow
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
