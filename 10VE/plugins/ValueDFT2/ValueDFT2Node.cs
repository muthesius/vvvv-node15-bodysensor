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
	[PluginInfo(Name = "DFT2", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueDFT2Node : IPluginEvaluate
	{
		#region fields & pins 
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Output("DC")]
		public ISpread<double> FDC;
		
		[Output("Output")]
		public ISpread<double> FOutput;
		
		[Output("Peaks")]
		public ISpread<double> FPeaks;

		[Output("Hz")]
		public ISpread<double> FHz;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			FPeaks.SliceCount = 0;

	        int n = FInput.SliceCount;
	        int m = n / 2;// I use m = n / 2d;
	        double[] real = new double[n];
	        double[] imag = new double[n];
	       // double[] result = new double[m];
			FOutput.SliceCount = m;
			
			double mx = 0;
			double b = 0;
			
	        double pi_div = 2.0 * Math.PI / n;
	        for (int w = 0; w < m; w++)
	        {
	            double a = w * pi_div;
	            for (int t = 0; t < n; t++)
	            {
	                real[w] += FInput[t] * Math.Cos(a * t);
	                imag[w] += FInput[t] * Math.Sin(a * t);
	            }
	        	
	        	double r = Math.Sqrt(real[w] * real[w] + imag[w] * imag[w]) / n;
	        	
	        	//bin value
	            FOutput[w] = r; 
	        	//max bin
	        	if((w>0) && (Math.Abs(r -mx)>0.01)) 
	        	{
	        		mx = r;
	        		FPeaks.Add(w-1);
	        		b = (b + (w-1)) / 2;
	        	}
	        }
			
			FDC[0] = FOutput[0];
			FOutput.RemoveAt(0);
			FPeaks.RemoveAt(0);
			
			//(Bin*SampleRate)/Resolution
			//FPeaks
			FHz[0] = b * (FInput.SliceCount*2) / FInput.SliceCount;
			
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
