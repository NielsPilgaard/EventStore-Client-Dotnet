using System.Threading.Tasks;
using Xunit;

namespace EventStore.Client.Security {
	public class multiple_role_security : IClassFixture<multiple_role_security.Fixture> {
		private readonly Fixture _fixture;

		public multiple_role_security(Fixture fixture) {
			_fixture = fixture;
		}

		[Fact]
		public async Task multiple_roles_are_handled_correctly() {
			await Assert.ThrowsAsync<AccessDeniedException>(() => _fixture.ReadEvent("usr-stream"));
			await Assert.ThrowsAsync<StreamNotFoundException>(() => _fixture.ReadEvent("usr-stream", userCredentials: TestCredentials.TestUser1));
			await Assert.ThrowsAsync<StreamNotFoundException>(() => _fixture.ReadEvent("usr-stream", TestCredentials.TestUser2));
			await Assert.ThrowsAsync<StreamNotFoundException>(() => _fixture.ReadEvent("usr-stream", userCredentials: TestCredentials.TestAdmin));

			await Assert.ThrowsAsync<AccessDeniedException>(() => _fixture.AppendStream("usr-stream"));
			await _fixture.AppendStream("usr-stream", userCredentials: TestCredentials.TestUser1);
			await Assert.ThrowsAsync<AccessDeniedException>(() => _fixture.AppendStream("usr-stream", TestCredentials.TestUser2));
			await _fixture.AppendStream("usr-stream", userCredentials: TestCredentials.TestAdmin);

			await _fixture.DeleteStream("usr-stream1");
			await _fixture.DeleteStream("usr-stream2", userCredentials: TestCredentials.TestUser1);
			await _fixture.DeleteStream("usr-stream3", TestCredentials.TestUser2);
			await _fixture.DeleteStream("usr-stream4", userCredentials: TestCredentials.TestAdmin);
		}


		public class Fixture : SecurityFixture {
			protected override Task When() {
				var settings = new SystemSettings(
					new StreamAcl(new[] {"user1", "user2"}, new[] {"$admins", "user1"},
						new[] {"user1", SystemRoles.All}));
				return Client.SetSystemSettingsAsync(settings, userCredentials: TestCredentials.TestAdmin);
			}
		}
	}
}
