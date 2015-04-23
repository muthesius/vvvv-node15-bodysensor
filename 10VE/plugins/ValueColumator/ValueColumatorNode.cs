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
 	[PluginInfo(Name = "Columator", Category = "Value", Help = "Basic template with one value in/out", Tags = "", AutoEvaluate=true)]
	#endregion PluginInfo
	public class ValueColumatorNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 0)]
		public ISpread<double> FInput;
		
		[Input("VectorSize", DefaultValue = 8)]
		public ISpread<int> FVectorSize;
		
		[Input("Reset", IsBang = true)]
		public IDiffSpread<bool> FReset;
		
		[Output("Output1")]
		public ISpread<double> FOutput1;
		
		[Output("FOutput1Changed", IsSingle = true)]
		public ISpread<bool>FOutput1Changed;
		
		[Output("Output2")]
		public ISpread<double> FOutput2;
		
		[Output("FOutput2Changed", IsSingle = true)]
		public ISpread<bool>FOutput2Changed;


		[Import()]
		public ILogger FLogger;
		#endregion fields & pins     

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			int sz = 8; //FVectorSize[0];
			bool o1 = false;
			bool o2 = false;
			
			
			FOutput1.SliceCount = sz;
			FOutput2.SliceCount = sz;
			
			
			for (int i = 0; i < FInput.SliceCount; i = i + sz)
			{
				if(FInput[i] == 1) {
					o1 = true;
					for (int j = 0; j<sz; j++) {
						if(j == 1) {
							
							if(FInput[i+j] > 0)
								FOutput1[j] = 1;
							else
								FOutput1[j] = FInput[i+j];
							
						}
						else 
				 			FOutput1[j] = (FOutput1[j] + FInput[i+j]) / 2;
						
					}
				 	FOutput1[0] = FInput[i];
					
				}
				else if(FInput[i] == 2) 
				{
				 	o2 = true;
					for (int j = 0; j<sz; j++) {
						if(j == 1) {
							if(FInput[i+j] > 0)
								FOutput2[j] = 1;
							else
								FOutput2[j] = FInput[i+j]; //Convert.ToDouble( (FOutput2[j]>0) || (FInput[i+j]>0));
							
						}
						else
				 			FOutput2[j] = (FOutput2[j] + FInput[i+j]) / 2;
					}
				 	FOutput2[0] = FInput[i];
				 }
			}
			
			FOutput1Changed[0] = o1;
			FOutput2Changed[0] = o2;
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
