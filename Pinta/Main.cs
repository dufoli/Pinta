// 
// Main.cs
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
using Gtk;
using Mono.Options;
using System.Collections.Generic;

namespace Pinta
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			int threads = -1;
			
			var p = new OptionSet () {
				{ "rt|render-threads=", "number of threads to use for rendering", (int v) => threads = v }
			};

			List<string> extra;
			
			try {
				extra = p.Parse (args);
			} catch (OptionException e) {
				Console.Write ("Pinta: ");
				Console.WriteLine (e.Message);
				return;
			}
			
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			
			if (threads != -1)
				Pinta.Core.PintaCore.System.RenderThreads = threads;
				
			if (extra.Count > 0) {
				// Not sure what this does for Mac, so I'm not touching it
				if (Platform.GetOS () == Platform.OS.Mac) {
					string arg = args[0];

					if (args[0].StartsWith ("-psn_")) {
						if (args.Length > 1)
							arg = args[1];
						else
							arg = null;
					}
				
					if (arg != null && arg != "")
						win.OpenFile (arg);
				} else {
					win.OpenFile (extra[0]);
				}				
			}
			
			Application.Run ();
		}
	}
}
