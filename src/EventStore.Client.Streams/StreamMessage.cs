namespace EventStore.Client {
	/// <summary>
	/// The base record of all stream messages.
	/// </summary>
	public abstract record StreamMessage {
		/// <summary>
		/// A <see cref="EventStore.Client.StreamMessage"/> that represents a <see cref="EventStore.Client.ResolvedEvent"/>.
		/// </summary>
		/// <param name="ResolvedEvent">The <see cref="EventStore.Client.ResolvedEvent"/>.</param>
		public record Event(ResolvedEvent ResolvedEvent) : StreamMessage;

		/// <summary>
		/// A <see cref="StreamMessage"/> representing a stream that was not found.
		/// </summary>
		public record NotFound : StreamMessage {
			internal static readonly NotFound Instance = new();
		}

		/// <summary>
		/// A <see cref="StreamMessage"/> representing a successful read operation.
		/// </summary>
		public record Ok : StreamMessage {
			internal static readonly Ok Instance = new();
		};

		/// <summary>
		/// A <see cref="EventStore.Client.StreamMessage"/> indicating the first position of a stream.
		/// </summary>
		/// <param name="StreamPosition">The <see cref="EventStore.Client.StreamPosition"/>.</param>
		public record FirstStreamPosition(StreamPosition StreamPosition) : StreamMessage;

		/// <summary>
		/// A <see cref="EventStore.Client.StreamMessage"/> indicating the last position of a stream.
		/// </summary>
		/// <param name="StreamPosition">The <see cref="EventStore.Client.StreamPosition"/>.</param>
		public record LastStreamPosition(StreamPosition StreamPosition) : StreamMessage;

		/// <summary>
		/// A <see cref="EventStore.Client.StreamMessage"/> indicating the last position of the $all stream.
		/// </summary>
		/// <param name="Position">The <see cref="EventStore.Client.Position"/>.</param>
		public record LastAllStreamPosition(Position Position) : StreamMessage;

		/// <summary>
		/// A <see cref="EventStore.Client.StreamMessage"/> that could not be identified, usually indicating a lower client compatibility level than the server supports.
		/// </summary>
		public record Unknown : StreamMessage {
			internal static readonly Unknown Instance = new();
		}
	}
}
