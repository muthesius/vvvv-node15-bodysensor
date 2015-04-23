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
	[PluginInfo(Name = "XCorr", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueXCorrNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input 1", DefaultValue = 1.0)]
		public ISpread<double> FInput1;

		[Input("Input 2", DefaultValue = 1.0)]
		public ISpread<double> FInput2;
		
		[Output("Output")]
		public ISpread<double> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 1;

    		double[] array_xy = new double[FInput1.SliceCount];
    		double[] array_xp2 = new double[FInput1.SliceCount];
    		double[] array_yp2 = new double[FInput1.SliceCount];
			
		    for (int i = 0; i < FInput1.SliceCount; i++)
		        array_xy[i] = FInput1[i] * FInput2[i];
			
		    for (int i = 0; i < FInput1.SliceCount; i++)
		        array_xp2[i] = Math.Pow(FInput1[i], 2.0);
			
		    for (int i = 0; i < FInput1.SliceCount; i++)
		        array_yp2[i] = Math.Pow(FInput2[i], 2.0);
			
		    double sum_x = 0;
		    double sum_y = 0;
		    foreach (double n in FInput1) sum_x += n;
		    foreach (double n in FInput2) sum_y += n;
			
		    double sum_xy = 0;
		    foreach (double n in array_xy) sum_xy += n;
		   
			double sum_xpow2 = 0;
		    foreach (double n in array_xp2) sum_xpow2 += n;
		    double sum_ypow2 = 0;
		   
			foreach (double n in array_yp2) sum_ypow2 += n;
		   
			double Ex2 = Math.Pow(sum_x, 2.00);
		    double Ey2 = Math.Pow(sum_y, 2.00);
		
		    FOutput[0] = (FInput1.SliceCount * sum_xy - sum_x * sum_y) / Math.Sqrt((FInput1.SliceCount * sum_xpow2 - Ex2) * (FInput1.SliceCount * sum_ypow2 - Ey2));
		
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
