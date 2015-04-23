#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using KalmanFilter;
#endregion usings

namespace VVVV.Nodes
{
	
	#region PluginInfo
	[PluginInfo(Name = "Kalman", AutoEvaluate = true, Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueKalmanNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0, Order = -1)]
		public IDiffSpread<double> FInput;
		
		
		[Input("Pn", IsSingle = true, DefaultValue = 2.0)]
		public IDiffSpread<double> FPN;
		
		[Input("Mn", IsSingle = true, DefaultValue = 1.0)]
		public IDiffSpread<double> FMN;
		
		// Measurement to position state minimal variance.
		[Input("Qx", IsSingle = true, DefaultValue = 0.100)]
		public IDiffSpread<double> FQX;
		
		// Measurement to velocity state minimal variance.
		[Input("Qv", IsSingle = true, DefaultValue = 0.100)]
		public IDiffSpread<double> FQV;
		
		// Measurement covariance (sets minimal gain).
		[Input("R", IsSingle = true, DefaultValue = 0.100)]
		public IDiffSpread<double> FR;
		
		// Initial variance.
		[Input("Pd", IsSingle = true, DefaultValue = 400.0)]
		public IDiffSpread<double> FPD;
		
		[Input("Dt", IsSingle = true, DefaultValue = 1.0)]
		public IDiffSpread<double> FDT;
		
		//----
		
		[Input("Reset", IsBang = true, IsSingle = true, DefaultValue =0, Order = 256)]
		public IDiffSpread<bool> FReset;
		

		[Output("Output")]
		public ISpread<double> FOutput;
		
		[Output("Velocity")]
		public ISpread<double> FVelocity;
		
		[Output("Gain")]
		public ISpread<double> FGain;

		[Import()]
		public ILogger FLogger;
		
		Kalman1D k = new Kalman1D();
		
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if ( (FReset.IsChanged && FReset[0]) || (FPN.IsChanged || FMN.IsChanged || FR.IsChanged || FPD.IsChanged) )
			{
				k.Reset(FPN[0], FMN[0], FR[0], FPD[0], FInput[0]);
				FLogger.Log(LogType.Debug, "Kalman Reset");
			}
			
				
			FOutput.SliceCount = SpreadMax;
			FVelocity.SliceCount = SpreadMax;
			FGain.SliceCount = SpreadMax;
	
			if(FInput.IsChanged) 
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					FOutput[i] = k.Update(FInput[i], FDT[i]);
	                //FOutput[i + 1] = FOutput[i];
	                FVelocity[i] = k.Velocity;
	                FGain[i] = k.LastGain;
				}
			
			}
			else
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					FOutput[i] = k.Predicition(FDT[i]);
	                //FOutput[i + 1] = FOutput[i];
	                FVelocity[i] = k.Velocity;
	                FGain[i] = k.LastGain;
				}
			}
			//
		}
	}
}
