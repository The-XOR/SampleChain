using System.Collections.Generic;

namespace ConsoleApplication1
{
	public class WavCollection
	{
		private readonly List<Wav> waveList;
		public int NumSamples => waveList.Count;
		public string GetName(int k) => waveList[k].Name;

		public WavCollection(List<Wav> list)
		{
			waveList = list;
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

		public int MaxDuration() => maxPtr()?.Duration ?? 0;

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

		public Wav GetChain()
		{
			roundUp();
			Wav total = new Wav();
			foreach(Wav wf in waveList)
			{
				total.Cat(wf);
			}

			return total;
		}

	}
}
