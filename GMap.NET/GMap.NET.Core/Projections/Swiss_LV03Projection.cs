
namespace GMap.NET.Projections
{
    using System;
    using System.Collections.Generic;

    public class Swiss_LV03Projection : PureProjection
    {
        #region Members
        private const int ZOOM_OFFSET = 8;
        private const double STANDARD_ALTITUDE = 548.0;
        private const double MATRIX_MIN_X = 420000.0;
        private const double MATRIX_MAX_X = 900000.0;
        private const double MATRIX_MIN_Y = 030000.0;
        private const double MATRIX_MAX_Y = 350000.0;
        private const double MATRIX_HEIGHT = MATRIX_MAX_Y - MATRIX_MIN_Y;
        private readonly RectLatLng m_rllBounds = RectLatLng.FromLTRB(5.1402988011045343, 48.230617093859344, 11.477436450865998, 45.398122325706872);
        private readonly GSize m_gsTileSize = new GSize(256, 256);
        private static readonly double[] m_ardResolutions = new double[] { 4000, 3750, 3500, 3250, 3000, 2750, 2500, 2250, 2000, 1750, 1500, 1250, 1000, 750, 650, 500, 250, 100, 50, 20, 10, 5, 2.5, 2, 1.5, 1, 0.5, 0.25, 0.1, 0.05, 0.01, 0.005, 0.001 };

        private swisstopo.geodesy.reframe.Reframe m_rfConverter = new swisstopo.geodesy.reframe.Reframe();

        public static readonly Swiss_LV03Projection Instance = new Swiss_LV03Projection();
        #endregion

        #region Properties
        public override double Axis { get { return (6377397.155); } }
        public override RectLatLng Bounds { get { return (m_rllBounds); } }
        public override GSize TileSize { get { return (m_gsTileSize); } }
        public override double Flattening { get { return ((1.0 / 299.1528128)); } }
        #endregion

        #region Public functions
        public override GPoint FromLatLngToPixel(double p_dLat, double p_dLng, int p_nZoom)
        {
            var l_gpResult = GPoint.Empty;

            double l_dLat = Clip(p_dLat, m_rllBounds.Bottom, m_rllBounds.Top);
            double l_dLng = Clip(p_dLng, m_rllBounds.Left, m_rllBounds.Right);

            var l_pllConverted = WGStoCH(l_dLat, l_dLng);

            double l_dResolution = GetGroundResolution(p_nZoom, 0.0);

            double l_dXPixel = Math.Floor((l_pllConverted.Lng - MATRIX_MIN_X) / l_dResolution);
            double l_dYPixel = Math.Floor((MATRIX_MAX_Y - l_pllConverted.Lat) / l_dResolution);

            l_gpResult = new GPoint((long)l_dXPixel, (long)l_dYPixel);

            return (l_gpResult);
        }
        public override PointLatLng FromPixelToLatLng(long p_lXPos, long p_lYPos, int p_nZoom)
        {
            double l_dResolution = GetGroundResolution(p_nZoom, 0.0);

            double l_dLV03_X = MATRIX_MIN_X + (p_lXPos * l_dResolution);
            double l_dLV03_Y = MATRIX_MAX_Y - (p_lYPos * l_dResolution);

            l_dLV03_X = Clip(l_dLV03_X, MATRIX_MIN_X, MATRIX_MAX_X);
            l_dLV03_Y = Clip(l_dLV03_Y, MATRIX_MIN_Y, MATRIX_MAX_Y);

            var l_pllResult = CHtoWGS(l_dLV03_X, l_dLV03_Y);

            return (l_pllResult);
        }
        public override double GetGroundResolution(int zoom, double latitude)
        {
            double l_dResult = 0.0;

            if (zoom < (m_ardResolutions.Length - ZOOM_OFFSET))
            {
                l_dResult = m_ardResolutions[zoom + ZOOM_OFFSET];
            }

            return l_dResult;
        }
        public override GSize GetTileMatrixMinXY(int zoom)
        {
            return (new GSize(0L, 0L));
        }
        public override GSize GetTileMatrixMaxXY(int zoom)
        {
            switch (zoom + ZOOM_OFFSET)
            {
                case 0: return (new GSize(0, 0));
                case 1: return (new GSize(0, 0));
                case 2: return (new GSize(0, 0));
                case 3: return (new GSize(0, 0));
                case 4: return (new GSize(0, 0));
                case 5: return (new GSize(0, 0));
                case 6: return (new GSize(0, 0));
                case 7: return (new GSize(0, 0));
                case 8: return (new GSize(0, 0));
                case 9: return (new GSize(1, 0));
                case 10: return (new GSize(1, 0));
                case 11: return (new GSize(1, 0));
                case 12: return (new GSize(1, 1));
                case 13: return (new GSize(2, 1));
                case 14: return (new GSize(2, 1));
                case 15: return (new GSize(3, 2));
                case 16: return (new GSize(7, 4));
                case 17: return (new GSize(18, 12));
                case 18: return (new GSize(37, 24));
                case 19: return (new GSize(93, 62));
                case 20: return (new GSize(187, 124));
                case 21: return (new GSize(374, 249));
                case 22: return (new GSize(749, 499));
                case 23: return (new GSize(937, 624));
                case 24: return (new GSize(1249, 833));
                case 25: return (new GSize(1874, 1249));
                case 26: return (new GSize(3749, 2499));
                case 27: return (new GSize(7499, 4999));
                case 28: return (new GSize(18749, 12499));
            }

            return (GSize.Empty);
        }
        #endregion

        #region Private functions

        private PointLatLng WGStoCH(double p_dLat, double p_dLng)
        {
            double l_dLat = p_dLat;
            double l_dLng = p_dLng;
            double l_dAlt = STANDARD_ALTITUDE;

            try
            {
                m_rfConverter.ComputeGpsref(ref l_dLng, ref l_dLat, ref l_dAlt, swisstopo.geodesy.reframe.Reframe.ProjectionChange.ETRF93GeographicToLV95);
                m_rfConverter.ComputeReframe(ref l_dLng, ref l_dLat, ref l_dAlt, swisstopo.geodesy.reframe.Reframe.PlanimetricFrame.LV95, 0, 0, 0);

                return (new PointLatLng(l_dLat, l_dLng));
            }
            catch
            {
                return (PointLatLng.Empty);
            }
        }
        private PointLatLng CHtoWGS(double p_dX, double p_dY)
        {
            double l_dLV03_X = p_dX;
            double l_dLV03_Y = p_dY;
            double l_dAlt = STANDARD_ALTITUDE;

            try
            {
                m_rfConverter.ComputeReframe(ref l_dLV03_X, ref l_dLV03_Y, ref l_dAlt, 0, swisstopo.geodesy.reframe.Reframe.PlanimetricFrame.LV95, 0, 0);

                m_rfConverter.ComputeGpsref(ref l_dLV03_X, ref l_dLV03_Y, ref l_dAlt, swisstopo.geodesy.reframe.Reframe.ProjectionChange.LV95ToETRF93Geographic);

                return (new PointLatLng(l_dLV03_Y, l_dLV03_X));
            }
            catch
            {
                return (PointLatLng.Empty);
            }
        }
        #endregion
    }
}

