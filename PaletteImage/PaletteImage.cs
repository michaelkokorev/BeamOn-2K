using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Resources;
using System.Reflection;

namespace PaletteImage
{
    public partial class PaletteImage : PictureBox
    {
        public class ChangePaletteEventArgs : EventArgs
        {
            private ColorPalette m_cpPalette = null;
            private PixelFormat m_pixelFormat = PixelFormat.Format8bppIndexed;
            private Color[] m_color = null;

            public ChangePaletteEventArgs(ColorPalette cpPalette, PixelFormat pixelFormat, Color[] color)
            {
                m_cpPalette = cpPalette;
                m_pixelFormat = pixelFormat;

                if (m_pixelFormat == PixelFormat.Format8bppIndexed)
                    m_color = null;
                else
                {
                    m_color = new Color[color.Length];
                    color.CopyTo(m_color, 0);
                }
            }

            public PixelFormat Format
            {
                get { return m_pixelFormat; }
            }

            public Color[] colorArray
            {
                get { return m_color; }
            }

            public ColorPalette Palette
            {
                get { return m_cpPalette; }
            }
        }

        public delegate void ChangePalette(object sender, ChangePaletteEventArgs e);
        public event ChangePalette OnChangePalette;

        const Int16 M_DELTA = 2;

        Bitmap m_bmpPalette = null;
        Color[] m_color = null;
        Color[] resColor = null;
        Color[] m_colorReal = null;

        Object m_lLockBMP = new Object();

        Int16 m_iLinePaletteUp = 255;
        Int16 m_iLinePaletteDown = 0;

        Single m_sPaletteLevelUp = 100f;
        Boolean m_bStartMoveLevelUp = false;

        PixelFormat m_pixelFormat = PixelFormat.Format8bppIndexed;

