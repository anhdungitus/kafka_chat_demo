using System;
using System.Linq;
using System.Windows.Forms;

namespace ChattingClient
{
    public partial class SelectUser : Form
    {
        public SelectUser()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var db = new ChatDbContext())
            {
                var id = int.Parse(textBox1.Text);
                var user = db.Users.AsQueryable().FirstOrDefault(s => s.Id == id);
                if (user != null)
                {
                    this.Hide();
                    var form2 = new Form1(user.Id);
                    form2.Closed += (s, args) => this.Close();
                    form2.Show();
                }
            }
        }
    }
}