using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proektna
{
    /// <summary>
    /// Used in the Line class to indicate whether the line should be normal or dashed.
    /// </summary>
    public enum Type { Solid, Dashed }

    /// <summary>
    ///  Line contains all the necessary attributes for describing a line in the program.
    ///  It also contains methods for drawing, moving, resizing the line and helper methods
    ///  for achieving these functionalities.
    /// </summary>

    [Serializable]
    public class Line : IDrawable
    {
        public int X1 { get; set; }                                         //position along x-axis of the first point
        public int Y1 { get; set; }                                         //position along y-axis of the first point
        public int X2 { get; set; }                                         //position along y-axis of the second point
        public int Y2 { get; set; }                                         //position along y-axis of the second point     
        public Type Type { get; set; }                                      //Type of the line. Solid or Dashed
        public string Text { get; set; }                                    //Text describing the relationship between the two elements connected with the line
        public int TextSize { get; set; }                                   //Size with which to write the text
        public bool Selected { get; set; }                                  //Indicates whether the line is selected or not
        public Color TextColor { get; set; }                                //Color of text
        public Color BorderColor { get; set; }                              //Color of line
        public Color FillColor { get; set; }
        public int BorderWidth { get; set; }                                //Thickness of line
        public int X { get { return Math.Min(X1, X2); } set { } }           //position along x-axis of the leftmost point
        public int Y { get { return Math.Min(Y1, Y2); } set { } }           //position along y-axis of the upmost point
        public int Height { get { return Math.Abs(Y1 - Y2); } set { } }     //distance along y-axis between the two points
        public int Width { get { return Math.Abs(X1 - X2); } set { } }      //distance along x-axis between the two points
        public DashStyle DashStyle { get; set; }


        public Line(int x1, int y1, int x2, int y2, Color c, Color t)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            BorderColor = c;
            TextColor = t;
            Type = Type.Solid;
            Text = "Text ...";
            TextSize = 8;
            BorderWidth = 3;
            X = X1;
            Y = Y1;
            this.DashStyle = DashStyle.Solid;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="l">Original line which is copied</param>
        public Line(Line l)
        {
            X1 = l.X1;
            Y1 = l.Y1;
            X2 = l.X2;
            Y2 = l.Y2;
            BorderColor = l.BorderColor;
            TextColor = l.TextColor;
            Type = l.Type;
            Text = l.Text;
            TextSize = l.TextSize;
            BorderWidth = l.BorderWidth;
            DashStyle = l.DashStyle;
        }

        /// <summary>
        /// Method for drawing the line on a given Graphics object. Draws the line between the two points, draws
        /// arrow shape at the second point and writes the text in the middle underneath the line.
        /// </summary>
        /// <param name="g">Where the line is drawn</param>
        public void Draw(Graphics g)
        {
            Pen pen = new Pen(BorderColor, BorderWidth);
            AdjustableArrowCap cap = new AdjustableArrowCap((float)Math.Log(BorderWidth) + 2, (float)Math.Log(BorderWidth) + 2); // The arrow shape at the end of the line with size corelated with the width of the line
            pen.CustomEndCap = cap;
            pen.DashStyle = this.DashStyle;
            if (Type == Type.Dashed)
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            g.DrawLine(pen, X1, Y1, X2, Y2);

            Font f = new Font("Arial", TextSize);
            SolidBrush b = new SolidBrush(TextColor);

            float theta = angle();                      // Angle between the line and x-axis
            float thetaRad = angle(X1, Y1, X2, Y2);     // The angle in Rad for calculating Sin() and Cos()
            float thetaRad1 = thetaRad + 90;            // The direction in which the text is to be moved by the width of the line
            if (theta > 90 || theta < -90)
                theta += 180;                           // So the text isn't upside-down

            // Calculating the offset between the leftmost point and the begining of the text
            float textSize = g.MeasureString(Text, f).Width;
            float length = (float)Math.Sqrt((X1 - X2) * (X1 - X2) + (Y1 - Y2) * (Y1 - Y2));
            float offset = (length - TextSize) / 2;
            float sin = (float)(Math.Sin(thetaRad));
            float cos = (float)(Math.Cos(thetaRad));
            float dx = (float)(cos * offset);
            float dy = (float)(sin * offset);

            float sin1 = (float)(Math.Sin(thetaRad1));
            float cos1 = (float)(Math.Cos(thetaRad1));
            float dx1 = (cos1 * BorderWidth / 2);
            float dy1 = (sin1 * BorderWidth / 2);

            int x, y; // Determining leftmost point
            if (X1 < X2)
            {
                x = X1;
                y = Y1;
            }
            else
            {
                x = X2;
                y = Y2;
            }

            DrawRotatedTextAt(g, theta, Text, (int)(x + dx + dx1), (int)(y + dy + dy1), f, b);
            pen.Dispose();

            if (Selected)   //If the line is selected draw red squares at each point for resizing
            {
                SolidBrush brush = new SolidBrush(Color.Red);
                g.FillRectangle(brush, X1 - 3, Y1 - 3, 6, 6);
                g.FillRectangle(brush, X2 - 3, Y2 - 3, 6, 6);
                brush.Dispose();
            }
        }

        /// <summary>
        /// Calculating the angle between the line and the x-axis.
        /// Used in the Draw() method.
        /// </summary>
        /// <returns>The angle between the line and the x-axis in degrees</returns>
        public float angle()
        {
            double Rad2Deg = 180.0 / Math.PI;
            return (float)(Math.Atan2(Y2 - Y1, X2 - X1) * Rad2Deg);
        }

        /// <summary>
        /// Calculating the angle between the line defined by two points given as arguments
        /// and the x-axis
        /// </summary>
        /// <returns>The angle between the line and the x-axis in Radians</returns>
        private float angle(int x1, int y1, int x2, int y2)
        {
            if (x1 < x2)
                return (float)(Math.Atan2(y2 - y1, x2 - x1));
            else
                return (float)(Math.Atan2(y1 - y2, x1 - x2));
        }

        /// <summary>
        /// Drawing text at specified angle
        /// </summary>
        /// <param name="gr">Where to draw text</param>
        /// <param name="angle">At which angle to draw</param>
        /// <param name="txt">The text to be drawn</param>
        /// <param name="x">Position along x-axis</param>
        /// <param name="y">Position along y-axis</param>
        /// <param name="the_font">Font of the text</param>
        /// <param name="the_brush">The brush used for drawing</param>
        private void DrawRotatedTextAt(Graphics gr, float angle, string txt, int x, int y, Font the_font, Brush the_brush)
        {
            // Save the graphics state.
            GraphicsState state = gr.Save();
            gr.ResetTransform();

            // Rotate.
            gr.RotateTransform(angle);

            // Translate to desired position. Be sure to append
            // the rotation so it occurs after the rotation.
            gr.TranslateTransform(x, y, MatrixOrder.Append);

            // Draw the text at the origin.
            gr.DrawString(txt, the_font, the_brush, 0, 0);

            // Restore the graphics state.
            gr.Restore(state);
        }

        /// <summary>
        /// Selecting/Unselecting the line on click
        /// </summary>
        /// <param name="p">Location of the click</param>
        /// <param name="ctrl">Shows if Control key is held</param>
        public void isSelected(Point p, bool ctrl)
        {
            var isOnLine = false;
            using (var path = new GraphicsPath())
            {
                using (var pen = new Pen(Brushes.Black, BorderWidth))
                {
                    path.AddLine(X1, Y1, X2, Y2);
                    isOnLine = path.IsOutlineVisible(p, pen);       // Determine if the click is on the line
                }
            }
            if (ctrl)
            {
                if (isOnLine)
                    Selected = !Selected;    // If Control is held reverse the Selected property
            }
            else
                Selected = isOnLine;         // Else if clicked Selected is true, if not Selected is false
        }

        /// <summary>
        /// Move both points by the given margins
        /// </summary>
        /// <param name="dx">Distance to move along x-axis</param>
        /// <param name="dy">Distance to move along y-axis</param>
        public void Move(int dx, int dy)
        {
            X1 += dx;
            X2 += dx;
            Y1 += dy;
            Y2 += dy;
            X = X1;
            Y = Y1;
        }

        /// <summary>
        /// Determine whether a click is on the line
        /// </summary>
        /// <param name="p">Location of the click</param>
        /// <returns>True: the location is on the line, False: the location is not on the line</returns>
        public bool isClicked(Point p)
        {
            var isOnLine = false;
            using (var path = new GraphicsPath())
            {
                using (var pen = new Pen(Brushes.Black, 3))
                {
                    path.AddLine(X1, Y1, X2, Y2);
                    isOnLine = path.IsOutlineVisible(p, pen);
                }
            }
            return isOnLine;
        }

        /// <summary>
        /// Determine whether the resize square at the second point is clicked.
        /// Used for resizing by moving the second point.
        /// </summary>
        /// <param name="click">Location of the click</param>
        /// <returns>True: the click was above the square, False: the click wasn't above the square</returns>
        public bool IsResizeSquareClicked(Point click)
        {
            if (Selected)
            {
                Rectangle resize = new Rectangle(X2 - 3, Y2 - 3, 6, 6);
                if (resize.Contains(click))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determine whether the resize square at the first point is clicked.
        /// Used for resizing by moving the first point.
        /// </summary>
        /// <param name="click">Location of the click</param>
        /// <returns>True: the click was above the square, False: the click wasn't above the square</returns>
        public bool IsFirstPointClicked(Point click)
        {
            if (Selected)
            {
                Rectangle resize = new Rectangle(X1 - 3, Y1 - 3, 6, 6);
                if (resize.Contains(click))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Move the second point
        /// </summary>
        /// <param name="dx">Distance to move along x-axis</param>
        /// <param name="dy">Distance to move along y-axis</param>
        public void Resize(int dx, int dy)
        {
            X2 += dx;
            Y2 += dy;
        }

        /// <summary>
        /// Move the first point
        /// </summary>
        /// <param name="dx">Distance to move along x-axis</param>
        /// <param name="dy">Distance to move along y-axis</param>
        public void FirstPointResize(int dx, int dy)
        {
            X1 += dx;
            Y1 += dy;
            X = X1;
            Y = Y1;
        }

        /// <summary>
        /// Shows whether a certain point from the select square in the main form
        /// is on the line.
        /// </summary>
        /// <param name="p">Location of the point</param>
        public void selectRectangle(Point p)
        {
            var isOnLine = false;
            using (var path = new GraphicsPath())
            {
                using (var pen = new Pen(Brushes.Black, BorderWidth))
                {
                    path.AddLine(X1, Y1, X2, Y2);
                    isOnLine = path.IsOutlineVisible(p, pen);
                }
            }
            if (!Selected)
                Selected = isOnLine;
        }
        public override bool Equals(object obj)         //Method to compare two objects if they are
        {                                               //from the same class and have the same attributes    
            if (!(obj is Line))
                return false;
            Line l = obj as Line;
            if (X1 != l.X1)
                return false;
            if (Y1 != l.Y1)
                return false;
            if (X2 != l.X2)
                return false;
            if (Y2 != l.Y2)
                return false;
            if (BorderColor != l.BorderColor)
                return false;
            if (TextColor != l.TextColor)
                return false;
            if (Type != l.Type)
                return false;
            if (Text != l.Text)
                return false;
            if (TextSize != l.TextSize)
                return false;
            if (BorderWidth != l.BorderWidth)
                return false;
            return true;
        }
    }
}
