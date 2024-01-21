using System.Diagnostics;
using System.Threading.Tasks;

namespace FellOnline.Shared
{
	public static class FProcessExtensions
	{
		// Extension method for asynchronous process waiting
		public static Task WaitForExitAsync(this Process process)
		{
			var tcs = new TaskCompletionSource<object>();
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => tcs.SetResult(null);
			return tcs.Task;
		}
	}
}