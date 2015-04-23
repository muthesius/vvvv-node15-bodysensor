#region usings
using System;
using System.IO;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(
		Name = "OSCReader",
		Category = "Raw",
		Help = "Basic raw template which copies up to count bytes from the input to the output", 
		Tags = "", 
		AutoEvaluate = true)]
	#endregion PluginInfo
	
	public class RawOSCReaderNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Stream> FStreamIn;

		[Input("Reset", IsBang = true)]
		public IDiffSpread<bool> FReset;
		
		[Input("Index")]
		public IDiffSpread<int> FNext;
	
		[Output("Frame")]
		public ISpread<long> FFrame;
		

		[Output("Output")]
		public ISpread<Stream> FStreamOut;

		//when dealing with byte streams (what we call Raw in the GUI) it's always
		//good to have a byte buffer around. we'll use it when copying the data.
		readonly byte[] FBuffer = new byte[1024];
		#endregion fields & pins

		//called when all inputs and outputs defined above are assigned from the host
		public void OnImportsSatisfied()
		{
			//start with an empty stream output
			FStreamOut.SliceCount = 0;
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			//ResizeAndDispose will adjust the spread length and thereby call
			//the given constructor function for new slices and Dispose on old
			//slices.
			FStreamOut.ResizeAndDispose(spreadMax, () => new MemoryStream());
			FFrame.SliceCount = spreadMax;
			
			for (int i = 0; i < spreadMax; i++) {
				//get the input stream
				var inputStream = FStreamIn[i];
				//get the output stream (this works because of ResizeAndDispose above)
				var outputStream = FStreamOut[i];
				
				if(FReset.IsChanged && FReset[0])
				{
					FStreamIn[i].Seek(0, SeekOrigin.Begin);
				}
				
				if(FNext.IsChanged)
				{
					//get framenum
					inputStream.Read(FBuffer,0,8);
					UInt64 Frame = BitConverter.ToUInt64(FBuffer, 0);
					FFrame[i] = inputStream.Position;
				
					//get packagesize
					inputStream.Read(FBuffer,0,4);
					long OscMsgSize = BitConverter.ToUInt32(FBuffer, 0);
				
					//get the number of bytes we should copy (avoid negative values)
					var count = Math.Max(OscMsgSize, 0);
					//see how many bytes should be copied from the input stream
					var numBytesToCopy = Math.Min(inputStream.Length, count);

					if(inputStream.Position >= inputStream.Length) 
						inputStream.Position = 0;
					
					outputStream.Position = 0;
					//set the length of the output stream
					outputStream.SetLength(numBytesToCopy);

					//finally copy the data
					while (numBytesToCopy > 0) {
						//make sure we don't read more than we need or more than
						//our byte buffer can hold
						var chunkSize = (int)Math.Min(numBytesToCopy, FBuffer.Length);
						//the stream's read method returns how many bytes have actually
						//been read into the buffer
						var numBytesRead = inputStream.Read(FBuffer, 0, chunkSize);
						//in case nothing has been read we need to leave the loop
						//as we requested more than there was available
						if (numBytesRead == 0)
							break;
						//write the number of bytes read to the output stream
						outputStream.Write(FBuffer, 0, numBytesRead);
						//decrease the total amount of bytes we still need to read
						numBytesToCopy -= numBytesRead;
					}
					//this will force the changed flag of the output pin to be set
					FStreamOut.Flush(true);
				}
			}
			
		}
	}
}
