using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laserforce
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
        }

        private void btnEnterPassword_Click(object sender, EventArgs e)
        {
            var password = txtPassword.Text;
            if (password == "dmihawkgreg" || password == "gndsteve")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid password");
            }
        }
    }
}
