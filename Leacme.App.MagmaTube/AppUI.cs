// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Leacme.Lib.MagmaTube;

namespace Leacme.App.MagmaTube {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();

		public AppUI() {

			rootPan.Spacing = 2;

			var vidu = App.HorizontalFieldWithButton;
			vidu.holder.HorizontalAlignment = HorizontalAlignment.Center;
			vidu.label.Text = "Video address:";
			vidu.field.Width = 700;
			vidu.field.Watermark = "https://www.youtube.com/watch?v=12345";
			vidu.button.Content = "Get Video";

			var vidThumb = new Image();

			vidThumb.Height = 170;

			var infName = App.TextBlock;
			infName.FontSize = 20;
			infName.FontWeight = FontWeight.Bold;
			infName.TextAlignment = TextAlignment.Center;
			infName.Text = "";

			var infUpl = App.TextBlock;
			infUpl.TextAlignment = TextAlignment.Center;
			infUpl.Text = "Uploaded on: ";

			var infAuth = App.TextBlock;
			infAuth.TextAlignment = TextAlignment.Center;
			infAuth.Text = "Author: ";

			var btv = App.Button;
			btv.Content = "Download Video";

			var bta = App.Button;
			bta.Content = "Download Audio";

			var dlLab = App.TextBlock;
			dlLab.Text = "Progress: ";

			var dlLab2 = App.TextBlock;
			dlLab2.Text = "0";

			var prog = new Progress<double>();
			prog.ProgressChanged += (z, zz) => dlLab2.Text = Math.Round(zz * 100).ToString();

			var dlLab3 = App.TextBlock;
			dlLab3.Text = "%";

			btv.IsEnabled = bta.IsEnabled = false;

			var dlHrl = App.HorizontalStackPanel;
			dlHrl.HorizontalAlignment = HorizontalAlignment.Center;
			dlHrl.Children.AddRange(new List<IControl> { btv, bta, new Control { Width = 20 }, dlLab, dlLab2, dlLab3 });

			var expStat = new Expander();
			expStat.Header = "Statistics";

			var expDescr = new Expander();
			expDescr.Header = "Description";

			var expKwd = new Expander();
			expKwd.Header = "Keywords";

			expDescr.Margin = expKwd.Margin = expStat.Margin = new Thickness(20, 0);

			var expHdr = new StackPanel();
			expHdr.Children.AddRange(new List<IControl> { expStat, expDescr, expKwd });

			var srl = App.ScrollViewer;
			srl.Height = 150;
			srl.Background = Brushes.Transparent;

			srl.Content = expHdr;

			vidu.button.Click += async (z, zz) => {
				try {
					((App)Application.Current).LoadingBar.IsIndeterminate = true;
					var vd = await lib.GetVideoData(new Uri(vidu.field.Text));

					using (var st = await lib.GetThumbnailAsync(new Uri(vd.VideoMetadata.Thumbnails.HighResUrl))) {
						vidThumb.Source = new Bitmap(st);
					}

					infName.Text = vd.VideoMetadata.Title;
					infUpl.Text = "Uploaded on: " + vd.VideoMetadata.UploadDate.ToString("d MMMM, yyyy");
					infAuth.Text = "Author: " + vd.VideoMetadata.Author;
					expDescr.Content = vd.VideoMetadata.Description;
					expKwd.Content = string.Join(", ", vd.VideoMetadata.Keywords);
					expStat.Content = vd.VideoMetadata.Statistics.ViewCount + " views, " + Math.Round(vd.VideoMetadata.Statistics.AverageRating * 20) + "% rating, " + vd.VideoMetadata.Statistics.LikeCount + "/" + vd.VideoMetadata.Statistics.DislikeCount + " likes/dislikes, " + "duration: " + vd.VideoMetadata.Duration;

					dlHrl.Children.Remove(btv);
					dlHrl.Children.Remove(bta);
					btv = App.Button;
					btv.Content = "Download Video";

					bta = App.Button;
					bta.Content = "Download Audio";

					btv.Click += async (zzz, zzzz) => {
						await lib.DownloadFileAsync(vd, progress: prog);
					};

					bta.Click += async (zzz, zzzz) => {
						await lib.DownloadFileAsync(vd, true, progress: prog);
					};

					btv.IsEnabled = bta.IsEnabled = true;
					dlLab2.Text = "0";

					dlHrl.Children.InsertRange(0, new List<IControl> { btv, bta });

					((App)Application.Current).LoadingBar.IsIndeterminate = false;
				} catch (Exception) {
					((App)Application.Current).LoadingBar.IsIndeterminate = false;
				}
			};
			rootPan.Children.AddRange(new List<IControl> { vidu.holder, vidThumb, infName, infUpl, infAuth, dlHrl, srl });

		}
	}
}