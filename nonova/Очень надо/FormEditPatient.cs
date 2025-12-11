using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Очень_надо
{
    public partial class FormEditPatient: Form
    {
        private string ConnectionString = @"ВАШ_CONNECTION_STRING";
        public FormEditPatient()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadPatientData();
        }
        private void InitializeCustomComponents()
        {
            btnSearch.Click += BtnSearch_Click;
            btnSave.Click += BtnSave_Click;
            btnClose.Click += BtnClose_Click;
            dgvPatient.CellClick += DgvPatient_CellClick;

            dgvPatient.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPatient.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPatient.AllowUserToAddRows = false;
        }

        // === Загрузка всех пациентов в DataGridView ===
        private void LoadPatientData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT PatientID, LastName, FirstName, MiddleName, BirthDate, Gender, Phone, Address FROM Patients";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvPatient.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }
        private void FormEditPatient_Load(object sender, EventArgs e)
        {
            dgvPatient.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPatient.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPatient.AllowUserToAddRows = false;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtPatientID.Text))
                {
                    MessageBox.Show("Выберите пациента для изменения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query =
                        "UPDATE Patients SET LastName=@LastName, FirstName=@FirstName, MiddleName=@MiddleName, " +
                        "BirthDate=@BirthDate, Gender=@Gender, Phone=@Phone, Address=@Address " +
                        "WHERE PatientID=@PatientID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@MiddleName", txtMiddleName.Text);
                        cmd.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value);
                        cmd.Parameters.AddWithValue("@Gender", cmbGender.Text);
                        cmd.Parameters.AddWithValue("@Phone", mtxtPhone.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@PatientID", txtPatientID.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Пациент с таким ID не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                LoadPatientData();
                MessageBox.Show("Данные успешно обновлены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM Patients WHERE LastName LIKE @LastName";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@LastName", "%" + txtSearch.Text + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvPatient.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void FormEditPatient_Load_1(object sender, EventArgs e)
        {

        }

        private void LoadPatientData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string query = "SELECT * FROM Patients";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvPatient.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }

        }
        private void DgvPatient_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvPatient.Rows[e.RowIndex];

            txtPatientID.Text = row.Cells["PatientID"].Value.ToString();
            txtLastName.Text = row.Cells["LastName"].Value.ToString();
            txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
            txtMiddleName.Text = row.Cells["MiddleName"].Value.ToString();

            if (row.Cells["BirthDate"].Value != DBNull.Value)
                dtpBirthDate.Value = Convert.ToDateTime(row.Cells["BirthDate"].Value);

            cmbGender.Text = row.Cells["Gender"].Value.ToString();
            mtxtPhone.Text = row.Cells["Phone"].Value.ToString();
            txtAddress.Text = row.Cells["Address"].Value.ToString();
        }
        public FormEditPatient(Patient patient) : this()
        {
            // Заполняем поля формы данными пациента
            txtPatientID.Text = patient.SNILS;
            txtFirstName.Text = patient.FirstName;
            txtLastName.Text = patient.LastName;
            txtMiddleName.Text = patient.MiddleName;
            dtpBirthDate.Value = patient.BirthDate;
            cmbGender.Text = patient.Gender;
            txtAddress.Text = patient.Address;
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void BtnSearch_Click(object sender, EventArgs e)
        {
            // код поиска
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // код сохранения
        }
    }
}
