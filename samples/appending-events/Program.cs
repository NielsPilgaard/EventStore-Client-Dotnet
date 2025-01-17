﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;

namespace appending_events {
	class Program {
		static async Task<int> Main(string[] args) {
			var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
			settings.OperationOptions.ThrowOnAppendFailure = false;
			await using var client = new EventStoreClient(
				settings
			);
			await AppendToStream(client);
			await AppendWithConcurrencyCheck(client);
			await AppendWithNoStream(client);
			await AppendWithSameId(client);

			return 0;
		}

		private static async Task AppendToStream(EventStoreClient client) {
			#region append-to-stream
			var eventData = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"1\" \"value\": \"some value\"}")
			);

			await client.AppendToStreamAsync(
				"some-stream",
				StreamState.NoStream,
				new List<EventData> {
					eventData
				});
			#endregion append-to-stream
		}

		private static async Task AppendWithSameId(EventStoreClient client) {
			#region append-duplicate-event
			var eventData = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"1\" \"value\": \"some value\"}")
			);

			await client.AppendToStreamAsync(
				"same-event-stream",
				StreamState.Any,
				new List<EventData> {
					eventData
				});

			// attempt to append the same event again
			await client.AppendToStreamAsync(
				"same-event-stream",
				StreamState.Any,
				new List<EventData> {
					eventData
				});
			#endregion append-duplicate-event
		}

		private static async Task AppendWithNoStream(EventStoreClient client) {
			#region append-with-no-stream
			var eventDataOne = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"1\" \"value\": \"some value\"}")
			);

			var eventDataTwo = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"2\" \"value\": \"some other value\"}")
			);

			await client.AppendToStreamAsync(
				"no-stream-stream",
				StreamState.NoStream,
				new List<EventData> {
					eventDataOne
				});

			// attempt to append the same event again
			await client.AppendToStreamAsync(
				"no-stream-stream",
				StreamState.NoStream,
				new List<EventData> {
					eventDataTwo
				});
			#endregion append-with-no-stream
		}

		private static async Task AppendWithConcurrencyCheck(EventStoreClient client) {
			await client.AppendToStreamAsync("concurrency-stream", StreamRevision.None,
				new[] {new EventData(Uuid.NewUuid(), "-", ReadOnlyMemory<byte>.Empty)});
			#region append-with-concurrency-check

			var clientOneRead = client.ReadStreamAsync(
				Direction.Forwards,
				"concurrency-stream",
				StreamPosition.Start);
			var clientOneRevision = (await clientOneRead.LastAsync()).Event.EventNumber.ToUInt64();

			var clientTwoRead = client.ReadStreamAsync(Direction.Forwards, "concurrency-stream", StreamPosition.Start);
			var clientTwoRevision = (await clientTwoRead.LastAsync()).Event.EventNumber.ToUInt64();

			var clientOneData = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"1\" \"value\": \"clientOne\"}")
			);

			await client.AppendToStreamAsync(
				"no-stream-stream",
				clientOneRevision,
				new List<EventData> {
					clientOneData
				});

			var clientTwoData = new EventData(
				Uuid.NewUuid(),
				"some-event",
				Encoding.UTF8.GetBytes("{\"id\": \"2\" \"value\": \"clientTwo\"}")
			);

			await client.AppendToStreamAsync(
				"no-stream-stream",
				clientTwoRevision,
				new List<EventData> {
					clientTwoData
				});
			#endregion append-with-concurrency-check
		}
		
		protected async Task AppendOverridingUserCredentials(EventStoreClient client, CancellationToken cancellationToken)
		{
			var eventData = new EventData(
				Uuid.NewUuid(),
				"TestEvent",
				Encoding.UTF8.GetBytes("{\"id\": \"1\" \"value\": \"some value\"}")
			);
			
			#region overriding-user-credentials
			await client.AppendToStreamAsync(
				"some-stream",
				StreamState.Any,
				new[] { eventData },
				userCredentials: new UserCredentials("admin", "changeit"),
				cancellationToken: cancellationToken
			);
			#endregion overriding-user-credentials
		}
	}
}
