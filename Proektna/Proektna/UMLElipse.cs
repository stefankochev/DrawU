using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proektna
{
    [Serializable]
    public class UMLElipse : IDrawable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public int TextSize { get; set; }
        public bool Selected { get; set; }
        public int BorderWidth { get; set; }
        public DashStyle DashStyle { get; set; }

        public UMLElipse(int x,int y,Color BorderColor, Color FillColor, Color TextColor)
        {
            X = x;
            Y = y;
            Width = 130;
            Height = 80;
            this.BorderColor = BorderColor;
            this.FillColor = FillColor;
            Text = "Text ";
            this.TextColor = TextColor;
            TextSize = 14;
            BorderWidth = 2;
        }
        public UMLElipse(UMLElipse o)
        {
            X = o.X;
            Y = o.Y;
            Width = o.Width;
            Height = o.Height;
            BorderColor = o.BorderColor;
            FillColor = o.FillColor;
            Text = o.Text;
            TextColor = o.TextColor;
            TextSize = o.TextSize;
            BorderWidth = o.BorderWidth;
        }
        public void Draw(Graphics g)
        {
            SolidBrush b = new SolidBrush(FillColor);
            Pen pen = new Pen(BorderColor, BorderWidth);
            Brush textBrush = new SolidBrush(TextColor);
            g.FillEllipse(b, X, Y, Width, Height);
            g.DrawEllipse(pen, X, Y, Width, Height);
            Font f = new Font("Arial", TextSize);
            float textLenght = g.MeasureString(Text, f).Width;
            float textHeight = g.MeasureString(Text, f).Height;
            float offX = (Width - textLenght) / 2;
            float offY = (Height - textHeight) / 2;
            g.DrawString(Text, f, textBrush, X + offX, Y + offY);
            b.Dispose();
            pen.Dispose();
            textBrush.Dispose();

            if (Selected)
            {
                Pen p = new Pen(Color.Black);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                Rectangle rect = new Rectangle(X - BorderWidth / 2, Y - BorderWidth / 2, Width + BorderWidth, Height + BorderWidth);
                g.DrawRectangle(p, rect);

                Point endPoint = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                SolidBrush brush = new SolidBrush(Color.Red);
                g.FillRectangle(brush, endPoint.X - 4, endPoint.Y - 4, 8, 8);

                p.Dispose();
                brush.Dispose();
            }
        }

        public void isSelected(Point p,bool ctrl)
        {
            Rectangle r = new Rectangle(X, Y, Width, Height);
            if (ctrl)
            {
                if (r.Contains(p))
                    Selected = !Selected;
            }
            else
                Selected = r.Contains(p);
        }

        public void Move(int dx, int dy)
        {
            X += dx;
            Y += dy;
        }

        public bool isClicked(Point p)
        {
            Rectangle r = new Rectangle(X, Y, Width, Height);
            return r.Contains(p);
        }

        public bool IsResizeSquareClicked(Point click)
        {
            if (Selected)
            {
                Point endPoint = new Point(X + Width, Y + Height);
                Rectangle resize = new Rectangle(endPoint.X - 4, endPoint.Y - 4, 8, 8);
                if (resize.Contains(click))
                    return true;
            }

            return false;
        }

        public void Resize(int dx, int dy)
        {
            Height = Math.Max(10, Height + dy);
            Width = Math.Max(10, Width + dx);
        }

        public bool IsFirstPointClicked(Point click)
        {
            return false;
        }

        public void FirstPointResize(int dx, int dy)
        {
            Point endPoint = new Point(X + Width, Y + Height);
            X += dx;
            Y += dy;
            Width = Math.Max(10, endPoint.X - X);
            Height = Math.Max(10, endPoint.Y - Y);
        }

        public void selectRectangle(Point p)
        {
            Rectangle r = new Rectangle(X, Y, Width, Height);
            if(!Selected)
                Selected= r.Contains(p);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is UMLElipse))
                return false;
            UMLElipse e = obj as UMLElipse;
            if (X != e.X)
                return false;
            if (Y != e.Y)
                return false;
            if (Width != e.Width)
                return false;
            if (Height != e.Height)
                return false;
            if (BorderColor != e.BorderColor)
                return false;
            if (FillColor != e.FillColor)
                return false;
            if (Text != e.Text)
                return false;
            if (TextColor != e.TextColor)
                return false;
            if (TextSize != e.TextSize)
                return false;
            if (BorderWidth != e.BorderWidth)
                return false;
            return true;
        }
    }
}
