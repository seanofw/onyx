using System.Collections;
using System.Collections.Immutable;

namespace Onyx
{
	/// <summary>
	/// This class collects messages as they are emitted from various sources.
	/// All methods on this class are atomic and thread-safe.
	/// </summary>
	public class Messages : ICollection<Message>
	{
		private volatile ImmutableList<Message> _messages = ImmutableList<Message>.Empty;

		public Message this[int index] => _messages[index];

		public int Count => _messages.Count;

		bool ICollection<Message>.IsReadOnly => false;

		public void Add(Message message)
		{
		retry:
			ImmutableList<Message> oldMessages = _messages;
			ImmutableList<Message> newMessages = oldMessages.Add(message);
			if (Interlocked.CompareExchange(ref _messages, newMessages, oldMessages) != oldMessages)
				goto retry;
		}

		public void AddRange(IEnumerable<Message> messages)
		{
			Message[] hardenedMessages = messages.ToArray();
		retry:
			ImmutableList<Message> oldMessages = _messages;
			ImmutableList<Message> newMessages = oldMessages.AddRange(hardenedMessages);
			if (Interlocked.CompareExchange(ref _messages, newMessages, oldMessages) != oldMessages)
				goto retry;
		}

		bool ICollection<Message>.Remove(Message item)
			=> throw new NotSupportedException();

		public void Clear()
			=> _messages = ImmutableList<Message>.Empty;

		public bool Contains(Message item)
			=> _messages.Contains(item);

		public void CopyTo(Message[] array, int arrayIndex)
			=> _messages.CopyTo(array, arrayIndex);

		public IEnumerator<Message> GetEnumerator()
			=> _messages.GetEnumerator();

		public int IndexOf(Message item)
			=> _messages.IndexOf(item);

		IEnumerator IEnumerable.GetEnumerator()
			=> _messages.GetEnumerator();
	}
}
