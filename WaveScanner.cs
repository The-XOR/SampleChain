using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApplication1
{
	public class WaveScanner
	{		
		private readonly List<Wav> waveList;
		public int NumFiles => waveList.Count;
		public List<Wav> Samples => waveList;

		public WaveScanner()
		{
			waveList = new List<Wav>();
		}

		private void dumpStats()
		{
			foreach(Wav wf in waveList)
			{
				Console.WriteLine("File {0}: S/R={1} Channels={2} Depth={3} PCM={4}", wf.Name, wf.SR, wf.NumChannels, wf.Depth, wf.PCM ? "Y" : "N");
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
			bool pcm = false;
			foreach(Wav wf in waveList)
			{
				if(sr == 0)
				{					
					numchannels = wf.NumChannels;
					sr= wf.SR;
					depth=wf.Depth;
					pcm = wf.PCM;
				}  else
				{
					int curchannels = wf.NumChannels;
					int cursr = wf.SR;
					int curdepth = wf.Depth;
				//	if(cursr != sr || curchannels != numchannels || depth != curdepth || wf.PCM != pcm)
					if(cursr != sr || curchannels != numchannels || depth != curdepth)
					{
						dumpStats();
						return "Mixed file format error";
					}
				}
			}

			return null;
		}

		public string GetLongestSample(out int dur)
		{
			Wav wf = maxPtr();
			if(wf != null)
			{
				dur = wf.Duration;
				return wf.Name;
			}

			dur = 0;
			return null;
		}

		public int MaxDuration()
		{
			return maxPtr()?.Duration ?? 0;
		}

		private Wav maxPtr()
		{
			Wav rv = null;
			int curDur = 0;
			foreach(Wav wf in waveList)
			{
				int dur = wf.OrigDuration;
				if(dur > curDur)
				{
					curDur = dur;
					rv = wf;
				}
			}

			return rv;
		}

		private void roundUp()
		{
			int value = MaxDuration();
			foreach(Wav wf in waveList)
			{
				wf.RoundUp(value);
			}			
		}

		public void Save(string newName)
		{
			roundUp();
			AR.Save(newName, this);
			OT.Save(newName, this);
		}
	}
}
