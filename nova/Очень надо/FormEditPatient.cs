using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
namespace Очень_надо
{
    public partial class FormEditPatient: Form
    {
        private int selectedBedId = 0;
        public FormEditPatient()
        {
            InitializeComponent();
            InitializeMaskedTextBox();
        }
        private void InitializeMaskedTextBox()
        {
            // Устанавливаем маску для ввода 11 цифр
            txtPatientID.Mask = "000-000-000 00";  // 11 цифр
            txtPatientID.ValidatingType = typeof(int); // Устанавливаем тип для валидации
        }

        private void FormEditPatient_Load(object sender, EventArgs e)
        {

            LoadDepartments();
        }

        private void LoadDepartments()
        {
            tabControlDepartments.TabPages.Clear();

            DataTable dt = Database.ExecuteQuery("SELECT * FROM Отделения");
            foreach (DataRow dr in dt.Rows)
            {
                TabPage tabDept = new TabPage(dr["Название"].ToString());
                TabControl tabRoomsInner = new TabControl { Dock = DockStyle.Fill };

                tabRoomsInner.DrawMode = TabDrawMode.OwnerDrawFixed;
                tabRoomsInner.DrawItem += TabRooms_DrawItem;

                tabDept.Controls.Add(tabRoomsInner);
                tabControlDepartments.TabPages.Add(tabDept);

                LoadRooms(tabRoomsInner, (int)dr["IDОтделения"]);
            }
        }

        private void LoadRooms(TabControl tabRooms, int departmentId)
        {
            tabRooms.TabPages.Clear();

            DataTable dt = Database.ExecuteQuery(
                "SELECT * FROM Палаты WHERE IDОтделения=@depId",
                new SqlParameter[] { new SqlParameter("@depId", departmentId) }
            );

            foreach (DataRow dr in dt.Rows)
            {
                TabPage tabRoom = new TabPage(dr["Номер"].ToString());
                tabRoom.Tag = dr["IDПалаты"];
                string gender = dr["Пол"].ToString().Trim();
                tabRoom.AccessibleDescription = gender;

                tabRoom.BackColor = gender.ToLower() == "мужской" ? Color.LightBlue : Color.LightPink;

                FlowLayoutPanel flpBeds = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = tabRoom.BackColor
                };

                LoadBeds(flpBeds, (int)dr["IDПалаты"], gender);

                tabRoom.Controls.Add(flpBeds);
                tabRooms.TabPages.Add(tabRoom);
            }
        }

        private void LoadBeds(FlowLayoutPanel flp, int roomId, string gender)
        {
            flp.Controls.Clear();
            DataTable dt = Database.ExecuteQuery(
                "SELECT * FROM Койки WHERE IDПалаты=@roomId",
                new SqlParameter[] { new SqlParameter("@roomId", roomId) }
            );

            foreach (DataRow dr in dt.Rows)
            {
                Button btnBed = new Button
                {
                    Text = "Койка " + dr["IDКойки"].ToString(),
                    Tag = dr["IDКойки"],
                    Width = 100,
                    Height = 60,
                    Margin = new Padding(10),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

                string status = dr["Статус"].ToString().Trim().ToLower();
                btnBed.BackColor = status == "свободна" ? Color.LightGreen :
                                   status == "занята" ? Color.Red : Color.Gray;

                btnBed.ForeColor = gender.ToLower() == "мужской" ? Color.Blue : Color.Magenta;
                btnBed.Click += Bed_Click;

                flp.Controls.Add(btnBed);
            }
        }

        private void TabRooms_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl == null) return;

            TabPage page = tabControl.TabPages[e.Index];
            string gender = page.AccessibleDescription?.ToLower() ?? "мужской";

            Color backColor = gender == "мужской" ? Color.LightBlue : Color.LightPink;

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            Rectangle paddedBounds = e.Bounds;
            paddedBounds.Inflate(-2, -2);

            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, Color.Black,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void Bed_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            selectedBedId = Convert.ToInt32(btn.Tag);

            DataTable bedDt = Database.ExecuteQuery(
                "SELECT Статус FROM Койки WHERE IDКойки=@bedId",
                new SqlParameter[] { new SqlParameter("@bedId", selectedBedId) }
            );

            if (bedDt.Rows.Count == 0) return;

            string status = bedDt.Rows[0]["Статус"].ToString().Trim().ToLower();

