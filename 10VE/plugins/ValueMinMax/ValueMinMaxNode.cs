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
	[PluginInfo(Name = "MinMax", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueMinMaxNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Reset", IsBang = true, IsSingle = true)]
		public IDiffSpread<bool> FReset;

		[Output("Min")]
		public ISpread<double> FMin;
		
		[Output("Max")]
		public ISpread<double> FMax;

		
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			
			
			FMin.SliceCount = SpreadMax;
			FMax.SliceCount = SpreadMax;
			
			
			
			for (int i = 0; i < SpreadMax; i++) {
				if(FReset[0]) {
					FMin[i] = FInput[i];
					FMax[i] = FInput[i];
				}
				FMax[i] = Math.Max(FInput[i], FMax[i]);
				FMin[i] = Math.Min(FInput[i], FMin[i]);
			}
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
