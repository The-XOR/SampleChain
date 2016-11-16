using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApplication1
{
	public class splitter
	{
		public int chainLen;
		public int fillers;
		public int chainOffset;
		public splitter(int a, int b = 0)
		{
			chainLen = a - b;
			fillers = b;
			chainOffset = 0;
		}
	}

	public class WaveScanner
	{		
		private readonly List<Wav> waveList;
		public int NumFiles => waveList.Count;

		public WaveScanner()
		{
			waveList = new List<Wav>();
		}

		private void dumpStats()
		{
			foreach(Wav wf in waveList)
			{
				Console.WriteLine("File {0}: S/R={1} Channels={2} Depth={3}", wf.Name, wf.SR, wf.NumChannels, wf.Depth);
			}			
		}

		public string LoadDir(string dir)
		{
			List<string> filelist = Directory.GetFiles(dir, "*.wav").ToList();
			return _load(filelist);
		}

		public string Load(string filelist)
		{
			List<string> lines = new List<string>(File.ReadAllLines(filelist));
			return _load(lines);
		}

		private string _load(List<string> lines)
		{
			lines = lines.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

			if(!lines.Any())
				return "Empty filelist";

			foreach(string _line in lines)
			{
				string line = _line.Replace("\"", "");
				if(!File.Exists(line))
				{
					return string.Format("Missing input file: {0}", line);
				}

				waveList.Add(new Wav(line));
			}

			int sr = 0,
				numchannels = 0,
				depth = 0;
			
			foreach(Wav wf in waveList)
			{
				if(sr == 0)
				{					
					numchannels = wf.NumChannels;
					sr= wf.SR;
					depth=wf.Depth;
				}  else
				{
					int curchannels = wf.NumChannels;
					int cursr = wf.SR;
					int curdepth = wf.Depth;
					if(cursr != sr || curchannels != numchannels || depth != curdepth)
					{
						dumpStats();
						return "Mixed file format error";
					}
				}
			}

			return null;
		}

		
		public void Save(string newName, bool createAR, bool createOT, int overhead, bool createLogs)
		{
			if(createAR)
				AR.Save(newName, this, overhead, createLogs);
			if(createOT)
				OT.Save(newName, this, overhead, createLogs);
		}

		public WavCollection GetSamples(splitter chain)
		{
			return GetSamples(chain.chainOffset, chain.chainOffset+chain.chainLen);
		}

		public WavCollection GetSamples(int from = 0, int to = -1)
		{
			if(to < 0)
				to = waveList.Count;

			List<Wav> sublist = new List<Wav>();
			for(int k = from; k < to; k++)
			{
				sublist.Add(new Wav(waveList[k]));
			}

			return new WavCollection(sublist);
		}
	}
}
