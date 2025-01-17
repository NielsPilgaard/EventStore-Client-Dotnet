using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EventStore.Client {
	public class creating_a_user : IClassFixture<creating_a_user.Fixture> {
		private readonly Fixture _fixture;

		public creating_a_user(Fixture fixture) {
			_fixture = fixture;
		}

		public static IEnumerable<object?[]> NullInputCases() {
			var loginName = "ouro";
			var fullName = "greg";
			var groups = new[] { "foo", "bar" };
			var password = "foofoofoo";

			yield return new object?[] { null, fullName, groups, password, nameof(loginName) };
			yield return new object?[] { loginName, null, groups, password, nameof(fullName) };
			yield return new object?[] { loginName, fullName, null, password, nameof(groups) };
			yield return new object?[] { loginName, fullName, groups, null, nameof(password) };
		}

		[Theory, MemberData(nameof(NullInputCases))]
		public async Task with_null_input_throws(string loginName, string fullName, string[] groups, string password,
			string paramName) {
			var ex = await Assert.ThrowsAsync<ArgumentNullException>(
				() => _fixture.Client.CreateUserAsync(loginName, fullName, groups, password,
					userCredentials: TestCredentials.Root));
			Assert.Equal(paramName, ex.ParamName);
		}

		public static IEnumerable<object?[]> EmptyInputCases() {
			var loginName = "ouro";
			var fullName = "greg";
			var groups = new[] { "foo", "bar" };
			var password = "foofoofoo";

			yield return new object?[] { string.Empty, fullName, groups, password, nameof(loginName) };
			yield return new object?[] { loginName, string.Empty, groups, password, nameof(fullName) };
			yield return new object?[] { loginName, fullName, groups, string.Empty, nameof(password) };
		}

		[Theory, MemberData(nameof(EmptyInputCases))]
		public async Task with_empty_input_throws(string loginName, string fullName, string[] groups, string password,
			string paramName) {
			var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
				() => _fixture.Client.CreateUserAsync(loginName, fullName, groups, password,
					userCredentials: TestCredentials.Root));
			Assert.Equal(paramName, ex.ParamName);
		}

		[Theory, ClassData(typeof(InvalidCredentialsCases))]
		public async Task with_user_with_insufficient_credentials_throws(string loginName,
			UserCredentials userCredentials) {
			if (userCredentials == null)
				await Assert.ThrowsAsync<AccessDeniedException>(
					() => _fixture.Client.CreateUserAsync(loginName, "Full Name", new[] { "foo", "bar" },
						"password"));
			else
				await Assert.ThrowsAsync<NotAuthenticatedException>(
					() => _fixture.Client.CreateUserAsync(loginName, "Full Name", new[] { "foo", "bar" },
						"password", userCredentials: userCredentials));
		}

		[Fact]
		public async Task can_be_read() {
			var loginName = Guid.NewGuid().ToString();
			await _fixture.Client.CreateUserAsync(loginName, "Full Name", new[] { "foo", "bar" }, "password",
				userCredentials: TestCredentials.Root);

			var details = await _fixture.Client.GetUserAsync(loginName, userCredentials: TestCredentials.Root);

			Assert.Equal(new UserDetails(loginName, "Full Name", new[] { "foo", "bar" }, false, details.DateLastUpdated),
				details);
		}

		public class Fixture : EventStoreClientFixture {
			protected override Task Given() => Task.CompletedTask;
			protected override Task When() => Task.CompletedTask;
		}
	}
}
