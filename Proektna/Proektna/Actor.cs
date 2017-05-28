using Proektna.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proektna
{
    [Serializable]
    public class Actor : IDrawable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public int TextSize { get; set; }
        public Color TextColor { get; set; }
        public bool drawn { get; set; }
        public bool Selected { get; set; }
        public Color BorderColor { get; set; }
        public Color FillColor { get; set; }
        public int BorderWidth { get; set; }
        public DashStyle DashStyle { get; set; }

        public Actor(int x,int y,Color textColor)
        {
            X = x;
            Y = y;
            Image = Resources._500px_Stick_Figure_svg;
            Width = 50;
            Height = 130;
            TextSize = 10;
            TextColor = textColor;
            Text = "Text ...";
            drawn = false;
        }
        public Actor(Actor o)
        {
            X = o.X;
            Y = o.Y;
            Width = o.Width;
            Height = o.Height;
            Text = o.Text;
            TextColor = o.TextColor;
            TextSize = o.TextSize;
            drawn = o.drawn;
            Image = Resources._500px_Stick_Figure_svg;
        }
        public void Draw(Graphics g)
        {
            int imgH = (int)(Height * 0.8);
            int textH = Height - imgH;
            Brush textBrush = new SolidBrush(TextColor);
            g.DrawImage(Image, X, Y, Width, imgH);
            Font f = new Font("Arial", TextSize);
            g.DrawString(Text, f, textBrush, X,Y+imgH+10);
            textBrush.Dispose();
            if(Selected)
            {
                Pen p = new Pen(Color.Black);
                p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawRectangle(p, X, Y, Width, Height);

                Point endPoint = new Point(X + Width, Y + Height);
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
            {
                Selected = r.Contains(p);
            }
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
                Selected=r.Contains(p);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Actor))
                return false;
            Actor a = obj as Actor;
            if (X != a.X)
                return false;
            if (Y != a.Y)
                return false;
            if (Width != a.Width)
                return false;
            if (Height != a.Height)
                return false;
            if (Text != a.Text)
                return false;
            if (TextColor != a.TextColor)
                return false;
            if (TextSize != a.TextSize)
                return false;
            if (drawn != a.drawn)
                return false;
            return true;
        }
    }
}
