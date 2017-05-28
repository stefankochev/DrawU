using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proektna
{
    public interface IDrawable
    {
        string Text { get; set; }
        int X { get; set; }
        int Y { get; set; }
        void Draw(Graphics g);
        void isSelected(Point p,bool ctrl);
        bool isClicked(Point p);
        //void ResizeRectClicked(Point p);
        void Move(int dx,int dy);
        bool Selected { get; set; }
        bool IsResizeSquareClicked(Point click);
        void Resize(int dx, int dy);
        bool IsFirstPointClicked(Point click);
        void FirstPointResize(int dx, int dy);
        Color BorderColor { get; set; }
        Color TextColor { get; set; }
        Color FillColor { get; set; }
        int BorderWidth { get; set; }
        void selectRectangle(Point p);
        int Height { get; set; }
        int Width { get; set; }
        DashStyle DashStyle { get; set; }
    }
}
