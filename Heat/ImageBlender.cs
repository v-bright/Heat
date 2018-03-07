using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Heat
{
    public class ImageBlender
    {
        #region Enums

        public enum BlendOperation
        {
            SourceCopy = 1,
            RopMergePaint,
            RopNotSourceErase,
            RopSourceAnd,
            RopSourceErase,
            RopSourceInvert,
            RopSourcePaint,
            BlendDarken,
            BlendMultiply,
            BlendColorBurn,
            BlendLighten,
            BlendScreen,
            BlendColorDodge,
            BlendOverlay,
            BlendSoftLight,
            BlendHardLight,
            BlendPinLight,
            BlendDifference,
            BlendExclusion,
            BlendHue,
            BlendSaturation,
            BlendColor,
            BlendLuminosity
        }

        #endregion

        #region Nested type: PerChannelProcessDelegate

        private delegate byte PerChannelProcessDelegate(ref byte nSrc, ref byte nDst);

        #endregion

        #region Nested type: RGBProcessDelegate

        private delegate void RgbProcessDelegate(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB);

        #endregion

        #region Constants

        public const float BWeight = 0.144f;
        public const float GWeight = 0.587f;
        public const ushort Hlsmax = 360;
        public const byte Hundefined = 0;

        // NTSC defined color weights
        public const float RWeight = 0.299f;

        public const byte Rgbmax = 255;

        #endregion

        #region Methods

        // Adjustment values are between -1.0 and 1.0
        public void AdjustBrightness(Image img, float adjValueR, float adjValueG, float adjValueB)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[]
            {
                new[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                new[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                new[] {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},
                new[] {adjValueR, adjValueG, adjValueB, 0.0f, 1.0f}
            });
            ApplyColorMatrix(ref img, cMatrix);
        }

        // Adjustment values are between -1.0 and 1.0
        public void AdjustBrightness(Image img, float adjValue)
        {
            AdjustBrightness(img, adjValue, adjValue, adjValue);
        }

        // Saturation. 0.0 = desaturate, 1.0 = identity, -1.0 = complementary colors
        public void AdjustSaturation(Image img, float sat, float rweight, float gweight, float bweight)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[]
            {
                new[] {(1.0f - sat) * rweight + sat, (1.0f - sat) * rweight, (1.0f - sat) * rweight, 0.0f, 0.0f},
                new[] {(1.0f - sat) * gweight, (1.0f - sat) * gweight + sat, (1.0f - sat) * gweight, 0.0f, 0.0f},
                new[] {(1.0f - sat) * bweight, (1.0f - sat) * bweight, (1.0f - sat) * bweight + sat, 0.0f, 0.0f},
                new[] {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},
                new[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
            });
            ApplyColorMatrix(ref img, cMatrix);
        }

        // Saturation. 0.0 = desaturate, 1.0 = identity, -1.0 = complementary colors
        public void AdjustSaturation(Image img, float sat)
        {
            AdjustSaturation(img, sat, RWeight, GWeight, BWeight);
        }

        public void ApplyColorMatrix(ref Image img, ColorMatrix colMatrix)
        {
            var graphics = Graphics.FromImage(img);
            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(colMatrix);
            graphics.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
                0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attrs);
            graphics.Dispose();
        }

        // use source Color
        private void BlendColor(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;

            RgbtoHls(sR, sG, sB, out sH, out sL, out sS);
            RgbtoHls(dR, dG, dB, out dH, out dL, out dS);
            HlstoRgb(sH, dL, sS, out dR, out dG, out dB);
        }

        // Color Burn 
        private static byte BlendColorBurn(ref byte src, ref byte dst)
        {
            return src == 0 ? (byte) 0 : (byte) Math.Max(Math.Min(255 - (255 - dst) * 255 / src, 255), 0);
        }

        // Color Dodge 
        private static byte BlendColorDodge(ref byte src, ref byte dst)
        {
            return src == 255 ? (byte) 255 : (byte) Math.Max(Math.Min(dst * 255 / (255 - src), 255), 0);
        }

        // Choose darkest color 
        private static byte BlendDarken(ref byte src, ref byte dst)
        {
            return src < dst ? src : dst;
        }

        // difference 
        private static byte BlendDifference(ref byte src, ref byte dst)
        {
            return (byte) (src > dst ? src - dst : dst - src);
        }

        // exclusion 
        private static byte BlendExclusion(ref byte src, ref byte dst)
        {
            return (byte) (src + dst - 2 * dst * src / 255f);
        }

        // hard light 
        private static byte BlendHardLight(ref byte src, ref byte dst)
        {
            return src < 128
                ? (byte) Math.Max(Math.Min(src / 255.0f * dst / 255.0f * 255.0f * 2, 255), 0)
                : (byte) Math.Max(Math.Min(255 - (255 - src) / 255.0f * (255 - dst) / 255.0f * 255.0f * 2, 255), 0);
        }

        // use source Hue
        private void BlendHue(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;

            RgbtoHls(sR, sG, sB, out sH, out sL, out sS);
            RgbtoHls(dR, dG, dB, out dH, out dL, out dS);
            HlstoRgb(sH, dL, dS, out dR, out dG, out dB);
        }

        public void BlendImages(Image destinationImage, int destinationStartX, int destinationStartY,
            int destinationWidth, int destinationHeight,
            Image sourceImage, int sourceStartX, int sourceStartY, BlendOperation blendOperation)
        {
            if (destinationImage == null)
                throw new Exception("Destination image must be provided");

            if (destinationImage.Width < destinationStartX + destinationWidth ||
                destinationImage.Height < destinationStartY + destinationHeight)
                throw new Exception("Destination image is smaller than requested dimentions");

            if (sourceImage == null)
                throw new Exception("Source image must be provided");

            if (sourceImage.Width < sourceStartX + destinationWidth ||
                sourceImage.Height < sourceStartY + destinationHeight)
                throw new Exception("Source image is smaller than requested dimentions");

            Bitmap tempBmp = null;
            var graphics = Graphics.FromImage(destinationImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;

            switch (blendOperation)
            {
                case BlendOperation.SourceCopy:
                    graphics.DrawImage(sourceImage,
                        new Rectangle(destinationStartX, destinationStartY, destinationWidth, destinationHeight),
                        sourceStartX, sourceStartY, destinationWidth, destinationHeight, GraphicsUnit.Pixel);
                    break;

                case BlendOperation.RopMergePaint:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, MergePaint);
                    break;

                case BlendOperation.RopNotSourceErase:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, NotSourceErase);
                    break;

                case BlendOperation.RopSourceAnd:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, SourceAnd);
                    break;

                case BlendOperation.RopSourceErase:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, SourceErase);
                    break;

                case BlendOperation.RopSourceInvert:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, SourceInvert);
                    break;

                case BlendOperation.RopSourcePaint:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, SourcePaint);
                    break;

                case BlendOperation.BlendDarken:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendDarken);
                    break;

                case BlendOperation.BlendMultiply:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendMultiply);
                    break;

                case BlendOperation.BlendScreen:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendScreen);
                    break;

                case BlendOperation.BlendLighten:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendLighten);
                    break;

                case BlendOperation.BlendHardLight:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendHardLight);
                    break;

                case BlendOperation.BlendDifference:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendDifference);
                    break;

                case BlendOperation.BlendPinLight:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendPinLight);
                    break;

                case BlendOperation.BlendOverlay:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendOverlay);
                    break;

                case BlendOperation.BlendExclusion:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendExclusion);
                    break;

                case BlendOperation.BlendSoftLight:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendSoftLight);
                    break;

                case BlendOperation.BlendColorBurn:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendColorBurn);
                    break;

                case BlendOperation.BlendColorDodge:
                    tempBmp = PerChannelProcess(ref destinationImage, destinationStartX, destinationStartY,
                        destinationWidth, destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendColorDodge);
                    break;

                case BlendOperation.BlendHue:
                    tempBmp = RgbProcess(ref destinationImage, destinationStartX, destinationStartY, destinationWidth,
                        destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendHue);
                    break;

                case BlendOperation.BlendSaturation:
                    tempBmp = RgbProcess(ref destinationImage, destinationStartX, destinationStartY, destinationWidth,
                        destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendSaturation);
                    break;

                case BlendOperation.BlendColor:
                    tempBmp = RgbProcess(ref destinationImage, destinationStartX, destinationStartY, destinationWidth,
                        destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendColor);
                    break;

                case BlendOperation.BlendLuminosity:
                    tempBmp = RgbProcess(ref destinationImage, destinationStartX, destinationStartY, destinationWidth,
                        destinationHeight,
                        ref sourceImage, sourceStartX, sourceStartY, BlendLuminosity);
                    break;
            }

            if (tempBmp != null)
            {
                graphics.DrawImage(tempBmp, 0, 0, tempBmp.Width, tempBmp.Height);
                tempBmp.Dispose();
            }

            graphics.Dispose();
        }

        public void BlendImages(Image destinationImage, Image srcImage, BlendOperation blendOperation)
        {
            BlendImages(destinationImage, 0, 0, destinationImage.Width, destinationImage.Height, srcImage, 0, 0,
                blendOperation);
        }

        public void BlendImages(Image destinationImage, BlendOperation blendOperation)
        {
            BlendImages(destinationImage, 0, 0, destinationImage.Width, destinationImage.Height, null, 0, 0,
                blendOperation);
        }

        public void BlendImages(Image destinationImage, int destinationStartX, int destinationStartY,
            BlendOperation blendOperation)
        {
            BlendImages(destinationImage, destinationStartX, destinationStartY,
                destinationImage.Width - destinationStartX, destinationImage.Height - destinationStartY, null, 0, 0,
                blendOperation);
        }

        public void BlendImages(Image destinationImage, int destinationStartX, int destinationStartY,
            int destinationWidth, int destinationHeight, BlendOperation blendOperation)
        {
            BlendImages(destinationImage, destinationStartX, destinationStartY, destinationWidth, destinationHeight,
                null, 0, 0, blendOperation);
        }

        // Choose lightest color 
        private static byte BlendLighten(ref byte src, ref byte dst)
        {
            return src > dst ? src : dst;
        }

        // use source Luminosity
        private void BlendLuminosity(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;

            RgbtoHls(sR, sG, sB, out sH, out sL, out sS);
            RgbtoHls(dR, dG, dB, out dH, out dL, out dS);
            HlstoRgb(dH, sL, dS, out dR, out dG, out dB);
        }

        // Multiply
        private static byte BlendMultiply(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(src / 255.0f * dst / 255.0f * 255.0f, 255), 0);
        }

        // overlay 
        private static byte BlendOverlay(ref byte src, ref byte dst)
        {
            return dst < 128
                ? (byte) Math.Max(Math.Min(src / 255.0f * dst / 255.0f * 255.0f * 2, 255), 0)
                : (byte) Math.Max(Math.Min(255 - (255 - src) / 255.0f * (255 - dst) / 255.0f * 255.0f * 2, 255), 0);
        }

        // pin light 
        private static byte BlendPinLight(ref byte src, ref byte dst)
        {
            return src < 128 ? (dst > src ? src : dst) : (dst < src ? src : dst);
        }

        // use source Saturation
        private void BlendSaturation(byte sR, byte sG, byte sB, ref byte dR, ref byte dG, ref byte dB)
        {
            ushort sH, sL, sS, dH, dL, dS;

            RgbtoHls(sR, sG, sB, out sH, out sL, out sS);
            RgbtoHls(dR, dG, dB, out dH, out dL, out dS);
            HlstoRgb(dH, dL, sS, out dR, out dG, out dB);
        }

        // Screen
        private static byte BlendScreen(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(255 - (255 - src) / 255.0f * (255 - dst) / 255.0f * 255.0f, 255), 0);
        }

        // Soft Light (XFader formula)  
        private static byte BlendSoftLight(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(
                Math.Min(dst * src / 255f + dst * (255 - (255 - dst) * (255 - src) / 255f - dst * src / 255f) / 255f,
                    255), 0);
        }

        // Weights between 0.0 and 1.0
        public void Desaturate(Image img, float rWeight, float gWeight, float bWeight)
        {
            AdjustSaturation(img, 0.0f, rWeight, gWeight, bWeight);
        }

        // Desaturate using "default" NTSC defined color weights
        public void Desaturate(Image img)
        {
            AdjustSaturation(img, 0.0f, RWeight, GWeight, BWeight);
        }

        public void HlstoRgb(ushort h, ushort l, ushort s, out byte r, out byte g, out byte b)
        {
            if (s == 0)
            {
                /* achromatic case */
                r = g = b = (byte) (l * Rgbmax / Hlsmax);
            }
            else
            {
                /* chromatic case */
                float magic2;
                if (l <= Hlsmax / 2)
                    magic2 = (l * (Hlsmax + s) + Hlsmax / 2) / Hlsmax;
                else
                    magic2 = l + s - (l * s + Hlsmax / 2) / Hlsmax;

                var magic1 = 2 * l - magic2;

                r = (byte) ((HueToRgb(magic1, magic2, h + Hlsmax / 3) * Rgbmax + Hlsmax / 2) / Hlsmax);
                g = (byte) ((HueToRgb(magic1, magic2, h) * Rgbmax + Hlsmax / 2) / Hlsmax);
                b = (byte) ((HueToRgb(magic1, magic2, h - Hlsmax / 3) * Rgbmax + Hlsmax / 2) / Hlsmax);
            }
        }

        /* utility routine for HLStoRGB */

        private static float HueToRgb(float n1, float n2, float hue)
        {
            /* range check: note values passed add/subtract thirds of range */
            if (hue < 0)
                hue += Hlsmax;

            if (hue > Hlsmax)
                hue -= Hlsmax;

            /* return r,g, or b value from this tridrant */
            if (hue < Hlsmax / 6)
                return n1 + ((n2 - n1) * hue + Hlsmax / 12) / (Hlsmax / 6);
            if (hue < Hlsmax / 2)
                return n2;
            if (hue < Hlsmax * 2 / 3)
                return n1 + ((n2 - n1) * (Hlsmax * 2 / 3 - hue) + Hlsmax / 12) / (Hlsmax / 6);
            return n1;
        }

        // Invert image
        public void Invert(Image img)
        {
            if (img == null)
                throw new Exception("Image must be provided");

            var cMatrix = new ColorMatrix(new[]
            {
                new[] {-1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new[] {0.0f, -1.0f, 0.0f, 0.0f, 0.0f},
                new[] {0.0f, 0.0f, -1.0f, 0.0f, 0.0f},
                new[] {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},
                new[] {1.0f, 1.0f, 1.0f, 0.0f, 1.0f}
            });
            ApplyColorMatrix(ref img, cMatrix);
        }

        // (NOT Source) OR Destination
        private static byte MergePaint(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min((255 - src) | dst, 255), 0);
        }

        // NOT (Source OR Destination)
        private static byte NotSourceErase(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(255 - (src | dst), 255), 0);
        }

        private static Bitmap PerChannelProcess(ref Image destImg, int destX, int destY, int destWidth, int destHeight,
            ref Image srcImg, int srcX, int srcY,
            PerChannelProcessDelegate channelProcessFunction)
        {
            var dst = new Bitmap(destImg);
            var src = new Bitmap(srcImg);

            var dstBd = dst.LockBits(new Rectangle(destX, destY, destWidth, destHeight), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            var srcBd = src.LockBits(new Rectangle(srcX, srcY, destWidth, destHeight), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            var dstStride = dstBd.Stride;
            var srcStride = srcBd.Stride;

            var dstScan0 = dstBd.Scan0;
            var srcScan0 = srcBd.Scan0;

            unsafe
            {
                var pDst = (byte*) (void*) dstScan0;
                var pSrc = (byte*) (void*) srcScan0;

                for (var y = 0; y < destHeight; y++)
                for (var x = 0; x < destWidth * 3; x++)
                    pDst[x + y * dstStride] =
                        channelProcessFunction(ref pSrc[x + y * srcStride], ref pDst[x + y * dstStride]);
            }

            src.UnlockBits(srcBd);
            dst.UnlockBits(dstBd);

            src.Dispose();

            return dst;
        }

        private static Bitmap RgbProcess(ref Image destImg, int destX, int destY, int destWidth, int destHeight,
            ref Image srcImg, int srcX, int srcY,
            RgbProcessDelegate rgbProcessFunction)
        {
            var dst = new Bitmap(destImg);
            var src = new Bitmap(srcImg);

            var dstBd = dst.LockBits(new Rectangle(destX, destY, destWidth, destHeight), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);
            var srcBd = src.LockBits(new Rectangle(srcX, srcY, destWidth, destHeight), ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            var dstStride = dstBd.Stride;
            var srcStride = srcBd.Stride;

            var dstScan0 = dstBd.Scan0;
            var srcScan0 = srcBd.Scan0;

            unsafe
            {
                var pDst = (byte*) (void*) dstScan0;
                var pSrc = (byte*) (void*) srcScan0;

                for (var y = 0; y < destHeight; y++)
                for (var x = 0; x < destWidth; x++)
                    rgbProcessFunction(
                        pSrc[x * 3 + 2 + y * srcStride], pSrc[x * 3 + 1 + y * srcStride], pSrc[x * 3 + y * srcStride],
                        ref pDst[x * 3 + 2 + y * dstStride], ref pDst[x * 3 + 1 + y * dstStride],
                        ref pDst[x * 3 + y * dstStride]
                    );
            }

            src.UnlockBits(srcBd);
            dst.UnlockBits(dstBd);

            src.Dispose();

            return dst;
        }

        public void RgbtoHls(byte r, byte g, byte b, out ushort h, out ushort l, out ushort s)
        {
            /* calculate lightness */
            var cMax = Math.Max(Math.Max(r, g), b);
            var cMin = Math.Min(Math.Min(r, g), b);
            l = (ushort) (((cMax + cMin) * Hlsmax + Rgbmax) / (2 * Rgbmax));

            if (cMax == cMin)
            {
                /* r=g=b --> achromatic case */
                s = 0; /* saturation */
                h = Hundefined; /* hue */
            }
            else
            {
                /* chromatic case */
                /* saturation */
                if (l <= Hlsmax / 2)
                    s = (ushort) (((cMax - cMin) * Hlsmax + (cMax + cMin) / 2) / (cMax + cMin));
                else
                    s = (ushort) (((cMax - cMin) * Hlsmax + (2 * Rgbmax - cMax - cMin) / 2) /
                                  (2 * Rgbmax - cMax - cMin));

                /* hue */
                float rdelta =
                    ((cMax - r) * (Hlsmax / 6) + (cMax - cMin) / 2) /
                    (cMax - cMin); /* intermediate value: % of spread from max */
                float gdelta =
                    ((cMax - g) * (Hlsmax / 6) + (cMax - cMin) / 2) /
                    (cMax - cMin); /* intermediate value: % of spread from max */
                float bdelta =
                    ((cMax - b) * (Hlsmax / 6) + (cMax - cMin) / 2) /
                    (cMax - cMin); /* intermediate value: % of spread from max */

                if (r == cMax)
                    h = (ushort) (bdelta - gdelta);
                else if (g == cMax)
                    h = (ushort) (Hlsmax / 3 + rdelta - bdelta);
                else /* B == cMax */
                    h = (ushort) (2 * Hlsmax / 3 + gdelta - rdelta);

                if (h < 0)
                    h += Hlsmax;
                if (h > Hlsmax)
                    h -= Hlsmax;
            }
        }

        // Source AND Destination
        private static byte SourceAnd(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(src & dst, 255), 0);
        }

        // Source AND (NOT Destination)
        private static byte SourceErase(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(src & (255 - dst), 255), 0);
        }

        // Source XOR Destination
        private static byte SourceInvert(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(src ^ dst, 255), 0);
        }

        // Source OR Destination
        private static byte SourcePaint(ref byte src, ref byte dst)
        {
            return (byte) Math.Max(Math.Min(src | dst, 255), 0);
        }

        #endregion
    }
}