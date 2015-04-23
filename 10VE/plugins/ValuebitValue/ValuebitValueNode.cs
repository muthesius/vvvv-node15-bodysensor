#region usings
using System;
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
	[PluginInfo(Name = "bitValue", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValuebitValueNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultString = "00000000")]
		public ISpread<string> FInput;

		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = FInput.SliceCount;
			int bitCount = 8;
			for(int i = 0; i< FInput.SliceCount; i++)
			{
				if (FInput[i].Length == bitCount && FInput[i].StartsWith(@"1") )
	            	FOutput[i] =  Convert.ToInt32(FInput[i].PadLeft(32, '1'),2);
	        	else
	            	FOutput[i] =  Convert.ToInt32(FInput[i],2);
			}
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
