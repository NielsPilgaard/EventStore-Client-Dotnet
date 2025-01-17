using System.Threading.Tasks;
using Xunit;

namespace EventStore.Client.SubscriptionToStream {
	public class can_create_duplicate_name_on_different_streams
		: IClassFixture<can_create_duplicate_name_on_different_streams.Fixture> {
		public can_create_duplicate_name_on_different_streams(Fixture fixture) {
			_fixture = fixture;
		}

		private const string Stream =
			nameof(can_create_duplicate_name_on_different_streams);

		private readonly Fixture _fixture;

		public class Fixture : EventStoreClientFixture {
			protected override Task Given() => Task.CompletedTask;

			protected override Task When() =>
				Client.CreateToStreamAsync(Stream, "group3211",
					new PersistentSubscriptionSettings(), userCredentials: TestCredentials.Root);
		}

		[Fact]
		public Task the_completion_succeeds() =>
			_fixture.Client.CreateToStreamAsync("someother" + Stream,
				"group3211", new PersistentSubscriptionSettings(), userCredentials: TestCredentials.Root);
	}
}
