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
	[PluginInfo(Name = "Heartbeat", Category = "Value", Help = "pulse tracker", Tags = "pulse tracker", AutoEvaluate = true)]
	#endregion PluginInfo
	public class ValueHeartbeatNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public IDiffSpread<double> FInput;

		[Input("Reset", IsBang = true, IsSingle = true)]
		public IDiffSpread<bool> FReset;
		
		[Output("Output")]
		public ISpread<bool> FOutput;
		
		[Output("BPM")]
		public ISpread<int> FBPM;
		
		[Output("IBI")]
		public ISpread<double> FIBI;
		
		[Output("Peak")]
		public ISpread<bool> FPeak;
		
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		// these variables are volatile because they are used during the interrupt service routine!
		int BPM;                   		// used to hold the pulse rate
	 	double Signal;               	// holds the incoming raw data
		ulong IBI = 600;            	// holds the time between beats, the Inter-Beat Interval
		bool Pulse = false;     		// true when pulse wave is high, false when it's low
		bool QS = false;        		// becomes true when finds a beat. 
				
	 	ulong[] rate = new ulong[10];   // used to hold last ten IBI values
		ulong sampleCounter = 0;        // used to determine pulse timing, ulong
		ulong lastBeatTime = 0;         // used to find the inter beat interval, ulong
		double P =0.5;                  // used to find peak in pulse wave
		double T = 0.5;                 // used to find trough in pulse wave
		double thresh = 0.5;            // used to find instant moment of heart beat
		double amp = 0.1;               // used to hold amplitude of pulse waveform
		bool firstBeat = true;        	// used to seed rate array so we startup with reasonable BPM
		bool secondBeat = true;       	// used to seed rate array so we startup with reasonable BPM 
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			FIBI.SliceCount = SpreadMax;
			FBPM.SliceCount = SpreadMax;
			FPeak.SliceCount = SpreadMax;
			
			if(FReset.IsChanged && FReset[0])
			{
				thresh = 0.5;                          // set thresh default
				P = 0.5;                               // set P default
				T = 0.5;                               // set T default
				amp = 0.1;
				lastBeatTime = sampleCounter;          // bring the lastBeatTime up to date        
				firstBeat = true;                      // set these to avoid noise
				secondBeat = true;
			}
			
			if(FInput.IsChanged) 
			{
				for (int i = 0; i < SpreadMax; i++) {
				
					sampleCounter += 167;                         // keep track of the time in mS with this variable
	    			Signal = FInput[i];
					ulong N = ( sampleCounter - lastBeatTime);       // monitor the time since the last beat to avoid noise
					FIBI[i] = IBI;
					
					//FLogger.Log(LogType.Debug, "N:" + N.ToString());
					
					//  find the peak and trough of the pulse wave
	    			if( (Signal < thresh) && (N > (IBI/5)*3))		// avoid dichrotic noise by waiting 3/5 of last IBI
					{       
	        			if (Signal < T){                        // T is the trough
	            			T = Signal;                         // keep track of lowest point in pulse wave 
	         			}
	       			}
	      
	    			if(Signal > thresh && Signal > P)          // thresh condition helps avoid noise
	        		{
						P = Signal;                             // P is the peak
	        			//FPeak[i] = true;
	       			}                                        // keep track of highest point in pulse wave
	    
					
					
	  				//  NOW IT'S TIME TO LOOK FOR THE HEART BEAT
	  				// signal surges up in value every time there is a pulse
					if (N > 250)                                   // avoid high frequency noise
	  				{
	  					
	  					if((Signal> thresh) && (Pulse == false)) {
	  					
	  						FPeak[i] = true;
	  					}
	  					
	  				
						if ( (Signal > thresh) && (Pulse == false) && (N > (IBI/5)*3) )
	  					{
	    					Pulse = true;                               	// set the Pulse flag when we think there is a pulse
	    					
	    					FOutput[i] = true;
	  						IBI = (sampleCounter - lastBeatTime);      		// measure time between beats in mS
	    					lastBeatTime = sampleCounter;               	// keep track of time for next pulse
	         
	         				if(firstBeat){                         			// if it's the first time we found a beat, if firstBeat == TRUE
	             				firstBeat = false;                 			// clear firstBeat flag
	             				//return;                            			// IBI value is unreliable so discard it
	            				//FPeak[i] = true;
	         					
	         				}
	         				else 
	  						{
	  					
		  						if(secondBeat){                        			// if this is the second beat, if secondBeat == TRUE
		            				secondBeat = false;                 		// clear secondBeat flag
		               				for(int j=0; j<=9; j++){         			// seed the running total to get a realisitic BPM at startup
		                    			rate[j] = IBI;                      
		                    		}
		  							FPeak[i] = true;
		            			}
		  						
		  						// keep a running total of the last 10 IBI values
		    					ulong runningTotal = 0;                   		// clear the runningTotal variable, word    
		
							    for(int j=0; j<=8; j++){                		// shift data in the rate array
							          rate[j] = rate[j+1];              		// and drop the oldest IBI value 
							          runningTotal += (rate[j]);        // add up the 9 oldest IBI values
							        }
							        
							    rate[9] = IBI;                          		// add the latest IBI to the rate array
							    runningTotal += (rate[9]);              // add the latest IBI to runningTotal
							    runningTotal /= 10;                     		// average the last 10 IBI values 
							    BPM = (int)(60000/runningTotal);               		// how many beats can fit into a minute? that's BPM!
							    QS = true;                              		// set Quantified Self flag 
	  							FBPM[i] = BPM;
							    // QS FLAG IS NOT CLEARED INSIDE THIS ISR
	  							
	  						}
						}                       
					}
	
	  				if (Signal < thresh && Pulse == true)	   // when the values are going down, the beat is over
					{     
				      	FOutput[i] = false;
						Pulse = false;                         // reset the Pulse flag so we can do it again
				      	amp = P - T;                           // get amplitude of the pulse wave
				      	thresh = amp/2 + T;                    // set thresh at 50% of the amplitude
				      	P = thresh;                            // reset these for next time
				      	T = thresh;
						FPeak[i] = false;
				    }
	  
				  	if (N > 2500){                             // if 2.5 seconds go by without a beat
				      thresh = 0.5;                          // set thresh default
				      P = 0.5;                               // set P default
				      T = 0.5;                               // set T default
				      lastBeatTime = sampleCounter;          // bring the lastBeatTime up to date        
				      firstBeat = true;                      // set these to avoid noise
				      secondBeat = true;                     // when we get the heartbeat back
				  	  FPeak[i] = false;
				    }
					
				}
				
			}
			
			if (QS) QS = false;
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
