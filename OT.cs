using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
	public static class OT
	{
		public static void Save(string newName, WaveScanner ws)
		{
			Wav total = new Wav();
			foreach(Wav wf in ws.Samples)
			{
				total.Cat(wf);
			}

			// # of samplechain is always EVEN
			int[] possible_lengths = {0, 2, 4, 8, 16, 32, 48, 64};
			int round = possible_lengths.FirstOrDefault(x => x >= ws.Samples.Count);
			int remainder = round > 0 ? round - ws.Samples.Count : 0;
			
			for(int k = 0; k < remainder; k++)
				total.CatSilence(ws.MaxDuration());

			int tot = ws.Samples.Count + remainder;
			string wavName, logName;
			mkNames(newName, tot, out wavName, out logName);

			total.Save(wavName);
			saveLog(logName, remainder, ws);
		}

		private static void mkNames(string name, int slices, out string wavName, out string logName)
		{
			string dir = Path.GetDirectoryName(name);
			string n = $"#{slices}_OT_{Path.GetFileName(name)}";
			wavName = Path.Combine(dir, n);
			logName = Path.ChangeExtension(wavName, ".txt");
		}

		private static void saveLog(string name, int remainder, WaveScanner ws)
		{
			int tot = ws.Samples.Count + remainder;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0} samples in chain", ws.Samples.Count);
			if(remainder > 0)
				sb.AppendFormat(" +{0} fillers ({1} total)", remainder, tot);
			sb.AppendLine();

			int dur;
			string longest = ws.GetLongestSample(out dur);
			sb.AppendFormat("Slice length: {0} (sample name: {1})", dur, longest);
			sb.AppendLine();

			if(tot <= 64)
				append(sb, remainder, ws);
			else
				sb.AppendLine("Too many samples for Octatrack slices (max is 64 slices)");

			File.WriteAllText(name, sb.ToString());
		}

		private static void append(StringBuilder sb,  int remainder, WaveScanner ws)
		{
			sb.AppendLine();
			sb.AppendLine("-------------------------- OCTATRACK --------------------------");

			int n = 0;
			foreach(Wav wf in ws.Samples)
			{
				sb.AppendFormat("Slice #{0,3} => {1}", n++, wf.Name);
				sb.AppendLine();
			}
			for(int k = 0; k < remainder; k++)
			{
				sb.AppendFormat("Slice #{0,3} => (silence)", ws.Samples.Count + k);
				sb.AppendLine();
			}
			sb.AppendLine();
		}

	}
}
