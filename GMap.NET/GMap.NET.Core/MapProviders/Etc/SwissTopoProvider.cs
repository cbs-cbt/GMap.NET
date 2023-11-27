
namespace GMap.NET.MapProviders
{
    using System;
    using System.Drawing;
    using System.IO;
    using GMap.NET.Projections;

    /// <summary>
    /// Swisstopo map provider, https://api3.geo.admin.ch/services/sdiservices.html#wmts
    /// </summary>
    public abstract class SwisstopoProviderBase : GMapProvider
    {
        #region Members
        protected GMapProvider[] overlays;
        protected const int ZOOM_OFFSET = 8;
        #endregion

        #region Properties
        public override PureProjection Projection => MercatorProjection.Instance;
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
        public SwisstopoProviderBase()
        {
            RefererUrl = "https://api3.geo.admin.ch/services/sdiservices.html#wmts";
            Copyright = "© Données:CNES, Spot Image, swisstopo, NPOC";
            Area = new RectLatLng(45.398181, 5.140242, 2.83247, 6.337328);
            MinZoom = 2;
            MaxZoom = 19;
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

    public class SwisstopoSatelliteProvider : SwisstopoProviderBase
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
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.swissimage/default/current/3857/{0}/{1}/{2}.jpeg", zoom, pos.X, pos.Y);
        }
        #endregion
    }

    public class SwisstopoProvider : SwisstopoProviderBase
    {
        #region Members
        public static readonly SwisstopoProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("FD06165B-FF31-4B50-974E-3AB7FCDC1132");
        public override string Name { get; } = "SwisstopoMap";
        #endregion

        #region Constructor
        static SwisstopoProvider()
        {
            Instance = new SwisstopoProvider();
        }
        #endregion

        #region Private functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts20.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/current/3857/{0}/{1}/{2}.jpeg", zoom, pos.X, pos.Y);
        }
        #endregion
    }

    public class SwisstopoDroneFlightRestrictionsProvider : SwisstopoProviderBase
    {
        #region Members
        public static readonly SwisstopoDroneFlightRestrictionsProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("A16B7FA5-DDBF-453E-A9F8-0438C6CCACEF");
        public override string Name { get; } = "SwisstopoDroneFlightRestrictions";
        public GMapProvider BackgroundProvider { get; set; } = SwisstopoSatelliteProvider.Instance;
        public float Opacity { get; set; } = 0.5F;
        /*
        To implement : ability to set opacity of layers
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { SwisstopoSatelliteProvider.Instance, this };
                }
                
                return overlays;
            }
        }
        */
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
            using (var l_bmpOverlay = System.Drawing.Bitmap.FromStream(l_piOverlay.Data))
            using (var l_piBackground = BackgroundProvider.GetTileImage(pos, zoom))
            using (var l_bmpBackground = System.Drawing.Bitmap.FromStream(l_piBackground.Data))
            using (var l_bmpResult = new System.Drawing.Bitmap(l_bmpBackground.Width, l_bmpBackground.Height))
            using (var l_gResult = System.Drawing.Graphics.FromImage(l_bmpResult))
            {
                l_gResult.DrawImage(l_bmpBackground, 0, 0);

                var l_iaAttributes = new System.Drawing.Imaging.ImageAttributes();
                l_iaAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix() { Matrix33 = Opacity });

                var l_rectDest = new System.Drawing.Rectangle(0, 0, l_bmpBackground.Width, l_bmpBackground.Height);

                l_gResult.DrawImage(l_bmpOverlay, l_rectDest, 0, 0,
                    l_bmpBackground.Width, l_bmpBackground.Height, System.Drawing.GraphicsUnit.Pixel, l_iaAttributes);

                using (var l_msResult = new MemoryStream())
                {
                    l_bmpResult.Save(l_msResult, System.Drawing.Imaging.ImageFormat.Jpeg);

                    l_piResult = TileImageProxy.FromArray(l_msResult.ToArray());                
                }
            }

            return l_piResult;
        }
        #endregion

        #region Private functions
        protected override string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.bazl.einschraenkungen-drohnen/default/current/3857/{0}/{1}/{2}.png", zoom, pos.X, pos.Y);
        }
        #endregion
    }
}
