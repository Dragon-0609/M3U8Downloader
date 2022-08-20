using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
			if (args.Length == 0)
			{
				Console.WriteLine ("Set Main Link\nFor example:\nFrom https://de3.libria.fun/videos/media/ts/9210/6/480/4b74a2ffeb058591dfdc6caffd08953b.m3u8\nto  https://de3.libria.fun/videos/media/ts/9210/6/480/");
				string main_url = Console.ReadLine ();
				Console.WriteLine ("Write the path to m3u8");
				string file = Console.ReadLine ();
				if (file.StartsWith ("\"") && file.EndsWith ("\""))
				{
					file = file.Replace ("\"", "");
				}

				Environment.CurrentDirectory = Path.GetDirectoryName (file);

				string[] content = File.ReadAllText (file).Split (new char[] { '\n' }, StringSplitOptions.None)
									   .Where (line => line.Contains (".ts")).ToArray ();
				Console.WriteLine ("File parsing end.\nStarting downloading");
				CleanDestination ();
				all = DownloadEachSegment (main_url, content);
				Console.WriteLine ("Downloading end");

			} else
			{
				all = int.Parse (args[0]);
				Environment.CurrentDirectory = Path.GetDirectoryName (args[1]);
			}

			/*
			Console.WriteLine ("Write first saved file name. Default: Без названия");
			string rename = Console.ReadLine ();*/
			/*if (rename.Length == 0)
				rename = "Без названия";*/
			// RenameAll (rename, all);
			Merge (all);
			Console.ReadKey ();
		}

		private static int DownloadEachSegment(string main_url, string[] content){
			int i = 0;
			int max = content.Length;
			WebDownloader downloader = new WebDownloader ();
			foreach (string line in content) {
				
				Task task = downloader.Download (main_url+line, i);
				task.Wait ();
				i++;
				
				Console.WriteLine ("Downloaded: {0} / {1}",i, max);
				// Console.ReadLine ();
			}
			return i;
		}

		private static void CleanDestination ()
		{
			string[] files = Directory.GetFiles (Environment.CurrentDirectory, "segment_*", SearchOption.TopDirectoryOnly);
			foreach (string file in files)
			{
				File.Delete (file);
			}
		}

		private static void RenameAll(string baseName, int max){
			int current = 0;
			if (baseName.Contains ("(")) {
				current = int.Parse (baseName.Split (new char[]{ '(', ')' }) [1]);
			}

			for (int i = 0; i < max; i++) {
				string path;
				if (i != 0)
					path = baseName + " (" + current + ")";
				else
					path = baseName;
				current++;
				if (File.Exists(path))
					File.Move (path, "segment_" + i + ".ts");
			}
		}

		private static void Merge(int max){
			RenameFile ("full.ts");
			RenameFile ("output.mp4");
			
			
			string command = "type ";
			int last = max - 1;
			for (int i = 0; i < max; i++) {
				command += "\"segment_" + i + ".ts\"";
				if (i != last)
					command += ", ";
			}
			command += " > full.ts";

			Process cmd = new Process ();
			cmd.StartInfo.FileName = "cmd.exe";
			cmd.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
			cmd.StartInfo.Arguments = "/k " + command;
			Console.WriteLine ("After merge, write to convert to mp4: \nffmpeg -i full.ts -c:v libx264 -preset medium -tune film -crf 23 -strict experimental -c:a aac -b:a 192k output.mp4");
			cmd.Start ();
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
	}
}
