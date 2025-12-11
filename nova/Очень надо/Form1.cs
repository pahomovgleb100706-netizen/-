using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace Очень_надо
{

    public partial class Form1: Form
    {
        string ConnectionString = "Data Source=DESKTOP-OEDFTS2\\SQLEXPRESS;Initial Catalog=TIT;Integrated Security=True;";

        public string ConnectionString1 { get => ConnectionString3; set => ConnectionString3 = value; }
        public string ConnectionString2 { get => ConnectionString3; set => ConnectionString3 = value; }
        public string ConnectionString3 { get => ConnectionString; set => ConnectionString = value; }
        public string ConnectionString4 { get => ConnectionString; set => ConnectionString = value; }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text;
            string password = txtPassword.Text;

            DataTable dt = Database.ExecuteQuery(
                "SELECT * FROM Сотрудники WHERE Логин=@login AND Пароль=@pass",
                new SqlParameter[]
                {
                new SqlParameter("@login", login),
                new SqlParameter("@pass", password)
                });

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Неверный логин или пароль!");
                return;
            }

            string role = dt.Rows[0]["Должность"].ToString();

            if (role == "Регистратор")
            {
                Form2 f2 = new Form2();
                f2.Show();
            }
            else if (role == "Врач")
            {
                FormPatients fP = new FormPatients();
                fP.Show();
            }
            else if (role == "Администратор")
            {
                FormAdmin fA = new FormAdmin();
                fA.Show();
            }

            this.Hide();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtLogin_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
