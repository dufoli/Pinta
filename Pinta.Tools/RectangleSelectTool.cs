// 
// RectangleSelectTool.cs
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
using Pinta.Core;
using Mono.Unix;

namespace Pinta.Tools
{
	[System.ComponentModel.Composition.Export (typeof (BaseTool))]
	public class RectangleSelectTool : SelectTool
	{
		public override string Name {
			get { return Catalog.GetString ("Rectangle Select"); }
		}
		public override string Icon {
			get { return "Tools.RectangleSelect.png"; }
		}
		public override string StatusBarText {
			get { return Catalog.GetString ("Click and drag to draw a rectangular selection. Hold shift to constrain to a square."); }
		}
		public override int Priority { get { return 5; } }

		protected override Rectangle DrawShape (Rectangle r, Layer l)
		{
			Document doc = PintaCore.Workspace.ActiveDocument;

			Path path = doc.SelectionPath;
			
			using (Context g = new Context (l.Surface))
				doc.SelectionPath = g.CreateRectanglePath (r);
			
			(path as IDisposable).Dispose ();
			
			// Add some padding for invalidation
			return new Rectangle (r.X, r.Y, r.Width + 2, r.Height + 2);
		}
	}
}
