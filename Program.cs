using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace ConsoleApplication1
{
	class Options
	{
		[Option('i', "input", DefaultValue = null, Required = false, HelpText = "Filelist to read.")]
		public string InputFile { get; set; }

		[Option('f', "folder", DefaultValue = null, Required = false, HelpText = "Folder to read.")]
		public string InputFolder { get; set; }

		[Option('o', "output", DefaultValue = null, Required = false, HelpText = "Output file name (will be decorated)")]
		public string OutputFile { get; set; }

		[Option('m', "maxoverhead", DefaultValue = 2, Required =false, HelpText = "Max # of fillers in chains.")]
		public int MaxOverhead { get; set; }

		[Option('r', "rytm", DefaultValue = false, Required = false, HelpText = "Create Analog RYTM chains.")]
		public bool CreateAR { get; set; }

		[Option('t', "octatrack", DefaultValue = false, Required = false, HelpText = "Create Octatrack chains.")]
		public bool CreateOT { get; set; }

		[Option('l', "logs", DefaultValue = false, Required = false, HelpText = "Create log files for generated chains.")]
		public bool CreateLogs { get; set; }

		[ParserState]
		public IParserState LastParserState { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}		
	}

	class Program
	{
		static int Main(string[] args)
		{
			Options opt = new Options();
			if(!Parser.Default.ParseArguments(args, opt) || (string.IsNullOrWhiteSpace(opt.InputFile) && string.IsNullOrWhiteSpace(opt.InputFolder)))
			{
				// Values are available here
				Console.WriteLine(opt.GetUsage());
				return 1;
			}

			if(!opt.CreateAR && !opt.CreateOT)
			{
				Console.WriteLine(opt.GetUsage());
				Console.WriteLine("Nothing to do: no output chain specified.");
				return 1;
			}

			WaveScanner ws = new WaveScanner();
			string errorcode = string.IsNullOrWhiteSpace(opt.InputFolder) ? ws.Load(opt.InputFile) : ws.LoadDir(opt.InputFolder);
			if(!string.IsNullOrEmpty(errorcode))
			{
				Console.WriteLine(errorcode);
				return 1;
			}
			
			string out_fn;
			if(string.IsNullOrWhiteSpace(opt.OutputFile))
			{
				string fn = string.IsNullOrWhiteSpace(opt.InputFolder) ? opt.InputFile : opt.InputFolder;
				string dir = Path.GetDirectoryName(fn);
				string nm = Path.GetFileNameWithoutExtension(fn) + ".wav";
				out_fn = Path.Combine(dir, nm);
			} else
			{
				out_fn = opt.OutputFile;
			}

			Console.WriteLine("Creating chains, please wait...");
			ws.Save(out_fn, opt.CreateAR, opt.CreateOT, opt.MaxOverhead, opt.CreateLogs);

			return 0;
		}
	}
}
