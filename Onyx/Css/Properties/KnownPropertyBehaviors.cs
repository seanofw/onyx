using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties
{
	public static class KnownPropertyBehaviors
	{
		/// <summary>
		/// The core table that describes what happens when each property changes.
		/// </summary>
		private static IReadOnlyDictionary<KnownPropertyKind, UpdateBehavior> _propertyUpdateBehaviors
			= new Dictionary<KnownPropertyKind, UpdateBehavior>
			{
				[KnownPropertyKind.Unknown] = UpdateBehavior.None,

				// Flex container alignment: re-arranges children within a fixed container size.
				[KnownPropertyKind.AlignContent] = UpdateBehavior.Layout,
				[KnownPropertyKind.AlignItems] = UpdateBehavior.Layout,
				[KnownPropertyKind.AlignSelf] = UpdateBehavior.Layout,

				// Aural CSS: no visual effect.
				[KnownPropertyKind.Azimuth] = UpdateBehavior.None,

				// Backgrounds: purely visual, no effect on layout.
				[KnownPropertyKind.BackgroundAttachment] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundImage] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundPosition] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundOrigin] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundRepeat] = UpdateBehavior.Paint,
				[KnownPropertyKind.BackgroundSize] = UpdateBehavior.Paint,
				[KnownPropertyKind.Background] = UpdateBehavior.Paint,

				// Border geometry: collapse and spacing affect table cell layout;
				// widths and styles (style:none zeroes effective width) affect box dimensions;
				// colors and radii are purely visual.
				[KnownPropertyKind.BorderCollapse] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderSpacing] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderTop] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderRight] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderBottom] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderLeft] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderTopColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderRightColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderBottomColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderLeftColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderTopStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderRightStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderBottomStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderLeftStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderTopWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderRightWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderBottomWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderLeftWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.BorderTopLeftRadius] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderTopRightRadius] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderBottomLeftRadius] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderBottomRightRadius] = UpdateBehavior.Paint,
				[KnownPropertyKind.BorderRadius] = UpdateBehavior.Paint,
				[KnownPropertyKind.Border] = UpdateBehavior.Measure,

				// Positioned element offsets: re-arranges this element within its containing
				// block without changing its measured size or affecting siblings.
				[KnownPropertyKind.Bottom] = UpdateBehavior.Layout,
				// Box-shadow: purely visual, does not participate in the box model.
				[KnownPropertyKind.BoxShadow] = UpdateBehavior.Paint,
				// Box-sizing: changes how width/height are interpreted (content-box vs border-box).
				[KnownPropertyKind.BoxSizing] = UpdateBehavior.Measure,
				// Caption-side: moves the table caption above or below the table body.
				[KnownPropertyKind.CaptionSide] = UpdateBehavior.Measure,
				// Clear: affects vertical placement relative to floats, changing the element's
				// position in the flow and potentially the parent's height.
				[KnownPropertyKind.Clear] = UpdateBehavior.Measure,
				// Clip: purely visual clipping, does not affect the box model.
				[KnownPropertyKind.Clip] = UpdateBehavior.Paint,
				// Color: text and currentColor, purely visual.
				[KnownPropertyKind.Color] = UpdateBehavior.Paint,
				// Content: changes the content of pseudo-elements; can affect their measured size.
				[KnownPropertyKind.Content] = UpdateBehavior.Measure,

				// Counters: only matter when referenced by content:counter(...); no direct visual effect.
				[KnownPropertyKind.CounterIncrement] = UpdateBehavior.None,
				[KnownPropertyKind.CounterReset] = UpdateBehavior.None,

				// Aural CSS: no visual effect.
				[KnownPropertyKind.CueAfter] = UpdateBehavior.None,
				[KnownPropertyKind.CueBefore] = UpdateBehavior.None,
				[KnownPropertyKind.Cue] = UpdateBehavior.None,

				// Cursor: interaction property, no rendered visual output.
				[KnownPropertyKind.Cursor] = UpdateBehavior.None,

				// Direction: RTL/LTR changes inline layout direction; affects text measurement.
				[KnownPropertyKind.Direction] = UpdateBehavior.Measure,
				// Display: changes the box type entirely; requires full box reconstruction.
				[KnownPropertyKind.Display] = UpdateBehavior.FullRefresh,
				// Aural CSS: no visual effect.
				[KnownPropertyKind.Elevation] = UpdateBehavior.None,
				// Empty-cells: whether empty table cells show borders/backgrounds; purely visual.
				[KnownPropertyKind.EmptyCells] = UpdateBehavior.Paint,

				// Flex: all flex properties affect how items are sized and distributed.
				[KnownPropertyKind.Flex] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexBasis] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexDirection] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexGrow] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexFlow] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexShrink] = UpdateBehavior.Measure,
				[KnownPropertyKind.FlexWrap] = UpdateBehavior.Measure,

				// Float: removes or restores the element from normal flow; requires full box reconstruction.
				[KnownPropertyKind.Float] = UpdateBehavior.FullRefresh,

				// Font: all font properties affect text metrics and thus measured text dimensions.
				[KnownPropertyKind.FontFamily] = UpdateBehavior.Measure,
				[KnownPropertyKind.FontSize] = UpdateBehavior.Measure,
				[KnownPropertyKind.FontStyle] = UpdateBehavior.Measure,
				[KnownPropertyKind.FontWeight] = UpdateBehavior.Measure,
				[KnownPropertyKind.FontVariant] = UpdateBehavior.Measure,
				[KnownPropertyKind.Font] = UpdateBehavior.Measure,

				[KnownPropertyKind.Height] = UpdateBehavior.Measure,
				// Justify-content: distributes flex items along the main axis within a fixed container.
				[KnownPropertyKind.JustifyContent] = UpdateBehavior.Layout,
				// Positioned element offsets (continued).
				[KnownPropertyKind.Left] = UpdateBehavior.Layout,
				// Letter-spacing and word-spacing: affect text run widths, and thus line breaking.
				[KnownPropertyKind.LetterSpacing] = UpdateBehavior.Measure,
				[KnownPropertyKind.LineHeight] = UpdateBehavior.Measure,

				// List style: position (inside/outside) and type affect the marker's contribution
				// to layout; image affects what is drawn in the marker area.
				[KnownPropertyKind.ListStyleImage] = UpdateBehavior.Measure,
				[KnownPropertyKind.ListStylePosition] = UpdateBehavior.Measure,
				[KnownPropertyKind.ListStyleType] = UpdateBehavior.Measure,
				[KnownPropertyKind.ListStyle] = UpdateBehavior.Measure,

				// Margins: directly affect the space around the box in the flow.
				[KnownPropertyKind.MarginTop] = UpdateBehavior.Measure,
				[KnownPropertyKind.MarginRight] = UpdateBehavior.Measure,
				[KnownPropertyKind.MarginBottom] = UpdateBehavior.Measure,
				[KnownPropertyKind.MarginLeft] = UpdateBehavior.Measure,
				[KnownPropertyKind.Margin] = UpdateBehavior.Measure,

				[KnownPropertyKind.MaxHeight] = UpdateBehavior.Measure,
				[KnownPropertyKind.MaxWidth] = UpdateBehavior.Measure,
				[KnownPropertyKind.MinHeight] = UpdateBehavior.Measure,
				[KnownPropertyKind.MinWidth] = UpdateBehavior.Measure,

				// Opacity: purely visual compositing, no effect on layout.
				[KnownPropertyKind.Opacity] = UpdateBehavior.Paint,
				// Order: changes the visual ordering of flex items within the container.
				[KnownPropertyKind.Order] = UpdateBehavior.Layout,
				// Orphans: pagination hint; no effect in a screen renderer.
				[KnownPropertyKind.Orphans] = UpdateBehavior.None,

				// Outline: unlike borders, outlines do not participate in the box model at all.
				[KnownPropertyKind.OutlineColor] = UpdateBehavior.Paint,
				[KnownPropertyKind.OutlineOffset] = UpdateBehavior.Paint,
				[KnownPropertyKind.OutlineStyle] = UpdateBehavior.Paint,
				[KnownPropertyKind.OutlineWidth] = UpdateBehavior.Paint,
				[KnownPropertyKind.Outline] = UpdateBehavior.Paint,

				// Overflow: affects how content is displayed within the box, not the box's
				// own outer dimensions as seen by its parent.
				[KnownPropertyKind.Overflow] = UpdateBehavior.Layout,
				[KnownPropertyKind.OverflowX] = UpdateBehavior.Layout,
				[KnownPropertyKind.OverflowY] = UpdateBehavior.Layout,

				// Padding: directly affects the content area dimensions of the box.
				[KnownPropertyKind.PaddingTop] = UpdateBehavior.Measure,
				[KnownPropertyKind.PaddingRight] = UpdateBehavior.Measure,
				[KnownPropertyKind.PaddingBottom] = UpdateBehavior.Measure,
				[KnownPropertyKind.PaddingLeft] = UpdateBehavior.Measure,
				[KnownPropertyKind.Padding] = UpdateBehavior.Measure,

				// Page-break: pagination hints; no effect in a screen renderer.
				[KnownPropertyKind.PageBreakAfter] = UpdateBehavior.None,
				[KnownPropertyKind.PageBreakBefore] = UpdateBehavior.None,
				[KnownPropertyKind.PageBreakInside] = UpdateBehavior.None,

				// Aural CSS: no visual effect.
				[KnownPropertyKind.PauseAfter] = UpdateBehavior.None,
				[KnownPropertyKind.PauseBefore] = UpdateBehavior.None,
				[KnownPropertyKind.Pause] = UpdateBehavior.None,

				[KnownPropertyKind.PitchRange] = UpdateBehavior.None,
				[KnownPropertyKind.Pitch] = UpdateBehavior.None,

				[KnownPropertyKind.PlayDuring] = UpdateBehavior.None,
				// Position: changes how the element participates in its formatting context
				// (e.g., static → absolute removes it from normal flow); requires full reconstruction.
				[KnownPropertyKind.Position] = UpdateBehavior.FullRefresh,
				// Quotes: only matter when referenced by content:open-quote/close-quote; no direct visual effect.
				[KnownPropertyKind.Quotes] = UpdateBehavior.None,
				// Resize: interactive property, no effect on initial layout.
				[KnownPropertyKind.Resize] = UpdateBehavior.None,
				// Aural CSS: no visual effect.
				[KnownPropertyKind.Richness] = UpdateBehavior.None,
				// Positioned element offsets (continued).
				[KnownPropertyKind.Right] = UpdateBehavior.Layout,

				// Aural CSS: no visual effect.
				[KnownPropertyKind.SpeakHeader] = UpdateBehavior.None,
				[KnownPropertyKind.SpeakNumeral] = UpdateBehavior.None,
				[KnownPropertyKind.SpeakPunctuation] = UpdateBehavior.None,
				[KnownPropertyKind.Speak] = UpdateBehavior.None,
				// Special font: affects font selection and thus text metrics.
				[KnownPropertyKind.SpecialFont] = UpdateBehavior.Measure,
				[KnownPropertyKind.SpeechRate] = UpdateBehavior.None,
				[KnownPropertyKind.Stress] = UpdateBehavior.None,

				// Table-layout: fixed vs. auto changes how column widths are calculated.
				[KnownPropertyKind.TableLayout] = UpdateBehavior.Measure,

				// Text-align: positions inline content horizontally within a fixed-width block;
				// does not change the block's own width.
				[KnownPropertyKind.TextAlign] = UpdateBehavior.Layout,
				// Text-decoration: purely visual; does not affect text metrics.
				[KnownPropertyKind.TextDecoration] = UpdateBehavior.Paint,
				// Text-indent: affects first-line indentation, which can change line wrapping.
				[KnownPropertyKind.TextIndent] = UpdateBehavior.Measure,
				// Text-shadow: purely visual.
				[KnownPropertyKind.TextShadow] = UpdateBehavior.Paint,
				// Text-transform: changes rendered glyph widths (e.g., uppercase may be wider).
				[KnownPropertyKind.TextTransform] = UpdateBehavior.Measure,

				// Positioned element offsets (continued).
				[KnownPropertyKind.Top] = UpdateBehavior.Layout,
				// Unicode-bidi: affects bidi embedding levels and thus inline text layout.
				[KnownPropertyKind.UnicodeBidi] = UpdateBehavior.Measure,
				// Vertical-align: affects vertical positioning within a line box, which can
				// change line box height and thus the containing block's height.
				[KnownPropertyKind.VerticalAlign] = UpdateBehavior.Measure,
				// Visibility: hidden elements still occupy their layout space; no layout change.
				[KnownPropertyKind.Visibility] = UpdateBehavior.Paint,
				// Aural CSS: no visual effect.
				[KnownPropertyKind.VoiceFamily] = UpdateBehavior.None,
				[KnownPropertyKind.Volume] = UpdateBehavior.None,
				// White-space: controls line-breaking behavior; affects text layout.
				[KnownPropertyKind.WhiteSpace] = UpdateBehavior.Measure,
				// Widows: pagination hint; no effect in a screen renderer.
				[KnownPropertyKind.Widows] = UpdateBehavior.None,
				[KnownPropertyKind.Width] = UpdateBehavior.Measure,
				[KnownPropertyKind.WordSpacing] = UpdateBehavior.Measure,
				// Z-index: changes stacking order only; no effect on layout geometry.
				[KnownPropertyKind.ZIndex] = UpdateBehavior.Paint,
			};

		/// <summary>
		/// A mapping between properties and what they affect when they change.
		/// </summary>
		public static IReadOnlyDictionary<KnownPropertyKind, UpdateBehavior> PropertyUpdateBehaviors
			=> _propertyUpdateBehaviors;

		/// <summary>
		/// The set of properties that trigger just repaint when changed.
		/// </summary>
		public static UInt256 PaintProperties { get; } = ComputedStyle.ConvertKnownPropertiesToBits(
			_propertyUpdateBehaviors.Where(p => p.Value >= UpdateBehavior.Paint).Select(p => p.Key));

		/// <summary>
		/// The set of properties that trigger layout + paint when changed.
		/// </summary>
		public static UInt256 LayoutProperties { get; } = ComputedStyle.ConvertKnownPropertiesToBits(
			_propertyUpdateBehaviors.Where(p => p.Value >= UpdateBehavior.Layout).Select(p => p.Key));

		/// <summary>
		/// The set of properties that trigger measure + layout + paint when changed.
		/// </summary>
		public static UInt256 MeasureProperties { get; } = ComputedStyle.ConvertKnownPropertiesToBits(
			_propertyUpdateBehaviors.Where(p => p.Value >= UpdateBehavior.Measure).Select(p => p.Key));

		/// <summary>
		/// The set of properties that trigger full refresh when changed.
		/// </summary>
		public static UInt256 FullRefreshProperties { get; } = ComputedStyle.ConvertKnownPropertiesToBits(
			_propertyUpdateBehaviors.Where(p => p.Value >= UpdateBehavior.FullRefresh).Select(p => p.Key));
	}
}