            if (status == "занята")
            {
                LoadPatientData(selectedBedId);
            }
            else if (status == "свободна")
            {
                ClearPatientFields();
                MessageBox.Show("Койка свободна, вы можете выбрать её для нового пациента.");
            }
            else
            {
                MessageBox.Show("Койка повреждена или недоступна!");
            }
        }

        private void LoadPatientData(int bedId)
        {

            DataTable dt = Database.ExecuteQuery(
                @"SELECT p.Фамилия, p.Имя, p.Отчество, p.СНИЛС, p.Адрес, r.Диагноз, p.ДатаРождения
          FROM Койки k
          LEFT JOIN Регистрации r ON k.IDКойки = r.IDКойки AND r.ДатаВыписки IS NULL
          LEFT JOIN Пациенты p ON r.СНИЛС = p.СНИЛС
          WHERE k.IDКойки = @bedId",
                new SqlParameter[] { new SqlParameter("@bedId", bedId) }
            );

            if (dt.Rows.Count == 0)
            {
                ClearPatientFields();
                return;
            }

            DataRow dr = dt.Rows[0];

            txtLastName.Text = dr["Фамилия"] == DBNull.Value ? "" : dr["Фамилия"].ToString();
            txtFirstName.Text = dr["Имя"] == DBNull.Value ? "" : dr["Имя"].ToString();
            txtMiddleName.Text = dr["Отчество"] == DBNull.Value ? "" : dr["Отчество"].ToString();
            txtPatientID.Text = dr["СНИЛС"] == DBNull.Value ? "" : dr["СНИЛС"].ToString();
            txtAddress.Text = dr["Адрес"] == DBNull.Value ? "" : dr["Адрес"].ToString();
            txtDiagnosis.Text = dr["Диагноз"] == DBNull.Value ? "" : dr["Диагноз"].ToString();

            // Привязка даты рождения
            if (dr["ДатаРождения"] != DBNull.Value)
                dtpBirthDate.Value = Convert.ToDateTime(dr["ДатаРождения"]);
            else
                dtpBirthDate.Value = DateTime.Today;
        }

        private void ClearPatientFields()
        {
            txtLastName.Clear();
            txtFirstName.Clear();
            txtMiddleName.Clear();
            txtPatientID.Clear();
            txtAddress.Clear();
            txtDiagnosis.Clear();
        }
        private void tabControlDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку для сохранения данных.");
                return;
            }

            string lastName = txtLastName.Text;
            string firstName = txtFirstName.Text;
            string middleName = txtMiddleName.Text;
            string patientId = txtPatientID.Text;
            string address = txtAddress.Text;
            string diagnosis = txtDiagnosis.Text;

            if (string.IsNullOrEmpty(patientId))
            {
                MessageBox.Show("СНИЛС пациента не может быть пустым!");
                return;
            }
            DateTime birthDate = dtpBirthDate.Value;
            // Обновление таблицы Пациенты
            Database.ExecuteNonQuery(
    @"UPDATE Пациенты SET Фамилия=@lastName, Имя=@firstName, Отчество=@middleName, Адрес=@address, ДатаРождения=@birthDate 
      WHERE СНИЛС=@patientId",
    new SqlParameter[]
    {
        new SqlParameter("@lastName", lastName),
        new SqlParameter("@firstName", firstName),
        new SqlParameter("@middleName", middleName),
        new SqlParameter("@address", address),
        new SqlParameter("@birthDate", birthDate),
        new SqlParameter("@patientId", patientId) }
            );

            // Обновление таблицы Регистрации
            Database.ExecuteNonQuery(
                @"UPDATE Регистрации SET Фамилия=@lastName, Имя=@firstName, Отчество=@middleName, Диагноз=@diagnosis
                  WHERE IDКойки=@bedId AND ДатаВыписки IS NULL",
                new SqlParameter[]
                {
                    new SqlParameter("@lastName", lastName),
                    new SqlParameter("@firstName", firstName),
                    new SqlParameter("@middleName", middleName),
                    new SqlParameter("@diagnosis", diagnosis),
                    new SqlParameter("@bedId", selectedBedId)
                }
            );

            // Обновление таблицы ЕжемесячныеОтчёты
            Database.ExecuteNonQuery(
                @"UPDATE ЕжемесячныеОтчёты SET Фамилия=@lastName, Имя=@firstName, Отчество=@middleName, Диагноз=@diagnosis
                  WHERE IDКойки=@bedId",
                new SqlParameter[]
                {
                    new SqlParameter("@lastName", lastName),
                    new SqlParameter("@firstName", firstName),
                    new SqlParameter("@middleName", middleName),
                    new SqlParameter("@diagnosis", diagnosis),
                    new SqlParameter("@bedId", selectedBedId)
                }
            );

            MessageBox.Show("Данные пациента успешно обновлены!");
            LoadDepartments();
        }



        private void txtDiagnosis_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbGender_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDepartments();
        }

        private void txtMiddleName_TextChanged(object sender, EventArgs e)
        {

        }

        private void dtpBirthDate_ValueChanged(object sender, EventArgs e)
        {
            int age = DateTime.Today.Year - dtpBirthDate.Value.Year;
            if (dtpBirthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;
            txtAge.Text = age.ToString();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPatientID_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void txtAge_Click(object sender, EventArgs e)
        {

        }
    }
    }
