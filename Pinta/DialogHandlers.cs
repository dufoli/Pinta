// 
// FileActionHandler.cs
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
using Pinta.Core;
using Gtk;
using Mono.Unix;


namespace Pinta
{
	public class DialogHandlers
	{
		private MainWindow main_window;

		public DialogHandlers (MainWindow window)
		{
			main_window = window;
			
			PintaCore.Actions.File.New.Activated += HandlePintaCoreActionsFileNewActivated;
			PintaCore.Actions.File.Open.Activated += HandlePintaCoreActionsFileOpenActivated;
			PintaCore.Actions.File.OpenRecent.ItemActivated += HandleOpenRecentItemActivated;
			PintaCore.Actions.File.Close.Activated += HandlePintaCoreActionsFileCloseActivated;
			PintaCore.Actions.Image.Resize.Activated += HandlePintaCoreActionsImageResizeActivated;
			PintaCore.Actions.Image.CanvasSize.Activated += HandlePintaCoreActionsImageCanvasSizeActivated;
			PintaCore.Actions.Layers.Properties.Activated += HandlePintaCoreActionsLayersPropertiesActivated;
			PintaCore.Actions.Adjustments.BrightnessContrast.Activated += HandleEffectActivated<BrightnessContrastEffect>;
			PintaCore.Actions.Adjustments.Curves.Activated += HandleAdjustmentsCurvesActivated;
			PintaCore.Actions.Adjustments.Levels.Activated += HandleAdjustmentsLevelsActivated;
			PintaCore.Actions.Adjustments.Posterize.Activated += HandleEffectActivated <PosterizeEffect>;
			PintaCore.Actions.Adjustments.HueSaturation.Activated += HandleEffectActivated<HueSaturationEffect>;
			PintaCore.Actions.Effects.InkSketch.Activated += HandleEffectActivated<InkSketchEffect>;
			PintaCore.Actions.Effects.OilPainting.Activated += HandleEffectActivated<OilPaintingEffect>;
			PintaCore.Actions.Effects.PencilSketch.Activated += HandleEffectActivated<PencilSketchEffect>;
			PintaCore.Actions.Effects.Fragment.Activated += HandleEffectActivated<FragmentEffect>;
			PintaCore.Actions.Effects.GaussianBlur.Activated += HandleEffectActivated<GaussianBlurEffect>;
            PintaCore.Actions.Effects.SurfaceBlur.Activated += HandleEffectSurfaceBlurActivated;
            PintaCore.Actions.Effects.ZoomBlur.Activated += HandleEffectZoomBlurActivated;
            PintaCore.Actions.Effects.Unfocus.Activated += HandleEffectUnfocusActivated;
			PintaCore.Actions.Effects.RadialBlur.Activated += HandleEffectRadialBlurActivated;
			PintaCore.Actions.Effects.Bulge.Activated += HandleEffectActivated <BulgeEffect>;
			PintaCore.Actions.Effects.Dents.Activated += HandleEffectActivated <DentsEffect>;
			PintaCore.Actions.Effects.PolarInversion.Activated += HandleEffectActivated <PolarInversionEffect>;
			PintaCore.Actions.Effects.MotionBlur.Activated += HandleEffectActivated <MotionBlurEffect>;
			PintaCore.Actions.Effects.Glow.Activated += HandleEffectActivated <GlowEffect>;
			PintaCore.Actions.Effects.RedEyeRemove.Activated += HandleEffectActivated <RedEyeRemoveEffect>;
			PintaCore.Actions.Effects.Sharpen.Activated += HandleEffectActivated <SharpenEffect>;
			PintaCore.Actions.Effects.SoftenPortrait.Activated += HandleEffectActivated <SoftenPortraitEffect>;
			PintaCore.Actions.Effects.Clouds.Activated += HandleEffectCloudsActivated;
			PintaCore.Actions.Effects.JuliaFractal.Activated += HandleEffectJuliaFractalActivated;
			PintaCore.Actions.Effects.MandelbrotFractal.Activated += HandleEffectMandelbrotFractalActivated;
			PintaCore.Actions.Effects.EdgeDetect.Activated += HandleEffectActivated <EdgeDetectEffect>;
			PintaCore.Actions.Effects.Twist.Activated += HandleEffectActivated<TwistEffect>;
			PintaCore.Actions.Effects.Tile.Activated += HandleEffectActivated<TileEffect>;
			PintaCore.Actions.Effects.Pixelate.Activated += HandleEffectActivated<PixelateEffect>;
			PintaCore.Actions.Effects.FrostedGlass.Activated += HandleEffectActivated<FrostedGlassEffect>;
			PintaCore.Actions.Effects.Relief.Activated += HandleEffectActivated <ReliefEffect>;
			PintaCore.Actions.Effects.Emboss.Activated += HandleEffectActivated<EmbossEffect>;
            PintaCore.Actions.Effects.AddNoise.Activated += HandleEffectAddNoiseActivated;
            PintaCore.Actions.Effects.Median.Activated += HandleEffectMedianActivated;
            PintaCore.Actions.Effects.ReduceNoise.Activated += HandleEffectReduceNoiseActivated;
			PintaCore.Actions.Effects.Outline.Activated += HandleEffectOutlineActivated;
			PintaCore.Actions.View.Rulers.Toggled += HandlePintaCoreActionsViewRulersToggled;
			PintaCore.Actions.View.Pixels.Activated += HandlePixelsActivated;
			PintaCore.Actions.View.Inches.Activated += HandleInchesActivated;
			PintaCore.Actions.View.Centimeters.Activated += HandleCentimetersActivated;
			PintaCore.Actions.View.UnitComboBox.ComboBox.Changed += HandleUnitComboBoxComboBoxChanged;
		}

