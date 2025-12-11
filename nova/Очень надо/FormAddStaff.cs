using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace Очень_надо
{
    public partial class FormAddStaff: Form
    {
        private int staffId = 0;
        private void LoadPositions()
        {
            cbPosition.Items.Clear();
            cbPosition.Items.Add("Врач");
            cbPosition.Items.Add("Администратор");
            cbPosition.Items.Add("Регистратор");
        }
        public FormAddStaff()
        {
            InitializeComponent();
            staffId = 0;
            LoadDepartments();
            LoadPositions();
        }
         public FormAddStaff(int id)
        {
            InitializeComponent();
            staffId = id;
            LoadDepartments();
            LoadPositions();
            LoadStaffData();
        }


        private void LoadDepartments()
        {
            DataTable dt = Database.ExecuteQuery("SELECT Название FROM Отделен");
            cbDepartment.Items.Clear();

            foreach (DataRow dr in dt.Rows)
                cbDepartment.Items.Add(dr["Название"].ToString());
        }

        private void LoadStaffData()
        {
            DataTable dt = Database.ExecuteQuery(
                "SELECT * FROM Сотрудники WHERE IDСотрудника=@id",
                new SqlParameter[] { new SqlParameter("@id", staffId) });

            if (dt.Rows.Count == 0) return;

            DataRow dr = dt.Rows[0];

            txtLastName.Text = dr["Фамилия"].ToString();
            txtFirstName.Text = dr["Имя"].ToString();
            txtMiddleName.Text = dr["Отчество"].ToString();
            txtLogin.Text = dr["Логин"].ToString();
            txtPassword.Text = dr["Пароль"].ToString();
            cbPosition.SelectedItem = dr["Должность"].ToString();
            cbDepartment.SelectedItem = dr["НазваниеОтделения"].ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // === Проверка обязательных полей ===
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                cbPosition.SelectedItem == null ||
                cbDepartment.SelectedItem == null)
            {
                MessageBox.Show(
                    "Пожалуйста, заполните все обязательные поля!\n(Отчество можно оставить пустым)",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (staffId == 0)
            {
                // Добавление нового сотрудника
                Database.ExecuteNonQuery(
                    "INSERT INTO Сотрудники (Фамилия, Имя, Отчество, Логин, Пароль, Должность, НазваниеОтделения) " +
                    "VALUES (@last, @first, @middle, @login, @pass, @pos, @dept)",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", txtLastName.Text),
                new SqlParameter("@first", txtFirstName.Text),
                new SqlParameter("@middle", (object)txtMiddleName.Text ?? DBNull.Value),
                new SqlParameter("@login", txtLogin.Text),
                new SqlParameter("@pass", txtPassword.Text),
                new SqlParameter("@pos", cbPosition.SelectedItem.ToString()),
                new SqlParameter("@dept", cbDepartment.SelectedItem.ToString())
                    });
            }
            else
            {
                // Обновление существующего сотрудника
                Database.ExecuteNonQuery(
                    "UPDATE Сотрудники SET Фамилия=@last, Имя=@first, Отчество=@middle, Логин=@login, Пароль=@pass, " +
                    "Должность=@pos, НазваниеОтделения=@dept WHERE IDСотрудника=@id",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", txtLastName.Text),
                new SqlParameter("@first", txtFirstName.Text),
                new SqlParameter("@middle", (object)txtMiddleName.Text ?? DBNull.Value),
                new SqlParameter("@login", txtLogin.Text),
                new SqlParameter("@pass", txtPassword.Text),
                new SqlParameter("@pos", cbPosition.SelectedItem.ToString()),
                new SqlParameter("@dept", cbDepartment.SelectedItem.ToString()),
                new SqlParameter("@id", staffId)
                    });
            }

            MessageBox.Show("Данные сохранены!");
            this.Close();
        }

        private void FormAddStaff_Load(object sender, EventArgs e)
        {

        }

        private void cbPosition_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
