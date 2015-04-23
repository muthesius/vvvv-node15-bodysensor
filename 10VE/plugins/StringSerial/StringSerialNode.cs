#region usings
using System;
using System.ComponentModel.Composition;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Serial", Category = "String", Help = "Basic template with one string in/out", Tags = "")]
	#endregion PluginInfo
	public class StringSerialNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("SerialPort", DefaultString = "COM1")]
		public ISpread<string> FPort;

		[Input("BaudRate", DefaultValue = 57600)]
		public ISpread<int> FBaudRate;
		
		[Input("Parity", DefaultEnumEntry = "None")]
		public ISpread<Parity> FParity;
		
		[Input("StopBits", DefaultEnumEntry = "One")]
		public ISpread<StopBits> FStopBits;
		
		[Input("DataBits", DefaultValue = 8)]
		public ISpread<int> FDataBits;
		
		[Input("Handshake", DefaultEnumEntry = "None")]
		public ISpread<Handshake> FHandshake;
		
		[Input("Enabled", DefaultBoolean = false)]
		public IDiffSpread<bool> FEnabled;
		
		
		[Output("Output")]
		public ISpread<string> FOutput;

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		//private
		SerialPort port = null;
		CancellationTokenSource cts = new CancellationTokenSource();
		Task task = null;
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = SpreadMax;
			
			if(FEnabled.IsChanged)
			{
				if((port != null) && (!FEnabled[0]))
				{
					cts.Cancel();
			        try
			        {
			            task.Wait();
			        }
			        catch (AggregateException ex)
			        {
			            Console.WriteLine(ex.InnerException.Message);
			        }
			
			        port.Close();
					port = null;
				} 
				else if ((port == null) && (FEnabled[0]))
				{
					port = new SerialPort(FPort[0]);
					port.Open();
				}
			}
			
			if(port != null)
			{
				if(port.IsOpen) {
					 var task = ReadPort(port, cts.Token);
				}
			}
			

			//FLogger.Log(LogType.Debug, "Logging to Renderer (TTY)");
		}
		
		public static class TaskExt
    	{
        	public static async Task<TEventArgs> FromEvent<TEventHandler, TEventArgs>(
	            Func<Action<TEventArgs>, Action, Action<Exception>, TEventHandler> getHandler,
	            Action<TEventHandler> subscribe,
	            Action<TEventHandler> unsubscribe,
	            Action<Action<TEventArgs>, Action, Action<Exception>> initiate,
	            CancellationToken token) where TEventHandler : class
	        {
	            var tcs = new TaskCompletionSource<TEventArgs>();
	
	            Action<TEventArgs> complete = (args) => tcs.TrySetResult(args);
	            Action cancel = () => tcs.TrySetCanceled();
	            Action<Exception> reject = (ex) => tcs.TrySetException(ex);
	
	            TEventHandler handler = getHandler(complete, cancel, reject);
	
	            subscribe(handler);
	            try
	            {
	                using (token.Register(() => tcs.TrySetCanceled()))
	                {
	                    initiate(complete, cancel, reject);
	                    return await tcs.Task;
	                }
	            }
	            finally
	            {
	                unsubscribe(handler);
	            }
	        }
		}
		
		//internal
		private static async Task ReadPort(SerialPort port, CancellationToken token)
    	{
	        while (true)
	        {
	            token.ThrowIfCancellationRequested();
	
	            await TaskExt.FromEvent<SerialDataReceivedEventHandler, SerialDataReceivedEventArgs>(
	                (complete, cancel, reject) => // get handler
	                    (sender, args) => complete(args),
	                handler => // subscribe
	                    port.DataReceived += handler,
	                handler => // unsubscribe
	                    port.DataReceived -= handler,
	                (complete, cancel, reject) => // start the operation
	                    { if (port.BytesToRead != 0) complete(null); },
	                token);
	
	            FLogger.Log(LogType.Debug,"Received: " + port.ReadExisting());
	        }
	    }
	}
}