		#region Handlers
		private void HandlePintaCoreActionsFileNewActivated (object sender, EventArgs e)
		{
			bool canceled = false;

			if (PintaCore.Workspace.IsDirty) {
				var primary = Catalog.GetString ("Save the changes to image \"{0}\" before creating a new one?");
				var secondary = Catalog.GetString ("If you don't save, all changes will be permanently lost.");
				var markup = "<span weight=\"bold\" size=\"larger\">{0}</span>\n\n{1}\n";
				markup = string.Format (markup, primary, secondary);

				var md = new MessageDialog (PintaCore.Chrome.MainWindow, DialogFlags.Modal,
				                            MessageType.Question, ButtonsType.None, true,
				                            markup,
				                            System.IO.Path.GetFileName (PintaCore.Workspace.Filename));

				md.AddButton (Catalog.GetString ("Continue without saving"), ResponseType.No);
				md.AddButton (Stock.Cancel, ResponseType.Cancel);
				md.AddButton (Stock.Save, ResponseType.Yes);

				md.DefaultResponse = ResponseType.Cancel;

				ResponseType saveResponse = (ResponseType)md.Run ();
				md.Destroy ();

				if (saveResponse == ResponseType.Yes) {
					PintaCore.Actions.File.Save.Activate ();
				}
				else {
					canceled = saveResponse == ResponseType.Cancel;
				}
			}

			if (canceled) {
				return;
			}


			NewImageDialog dialog = new NewImageDialog ();

			dialog.ParentWindow = main_window.GdkWindow;
			dialog.WindowPosition = Gtk.WindowPosition.CenterOnParent;

			int response = dialog.Run ();

			if (response == (int)Gtk.ResponseType.Ok) {
				PintaCore.Workspace.ActiveDocument.HasFile = false;
				PintaCore.Workspace.ImageSize = new Cairo.Point (dialog.NewImageWidth, dialog.NewImageHeight);
				PintaCore.Workspace.CanvasSize = new Cairo.Point (dialog.NewImageWidth, dialog.NewImageHeight);
				
				PintaCore.Layers.Clear ();
				PintaCore.History.Clear ();
				PintaCore.Layers.DestroySelectionLayer ();
				PintaCore.Layers.ResetSelectionPath ();

				// Start with an empty white layer
				Layer background = PintaCore.Layers.AddNewLayer ("Background");

				using (Cairo.Context g = new Cairo.Context (background.Surface)) {
					g.SetSourceRGB (255, 255, 255);
					g.Paint ();
				}

				PintaCore.Workspace.Filename = "Untitled1";
				PintaCore.History.PushNewItem (new BaseHistoryItem ("gtk-new", "New Image"));
				PintaCore.Workspace.IsDirty = false;
				PintaCore.Actions.View.ZoomToWindow.Activate ();
				main_window.AddDocument (PintaCore.Layers.GetFlattenedImage ());
			}

			dialog.Destroy ();
		}

