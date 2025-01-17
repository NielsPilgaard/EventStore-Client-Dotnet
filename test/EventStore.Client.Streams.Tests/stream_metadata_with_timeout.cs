﻿using System;
using System.Threading.Tasks;
using Grpc.Core;
using Xunit;

namespace EventStore.Client {
	[Trait("Category", "Network")]
	public class stream_metadata_with_timeout : IClassFixture<stream_metadata_with_timeout.Fixture> {
		private readonly Fixture _fixture;

		public stream_metadata_with_timeout(Fixture fixture) {
			_fixture = fixture;
		}

		[Fact]
		public async Task set_with_any_stream_revision_fails_when_operation_expired() {
			var stream = _fixture.GetStreamName();
			var rpcException = await Assert.ThrowsAsync<RpcException>(() =>
				_fixture.Client.SetStreamMetadataAsync(stream, StreamState.Any, new StreamMetadata(),
					deadline: TimeSpan.Zero));

			Assert.Equal(StatusCode.DeadlineExceeded, rpcException.StatusCode);
		}

		[Fact]
		public async Task set_with_stream_revision_fails_when_operation_expired() {
			var stream = _fixture.GetStreamName();

			var rpcException = await Assert.ThrowsAsync<RpcException>(() =>
				_fixture.Client.SetStreamMetadataAsync(stream, new StreamRevision(0), new StreamMetadata(),
					deadline: TimeSpan.Zero));

			Assert.Equal(StatusCode.DeadlineExceeded, rpcException.StatusCode);
		}

		[Fact]
		public async Task get_fails_when_operation_expired() {
			var stream = _fixture.GetStreamName();
			var rpcException = await Assert.ThrowsAsync<RpcException>(() =>
				_fixture.Client.GetStreamMetadataAsync(stream, TimeSpan.Zero));
		}

		public class Fixture : EventStoreClientFixture {
			protected override Task Given() => Task.CompletedTask;
			protected override Task When() => Task.CompletedTask;
		}
	}
}
