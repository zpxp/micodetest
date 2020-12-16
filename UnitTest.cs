using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace micodetest
{
	public class UnitTest1
	{
		async Task<int> DelayWithReturn(int msDelay, int rtn)
		{
			await Task.Delay(msDelay);
			return rtn;
		}

		[Fact]
		public async Task TestTimeout()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				await Task.Delay(5000).ConfigureAwait(false, cts.Token, TimeSpan.FromMilliseconds(2));
				// it should not get here
				Assert.Equal("Task cancelled", "Task was not cancelled");
			}
			catch (OperationCanceledException e)
			{
				// task should be cancelled
				Assert.NotNull(e.CancellationToken);
			}
		}

		[Fact]
		public async Task TestTimeout2()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				await Task.Delay(5).ConfigureAwait(false, cts.Token, TimeSpan.FromMilliseconds(2000));
				// it should get here
				Assert.True(true);
			}
			catch (OperationCanceledException)
			{
				// task should not be cancelled
				Assert.Equal("Task was cancelled", "Task should not cancelled");
			}
		}

		[Fact]
		public async Task TestCancel()
		{
			using var cts = new CancellationTokenSource(2);
			try
			{
				await Task.Delay(5000).ConfigureAwait(false, cts.Token, Timeout.InfiniteTimeSpan);
				// it should not get here
				Assert.Equal("Task cancelled", "Task was not cancelled");
			}
			catch (OperationCanceledException e)
			{
				// task should be cancelled
				Assert.NotNull(e.CancellationToken);
			}
		}

		[Fact]
		public async Task TestTimeoutInfinite()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				await Task.Delay(5).ConfigureAwait(false, cts.Token, Timeout.InfiniteTimeSpan);
				// it should get here
				Assert.True(true);
			}
			catch (OperationCanceledException)
			{
				// task should not be cancelled
				Assert.Equal("Task was cancelled", "Task should not cancelled");
			}
		}

		[Fact]
		public async Task TestTimeoutGeneric()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				var result = await DelayWithReturn(5000, 6).ConfigureAwait(false, cts.Token, TimeSpan.FromMilliseconds(2));
				// it should not get here
				Assert.Equal("Task cancelled", "Task was not cancelled");
			}
			catch (OperationCanceledException e)
			{
				// task should be cancelled
				Assert.NotNull(e.CancellationToken);
			}
		}

		[Fact]
		public async Task TestTimeoutGeneric2()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				var result = await DelayWithReturn(5, 6).ConfigureAwait(false, cts.Token, TimeSpan.FromMilliseconds(2000));
				// it should get here
				Assert.Equal(6, result);
			}
			catch (OperationCanceledException)
			{
				// task should not be cancelled
				Assert.Equal("Task was cancelled", "Task should not cancelled");
			}
		}

		[Fact]
		public async Task TestTimeoutGenericInfinite2()
		{
			using var cts = new CancellationTokenSource();
			try
			{
				var result = await DelayWithReturn(5, 6).ConfigureAwait(false, cts.Token, Timeout.InfiniteTimeSpan);
				// it should get here
				Assert.Equal(6, result);
			}
			catch (OperationCanceledException)
			{
				// task should not be cancelled
				Assert.Equal("Task was cancelled", "Task should not cancelled");
			}
		}
	}
}
