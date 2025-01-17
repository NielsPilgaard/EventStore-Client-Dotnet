using System.Threading.Tasks;
using Xunit;

namespace EventStore.Client.SubscriptionToStream {
	public class update_non_existent
		: IClassFixture<update_non_existent.Fixture> {
		private const string Stream = nameof(update_non_existent);
		private const string Group = "nonexistent";
		private readonly Fixture _fixture;

		public update_non_existent(Fixture fixture) {
			_fixture = fixture;
		}

		[Regression.Fact(21, "20.x returns the wrong exception")]
		public async Task the_completion_fails_with_not_found() {
			await Assert.ThrowsAsync<PersistentSubscriptionNotFoundException>(
				() => _fixture.Client.UpdateToStreamAsync(Stream, Group,
					new PersistentSubscriptionSettings(), userCredentials: TestCredentials.Root));
		}

		public class Fixture : EventStoreClientFixture {
			protected override Task Given() => Task.CompletedTask;
			protected override Task When() => Task.CompletedTask;
		}
	}
}
