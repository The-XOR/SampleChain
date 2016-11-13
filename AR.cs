using System.IO;
using System.Text;

namespace ConsoleApplication1
{
	public static class AR
	{
		public static void Save(string newName, WaveScanner ws)
		{
			Wav total = new Wav();
			foreach(Wav wf in ws.Samples)
			{
				total.Cat(wf);
			}

			// # of samplechain is always EVEN
			int remainder = 0;
			while((120 % (ws.Samples.Count + remainder) != 0))
				remainder++;

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
			string n = $"#{slices}({120 / slices})_{Path.GetFileName(name)}";
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

			if(tot <= 120)
				append(sb, 120 / tot, remainder, ws);
			else
				sb.AppendLine("Too many samples for Analog RYTM chain (max is 120 samples)");

			File.WriteAllText(name, sb.ToString());
		}

		private static void append(StringBuilder sb, int increment, int remainder, WaveScanner ws)
		{
			sb.AppendLine();
			sb.AppendLine("-------------------------- ANALOG RYTM --------------------------");
			int n = 0;
			foreach(Wav wf in ws.Samples)
			{
				sb.AppendFormat("STA = {0,3} END = {1,3} => {2} ", n, n + increment, wf.Name);
				n += increment;
				sb.AppendLine();
			}
			for(int k = 0; k < remainder; k++)
			{
				sb.AppendFormat("STA = {0,3} END = {1,3} => (silence) ", n, n + increment);
				n += increment;
				sb.AppendLine();
			}

			sb.AppendLine();
		}
	}
}