		private void HandleOpenRecentItemActivated (object sender, EventArgs e)
		{
			bool canceled = false;

			if (PintaCore.Workspace.IsDirty) {
				var primary = Catalog.GetString ("Save the changes to image \"{0}\" before opening a new image?");
				var secondary = Catalog.GetString ("If you don't save, all changes will be permanently lost.");
				var markup = "<span weight=\"bold\" size=\"larger\">{0}</span>\n\n{1}\n";
				markup = string.Format (markup, primary, secondary);

				var md = new MessageDialog (PintaCore.Chrome.MainWindow, DialogFlags.Modal,
											MessageType.Question, ButtonsType.None, true,markup,
											System.IO.Path.GetFileName (PintaCore.Workspace.Filename));

				md.AddButton (Catalog.GetString ("Continue without saving"), ResponseType.No);
				md.AddButton (Stock.Cancel, ResponseType.Cancel);
				md.AddButton (Stock.Save, ResponseType.Yes);

				md.DefaultResponse = ResponseType.Cancel;

				var response = (ResponseType)md.Run ();
				md.Destroy ();

				if (response == ResponseType.Yes) {
					PintaCore.Actions.File.Save.Activate ();
				}
				else {
					canceled = response == ResponseType.Cancel;
				}
			}

			if (!canceled) {
				string fileUri = (sender as RecentAction).CurrentUri;

				main_window.OpenFile (new Uri (fileUri).LocalPath);

				PintaCore.Workspace.ActiveDocument.HasFile = true;
			}
		}


		private void HandlePintaCoreActionsFileOpenActivated (object sender, EventArgs e)
		{
			bool canceled = false;

			if (PintaCore.Workspace.IsDirty) {
				var primary = Catalog.GetString ("Save the changes to image \"{0}\" before opening a new image?");
				var secondary = Catalog.GetString ("If you don't save, all changes will be permanently lost.");
				var markup = "<span weight=\"bold\" size=\"larger\">{0}</span>\n\n{1}\n";
				markup = string.Format (markup, primary, secondary);

				var md = new MessageDialog (PintaCore.Chrome.MainWindow, DialogFlags.Modal,
				                            MessageType.Question, ButtonsType.None, true,
				                            markup,
				                            System.IO.Path.GetFileName (PintaCore.Workspace.Filename));

				md.AddButton (Catalog.GetString ("Continue without saving"), ResponseType.No);
				md.AddButton (Stock.Cancel, ResponseType.Cancel);
				md.AddButton (Stock.Save, ResponseType.Yes);

				md.DefaultResponse = ResponseType.Cancel;

				ResponseType response = (ResponseType)md.Run ();
				md.Destroy ();

				if (response == ResponseType.Yes) {
					PintaCore.Actions.File.Save.Activate ();
				}
				else {
					canceled = response == ResponseType.Cancel;
				}
			}

			if (!canceled) {
				var fcd = new Gtk.FileChooserDialog (Catalog.GetString ("Open Image File"), PintaCore.Chrome.MainWindow,
														FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
														Gtk.Stock.Open, Gtk.ResponseType.Ok);

				int response = fcd.Run ();

			
				if (response == (int)Gtk.ResponseType.Ok) {
					if (main_window.OpenFile (fcd.Filename)) {
						PintaCore.Actions.File.AddRecentFileUri (fcd.Uri);

						PintaCore.Workspace.ActiveDocument.HasFile = true;
					}
				}
	
				fcd.Destroy ();
			}
		}

		private void HandlePintaCoreActionsFileCloseActivated (object sender, EventArgs e)
		{
			var markup = " Do you really want to close {0} ?\n";

			var md = new MessageDialog (PintaCore.Chrome.MainWindow, DialogFlags.Modal,
			                            MessageType.Question, ButtonsType.OkCancel, 
			                            markup,
			                            System.IO.Path.GetFileName (PintaCore.Workspace.Filename));
			ResponseType response = (ResponseType)md.Run ();
				md.Destroy ();

				if (response == ResponseType.Ok) {
					main_window.CloseFile ();
					//TODO remove from tabgrid and switch file on workspace mgr
				}
		}

		private void HandlePintaCoreActionsImageResizeActivated (object sender, EventArgs e)
		{
			ResizeImageDialog dialog = new ResizeImageDialog ();

			dialog.ParentWindow = main_window.GdkWindow;
			dialog.WindowPosition = Gtk.WindowPosition.CenterOnParent;

			int response = dialog.Run ();

			if (response == (int)Gtk.ResponseType.Ok)
				dialog.SaveChanges ();

			dialog.Destroy ();
		}
		
		private void HandlePintaCoreActionsImageCanvasSizeActivated (object sender, EventArgs e)
		{
			ResizeCanvasDialog dialog = new ResizeCanvasDialog ();

			dialog.ParentWindow = main_window.GdkWindow;
			dialog.WindowPosition = Gtk.WindowPosition.CenterOnParent;

			int response = dialog.Run ();

			if (response == (int)Gtk.ResponseType.Ok)
				dialog.SaveChanges ();

			dialog.Destroy ();
		}
				
