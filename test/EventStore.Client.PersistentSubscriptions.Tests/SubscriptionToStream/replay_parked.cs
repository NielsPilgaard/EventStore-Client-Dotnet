using System.Threading.Tasks;
using Xunit;

namespace EventStore.Client.SubscriptionToStream {
	public class replay_parked : IClassFixture<replay_parked.Fixture> {
		private readonly Fixture _fixture;
		private const string GroupName = nameof(replay_parked);
		private const string StreamName = nameof(replay_parked);

		public replay_parked(Fixture fixture) {
			_fixture = fixture;
		}
		
		[Fact]
		public async Task does_not_throw() {
			await _fixture.Client.ReplayParkedMessagesToStreamAsync(
				StreamName,
				GroupName,
				userCredentials: TestCredentials.Root);
			
			await _fixture.Client.ReplayParkedMessagesToStreamAsync(
				StreamName,
				GroupName,
				stopAt: 100,
				userCredentials: TestCredentials.Root);
		}

		[Fact]
		public async Task throws_when_given_non_existing_subscription() {
			await Assert.ThrowsAsync<PersistentSubscriptionNotFoundException>(() => 
				_fixture.Client.ReplayParkedMessagesToStreamAsync(
					streamName: "NonExisting",
					groupName: "NonExisting",
					userCredentials: TestCredentials.Root));
		}
		
		[Fact]
		public async Task throws_with_no_credentials() {
			await Assert.ThrowsAsync<AccessDeniedException>(() =>
				_fixture.Client.ReplayParkedMessagesToStreamAsync(StreamName, GroupName));
		}

		[Fact(Skip = "Unable to produce same behavior with HTTP fallback!")]
        public async Task throws_with_non_existing_user() {
        	await Assert.ThrowsAsync<NotAuthenticatedException>(() =>
        		_fixture.Client.ReplayParkedMessagesToStreamAsync(
        			StreamName,
        			GroupName,
        			userCredentials: TestCredentials.TestBadUser));
        }
		
		[Fact]
		public async Task throws_with_normal_user_credentials() {
			await Assert.ThrowsAsync<AccessDeniedException>(() =>
				_fixture.Client.ReplayParkedMessagesToStreamAsync(
					StreamName,
					GroupName,
					userCredentials: TestCredentials.TestUser1));
		}
		
		public class Fixture : EventStoreClientFixture {
			protected override Task Given() =>
				Client.CreateToStreamAsync(
					StreamName,
					GroupName,
					new PersistentSubscriptionSettings(),
					userCredentials: TestCredentials.Root);
			protected override Task When() => Task.CompletedTask;
		}
	}
}
