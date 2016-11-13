using System;
using System.IO;
using System.Text;

namespace ConsoleApplication1
{
	public class Wav
	{	
		private class wavHeader
		{
			private const string sGroupID = "RIFF"; // RIFF
			private uint dwFileLength; // total file length minus 8, which is taken up by RIFF
			private const string sRiffType = "WAVE"; // always WAVE

			private wavHeader()
			{
			}

			public wavHeader(wavHeader copy)
			{
				dwFileLength = copy.dwFileLength;
			}

			public void Write(BinaryWriter writer, wavFormat format, wavData data)
			{
				calcLen(format, data);
				writer.Write(sGroupID.ToCharArray());
				writer.Write(dwFileLength);
				writer.Write(sRiffType.ToCharArray());
				
				// Write the format chunk
				format.Write(writer);

				// Write the data chunk
				data.Write(writer);
			}

			public void CloseHeader(BinaryWriter writer)
			{
				writer.Seek(sGroupID.Length, SeekOrigin.Begin);
				uint filesize = (uint)(writer.BaseStream.Length - sGroupID.Length - sRiffType.Length);
				writer.Write(filesize);				
			}

			public static wavHeader Read(BinaryReader reader)
			{
				wavHeader rv = new wavHeader();
				rv.read(reader);
				return rv;
			}

			private void read(BinaryReader reader)
			{
				reader.BaseStream.Seek(sGroupID.Length, SeekOrigin.Begin);
				dwFileLength = reader.ReadUInt32();
				reader.BaseStream.Seek(sRiffType.Length, SeekOrigin.Current);
			}

			private void calcLen(wavFormat format, wavData data)
			{
				dwFileLength = (uint)sRiffType.Length + format.getLength() + data.getLength();
			}
		}

		private class wavFormat
		{
			private const string sChunkID = "fmt ";
			public uint dwChunkSize; // Length of header in bytes, 16 for PCM
			public ushort wFormatTag; // 1 (MS PCM)
			public uint dwSamplesPerSec; // Frequency of the audio in Hz... 44100
			public ushort wChannels; // Number of channels
			private uint dwAvgBytesPerSec; // for estimating RAM allocation
			public ushort wBlockAlign; // sample frame size, in bytes
			public ushort wBitsPerSample; // bits per sample
			private byte[] fillBytes;

			internal uint getLength()
			{
				return (uint)sChunkID.Length + dwChunkSize;
			}

			private wavFormat()
			{
				//wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
				//dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
			}

			public wavFormat(wavFormat copy)
			{
				dwChunkSize = copy.dwChunkSize;
				wFormatTag = copy.wFormatTag;
				dwSamplesPerSec = copy.dwSamplesPerSec;
				wChannels = copy.wChannels;
				dwAvgBytesPerSec = copy.dwAvgBytesPerSec;
				wBlockAlign = copy.wBlockAlign;
				wBitsPerSample=copy.wBitsPerSample;
				fillBytes = copy.fillBytes;
			}

			public void Write(BinaryWriter writer)
			{
				writer.Write(sChunkID.ToCharArray());
				writer.Write(dwChunkSize);
				writer.Write(wFormatTag);
				writer.Write(wChannels);
				writer.Write(dwSamplesPerSec);
				writer.Write(dwAvgBytesPerSec);
				writer.Write(wBlockAlign);
				writer.Write(wBitsPerSample);
				writer.Write(fillBytes);
			}

			public static wavFormat Read(BinaryReader reader)
			{
				wavFormat rv = new wavFormat();
				rv.read(reader);
				return rv;
			}

			private void read(BinaryReader reader)
			{
				locate(reader);
				
				dwChunkSize = reader.ReadUInt32();		// (non conta, e' la lunghezza stessa)
				wFormatTag = reader.ReadUInt16();		// +2
				wChannels = reader.ReadUInt16();		//+2
				dwSamplesPerSec = reader.ReadUInt32();	//+4
				dwAvgBytesPerSec = reader.ReadUInt32();	//+4
				wBlockAlign = reader.ReadUInt16();		//+2
				wBitsPerSample = reader.ReadUInt16();   //+2
				fillBytes = reader.ReadBytes((int)dwChunkSize - 16);				
			}

			private void locate(BinaryReader reader)
			{
				byte[] chunkid = reader.ReadBytes(4);
				string s = Encoding.ASCII.GetString(chunkid);
				if(s != sChunkID)
				{
					long chunkSize = reader.ReadUInt32();
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					locate(reader);
				}
			}
		}

