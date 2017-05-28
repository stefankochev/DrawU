using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proektna
{
    [Serializable]
    public class UseCaseDiagramDocument
    {
        public List<IDrawable> Elements { get; set; }
        public UseCaseDiagramDocument()
        {
            Elements = new List<IDrawable>();
        }
        public UseCaseDiagramDocument(UseCaseDiagramDocument obj)
        {
            List<IDrawable> elem = new List<IDrawable>();
            foreach (IDrawable o in obj.Elements)
            {
                if (o is UMLElipse)
                {
                    UMLElipse elipse = o as UMLElipse;
                    UMLElipse newObject = new UMLElipse(elipse);
                    elem.Add(newObject);
                }
                else if (o is Line)
                {
                    Line line = o as Line;
                    Line newObject = new Line(line);
                    elem.Add(newObject);
                }
                else if (o is Actor)
                {
                    Actor actor = o as Actor;
                    Actor newObject = new Actor(actor);
                    elem.Add(newObject);
                }
            }
            Elements = elem;
        }
        public void Draw(Graphics g)
        {
            foreach(IDrawable d in Elements)
            {
                d.Draw(g);
            }
        }
        public void Add(IDrawable obj)
        {
            Elements.Add(obj);
        }
        public void Selected(Point p,bool ctrl)
        {
            foreach (IDrawable obj in Elements)
            {
                obj.isSelected(p,ctrl);
            }
         
        }

        public void Move(Point from, Point to)
        {
            int dx = -from.X + to.X;
            int dy = -from.Y + to.Y;

            foreach(IDrawable obj in Elements)
            {
                if (obj.Selected)
                {
                    obj.Move(dx, dy);
                }
            }
        }
    }
}
