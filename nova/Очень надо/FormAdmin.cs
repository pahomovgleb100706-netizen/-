using System;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace Очень_надо
{
    public partial class FormAdmin: Form
    {
        public FormAdmin()
        {
            InitializeComponent();
            LoadStaff();
        }
        private void LoadStaff()
        {
            dgvStaff.DataSource = Database.ExecuteQuery("SELECT IDСотрудника, Фамилия, Имя, Отчество, Логин, Должность, НазваниеОтделения FROM Сотрудники");
        }
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvStaff.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }

            int staffId = Convert.ToInt32(dgvStaff.SelectedRows[0].Cells["IDСотрудника"].Value);
            FormAddStaff fEdit = new FormAddStaff(staffId);
            fEdit.ShowDialog();
            LoadStaff();
        }
        

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvStaff.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника!");
                return;
            }

            int staffId = Convert.ToInt32(dgvStaff.SelectedRows[0].Cells["IDСотрудника"].Value);

            Database.ExecuteNonQuery("DELETE FROM Сотрудники WHERE IDСотрудника=@id",
                new SqlParameter[] { new SqlParameter("@id", staffId) });

            MessageBox.Show("Сотрудник удален!");
            LoadStaff();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            FormAddStaff fAdd = new FormAddStaff();
            fAdd.ShowDialog();
            LoadStaff();
        }

        private void FormAdmin_Load(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Show();   // показываем Form1
            this.Close(); // закрываем текущую форму (Form2)
        }
    }
}
