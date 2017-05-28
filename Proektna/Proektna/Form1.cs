using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proektna
{
    public partial class Form1 : Form
    {
        Bitmap buffer;
        bool actorStripSelected;
        bool actorToRedraw;
        bool lineStripSelected;
        bool ellipseStripSelected;
        UseCaseDiagramDocument Elements;
        Point firstPointDragStart;
        Point dragStart;
        Point resizeDragStart;
        IDrawable currentShape;
        bool ctrlDown;
        Color FillColor;
        Color BorderColor;
        Color TextColor;
        Point startPointSelectRectangle;
        Rectangle selectRectangle;
        bool previouslyShapeSelected;
        bool previousCtrlDown;
        IDrawable lastClickedShape;
        Color toolStripNotHover;
        Color toolStripHover;
        List<IDrawable> copyList;
        string fileName;
        UseCaseDiagramDocument undoElements;
        UseCaseDiagramDocument redoElements;
        bool undoDone;
        bool redoDone;
        public Form1()
        {
            InitializeComponent();
            actorStripSelected = false;
            lineStripSelected = false;
            ellipseStripSelected = false;
            Elements = new UseCaseDiagramDocument();
            firstPointDragStart = Point.Empty;
            dragStart = Point.Empty;
            resizeDragStart = Point.Empty;
            currentShape = null;
            FillColor = Color.YellowGreen;
            BorderColor = Color.CadetBlue;
            TextColor = Color.DarkOliveGreen;
            toolStripbtnFillColor.BackColor = FillColor;
            toolStripbtnBorderColor.BackColor = BorderColor;
            toolStripbtnTextColor.BackColor = TextColor;
            startPointSelectRectangle = Point.Empty;
            selectRectangle = Rectangle.Empty;
            previousCtrlDown = false;
            lastClickedShape = null;
            toolStripHover = Color.LightGray;
            toolStripNotHover = Color.WhiteSmoke;
            toolStripMobile.BackColor = toolStripNotHover;
            copyList = new List<IDrawable>();
            toolStripbtnFillColor.ToolTipText = "Fill Color";
            toolStripbtnBorderColor.ToolTipText = "Border Color";
            toolStripbtnTextColor.ToolTipText = "Text Color";
            undoElements = new UseCaseDiagramDocument();
            undoDone = true;
            redoDone = true;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (selectRectangle != Rectangle.Empty)
            {
                Brush selectRectangleBrush = new SolidBrush(Color.AliceBlue);
                Pen selectRectanglePen = new Pen(Color.Black, 1);
                selectRectanglePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.FillRectangle(selectRectangleBrush, selectRectangle);
                e.Graphics.DrawRectangle(selectRectanglePen, selectRectangle);
                selectRectangleBrush.Dispose();
                selectRectanglePen.Dispose();
            }
            Elements.Draw(e.Graphics);
            if (currentShape != null)
                currentShape.Draw(e.Graphics);
        }
        private void unselectToolStrips()
        {
            actorStripSelected = false;
            lineStripSelected = false;
            ellipseStripSelected = false;
            this.Cursor = Cursors.Default;
        }
        private void toolStripActor_Click(object sender, EventArgs e)
        {
            unselectToolStrips();
            actorStripSelected = true;
            this.Cursor = Cursors.Hand;
            unselectShapes();
            nudBorderWidth.Value = 2;
            toolStripMobile.Visible = false;
        }

        private void toolStripLine_Click(object sender, EventArgs e)
        {
            unselectToolStrips();
            lineStripSelected = true;
            this.Cursor = Cursors.Hand;
            unselectShapes();
            nudBorderWidth.Value = 2;
            toolStripMobile.Visible = false;
        }

        private void toolStripEllipse_Click(object sender, EventArgs e)
        {
            unselectToolStrips();
            ellipseStripSelected = true;
            this.Cursor = Cursors.Hand;
            unselectShapes();
            nudBorderWidth.Value = 2;
            toolStripMobile.Visible = false;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            if (startPointSelectRectangle != Point.Empty)
            {
                int minX = Math.Min(startPointSelectRectangle.X, e.X);
                int minY = Math.Min(startPointSelectRectangle.Y, e.Y);
                selectRectangle = new Rectangle(minX, minY, Math.Abs(startPointSelectRectangle.X - e.X), Math.Abs(startPointSelectRectangle.Y - e.Y));
                Invalidate(true);
            }
            else
                selectRectangle = Rectangle.Empty;
            if (actorStripSelected)
            {
                currentShape = new Actor(e.X, e.Y, TextColor);
                Invalidate(true);
            }
            else if (lineStripSelected)
            {
                currentShape = new Line(e.X, e.Y, e.X + 70, e.Y + 20, BorderColor, TextColor);
                Invalidate(true);
            }
            else if (ellipseStripSelected)
            {
                currentShape = new UMLElipse(e.X, e.Y, BorderColor, FillColor, TextColor);
                Invalidate(true);
            }
            else if (dragStart != Point.Empty)
            {
                Elements.Move(dragStart, e.Location);
                foreach(IDrawable obj in Elements.Elements)
                    EnlargeIfNeeded(obj);
                if (toolStripMobile.Visible)
                {
                    Point l = toolStripMobile.Location;
                    l.X += e.X - dragStart.X;
                    l.Y += e.Y - dragStart.Y;
                    toolStripMobile.Location = l;
                }
                dragStart = e.Location;
            }
            else if (resizeDragStart != Point.Empty)
            {
                int dx = e.X - resizeDragStart.X;
                int dy = e.Y - resizeDragStart.Y;
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                    {
                        obj.Resize(dx, dy);
                        EnlargeIfNeeded(obj);
                    }
                resizeDragStart = e.Location;
                
            }
            else if (firstPointDragStart != Point.Empty)
            {
                int dx = e.X - firstPointDragStart.X;
                int dy = e.Y - firstPointDragStart.Y;
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                    {
                        obj.FirstPointResize(dx, dy);
                        EnlargeIfNeeded(obj);
                    }
                firstPointDragStart = e.Location;
            }
            else
            {
                bool resizeCursor = false;
                bool isLine = false;
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.IsResizeSquareClicked(e.Location) || obj.IsFirstPointClicked(e.Location))
                    {
                        resizeCursor = true;
                        if (obj is Line)
                            isLine = true;
                    }
                if (resizeCursor && !ctrlDown)
                    if (isLine)
                        this.Cursor = Cursors.SizeAll;
                    else
                        this.Cursor = Cursors.SizeNWSE;
                else
                    this.Cursor = Cursors.Default;
            }
            Invalidate(true);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            if (currentShape != null)
            {
                undoElements = new UseCaseDiagramDocument(Elements);
                undoDone = false;
                unselectShapes();
                currentShape.Selected = true;
                Elements.Add(currentShape);
                EnlargeIfNeeded(currentShape);
                currentShape = null;
                unselectToolStrips();
            }
            else
            {
                bool Moveflag = false;
                bool resizeFlag = false;
                bool lineFirstPoint = false;
                bool anyShapeClicked = false;
                foreach (IDrawable obj in Elements.Elements)
                {
                    if (obj.IsFirstPointClicked(e.Location) && !ctrlDown)
                    {
                        undoElements = new UseCaseDiagramDocument(Elements);
                        undoDone = false;
                        firstPointDragStart = e.Location;
                        lineFirstPoint = true;
                    }
                    else if (obj.IsResizeSquareClicked(e.Location) && !ctrlDown)
                    {
                        undoElements = new UseCaseDiagramDocument(Elements);
                        undoDone = false;
                        resizeDragStart = e.Location;
                        resizeFlag = true;
                    }
                    else if (obj.Selected && obj.isClicked(e.Location) && !ctrlDown)
                    {
                        undoElements = new UseCaseDiagramDocument(Elements);
                        undoDone = false;
                        Moveflag = true;
                    }
                }
                if (!resizeFlag && !lineFirstPoint)
                {
                    if (Moveflag)
                    {
                        undoElements = new UseCaseDiagramDocument(Elements);
                        undoDone = false;
                        dragStart = e.Location;
                        this.Cursor = Cursors.SizeAll;
                    }
                    else
                        Elements.Selected(e.Location, ctrlDown);
                }
                if (!ctrlDown && !Moveflag && !resizeFlag)
                {
                    foreach (IDrawable obj in Elements.Elements)
                    {
                        obj.isSelected(e.Location, false);
                        if (obj.Selected)
                        {
                            anyShapeClicked = true;
                            previouslyShapeSelected = true;
                        }
                    }
                    if (!anyShapeClicked)
                        startPointSelectRectangle = e.Location;
                }
            }

            lastClickedShape = null;
            foreach (IDrawable obj in Elements.Elements)
            {
                if (obj.isClicked(e.Location))
                {
                    lastClickedShape = obj;
                }
            }
            if (lastClickedShape != null)
            {
                Point location = new Point(lastClickedShape.X , lastClickedShape.Y -30 );
                location.X -= panel1.HorizontalScroll.Value;
                location.Y -= panel1.VerticalScroll.Value;
                location.X = Math.Max(location.X, 0);
                toolStripMobile.Location = location;
                toolStripMobileChangeText.Text = lastClickedShape.Text;
                toolStripMobilecbBorderWidth.Text = lastClickedShape.BorderWidth.ToString();
                toolStripMobilebtnBorderColor.Enabled = true;
                toolStripMobilebtnFillColor.Enabled = true;
                if (lastClickedShape.GetType() == typeof(Actor))
                {
                    toolStripMobilebtnBorderColor.Enabled = false;
                    toolStripMobilebtnFillColor.Enabled = false;
                }
                else if (lastClickedShape.GetType() == typeof(Line))
                {
                    toolStripMobilebtnFillColor.Enabled = false;
                }         
                toolStripMobile.Visible = true;
            }
            else
            {
                toolStripMobile.Visible = false;
            }

            Invalidate(true);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                ctrlDown = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                unselectShapes();
                unselectToolStrips();
                currentShape = null;
            }
            else if (e.KeyCode == Keys.Left)
            {
                undoElements = new UseCaseDiagramDocument(Elements);
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.Move(-10, 0);
                toolStripMobile.Location = new Point(toolStripMobile.Location.X - 10, toolStripMobile.Location.Y);
            }
            else if (e.KeyCode == Keys.Right)
            {
                undoElements = new UseCaseDiagramDocument(Elements);
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                    {
                        obj.Move(10, 0);
                        EnlargeIfNeeded(obj);
                    }
                toolStripMobile.Location = new Point(toolStripMobile.Location.X + 10, toolStripMobile.Location.Y);
            }
            else if (e.KeyCode == Keys.Up)
            {
                undoElements = new UseCaseDiagramDocument(Elements);
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.Move(0, -10);
                toolStripMobile.Location = new Point(toolStripMobile.Location.X, toolStripMobile.Location.Y - 10);
            }
            else if (e.KeyCode == Keys.Down)
            {
                undoElements = new UseCaseDiagramDocument(Elements);
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                    {
                        obj.Move(0, 10);
                        EnlargeIfNeeded(obj);
                    }
                toolStripMobile.Location = new Point(toolStripMobile.Location.X, toolStripMobile.Location.Y + 10);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                deleteSelected();
            }

            Invalidate(true);
        }

        private void unselectShapes()
        {
            foreach (IDrawable obj in Elements.Elements)
            {
                obj.Selected = false;
            }
            toolStripMobile.Visible = false;
        }

        private void deleteSelected()
        {
            List<IDrawable> toDelete = new List<IDrawable>();
            foreach (IDrawable obj in Elements.Elements)
                if (obj.Selected)
                    toDelete.Add(obj);
            foreach (IDrawable del in toDelete)
                Elements.Elements.Remove(del);
            toolStripMobile.Visible = false;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
                copy();
            else if (e.Control && e.KeyCode == Keys.V)
                paste();
            else if (e.Control && e.KeyCode == Keys.X)
                cut();
            else if (e.Control && e.KeyCode == Keys.Z)
                undo();
            else if (e.Control && e.KeyCode==Keys.Y)
                redo();
            ctrlDown = false;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                return;
            UseCaseDiagramDocument docCopy = new UseCaseDiagramDocument(Elements);
            if (selectRectangle != Rectangle.Empty)
            {
                for (int x = selectRectangle.X; x < selectRectangle.X + selectRectangle.Width; x++)
                {
                    if (docCopy.Elements.Count == 0)
                        break;
                    for (int y = selectRectangle.Y; y < selectRectangle.Y + selectRectangle.Height; y++)
                    {
                        if (docCopy.Elements.Count == 0)
                            break;
                        for (int i = docCopy.Elements.Count - 1; i >= 0; i--)
                        {
                            int index = Elements.Elements.IndexOf(docCopy.Elements[i]);
                            if (index != -1)
                            {
                                Elements.Elements[index].isSelected(new Point(x, y), true);
                                if (Elements.Elements[index].Selected)
                                    docCopy.Elements.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            Invalidate(true);
            startPointSelectRectangle = Point.Empty;
            selectRectangle = Rectangle.Empty;
            dragStart = Point.Empty;
            resizeDragStart = Point.Empty;
            firstPointDragStart = Point.Empty;
            this.Cursor = Cursors.Default;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            unselectToolStrips();
            currentShape = null;
            unselectShapes();
            nudBorderWidth.Value = 2;
            toolStripMobile.Visible = false;
            Invalidate(true);
        }

        /// <summary>
        /// Setting the background color of selected elements
        /// </summary>
        private void toolStripbtnFillColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                FillColor = colorDialog1.Color;                 // The color picked from the ColorDialog
                toolStripbtnFillColor.BackColor = FillColor;    // Change the color of the button
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)                           // If the element is selected set its fill color
                        obj.FillColor = FillColor;
            }
        }

        private void toolStripbtnBorderColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                BorderColor = colorDialog1.Color;
                toolStripbtnBorderColor.BackColor = BorderColor;
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.BorderColor = BorderColor;
            }
        }

        private void toolStripbtnTextColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                TextColor = colorDialog1.Color;
                toolStripbtnTextColor.BackColor = TextColor;
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.TextColor = TextColor;
            }
        }

        private void nudBorderWidth_ValueChanged(object sender, EventArgs e)
        {
            foreach (IDrawable obj in Elements.Elements)
                if (obj.Selected)
                    obj.BorderWidth = (int)nudBorderWidth.Value;
            toolStripMobilecbBorderWidth.Text = nudBorderWidth.Value.ToString();
            Invalidate(true);
        }

        private void nudBorderWidth_KeyDown(object sender, KeyEventArgs e)
        {
            Form1_KeyDown(sender, e);
        }

        private void nudBorderWidth_KeyUp(object sender, KeyEventArgs e)
        {
            Form1_KeyUp(sender, e);
        }

        private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<IDrawable> selectedItems = new List<IDrawable>();
            foreach(IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    selectedItems.Add(obj);
                }
            }
            foreach(IDrawable obj in selectedItems)
            {
                Elements.Elements.Remove(obj);
            }
            foreach(IDrawable obj in selectedItems)
            {
                Elements.Elements.Add(obj);
            }
        }

        private void bringToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<IDrawable> selectedItems = new List<IDrawable>();
            foreach (IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    selectedItems.Add(obj);
                }
            }
            foreach (IDrawable obj in selectedItems)
            {
                Elements.Elements.Remove(obj);
            }
            foreach (IDrawable obj in selectedItems)
            {
                Elements.Elements.Insert(0, obj);
            }
        }

        private void toolStripMobilebtnFillColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.FillColor = colorDialog1.Color;
            }
            Invalidate(true);
        }

        private void toolStripMobilebtnBorderColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.BorderColor = colorDialog1.Color;
            }
            Invalidate(true);
        }

        private void toolStripMobilebtnTextColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (IDrawable obj in Elements.Elements)
                    if (obj.Selected)
                        obj.TextColor = colorDialog1.Color;
            }
            Invalidate(true);
        }

        private void toolStripMobileChangeText_TextChanged(object sender, EventArgs e)
        {
            foreach(IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    obj.Text = toolStripMobileChangeText.Text;
                }
            }
            Invalidate(true);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMobile_MouseEnter(object sender, EventArgs e)
        {
            toolStripMobile.BackColor = toolStripHover;
        }

        private void toolStripMobile_MouseHover(object sender, EventArgs e)
        {
            //toolStripMobile.BackColor = toolStripHover;
        }

        private void toolStripMobile_MouseLeave(object sender, EventArgs e)
        {
            toolStripMobile.BackColor = toolStripNotHover;
        }

        private void toolStripMobilecbBorderWidth_TextChanged(object sender, EventArgs e)
        {
            int width = 1;
            int.TryParse(toolStripMobilecbBorderWidth.Text, out width);
            if(width>=1 && width <= 10)
            {
                foreach (IDrawable obj in Elements.Elements)
                {
                    if(obj.Selected)
                        obj.BorderWidth = width;
                }
            }
           
            Invalidate(true);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Point newLocation = new Point(this.Width - 55, nudBorderWidth.Location.Y);
            nudBorderWidth.Location = newLocation;
            newLocation = new Point(this.Width - 85, nudBorderWidth.Location.Y);
            pictureBox2.Location = newLocation;
        }

        private void EnlargeIfNeeded(IDrawable obj)
        {
            //RIGHT
            if (obj.X + obj.Width > pictureBox1.Width - 20)
                pictureBox1.Width += 100;
            //DOWN
            if (obj.Y + obj.Height > pictureBox1.Height - 20)
                pictureBox1.Height += 100;
        }

        private void copy()
        {
            bool newListMade = false;
            foreach (IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    if (!newListMade)
                    {
                        copyList = new List<IDrawable>();
                        newListMade = true;
                    }
                    if (obj is UMLElipse)
                    {
                        
                        UMLElipse el = obj as UMLElipse;
                        UMLElipse o = new UMLElipse(el);
                        copyList.Add(o);
                    }
                    else if (obj is Line)
                    {
                        Line l = obj as Line;
                        Line o = new Line(l);
                        copyList.Add(o);
                    }
                    else
                    {
                        Actor a = obj as Actor;
                        Actor o = new Actor(a);
                        copyList.Add(o);
                    }
                    obj.Selected = false;
                }
            }
        }

        private void cut()
        {
            copy();
            undoElements = new UseCaseDiagramDocument(Elements);
            foreach (IDrawable obj in copyList)
                Elements.Elements.Remove(obj);
            toolStripMobile.Visible = false;
            Invalidate(true);
        }

        private void paste()
        {
            undoElements = new UseCaseDiagramDocument(Elements);
            foreach (IDrawable obj in copyList)
                {
                    obj.Selected = true;
                    if(obj is UMLElipse || obj is Actor)
                    {
                        obj.X += 10;
                        obj.Y += 10;
                    }
                    else
                    {
                        Line l = obj as Line;
                        l.X1 += 5;
                        l.Y1 += 5;
                        l.X2 += 5;
                        l.Y2 += 5;
                    }
                    Elements.Elements.Add(obj);
                }
            copyList = new List<IDrawable>();
            Invalidate(true);
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            copy();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            paste();
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            cut();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            paste();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Right)
            {
                RightClickMenu.Show(Cursor.Position);
            }
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            paste();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cut();
        }
        public void newDocument()
        {
            if(MessageBox.Show("Дали сакате да отворите нов документ?","Нов документ",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)
            {
                Elements = new UseCaseDiagramDocument();
                toolStripMobile.Visible = false;
                Invalidate(true);
            }
        }
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            newDocument();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newDocument();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(IDrawable obj in Elements.Elements)
            {
                obj.Selected = true;
                Invalidate(true);
            }
        }

        private void solidStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    obj.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                }
            }
            Invalidate(true);
        }

        private void dottedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (IDrawable obj in Elements.Elements)
            {
                if (obj.Selected)
                {
                    obj.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                }
            }
            Invalidate(true);
        }
        public void undo()         
        {
            if(!undoDone)           //if undo is not done yet
            {
                undoDone = true;
                redoDone = false;
                redoElements = new UseCaseDiagramDocument(Elements);  //save the elements for redo, deep copy
                Elements = new UseCaseDiagramDocument(undoElements);
                undoElements = new UseCaseDiagramDocument();          //delete the undo elements
                toolStripMobile.Visible = false;
                Invalidate(true);            //repaint
            }
        }
        public void redo()
        {
            if(!redoDone)
            {
                redoDone = true;
                undoDone = false;
                undoElements = new UseCaseDiagramDocument(Elements);
                Elements = new UseCaseDiagramDocument(redoElements);
                redoElements = new UseCaseDiagramDocument();
                toolStripMobile.Visible = false;
                Invalidate(true);
            }
        }
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undo();
        }
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            redo();
        }
        private void save()
        {
            if (fileName == null)
            {
                SaveFileDialog sv = new SaveFileDialog();
                sv.Filter = "DrawU file (*.drawu)|*.drawu";
                sv.Title = "Save a DrawU file";
                if (sv.ShowDialog() == DialogResult.OK)
                    fileName = sv.FileName;
            }
            if (fileName == null) return;
            System.Runtime.Serialization.IFormatter fm = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            FileStream sm = new FileStream(fileName, FileMode.Create);
            fm.Serialize(sm, Elements);
        }

        private void saveAs()
        {
            fileName = null;
            save();
        }

        private void open()
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "DrawU file (*.drawu)|*.drawu";
            op.Title = "Open a DrawU file";
            if (op.ShowDialog() == DialogResult.OK)
            {
                System.Runtime.Serialization.IFormatter fm = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                fileName = op.FileName;
                FileStream sm = new FileStream(fileName, FileMode.Open);
                Elements = fm.Deserialize(sm) as UseCaseDiagramDocument;
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            save();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileName = null;
            save();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            open();
        }

        /// <summary>
        /// Creates a new image object, draws all the elements on it and
        /// saves the image in .jpg or .bmp format
        /// </summary>
        private void ExportAsImage()
        {
            Image img = new Bitmap(pictureBox1.Width, pictureBox1.Height);  //Create Image with the same size as the drawing environment
            Graphics imgGraphics = Graphics.FromImage(img);                 //Create Graphics object for the image
            imgGraphics.Clear(Color.White);                                 //Set white background
            Elements.Draw(imgGraphics);                                     //Draw all the elements
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Bitmap (.bmp)|*.bmp | JPEG (.jpg) | *.jpg | PDF (.pdf) | *.pdf";
            save.Title = "Export As Image";
            if (save.ShowDialog() == DialogResult.OK)
            {
                img.Save(save.FileName);                                   //Save the image under the name from the SaveFileDialog
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAsImage();
        }
        public void saveOnExit()
        {
            if (fileName == null)
            {
                if (MessageBox.Show("Дали сакате да ја зачувате вашата работа?", "Зачувај последни прoмени?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    save();
                }
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveOnExit();
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            HelpForm h = new HelpForm();
            h.Show();
        }

        private void howToUseProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm h = new HelpForm();
            h.Show();
        }

        private void aboutAuthorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Стефан Кочев\nКристијан Колев\nЈован Калајџиески", "Автори:", MessageBoxButtons.OK);
        }
    }
}