		private void HandlePintaCoreActionsLayersPropertiesActivated (object sender, EventArgs e)
		{
			var dialog = new LayerPropertiesDialog ();
			
			int response = dialog.Run ();		
			
			if (response == (int)Gtk.ResponseType.Ok
			    && dialog.AreLayerPropertiesUpdated) {
				
				var historyMessage = GetLayerPropertyUpdateMessage(
						dialog.InitialLayerProperties,
						dialog.UpdatedLayerProperties);				
				
				var historyItem = new UpdateLayerPropertiesHistoryItem(
					"Menu.Layers.LayerProperties.png",
					historyMessage,
					PintaCore.Layers.CurrentLayerIndex,
					dialog.InitialLayerProperties,
					dialog.UpdatedLayerProperties);
				
				PintaCore.History.PushNewItem (historyItem);
				
				PintaCore.Workspace.Invalidate ();
				
			} else {
				
				var layer = PintaCore.Layers.CurrentLayer;
				var initial = dialog.InitialLayerProperties;
				initial.SetProperties (layer);
				
				if (layer.Opacity != initial.Opacity)
					PintaCore.Workspace.Invalidate ();
			}
				
			dialog.Destroy ();
		}
		
		private string GetLayerPropertyUpdateMessage (
			LayerProperties initial,
			LayerProperties updated)
		{

			string ret = null;
			int count = 0;
			
			if (updated.Opacity != initial.Opacity) {
				ret = "Layer Opacity";
				count++;
			}
				
			if (updated.Name != initial.Name) {
				ret = "Rename Layer";
				count++;
			}
			
			if (updated.Hidden != initial.Hidden) {
				ret = (updated.Hidden) ? "Hide Layer" : "Show Layer";
				count++;
			}
			
			if (ret == null || count > 1)
				ret = "Layer Properties";
			
			return ret;
		}		
		
		private void HandleEffectActivated<T> (object sender, EventArgs e)
			where T : BaseEffect, new ()
		{
			var effect = new T ();
			PintaCore.LivePreview.Start (effect);
		}
		
		private void HandleAdjustmentsCurvesActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new CurvesEffect ());	
		}

		private void HandleAdjustmentsLevelsActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new LevelsEffect ());
		}

		private void HandleEffectSurfaceBlurActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new SurfaceBlurEffect());
        }

		private void HandleEffectZoomBlurActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new ZoomBlurEffect());
        }

		private void HandleEffectUnfocusActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new UnfocusEffect());
        }

		private void HandleEffectRadialBlurActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new RadialBlurEffect ());
		}

		private void HandleEffectCloudsActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new CloudsEffect ());	
		}

		private void HandleEffectMandelbrotFractalActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new MandelbrotFractalEffect ());	
		}

		private void HandleEffectJuliaFractalActivated (object sender, EventArgs e)
		{
			PintaCore.Actions.Adjustments.PerformEffect (new JuliaFractalEffect ());	
		}

        private void HandleEffectAddNoiseActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new AddNoiseEffect());
        }

        private void HandleEffectMedianActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new MedianEffect());
        }

        private void HandleEffectReduceNoiseActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new ReduceNoiseEffect());
        }

		private void HandleEffectOutlineActivated(object sender, EventArgs e)
        {
            PintaCore.Actions.Adjustments.PerformEffect(new OutlineEffect());
        }

		private void HandlePintaCoreActionsViewRulersToggled (object sender, EventArgs e)
		{
			if (((ToggleAction)sender).Active)
				main_window.ShowRulers ();
			else
				main_window.HideRulers ();
		}

		private void HandleUnitComboBoxComboBoxChanged (object sender, EventArgs e)
		{
			switch (PintaCore.Actions.View.UnitComboBox.ComboBox.Active) {
				case 0://pixels
					main_window.ChangeRulersUnit (Gtk.MetricType.Pixels);
				break;
				case 1://inches
					main_window.ChangeRulersUnit (Gtk.MetricType.Inches);
				break;
				case 2://centimeters
					main_window.ChangeRulersUnit (Gtk.MetricType.Centimeters);
				break;
				
			}
		}

		private void HandleCentimetersActivated (object sender, EventArgs e)
		{
			main_window.ChangeRulersUnit (Gtk.MetricType.Centimeters);
		}

		private void HandleInchesActivated (object sender, EventArgs e)
		{
			main_window.ChangeRulersUnit (Gtk.MetricType.Inches);
		}

		private void HandlePixelsActivated (object sender, EventArgs e)
		{
			main_window.ChangeRulersUnit (Gtk.MetricType.Pixels);
		}

		#endregion
	}
}

