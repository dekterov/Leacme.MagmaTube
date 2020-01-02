// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using MMTools;
using MMTools.Runners;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Leacme.Lib.MagmaTube {

	public class Library {

		private string ffmpegPath;

		public Library() {

			MMToolsConfiguration.Register();
			var ffRunner = new MMRunner(MMAppType.FFMPEG);
			ffmpegPath = (string)ffRunner.GetType().GetProperty("ApplicationPath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ffRunner);
			AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

		}

		/// <summary>
		///	Gets the stream for the thumbnail image so can be displayed in the application.
		/// /// </summary>
		/// <param name="thumbnailUrl"></param>
		/// <returns></returns>
		public async Task<Stream> GetThumbnailAsync(Uri thumbnailUrl) {
			var resp = await new HttpClient().GetAsync(thumbnailUrl);
			return await resp.Content.ReadAsStreamAsync();
		}

		/// <summary>
		///	Get the information about the video if it exists.
		/// </summary>
		/// <param name="videoUrl"></param>
		/// <returns></returns>
		public async Task<VideoInfo> GetVideoData(Uri videoUrl) {
			var id = YoutubeClient.ParseVideoId(videoUrl.ToString());
			var client = new YoutubeClient();
			var videoInfo = await client.GetVideoAsync(id);
			var streamInfo = await client.GetVideoMediaStreamInfosAsync(id);
			return new VideoInfo(videoInfo, streamInfo);

		}

		/// <summary>
		/// Download the video in .mp4 or audio in .mp3 locally if it is valid.
		/// </summary>
		/// <param name="videoToDownload">The URL of the video.</param>
		/// <param name="audioOnly">If only the audio in .mp3 is required.</param>
		/// <param name="outputDirectory">The default directory is the desktop, but can be specified.</param>
		/// <param name="progress">The IProgress to monitor the download status.</param>
		/// <returns></returns>
		public async Task DownloadFileAsync(VideoInfo videoToDownload, bool audioOnly = false, Uri outputDirectory = null, IProgress<double> progress = null) {
			if (outputDirectory == null) {
				outputDirectory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
			}

			var client = new YoutubeClient();
			var converter = new YoutubeConverter(client, ffmpegPath);
			string ext;
			if (audioOnly) {
				ext = ".mp3";
			} else {
				ext = ".mp4";
			}
			await converter.DownloadVideoAsync(videoToDownload.VideoMetadata.Id, Path.Combine(outputDirectory.LocalPath, SanitizeFilename(videoToDownload.VideoMetadata.Title) + ext), ext.Substring(1), progress);
		}

		private string SanitizeFilename(string filename) {
			var inv = new char[] { '"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', '\x001c', '\x001d', '\x001e', '\x001f', ':', '*', '?', '\\', '/' };
			inv = inv.Concat(Path.GetInvalidPathChars()).ToArray().Concat(Path.GetInvalidFileNameChars()).ToArray();
			return string.Join("", filename.Split(inv));
		}
	}

	/// <summary>
	///	Provivides the video data, such as length, title, author, etc. and available downloadable streams.
	/// /// </summary>
	public class VideoInfo {
		public Video VideoMetadata { get; }
		public MediaStreamInfoSet StreamInfo { get; }

		public VideoInfo(Video videoMetadata, MediaStreamInfoSet streamInfo) {
			this.VideoMetadata = videoMetadata;
			this.StreamInfo = streamInfo;
		}
	}
}