        public PaletteImage()
        {
            ResourceManager resourceManager = new ResourceManager("PaletteImage.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

            Byte[] buffer = (Byte[])resourceManager.GetObject("Pallete");

            resColor = new Color[buffer.Length / 3];

            for (int i = 0; i < resColor.Length; i++)
            {
                resColor[i] = Color.FromArgb(buffer[i * 3 + 2], buffer[i * 3 + 1], buffer[i * 3]);
            }

            ChangePixelFormat();
        }

        public PixelFormat Format
        {
            get { return m_pixelFormat; }

            set
            {
                if ((value == PixelFormat.Format8bppIndexed) || (value == PixelFormat.Format24bppRgb))
                {
                    m_pixelFormat = value;
                    ChangePixelFormat();

                    try
                    {
                        OnChangePalette(this, new ChangePaletteEventArgs(Palette, m_pixelFormat, m_colorReal));
                    }
                    catch
                    {
                    }
                }
            }
        }

        public ColorPalette Palette
        {
            get { return (m_pixelFormat == PixelFormat.Format8bppIndexed) ? m_bmpPalette.Palette : null; }
        }

        public Color[] colorArray
        {
            get { return m_colorReal; }
        }

        public Single PaletteLevelUp
        {
            get { return m_sPaletteLevelUp; }
            set
            {
                if ((m_bmpPalette != null) && (value > 0f) && (value <= 100f))
                {
                    m_sPaletteLevelUp = value;
                    m_iLinePaletteUp = (Int16)Math.Ceiling((m_color.Length - 1) * m_sPaletteLevelUp / 100f);

                    ChangePaletteLevel();

                    try
                    {
                        OnChangePalette(this, new ChangePaletteEventArgs(m_bmpPalette.Palette, m_pixelFormat, m_colorReal));
                    }
                    catch
                    { }
                }
            }
        }

        private void ChangePixelFormat()
        {
            double l_dPaletteStepStep;

            m_bmpPalette = new Bitmap((int)(Math.Ceiling((Width + 31) / 32f) * 32), Height, m_pixelFormat);

            m_color = new Color[(m_pixelFormat == PixelFormat.Format8bppIndexed) ? 256 : 4096];
            m_colorReal = new Color[(m_pixelFormat == PixelFormat.Format8bppIndexed) ? 256 : 4096];

            l_dPaletteStepStep = (resColor.Length - 1) / (float)m_color.Length;

            for (int i = 0; i < m_color.Length; i++) m_color[m_color.Length - i - 1] = resColor[(int)Math.Ceiling(i * l_dPaletteStepStep)];

            for (int i = (m_pixelFormat == PixelFormat.Format8bppIndexed) ? 252 : 4032; i < m_color.Length; i++) m_color[i] = Color.FromArgb(255, 255, 255);

            m_sPaletteLevelUp = 100f;
            m_iLinePaletteUp = (Int16)(m_color.Length - 1);

            ChangePaletteLevel();
        }

        private void ChangePaletteLevel()
        {
            int i;

            for (i = 0; i < m_iLinePaletteUp; i++) m_colorReal[i] = m_color[(int)Math.Ceiling(i * 100 / m_sPaletteLevelUp)];
            for (; i < m_color.Length; i++) m_colorReal[i] = m_color[m_color.Length - 1];

            if ((m_pixelFormat == PixelFormat.Format8bppIndexed) && (m_bmpPalette != null))
            {
                ColorPalette pal = m_bmpPalette.Palette;

                for (i = 0; i < m_color.Length; i++) pal.Entries[i] = m_colorReal[i];

                m_bmpPalette.Palette = pal;
            }

            CreatePaletteImage();
        }

        private void CreatePaletteImage()
        {
            UInt16 uiColor;

            // Draw the modified image.
            if (m_bmpPalette != null)
            {
                try
                {
                    lock (m_lLockBMP)
                    {
                        if ((m_bmpPalette.Width > 0) && (m_bmpPalette.Height > 0))
                        {
                            // Lock the bitmap's bits.  
                            BitmapData bmpPaletteData = m_bmpPalette.LockBits(new Rectangle(0, 0, m_bmpPalette.Width, m_bmpPalette.Height),
                                                                                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                                                                                m_bmpPalette.PixelFormat);

                            // Declare an array to hold the bytes of the bitmap.
                            // This code is specific to a bitmap with 24 bits per pixels.
                            int numBytes = bmpPaletteData.Stride * m_bmpPalette.Height;
                            byte[] rgbValues = new byte[numBytes];

                            // Copy the RGB values into the array.
                            System.Runtime.InteropServices.Marshal.Copy(bmpPaletteData.Scan0, rgbValues, 0, numBytes);

                            double m_dCoeff = m_color.Length / (float)bmpPaletteData.Height;

                            for (int j = 0, iShift = 0; j < bmpPaletteData.Height; j++)
                            {
                                iShift = j * bmpPaletteData.Stride;
                                uiColor = (UInt16)(m_color.Length - 1 - Math.Ceiling(m_dCoeff * j));

                                if (bmpPaletteData.PixelFormat == PixelFormat.Format8bppIndexed)
                                {
                                    for (int i = 0; i < bmpPaletteData.Width; i++, iShift++) rgbValues[iShift] = (Byte)uiColor;
                                }
                                else
                                {
                                    for (int i = 0; i < bmpPaletteData.Width; i++, iShift += 3)
                                    {
                                        rgbValues[iShift] = m_colorReal[uiColor].B;
                                        rgbValues[iShift + 1] = m_colorReal[uiColor].G;
                                        rgbValues[iShift + 2] = m_colorReal[uiColor].R;
                                    }
                                }
                            }

                            // Copy the RGB values back to the bitmap
                            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpPaletteData.Scan0, numBytes);

                            // Unlock the bits.
                            m_bmpPalette.UnlockBits(bmpPaletteData);
                        }
                    }

                    Invalidate();
                }
                catch
                {
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int CorrectY = m_bmpPalette.Height - e.Y;

                if (Math.Abs(CorrectY - ((m_bmpPalette.Height * m_sPaletteLevelUp / 100f))) < M_DELTA)
                {
                    m_bStartMoveLevelUp = true;
                    PaletteLevelUp = (float)(CorrectY * 100 / (float)m_bmpPalette.Height);

                    this.Cursor = System.Windows.Forms.Cursors.HSplit;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((e.Button == System.Windows.Forms.MouseButtons.Left) && (m_bStartMoveLevelUp == true))
            {
                int CorrectY = m_bmpPalette.Height - e.Y;

                PaletteLevelUp = (float)(CorrectY * 100 / (float)m_bmpPalette.Height);
            }
            else
            {
                int CorrectY = m_bmpPalette.Height - e.Y;
                this.Cursor = (Math.Abs(CorrectY - ((m_bmpPalette.Height * m_sPaletteLevelUp / 100f))) < M_DELTA) ? System.Windows.Forms.Cursors.HSplit : System.Windows.Forms.Cursors.Default;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_bStartMoveLevelUp = false;
            this.Cursor = System.Windows.Forms.Cursors.Default;
            base.OnMouseUp(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            PaletteLevelUp = 100f;

            base.OnDoubleClick(e);
        }

        protected override void OnResize(EventArgs e)
        {
            m_bmpPalette = new Bitmap((int)(Math.Ceiling((Width + 31) / 32f) * 32), Height, m_pixelFormat);

            ChangePaletteLevel();

            CreatePaletteImage();

            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics grfx = e.Graphics;

            if (m_bmpPalette != null)
            {
                try
                {
                    lock (m_lLockBMP)
                    {
                        grfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                        grfx.DrawImage(m_bmpPalette, new Rectangle(Point.Empty, m_bmpPalette.Size));
                    }
                }
                catch
                {
                }
            }
        }
    }
}
