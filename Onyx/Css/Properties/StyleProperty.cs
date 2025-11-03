using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties
{
	public abstract record class StyleProperty
	{
		public SourceLocation? SourceLocation { get; init; }

		public KnownPropertyKind Kind { get; init; }
		private StylePropertyFlags _flags;

		public bool Inherit
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & StylePropertyFlags.Inherit) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			init => _flags = value
				? _flags |  StylePropertyFlags.Inherit
				: _flags & ~StylePropertyFlags.Inherit;
		}

		public bool Initial
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & StylePropertyFlags.Initial) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			init => _flags = value
				? _flags | StylePropertyFlags.Initial
				: _flags & ~StylePropertyFlags.Initial;
		}

		public bool Unset
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & StylePropertyFlags.Unset) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			init => _flags = value
				? _flags | StylePropertyFlags.Unset
				: _flags & ~StylePropertyFlags.Unset;
		}

		public bool HasSpecialApplication
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & (StylePropertyFlags.Unset | StylePropertyFlags.Inherit | StylePropertyFlags.Initial)) != 0;
		}

		public bool Important
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & StylePropertyFlags.Important) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			init => _flags = value
				? _flags | StylePropertyFlags.Important
				: _flags & ~StylePropertyFlags.Important;
		}

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => (_flags & StylePropertyFlags.Valid) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			init => _flags = value
				? _flags | StylePropertyFlags.Valid
				: _flags & ~StylePropertyFlags.Valid;
		}

		public static IReadOnlyDictionary<string, KnownPropertyKind> PropertyKindLookup => _propertyKindLookup;
		private static readonly Dictionary<string, KnownPropertyKind> _propertyKindLookup;

		static StyleProperty()
		{
			Dictionary<string, KnownPropertyKind> propertyKindLookup = new Dictionary<string, KnownPropertyKind>();

			foreach (KnownPropertyKind kind in Enum.GetValues(typeof(KnownPropertyKind)))
			{
				string name = kind.ToString();
				string hyphenized = name.Hyphenize();
				propertyKindLookup[hyphenized] = kind;
			}

			_propertyKindLookup = propertyKindLookup;
		}

		public override string ToString()
			=> string.Empty;

		public abstract ComputedStyle Apply(ComputedStyle style);

		public abstract ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source);

		private IReadOnlyCollection<StyleProperty>? _decomposed;

		public IReadOnlyCollection<StyleProperty> Decompose()
			=> _decomposed ??= DecomposeInternal().ToArray();

		protected virtual IEnumerable<StyleProperty> DecomposeInternal()
			=> [this];

		public virtual bool IsDecomposable => false;

		public virtual bool IsDefaultInherited => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected T Derive<T>()
			where T : StyleProperty, new()
			=> new T
			{
				_flags = _flags,
				SourceLocation = SourceLocation
			};

		protected Exception ShorthandException
			=> new NotSupportedException("Shorthand properties cannot be applied directly.");
	}
}
