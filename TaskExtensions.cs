using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace micodetest
{
	public static class TaskExtensions
	{
		public static ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext, CancellationToken token, TimeSpan timeout)
		{
			var cts = new CancellationTokenSource(timeout);
			// merge our token with the arg token that will be cancelled if either of the other 2 tokens get cancelled
			var link = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

			return WithCancellation(link, cts, task).ConfigureAwait(continueOnCapturedContext);
		}

		public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext, CancellationToken token, TimeSpan timeout)
		{
			var cts = new CancellationTokenSource(timeout);
			// merge our token with the arg token that will be cancelled if either of the other 2 tokens get cancelled
			var link = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

			return WithCancellation(link, cts, task).ConfigureAwait(continueOnCapturedContext);
		}


		private static async Task WithCancellation(CancellationTokenSource link, CancellationTokenSource cts, Task task)
		{
			// we need to dispose these 2 cts after the operation completes
			using (cts)
			using (link)
			{
				var tcs = new TaskCompletionSource<bool>();
				using (link.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
				{
					// if the given task completes then this function will exit
					// otherwise if the tcs task completes then it was cancelled and we throw
					if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
					{
						throw new OperationCanceledException(link.Token);
					}
				}
			}
		}


		// there isnt any simple way to reuse this WithCancellation for both the generic and non generic variants without 
		// unnecessary boxing or creating a proxy task and waiting on that. so we have a generic and non generic impl
		private static async Task<T> WithCancellation<T>(CancellationTokenSource link, CancellationTokenSource cts, Task<T> task)
		{
			// we need to dispose these 2 cts after the operation completes
			using (cts)
			using (link)
			{
				var tcs = new TaskCompletionSource<bool>();
				using (link.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
				{
					if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
					{
						throw new OperationCanceledException(link.Token);
					}
					return await task.ConfigureAwait(false); // return the result of the task
				}
			}
		}
	}
}
