namespace Onyx.Css.Types.Media
{
	public readonly struct MediaQueryContext
	{
		public MediaDimensions MediaDimensions { get; }
		public MediaInfo MediaInfo { get; }

		public MediaQueryContext(MediaDimensions mediaDimensions, MediaInfo mediaInfo)
		{
			MediaDimensions = mediaDimensions;
			MediaInfo = mediaInfo;
		}

		public override string ToString()
			=> $"{MediaDimensions}; {MediaInfo}";
	}
}
