using System.Collections.Generic;

namespace Heat
{
    public class Opacity
    {
        #region Methods

        public int[] BuildZoomMapping()
        {
            var zoomMapping = new List<int>();

            var numberOfOpacitySteps = _zoomTransparent - _zoomOpaque;

            if (numberOfOpacitySteps < 1) //don't want general fade
            {
                for (var i = 0; i <= MaxZoom; i++)
                    zoomMapping.Add(0);
            }
            else //want general fade
            {
                var opacityStep = Opaque / (float) numberOfOpacitySteps;
                for (var zoom = 0; zoom <= MaxZoom; zoom++)
                    if (zoom <= _zoomOpaque)
                        zoomMapping.Add(Opaque);
                    else if (zoom >= _zoomTransparent)
                        zoomMapping.Add(Transparent);
                    else
                        zoomMapping.Add((int) (Opaque - (zoom - (float) _zoomOpaque) * opacityStep));
            }
            return zoomMapping.ToArray();
        }

        #endregion

        #region Constants

        public const int DefaultOpacity = 50;
        public const int MaxZoom = 31;
        public const int Opaque = 255;
        public const int Transparent = 0;
        public const int ZoomOpaque = -15;
        public const int ZoomTransparent = 15;

        #endregion

        #region Fields

        private readonly int _zoomOpaque;
        private readonly int _zoomTransparent;

        #endregion

        #region Constructors

        public Opacity(int zoomOpaque, int zoomTransparent)
        {
            _zoomOpaque = zoomOpaque;
            _zoomTransparent = zoomTransparent;
        }

        public Opacity()
        {
            _zoomOpaque = ZoomOpaque;
            _zoomTransparent = ZoomTransparent;
        }

        #endregion
    }
}