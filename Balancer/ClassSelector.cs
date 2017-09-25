using System.Windows.Forms;

namespace Laserforce
{
    public class ClassSelector : ComboBox
    {
        public ClassSelector()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            var item = new DropDownItem(Items[e.Index].ToString());

            e.Graphics.DrawImage(item.Image, e.Bounds.Left, e.Bounds.Top);
            //e.Graphics.DrawString(item.Value, e.Font, new
            //        SolidBrush(e.ForeColor), e.Bounds.Left + item.Image.Width, e.Bounds.Top + 2);

            base.OnDrawItem(e);
        }
    }
}
