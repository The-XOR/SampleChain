using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
	public class OTSplitter
	{
		private readonly int[] bestFits = { 0, 2, 4, 8, 16, 32, 48, 64 };
	
		public static List<splitter> GetChains(int numSamples, int overhead)
		{
			OTSplitter instance = new OTSplitter();
			int offset = 0;
			List<splitter> rv = instance.mkChain(numSamples, overhead);
			foreach(splitter t in rv)
			{
				t.chainOffset = offset;
				offset += t.chainLen;
			}
			return rv;
		}

		private int getNearest(int b)
		{
			int minimo = int.MaxValue;
			int ptr = 0;
			for(int k = 0; k < bestFits.Length; k++)
			{
				if(b >= bestFits[k])
				{
					int v = Math.Abs(bestFits[k] - b);
					if(v < minimo)
					{
						minimo = v;
						ptr = k;
					}
				}
			}

			return bestFits[ptr];
		}

		private int getUpNearest(int b)
		{
			int minimo = int.MaxValue;
			int ptr = 0;
			for(int k = 0; k < bestFits.Length; k++)
			{
				if(b <= bestFits[k])
				{
					int v = Math.Abs(bestFits[k] - b);
					if(v < minimo)
					{
						minimo = v;
						ptr = k;
					}
				}
			}

			return bestFits[ptr];
		}
		private List<splitter> mkChain(int numsamples, int overhead, List<splitter> rv = null)
		{
			if(rv == null)
				rv = new List<splitter>();

			if(numsamples > 64)
			{
				rv.Add(new splitter(64));
				rv = mkChain(numsamples - 64, overhead, rv);
			} else
			{
				int nr = getNearest(numsamples);
				int oh = Math.Abs(nr - numsamples);
				if(oh == 0)
					rv.Add(new splitter(numsamples));
				else
				{
					int n2 = getUpNearest(nr + oh);
					if(n2 - numsamples <= overhead)
						rv.Add(new splitter(n2, n2 - numsamples));
					else
					{
						rv.Add(new splitter(nr));
						rv = mkChain(oh, overhead, rv);
					}
				}
			}

			return rv;
		}
	}
}
