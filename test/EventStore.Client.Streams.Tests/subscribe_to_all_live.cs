using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EventStore.Client {
	[Trait("Category", "LongRunning")]
	public class subscribe_to_all_live : IAsyncLifetime {
		private readonly Fixture _fixture;

		/// <summary>
		/// This class does not implement IClassFixture because it checks $all, and we want a fresh Node for each test.
		/// </summary>
		public subscribe_to_all_live(ITestOutputHelper outputHelper) {
			_fixture = new Fixture();
			_fixture.CaptureLogs(outputHelper);
		}

		[Fact]
		public async Task calls_subscription_dropped_when_disposed() {
			var dropped = new TaskCompletionSource<(SubscriptionDroppedReason, Exception?)>();

			using var subscription = await _fixture.Client
				.SubscribeToAllAsync(FromAll.End, EventAppeared, false, SubscriptionDropped)
				.WithTimeout();

			if (dropped.Task.IsCompleted) {
				Assert.False(dropped.Task.IsCompleted, dropped.Task.Result.ToString());
			}

			subscription.Dispose();

			var (reason, ex) = await dropped.Task.WithTimeout();

			Assert.Equal(SubscriptionDroppedReason.Disposed, reason);
			Assert.Null(ex);

			Task EventAppeared(StreamSubscription s, ResolvedEvent e, CancellationToken ct) => Task.CompletedTask;

			void SubscriptionDropped(StreamSubscription s, SubscriptionDroppedReason reason, Exception? ex) =>
				dropped.SetResult((reason, ex));
		}

		[Fact]
		public async Task calls_subscription_dropped_when_error_processing_event() {
			var stream = _fixture.GetStreamName();
			var dropped = new TaskCompletionSource<(SubscriptionDroppedReason, Exception?)>();
			var expectedException = new Exception("Error");

			using var subscription = await _fixture.Client
				.SubscribeToAllAsync(FromAll.End, EventAppeared, false, SubscriptionDropped)
				.WithTimeout();

			await _fixture.Client.AppendToStreamAsync(stream, StreamState.NoStream, _fixture.CreateTestEvents());

			var (reason, ex) = await dropped.Task.WithTimeout();

			Assert.Equal(SubscriptionDroppedReason.SubscriberError, reason);
			Assert.Same(expectedException, ex);

			Task EventAppeared(StreamSubscription s, ResolvedEvent e, CancellationToken ct) =>
				Task.FromException(expectedException);

			void SubscriptionDropped(StreamSubscription s, SubscriptionDroppedReason reason, Exception? ex) =>
				dropped.SetResult((reason, ex));
		}

		[Fact]
		public async Task subscribe_to_empty_database() {
			var appeared = new TaskCompletionSource<bool>();
			var dropped = new TaskCompletionSource<(SubscriptionDroppedReason, Exception?)>();

			using var subscription = await _fixture.Client
				.SubscribeToAllAsync(FromAll.End, EventAppeared, false, SubscriptionDropped)
				.WithTimeout();

			Assert.False(appeared.Task.IsCompleted);

			if (dropped.Task.IsCompleted) {
				Assert.False(dropped.Task.IsCompleted, dropped.Task.Result.ToString());
			}

			subscription.Dispose();

			var (reason, ex) = await dropped.Task.WithTimeout();

			Assert.Equal(SubscriptionDroppedReason.Disposed, reason);
			Assert.Null(ex);

			Task EventAppeared(StreamSubscription s, ResolvedEvent e, CancellationToken ct) {
				if (!SystemStreams.IsSystemStream(e.OriginalStreamId)) {
					appeared.TrySetResult(true);
				}

				return Task.CompletedTask;
			}

			void SubscriptionDropped(StreamSubscription s, SubscriptionDroppedReason reason, Exception? ex) =>
				dropped.SetResult((reason, ex));
		}

		[Fact]
		public async Task does_not_read_existing_events_but_keep_listening_to_new_ones() {
			var appeared = new TaskCompletionSource<bool>();
			var dropped = new TaskCompletionSource<(SubscriptionDroppedReason, Exception?)>();
			var appearedEvents = new List<EventRecord>();
			var afterEvents = _fixture.CreateTestEvents(10).ToArray();

			using var subscription = await _fixture.Client
				.SubscribeToAllAsync(FromAll.End, EventAppeared, false, SubscriptionDropped)
				.WithTimeout();

			foreach (var @event in afterEvents) {
				await _fixture.Client.AppendToStreamAsync($"stream-{@event.EventId:n}", StreamState.NoStream,
					new[] {@event});
			}

			await appeared.Task.WithTimeout();

			Assert.Equal(afterEvents.Select(x => x.EventId), appearedEvents.Select(x => x.EventId));

			if (dropped.Task.IsCompleted) {
				Assert.False(dropped.Task.IsCompleted, dropped.Task.Result.ToString());
			}

			subscription.Dispose();

			var (reason, ex) = await dropped.Task.WithTimeout();

			Assert.Equal(SubscriptionDroppedReason.Disposed, reason);
			Assert.Null(ex);

			Task EventAppeared(StreamSubscription s, ResolvedEvent e, CancellationToken ct) {
				if (!SystemStreams.IsSystemStream(e.OriginalStreamId)) {
					appearedEvents.Add(e.Event);

					if (appearedEvents.Count >= afterEvents.Length) {
						appeared.TrySetResult(true);
					}
				}

				return Task.CompletedTask;
			}

			void SubscriptionDropped(StreamSubscription s, SubscriptionDroppedReason reason, Exception? ex) =>
				dropped.SetResult((reason, ex));
		}

		public class Fixture : EventStoreClientFixture {
			protected override Task Given() =>
				Client.SetStreamMetadataAsync(SystemStreams.AllStream, StreamState.NoStream,
					new StreamMetadata(acl: new StreamAcl(SystemRoles.All)), userCredentials: TestCredentials.Root);

			protected override Task When() => Task.CompletedTask;
		}

		public Task InitializeAsync() => _fixture.InitializeAsync();
		public Task DisposeAsync() => _fixture.DisposeAsync();
	}
}
