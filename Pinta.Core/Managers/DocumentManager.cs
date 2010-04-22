// 
// DocumentManager.cs
//  
// Author:
//       dufoli <${AuthorEmail}>
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
using System.Collections.Generic;

namespace Pinta.Core
{
	//best is to reference tabstrip here but can not because cycle reference
	public class DocumentManager
	{
		//TODO think if we can not manage it in pintacore but if we do like that
		// we will have to transfert all event registration...
		
		public class DocumentData
		{
			public int current_layer;
			public List<Layer> layers;
			public Cairo.Path selection_path;
			public bool show_selection;
			public List<BaseHistoryItem> history;
			public int historyPointer;
			public Gtk.ListStore ListStore;
		}
		
		private List<DocumentData> docs;
		
		public DocumentManager ()
		{
			docs = new List<DocumentData> ();
		}

		public void Save (int index)
		{
			//if (index >= docs.Count)
			//	throw new ArgumentOutOfRangeException ();
			
			DocumentData doc = new DocumentData ();
			if (index == docs.Count)
				docs.Add(doc);
			else
				docs[index] = doc;
			
			PintaCore.Layers.Save (doc);
			PintaCore.History.Save (doc);
		}

		public void Restore (int index)
		{
			DocumentData doc = docs[index];
			
			PintaCore.Layers.Restore (doc);
			PintaCore.History.Restore (doc);
			PintaCore.Workspace.Invalidate ();
		}

		public void Remove (int index)
		{
			docs.RemoveAt (index);
		}
	}
}
