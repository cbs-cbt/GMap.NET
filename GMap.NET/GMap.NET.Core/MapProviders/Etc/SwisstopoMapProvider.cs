
namespace GMap.NET.MapProviders
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using GMap.NET.Projections;

    /// <summary>
    /// Swisstopo map provider, https://api3.geo.admin.ch/services/sdiservices.html#wmts
    /// </summary>
    public abstract class SwisstopoMapProviderBase : GMapProvider
    {
        #region Members
        protected GMapProvider[] overlays;
        protected const int ZOOM_OFFSET = 8;
        #endregion

        #region Properties
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

        #region Constructor
        public SwisstopoMapProviderBase()
        {
            RefererUrl = "https://map.geo.admin.ch/?topic=swisstopo&lang=en&bgLayer=ch.swisstopo.pixelkarte-farbe";
            Copyright = "© Données:CNES, Spot Image, swisstopo, NPOC";
            MaxZoom = 28;
            Area = Projection.Bounds;
        }
        #endregion

        #region Public functions
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

        #region Private functions
        protected virtual string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            throw (new NotImplementedException());
        }
        #endregion
    }

    public class SwisstopoSatelliteProvider : SwisstopoMapProviderBase
    {
        #region Members
        public static readonly SwisstopoSatelliteProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("A3D09DE4-222A-4A1D-9616-5BC77A9537C7");
        public override string Name { get; } = "SwisstopoSatellite";
        #endregion

        #region Constructor
        static SwisstopoSatelliteProvider()
        {
            Instance = new SwisstopoSatelliteProvider();
        }
        #endregion

        #region Private functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.swissimage/default/current/21781/{0}/{2}/{1}.jpeg", zoom + ZOOM_OFFSET, pos.X, pos.Y);
        }
        #endregion
    }

    public class SwisstopoMapProvider : SwisstopoMapProviderBase
    {
        #region Members
        public static readonly SwisstopoMapProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("FD06165B-FF31-4B50-974E-3AB7FCDC1132");
        public override string Name { get; } = "SwisstopoMap";
        #endregion

        #region Constructor
        static SwisstopoMapProvider()
        {
            Instance = new SwisstopoMapProvider();
        }
        #endregion

        #region Private functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/current/21781/{0}/{2}/{1}.jpeg", zoom + ZOOM_OFFSET, pos.X, pos.Y);
        }
        #endregion
    }

    public class SwisstopoDroneFlightRestrictionsProvider : SwisstopoMapProviderBase
    {
        #region Members
        public static readonly SwisstopoDroneFlightRestrictionsProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("A16B7FA5-DDBF-453E-A9F8-0438C6CCACEF");
        public override string Name { get; } = "SwisstopoDroneFlightRestrictions";
        #endregion

        #region Constructor
        static SwisstopoDroneFlightRestrictionsProvider()
        {
            Instance = new SwisstopoDroneFlightRestrictionsProvider();
        }
        #endregion

        #region Public functions
        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            PureImage l_piResult = null;

            using (var l_piOverlay = base.GetTileImage(pos, zoom))
            using (var l_bmpOverlay = Bitmap.FromStream(l_piOverlay.Data))
            using (var l_piBackground = SwisstopoMapProvider.Instance.GetTileImage(pos, zoom))
            using (var l_bmpBackground = Bitmap.FromStream(l_piBackground.Data))
            using (var l_bmpResult = new Bitmap(l_bmpBackground.Width, l_bmpBackground.Height))
            using (var l_gResult = Graphics.FromImage(l_bmpResult))
            {
                l_gResult.DrawImage(l_bmpBackground, 0, 0);

                var l_iaAttributes = new ImageAttributes();
                l_iaAttributes.SetColorMatrix(new ColorMatrix() { Matrix33 = 0.5F });

                var l_rectDest = new Rectangle(0, 0, l_bmpBackground.Width, l_bmpBackground.Height);

                l_gResult.DrawImage(l_bmpOverlay, l_rectDest, 0, 0,
                    l_bmpBackground.Width, l_bmpBackground.Height, GraphicsUnit.Pixel, l_iaAttributes);

                var l_msResult = new MemoryStream();

                l_bmpResult.Save(l_msResult, ImageFormat.Jpeg);

                l_piResult = TileImageProxy.FromStream(l_msResult);
            }

            return l_piResult;
        }
        #endregion

        #region Private functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.bazl.einschraenkungen-drohnen/default/current/21781/{0}/{2}/{1}.png", zoom + ZOOM_OFFSET, pos.X, pos.Y);
        }
        #endregion
    }
}