		private class wavData
		{
			private const string sChunkID = "data";     // "data"
			public byte[] samples;  // 8-bit audio

			/// <summary>
			/// Initializes a new data chunk with default values.
			/// </summary>
			private wavData()
			{
				samples = null;
			}

			internal uint getLength()
			{
				return (uint)sChunkID.Length + (uint)samples.Length;
			}

			public wavData(wavData copy)
			{
				samples = new byte[copy.samples.Length];
				for(int k = 0; k < copy.samples.Length; k++)
				{
					samples[k] = copy.samples[k];
				}
			}
			public void Write(BinaryWriter writer)
			{
				uint dwChunkSize = (uint)samples.Length;
				writer.Write(sChunkID.ToCharArray());
				writer.Write(dwChunkSize);
				foreach(byte s in samples)
				{
					writer.Write(s);
				}
			}

			public static wavData Read(BinaryReader reader)
			{
				wavData rv = new wavData();
				rv.read(reader);
				return rv;
			}

			private void read(BinaryReader reader)
			{
				locate(reader);

				uint dwChunkSize = reader.ReadUInt32();
				samples = reader.ReadBytes((int)dwChunkSize);
			}

			private void locate(BinaryReader reader)
			{
				byte[] chunkid = reader.ReadBytes(4);
				string s = Encoding.ASCII.GetString(chunkid);
				if(s != sChunkID)
				{
					long chunkSize = reader.ReadUInt32();
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					locate(reader);
				}
			}

			private byte[] createTempCopy(int extraSize)
			{
				byte[] temp = new byte[samples.Length + extraSize];
				for(int k = 0; k < samples.Length; k++)
				{
					temp[k] = samples[k];
				}
				return temp;
			}
			
			public void Cat(Wav inputFile)
			{
				byte[] temp = createTempCopy(inputFile.data.samples.Length);
				for(int k = 0; k < inputFile.data.samples.Length; k++)
				{
					temp[k + samples.Length] = inputFile.data.samples[k];
				}

				assignTempCopy(temp);
			}

			public void CatSilence(int len)
			{
				byte[] temp = createTempCopy(len);
				for(int k = 0; k < len; k++)
					temp[k + samples.Length] = 0;

				assignTempCopy(temp);
			}

			private void assignTempCopy(byte[] temp)
			{
				samples = temp;
			}
		}

		// Header, Format, Data chunks
		private wavHeader header = null;
		private wavFormat format = null;
		private wavData data = null;

		public int NumChannels => format?.wChannels ?? 0;

		public int SR => format != null ? (int)format.dwSamplesPerSec : 0;
		public int Depth => format?.wBitsPerSample ?? 0;

		public int Duration => data?.samples.Length ?? 0;
		public int OrigDuration { get; private set; }

		public bool PCM => format != null ? format.wFormatTag == 1 && format.dwChunkSize==16: false;

		public Wav()
		{
			clear();
		}

		public Wav(string filename)
		{
			Load(filename);
		}

		public string FileName { get; set; }
		public string Name { get; set; }

		private void clear()
		{
			header = null;
			format = null;
			data = null;
			FileName = null;
			Name = null;
		}

		public void Save(string filePath)
		{
			if(header != null)
			{
				using(FileStream fileStream = new FileStream(filePath, FileMode.Create))
				{
					using(BinaryWriter writer = new BinaryWriter(fileStream))
					{
						// Write the header
						header.Write(writer, format, data);

						header.CloseHeader(writer);

						// Clean up
						writer.Close();
					}

					fileStream.Close();
				}

			}
		}

		public void Load(string filePath)
		{
			clear();
			FileName = filePath;
			Name = Path.GetFileName(FileName);
			using(FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader reader = new BinaryReader(fileStream))
				{
					// Read the header
					header = wavHeader.Read(reader);

					// Read the format chunk
					format = wavFormat.Read(reader);

					// Read the data chunk
					data = wavData.Read(reader);
					OrigDuration = Duration;

					reader.Close();
				}
				fileStream.Close();
			}
		}

		public void RoundUp(int value)
		{
			if(Duration < value)
			{
				// quant sample servono per arrivare a roundup?
				int required_samples = value - Duration;
				if(required_samples > 0)
					data.CatSilence(required_samples);
			}		
		}

		public void Cat(Wav inputFile)
		{
			if(header == null)
			{
				header = new wavHeader(inputFile.header);
				format = new wavFormat(inputFile.format);
				data = new wavData(inputFile.data);
			} else
			{
				data.Cat(inputFile);
			}
		}

		public void CatSilence(int len)
		{
			data.CatSilence(len);
		}
	}
}