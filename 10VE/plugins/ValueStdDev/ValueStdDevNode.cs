#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Statistics", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueStdDevNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Output("Min")]
		public ISpread<double> FMin;
		
		[Output("Max")]
		public ISpread<double> FMax;
		
		[Output("Mean")]
		public ISpread<double> FMean;
		
		[Output("Variance")]
		public ISpread<double> FVariance;
		
		[Output("StdDev")]
		public ISpread<double> FStdDev;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{			
			//FMin.SliceCount = 1;
			//FMax.SliceCount = 1;
			//FMean.SliceCount = 1;
			//FStdDev.SliceCount = 1;
			
			List<double> numbers = FInput.ToList();
			FMin[0] = numbers.Min();
			FMax[0] = numbers.Max();
			
			//calculate mean
			double mean = 0;
			for(int i = 0; i < FInput.SliceCount; i++) mean += FInput[i];
			mean = mean / FInput.SliceCount;
			FMean[0] = mean;
			
			//calculate variance
            double variance = numbers.Sum(number => Math.Pow(number - mean, 2.0));
            variance =  variance / numbers.Count;
			FVariance[0] = variance;
			
			FStdDev[0] = Math.Sqrt(variance);
			
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
