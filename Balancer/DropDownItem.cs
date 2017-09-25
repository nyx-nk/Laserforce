using System.Drawing;
using System.Windows.Forms;

namespace Laserforce
{
    public class DropDownItem
    {
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
        private string value;

        public Image Image
        {
            get { return img; }
            set { img = value; }
        }
        private Image img;

        public DropDownItem() : this("")
        { }

        public DropDownItem(string val)
        {
            value = val;
            this.img = GetImageFor(val);
            Graphics g = Graphics.FromImage(img);
            Brush b = new SolidBrush(Color.FromName(val));
            g.DrawRectangle(Pens.White, 0, 0, img.Width, img.Height);
            g.FillRectangle(b, 1, 1, img.Width - 1, img.Height - 1);
        }

        public static Bitmap GetImageFor(string role)
        {
            switch (role)
            {
                case "Ammo":
                    return Properties.Resources.Ammo;

                case "Medic":
                    return Properties.Resources.Medic;

                case "Scout":
                    return Properties.Resources.Scout;

                case "Heavy":
                    return Properties.Resources.Heavy;

                case "Commander":
                    return Properties.Resources.Commander;

                default:
                    return null;
            }
        }

        public override string ToString()
        {
            return value;
        }
    }
}
