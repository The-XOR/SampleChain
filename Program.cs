
using System;
using System.IO;

namespace ConsoleApplication1
{
	class Program
	{
		static int Main(string[] args)
		{
			if(args.Length < 1)
			{
				Console.WriteLine("Usage: samplechain <filelist.txt> <outfile.wav>");
				Console.WriteLine("   where <filelist.txt> is the list of the WAV files to chain");
				Console.WriteLine("- or -");
				Console.WriteLine("samplechain <directory> <outfile.wav>");
				Console.WriteLine("   where <directory> is the folder containing the WAV files to chain");
				return 1;
			}

			string fn = args[0];
			if(!Directory.Exists(fn) && !File.Exists(fn))
			{
				Console.WriteLine("Invalid or missing filelist: {0}", fn);
				return 1;
			}

			WaveScanner ws = new WaveScanner();
			string errorcode;
			if(File.Exists(fn))
				errorcode = ws.Load(fn);
			else
				errorcode = ws.LoadDir(fn);

			if(!string.IsNullOrEmpty(errorcode))
			{
				Console.WriteLine(errorcode);
				return 1;
			}

			string out_fn;
			if(args.Length > 1)
				out_fn = args[1];
			else
			{
				string dir = Path.GetDirectoryName(fn);
				string nm = Path.GetFileNameWithoutExtension(fn) + "_chain.wav";
				out_fn = Path.Combine(dir, nm);
			}

			int dur;
			string longest = ws.GetLongestSample(out dur);
			Console.WriteLine($"{ws.NumFiles} files, max duration: {dur} samples (sample {longest})");
			ws.Save(out_fn);

			return 0;
		}
	}
}
