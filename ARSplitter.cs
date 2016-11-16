using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
	public class ARSplitter
	{
		private readonly int[] bestFits = { 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 24, 30, 40, 60, 120 };

		public static List<splitter> GetChains(int numSamples, int overhead)
		{
			ARSplitter instance = new ARSplitter();
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
		private List<splitter> mkChain(int numsamples,int overhead, List<splitter> rv = null)
		{
			if(rv == null)
				rv = new List<splitter>();

			if(numsamples > 120)
			{
				rv.Add(new splitter(120));
				rv = mkChain(numsamples - 120, overhead, rv);
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
