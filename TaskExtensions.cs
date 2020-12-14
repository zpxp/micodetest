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
			var cts = new CancellationTokenSource();
			if (timeout != Timeout.InfiniteTimeSpan)
			{
				cts.CancelAfter(timeout);
			}
			// merge our token with the arg token that will be cancelled if either of the other 2 tokens get cancelled
			var link = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
			var cancelableTask = Task.Factory.StartNew(
				async (_t) =>
				{
					var args = (object[])_t;
					var task = (Task)args[0];
					using var link = (CancellationTokenSource)args[1];
					// we pass in cts so we can dispose it after the async operation
					// we dont actually need cts for anythign here
					using var cts = (CancellationTokenSource)args[2];

					var tcs = new TaskCompletionSource<bool>();
					using (link.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
					{
						if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
						{
							throw new TaskCanceledException("Timeout");
						}
						await task; // now we wait on the operation
					}
				},
				// we pass in vars as state to prevent closure (more speed)
				new object[] { task, link, cts },
				// pass in our token with timeout 
				link.Token,
				TaskCreationOptions.DenyChildAttach,
				TaskScheduler.Default)
				// it returns Task<Task> so call this to flatten it
				.Unwrap();

			return cancelableTask.ConfigureAwait(continueOnCapturedContext);
		}

		public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext, CancellationToken token, TimeSpan timeout)
		{
			var cts = new CancellationTokenSource();
			if (timeout != Timeout.InfiniteTimeSpan)
			{
				cts.CancelAfter(timeout);
			}
			// merge our token with the arg token that will be cancelled if either of the other 2 tokens get cancelled
			var link = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);
			var cancelableTask = Task.Factory.StartNew(
				async (_t) =>
				{
					var args = (object[])_t;
					var task = (Task<TResult>)args[0];
					using var link = (CancellationTokenSource)args[1];
					// we pass in cts so we can dispose it after the async operation
					// we dont actually need cts for anythign here
					using var cts = (CancellationTokenSource)args[2];

					var tcs = new TaskCompletionSource<bool>();
					using (link.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
					{
						if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
						{
							throw new TaskCanceledException("Timeout");
						}
						return await task; // now we wait on the operation
					}
				},
				// we pass in vars as state to prevent closure (more speed)
				new object[] { task, link, cts },
				// pass in our token with timeout 
				link.Token,
				TaskCreationOptions.DenyChildAttach,
				TaskScheduler.Default)
				// it returns Task<Task> so call this to flatten it
				.Unwrap();

			return cancelableTask.ConfigureAwait(continueOnCapturedContext);
		}
	}
}
