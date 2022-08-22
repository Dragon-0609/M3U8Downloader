using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace M3U8Downloader
{
	class MainClass
	{
		
		[STAThread]
		public static void Main (string[] args)
		{
			RunApp (args);
			Console.ReadKey ();
		}

		private static void RunApp (string[] args)
		{
			int all = 0;
			string name = "full";
			int index = 0;
			if (args.Length == 0)
			{
				Console.WriteLine ("Set Main Link\nFor example:\nFrom https://de3.libria.fun/videos/media/ts/9210/6/480/4b74a2ffeb058591dfdc6caffd08953b.m3u8\nto  https://de3.libria.fun/videos/media/ts/9210/6/480/");
				string main_url = Console.ReadLine ();
				Console.WriteLine ("Write the path to m3u8");
				string file = Console.ReadLine ();
				file = RemoveQuotes (file);

				name = Path.GetFileNameWithoutExtension (file);
				Environment.CurrentDirectory = Path.GetDirectoryName (file);

				string[] content = File.ReadAllText (file).Split (new char[] { '\n' }, StringSplitOptions.None)
									   .Where (line => line.Contains (".ts")).ToArray ();
				Console.WriteLine ("File parsing end.\nStarting downloading");
				CleanDestination ();
				all = DownloadEachSegment (main_url, index, content);
				Console.WriteLine ("Downloading end");

			} else if (args.Length == 2)
			{
				string[] count = args[0].Split (new char[] { '-' }, StringSplitOptions.None);
				index = int.Parse (count[0]);
				all = int.Parse (count[1]);
				string file = args[1];
				file = RemoveQuotes (file);

				Environment.CurrentDirectory = Path.GetDirectoryName (file);
				name = Path.GetFileNameWithoutExtension (file);
			} else if (args.Length == 3)
			{
				index = int.Parse (args[0]);
				string main_url = args[1]; 
				string file = args[2];
				file = RemoveQuotes (file);
				name = Path.GetFileNameWithoutExtension (file);
				Environment.CurrentDirectory = Path.GetDirectoryName (file);
				
				string[] content = File.ReadAllText (file).Split (new char[] { '\n' }, StringSplitOptions.None)
									   .Where (line => line.Contains (".ts")).ToArray ();
				all = DownloadEachSegment (main_url, index, content);
				
			} else
			{
				Console.WriteLine("Invalid length of arguments. Acceptable: 0 or 2 args");
			}

			Merge (all, index, name);
			Console.ReadKey ();
		}
		
		private static string RemoveQuotes (string file)
		{
			if (file.StartsWith ("\"") && file.EndsWith ("\""))
			{
				file = file.Replace ("\"", "");
			}

			return file;
		}

		private static int DownloadEachSegment(string main_url, int index, string[] content){
			int max = content.Length;
			WebDownloader downloader = new WebDownloader ();
			for (int i1 = index; i1 < content.Length; i1++)
			{
				string line = content[i1];
				Task task = downloader.Download (main_url + line, i1);
				task.Wait ();
				Console.WriteLine ("Downloaded: {0} / {1}", i1+1, max);
				// Add ();
				// Console.ReadLine ();
			}

			return max;
		}

		private static void CleanDestination ()
		{
			string[] files = Directory.GetFiles (Environment.CurrentDirectory, "segment_*", SearchOption.TopDirectoryOnly);
			foreach (string file in files)
			{
				File.Delete (file);
			}
		}

		private static void Merge(int max, int index, string name){
			string file = $"{name}.ts";
			RenameFile (file);
			string mp4 = $"{name}.mp4";
			RenameFile (mp4);
			CombineMultipleFilesIntoSingleFile(max, index, file);
			Console.WriteLine (
				$"After merge, write to convert to mp4: \nffmpeg -i {file} -c:v libx264 -preset medium -tune film -crf 23 -strict experimental -c:a aac -b:a 192k {mp4}");

		}

		private static void RenameFile (string file, int count = 1)
		{
			if (File.Exists (file))
			{
				string destFileName = $"{Path.GetFileNameWithoutExtension (file)}_{count}{Path.GetExtension (file)}";
				if (File.Exists (destFileName))
				{
					count++;
					RenameFile (file, count);
				}
				else
				{
					File.Move (file, destFileName);
				}
			}
		}
		
		private static void CombineMultipleFilesIntoSingleFile(int max, int index, string outputFilePath)
		{
			string[] inputFilePaths = new string[max - index];
			int ind = index;
			for (int i = 0; i < inputFilePaths.Length; i++) {
				inputFilePaths[i] = "segment_" + ind + ".ts";
				ind++;
			}
			
			using (var outputStream = File.Create(outputFilePath))
			{
				foreach (string inputFilePath in inputFilePaths)
				{
					using (var inputStream = File.OpenRead(inputFilePath))
					{
						// Buffer size can be passed as the second argument.
						inputStream.CopyTo(outputStream);
					}
				}
			}
		}
	}
}
