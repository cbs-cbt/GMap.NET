
namespace GMap.NET.MapProviders
{
    using System;
    using GMap.NET.Projections;

    /// <summary>
    /// Swisstopo map provider, https://api3.geo.admin.ch/services/sdiservices.html#wmts
    /// </summary>
    public abstract class SwisstopoMapProviderBase : GMapProvider
    {
        #region Members Definition
        protected GMapProvider[] overlays;
        protected const int ZOOM_OFFSET = 8;
        #endregion

        #region Properties interface
        public override PureProjection Projection => Swiss_LV03Projection.Instance;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { this };
                }
                return overlays;
            }
        }
        #endregion

        #region Constructor / Destructor
        public SwisstopoMapProviderBase()
        {
            RefererUrl = "https://map.geo.admin.ch/?topic=swisstopo&lang=en&bgLayer=ch.swisstopo.pixelkarte-farbe";
            Copyright = "© Données:CNES, Spot Image, swisstopo, NPOC";
            MaxZoom = 28;
            Area = Projection.Bounds;
        }
        #endregion

        #region Public Functions
        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }
        public override void OnInitialized()
        {
            base.OnInitialized();

            //--------------------------------------------------------------------------------¦
            // Creates the cache instance                                                     ¦
            GMap.NET.Internals.Cache.Instance.CacheLocation.Clone();
        }
        #endregion

        #region Private Functions
        protected virtual string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            throw (new NotImplementedException());
        }
        #endregion
    }

    public class SwisstopoSatelliteProvider : SwisstopoMapProviderBase
    {
        #region Members Definition
        public static readonly SwisstopoSatelliteProvider Instance;
        #endregion

        #region Properties interface
        public override Guid Id { get; } = new Guid("A3D09DE4-222A-4A1D-9616-5BC77A9537C7");
        public override string Name { get; } = "SwisstopoSatellite";
        #endregion

        #region Constructor / Destructor
        static SwisstopoSatelliteProvider()
        {
            Instance = new SwisstopoSatelliteProvider();
        }
        #endregion

        #region Private Functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.swissimage/default/current/21781/{0}/{2}/{1}.jpeg", zoom + ZOOM_OFFSET, pos.X, pos.Y);
        }
        #endregion
    }

    public class SwisstopoMapProvider : SwisstopoMapProviderBase
    {
        #region Members Definition
        public static readonly SwisstopoMapProvider Instance;
        #endregion

        #region Properties interface
        public override Guid Id { get; } = new Guid("FD06165B-FF31-4B50-974E-3AB7FCDC1132");
        public override string Name { get; } = "SwisstopoMap";
        #endregion

        #region Constructor / Destructor
        static SwisstopoMapProvider()
        {
            Instance = new SwisstopoMapProvider();
        }
        #endregion

        #region Private Functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/current/21781/{0}/{2}/{1}.jpeg", zoom + ZOOM_OFFSET, pos.X, pos.Y);
        }
        #endregion
    }
}
