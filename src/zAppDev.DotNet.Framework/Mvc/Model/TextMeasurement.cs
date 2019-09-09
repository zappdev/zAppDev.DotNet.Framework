// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

using System.ComponentModel;

using UnitType = MigraDoc.DocumentObjectModel.UnitType;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class TextMeasurement
    {
        public MigraDoc.DocumentObjectModel.Font Font
        {
            get => this.font;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (font != value)
                {
                    font = value;
                    gdiFont = null;
                }
            }
        }
        MigraDoc.DocumentObjectModel.Font font;

        System.Drawing.Font gdiFont;
        System.Drawing.Graphics graphics;

        public TextMeasurement(MigraDoc.DocumentObjectModel.Font font)
        {
            this.font = font ?? throw new ArgumentNullException("font");
        }

        public System.Drawing.SizeF MeasureString(string text, UnitType unitType)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (!Enum.IsDefined(typeof(UnitType), unitType))
                throw new InvalidEnumArgumentException();

            System.Drawing.Graphics graphics = Realize();

            System.Drawing.SizeF size = graphics.MeasureString(text, gdiFont, new System.Drawing.PointF(0, 0), System.Drawing.StringFormat.GenericTypographic);
            switch (unitType)
            {
                case UnitType.Point:
                    break;

                case UnitType.Centimeter:
                    size.Width = (float)(size.Width * 2.54 / 72);
                    size.Height = (float)(size.Height * 2.54 / 72);
                    break;

                case UnitType.Inch:
                    size.Width = size.Width / 72;
                    size.Height = size.Height / 72;
                    break;

                case UnitType.Millimeter:
                    size.Width = (float)(size.Width * 25.4 / 72);
                    size.Height = (float)(size.Height * 25.4 / 72);
                    break;

                case UnitType.Pica:
                    size.Width = size.Width / 12;
                    size.Height = size.Height / 12;
                    break;

                default:
                    break;
            }
            return size;
        }

        System.Drawing.Graphics Realize()
        {
            if (graphics == null)
                graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

            graphics.PageUnit = System.Drawing.GraphicsUnit.Point;

            if (gdiFont == null)
            {
                System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
                if (font.Bold)
                    style |= System.Drawing.FontStyle.Bold;
                if (font.Italic)
                    style |= System.Drawing.FontStyle.Italic;

                gdiFont = new System.Drawing.Font(font.Name, font.Size, style, System.Drawing.GraphicsUnit.Point);
            }
            return graphics;
        }
    }
}
