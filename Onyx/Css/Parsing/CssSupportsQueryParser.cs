using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Css.Parsing
{
	public class CssSupportsQueryParser
	{
		#region Properties and fields

		/// <summary>
		/// The collection of messages (warnings/errors) emitted by the media query parser.
		/// </summary>
		public Messages Messages { get; }

		/// <summary>
		/// Whether we are parsing in strict mode, in which all warnings are emitted as errors.
		/// </summary>
		private readonly bool _strict;

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new parser.
		/// </summary>
		/// <param name="messages">The messages collection to which any additional messages
		/// will be added.  A messages collection will be created if one is not provided.</param>
		/// <param name="strict">Whether this is in strict mode or not.  In strict mode, all
		/// warnings will be emitted as errors.</param>
		public CssSupportsQueryParser(Messages? messages = null, bool strict = false)
		{
			Messages = messages ?? new Messages();
			_strict = strict;
		}

		#endregion
	}
}
