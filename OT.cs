using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApplication1
{
	public static class OT
	{
		public static void Save(string newName, WaveScanner ws, int overhead, bool createLogs)
		{
			List<splitter> chains = OTSplitter.GetChains(ws.NumFiles, overhead);
			int chain_n = 1;
			foreach(splitter chain in chains)
			{
				WavCollection samples = ws.GetSamples(chain);
				Wav total = samples.GetChain();

				for(int k = 0; k < chain.fillers; k++)
					total.CatSilence(samples.MaxDuration());

				int tot = chain.chainLen + chain.fillers;
				string wavName, logName;
				mkNames(newName, tot, chain_n, out wavName, out logName);

				total.Save(wavName);
				if(createLogs)
					saveLog(logName, chain.fillers, samples);
				chain_n++;
			}
		}

		private static void mkNames(string name, int slices, int chain_n, out string wavName, out string logName)
		{
			string dir = Path.GetDirectoryName(name);
			string n = $"#{slices}_OT_{Path.GetFileName(name)}_C{chain_n}";
			wavName = Path.Combine(dir, n);
			logName = Path.ChangeExtension(wavName, ".txt");
			wavName = Path.ChangeExtension(wavName, ".wav");
		}

		private static void saveLog(string name, int remainder, WavCollection samples)
		{
			int tot = samples.NumSamples + remainder;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0} samples in chain", samples.NumSamples);
			if(remainder > 0)
				sb.AppendFormat(" +{0} fillers ({1} total)", remainder, tot);
			sb.AppendLine();

			int dur;
			string longest = samples.GetLongestSample(out dur);
			sb.AppendFormat("Slice length: {0} (sample name: {1})", dur, longest);
			sb.AppendLine();

			if(tot <= 64)
				append(sb, remainder, samples);
			else
				sb.AppendLine("Too many samples for Octatrack slices (max is 64 slices)");

			File.WriteAllText(name, sb.ToString());
		}

		private static void append(StringBuilder sb,  int remainder, WavCollection samples)
		{
			sb.AppendLine();
			sb.AppendLine("-------------------------- OCTATRACK --------------------------");

			int n = 0;
			for(int k = 0; k < samples.NumSamples; k++)
			{
				sb.AppendFormat("Slice #{0,3} => {1}", n++, samples.GetName(k));
				sb.AppendLine();
			}
			for(int k = 0; k < remainder; k++)
			{
				sb.AppendFormat("Slice #{0,3} => (silence)", samples.NumSamples + k);
				sb.AppendLine();
			}
			sb.AppendLine();
		}

	}
}
