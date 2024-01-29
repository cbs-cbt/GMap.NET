
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
            Area = new RectLatLng(45.398181, 5.140242, 6.337328, -2.83247);
            MinZoom = 5;
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
            Internals.Cache.Instance.CacheLocation.Clone();
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

    public class SwisstopoMapProvider : SwisstopoProviderBase
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
            return string.Format("https://wmts.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/current/3857/{0}/{1}/{2}.jpeg", zoom, pos.X, pos.Y);
        }
        #endregion
    }

    public abstract class SwisstopoDroneFlightRestrictionsProviderBase : SwisstopoProviderBase
    {
        #region Properties
        public float Opacity { get; set; } = 0.5F;
        #endregion

        #region Public functions
        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            PureImage l_piResult = null;

            using (var l_piThis = base.GetTileImage(pos, zoom))
            using (var l_bmpThis = Image.FromStream(l_piThis.Data))
            using (var l_bmpResult = new Bitmap(l_bmpThis.Width, l_bmpThis.Height))
            using (var l_gResult = Graphics.FromImage(l_bmpResult))
            {
                var l_iaAttributes = new System.Drawing.Imaging.ImageAttributes();
                l_iaAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix() { Matrix33 = Opacity });

                var l_rectDest = new Rectangle(0, 0, l_bmpThis.Width, l_bmpThis.Height);

                l_gResult.DrawImage(l_bmpThis, l_rectDest, 0, 0,
                    l_bmpThis.Width, l_bmpThis.Height, GraphicsUnit.Pixel, l_iaAttributes);

                using (var l_msResult = new MemoryStream())
                {
                    l_bmpResult.Save(l_msResult, System.Drawing.Imaging.ImageFormat.Png);

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
    public class SwisstopoDroneFlightRestrictionsSatelliteProvider : SwisstopoDroneFlightRestrictionsProviderBase
    {
        #region Members
        public static readonly SwisstopoDroneFlightRestrictionsSatelliteProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("A16B7FA5-DDBF-453E-A9F8-0438C6CCACEF");
        public override string Name { get; } = "SwisstopoDroneFlightRestrictionsSatellite";
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
        #endregion

        #region Constructor
        static SwisstopoDroneFlightRestrictionsSatelliteProvider()
        {
            Instance = new SwisstopoDroneFlightRestrictionsSatelliteProvider();
        }
        #endregion
    }
    public class SwisstopoDroneFlightRestrictionsMapProvider : SwisstopoDroneFlightRestrictionsProviderBase
    {
        #region Members
        public static readonly SwisstopoDroneFlightRestrictionsMapProvider Instance;
        #endregion

        #region Properties
        public override Guid Id { get; } = new Guid("791EEA9D-412B-4B62-9A9F-C6246F9AB250");
        public override string Name { get; } = "SwisstopoDroneFlightRestrictionsMap";
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { SwisstopoMapProvider.Instance, this };
                }
                
                return overlays;
            }
        }
        #endregion

        #region Constructor
        static SwisstopoDroneFlightRestrictionsMapProvider()
        {
            Instance = new SwisstopoDroneFlightRestrictionsMapProvider();
        }
        #endregion
    }
}
