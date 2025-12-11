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
    public partial class Form3: Form
    {
        private Patient currentPatient;
        private int selectedBedId = 0;
        private Timer timer = new Timer();

        public Form3(Patient patient)
        {
            InitializeComponent();
            currentPatient = patient;
            LoadDepartments();
            BtnEdit.Click += BtnEdit_Click;
        }
        private void Form3_Load(object sender, EventArgs e)
        {

            timer.Interval = 3000; // 3 сек
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void TabRooms_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl == null) return;

            TabPage page = tabControl.TabPages[e.Index];
            string gender = page.AccessibleDescription?.ToLower() ?? "мужской";

            Color backColor = gender == "мужской" ? Color.LightBlue : Color.LightPink;
            Color foreColor = Color.Black;

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            Rectangle paddedBounds = e.Bounds;
            paddedBounds.Inflate(-2, -2);

            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (TabPage dept in tabControlDepartments.TabPages)
            {
                if (dept.Controls.Count == 0) continue;
                TabControl tabRooms = dept.Controls[0] as TabControl;
                if (tabRooms == null) continue;

                foreach (TabPage room in tabRooms.TabPages)
                {
                    if (room.Controls.Count == 0) continue;
                    FlowLayoutPanel flp = room.Controls[0] as FlowLayoutPanel;
                    if (flp == null) continue;

                    int roomId = (int)room.Tag;
                    string gender = room.AccessibleDescription;

                    LoadBeds(flp, roomId, gender);
                }
            }
        }
        

        private void LoadDepartments()
        {
            DataTable dt = Database.ExecuteQuery("SELECT * FROM Отделения");
            foreach (DataRow dr in dt.Rows)
            {
                TabPage tabDept = new TabPage(dr["Название"].ToString());
                TabControl tabRooms = new TabControl { Dock = DockStyle.Fill };

                // Важно! Устанавливаем OwnerDrawFixed для этого TabControl
                tabRooms.DrawMode = TabDrawMode.OwnerDrawFixed;
                tabRooms.DrawItem += TabRooms_DrawItem;

                tabDept.Controls.Add(tabRooms);
                tabControlDepartments.TabPages.Add(tabDept);

                LoadRooms(tabRooms, (int)dr["IDОтделения"]);
            }
        }

        private void LoadRooms(TabControl tabRooms, int departmentId)
        {
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

                // === Цвета для мужских/женских палат ===
                if (gender.ToLower() == "мужской")
                    tabRoom.BackColor = Color.LightBlue;    // мужская
                else
                    tabRoom.BackColor = Color.LightPink;    // женская

                FlowLayoutPanel flpBeds = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = tabRoom.BackColor   // чтобы цвет был одинаковым
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
                    Tag = dr["IDКойки"], // важно для выбранной койки
                    Width = 100,
                    Height = 60,
                    Margin = new Padding(10),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };

                string status = dr["Статус"].ToString().Trim().ToLower();
                btnBed.BackColor = status == "свободна" ? Color.LightGreen :
                                   status == "занята" ? Color.Red : Color.Gray;

                btnBed.ForeColor = gender.Trim().ToLower() == "мужской" ? Color.Blue : Color.Magenta;
                btnBed.Click += Bed_Click;

                flp.Controls.Add(btnBed);
            }
        }

        private void Bed_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (btn.Tag == null)
            {
                MessageBox.Show("Выберите койку!");
                return;
            }

            // Получаем ID палаты этой койки
            int bedId = Convert.ToInt32(btn.Tag);
            DataTable roomInfo = Database.ExecuteQuery(
                "SELECT IDПалаты, Статус FROM Койки WHERE IDКойки=@id",
                new SqlParameter[] { new SqlParameter("@id", bedId) });

            if (roomInfo.Rows.Count == 0) return;

            int roomId = Convert.ToInt32(roomInfo.Rows[0]["IDПалаты"]);
            string status = roomInfo.Rows[0]["Статус"].ToString().Trim().ToLower();

            // Получаем пол палаты
            DataTable roomGenderInfo = Database.ExecuteQuery(
                "SELECT Пол FROM Палаты WHERE IDПалаты=@roomId",
                new SqlParameter[] { new SqlParameter("@roomId", roomId) });

            if (roomGenderInfo.Rows.Count == 0) return;
            string roomGender = roomGenderInfo.Rows[0]["Пол"].ToString().Trim().ToLower();
            string patientGender = currentPatient.Gender.Trim().ToLower();

            // Проверка пола
            if (roomGender != patientGender)
            {
                MessageBox.Show($"Невозможно выбрать эту койку: {currentPatient.Gender} не может быть размещён в {roomGender} палате!");
                selectedBedId = 0;
                return;
            }

            // Проверка статуса койки
            if (status == "занята" || status == "поломана")
            {
                MessageBox.Show("Эта койка занята или повреждена!");
                selectedBedId = 0;
                return;
            }

            // Всё ок, выбираем койку
            selectedBedId = bedId;
            MessageBox.Show($"Выбрана койка {selectedBedId}");
        }


        private void btnConfirm_Click_1(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку!");
                return;
            }

            if (MessageBox.Show("Подтвердить выбор?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // 1. Добавляем пациента в таблицу Пациенты
                Database.ExecuteNonQuery(
                    "INSERT INTO Пациенты (СНИЛС, Имя, Фамилия, Отчество, ДатаРождения, Адрес, Пол) " +
                    "VALUES (@snils, @first, @last, @middle, @birth, @addr, @gender)",
                    new SqlParameter[]
                    {
                new SqlParameter("@snils", currentPatient.SNILS),
                new SqlParameter("@first", currentPatient.FirstName),
                new SqlParameter("@last", currentPatient.LastName),
                new SqlParameter("@middle", currentPatient.MiddleName),
                new SqlParameter("@birth", currentPatient.BirthDate),
                new SqlParameter("@addr", currentPatient.Address),
                new SqlParameter("@gender", currentPatient.Gender)
                    });

                // 2. Узнаём ID палаты, в которой стоит койка
                DataTable roomInfo = Database.ExecuteQuery(
                    "SELECT IDПалаты FROM Койки WHERE IDКойки=@id",
                    new SqlParameter[] { new SqlParameter("@id", selectedBedId) });

                int roomId = Convert.ToInt32(roomInfo.Rows[0]["IDПалаты"]);

                // 3. Узнаём отделение
                DataTable deptInfo = Database.ExecuteQuery(
                    "SELECT Название FROM Отделения WHERE IDОтделения = (SELECT IDОтделения FROM Палаты WHERE IDПалаты=@r)",
                    new SqlParameter[] { new SqlParameter("@r", roomId) });

                string departmentName = deptInfo.Rows[0]["Название"].ToString();

                // ДАТА, ГОД, МЕСЯЦ
                DateTime now = DateTime.Now;
                int year = now.Year;
                int month = now.Month;

                // 4. Добавляем запись в таблицу Регистрации
                Database.ExecuteNonQuery(
                    "INSERT INTO Регистрации (Фамилия, Имя, Отчество, СНИЛС, IDКойки, IDПалаты, ДатаПоступления, ДатаВыписки, Диагноз, НазваниеОтделения, Год, Месяц) " +
                    "VALUES (@last, @first, @middle, @snils, @bed, @room, @dateIn, NULL, @diag, @dept, @year, @month)",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", currentPatient.LastName),
                new SqlParameter("@first", currentPatient.FirstName),
                new SqlParameter("@middle", currentPatient.MiddleName),
                new SqlParameter("@snils", currentPatient.SNILS),
                new SqlParameter("@bed", selectedBedId),
                new SqlParameter("@room", roomId),
                new SqlParameter("@dateIn", now),
                new SqlParameter("@diag", currentPatient.Diagnosis),
                new SqlParameter("@dept", departmentName),
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
                    });

                // 5. Дублируем запись в ЕжемесячныеОтчёты
                Database.ExecuteNonQuery(
                    "INSERT INTO ЕжемесячныеОтчёты (Фамилия, Имя, Отчество, СНИЛС, IDКойки, IDПалаты, ДатаПоступления, ДатаВыписки, Диагноз, НазваниеОтделения, Год, Месяц) " +
                    "VALUES (@last, @first, @middle, @snils, @bed, @room, @dateIn, NULL, @diag, @dept, @year, @month)",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", currentPatient.LastName),
                new SqlParameter("@first", currentPatient.FirstName),
                new SqlParameter("@middle", currentPatient.MiddleName),
                new SqlParameter("@snils", currentPatient.SNILS),
                new SqlParameter("@bed", selectedBedId),
                new SqlParameter("@room", roomId),
                new SqlParameter("@dateIn", now),
                new SqlParameter("@diag", currentPatient.Diagnosis),
                new SqlParameter("@dept", departmentName),
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
                    });

                // 6. Обновляем статус койки
                Database.ExecuteNonQuery(
                    "UPDATE Койки SET Статус='Занята' WHERE IDКойки=@bedId",
                    new SqlParameter[] { new SqlParameter("@bedId", selectedBedId) });

                MessageBox.Show("Пациент успешно зарегистрирован и добавлен в месячный отчёт!");
                this.Close();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку для изменения!");
                return;
            }

            if (MessageBox.Show("Подтвердить изменение данных?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // 1. Получаем данные палаты и отделения
                DataTable roomInfo = Database.ExecuteQuery(
                    "SELECT IDПалаты FROM Койки WHERE IDКойки=@id",
                    new SqlParameter[] { new SqlParameter("@id", selectedBedId) });
                int roomId = Convert.ToInt32(roomInfo.Rows[0]["IDПалаты"]);

                DataTable deptInfo = Database.ExecuteQuery(
                    "SELECT Название FROM Отделения WHERE IDОтделения = (SELECT IDОтделения FROM Палаты WHERE IDПалаты=@r)",
                    new SqlParameter[] { new SqlParameter("@r", roomId) });
                string departmentName = deptInfo.Rows[0]["Название"].ToString();

                DateTime now = DateTime.Now;
                int year = now.Year;
                int month = now.Month;

                // 2. Обновляем данные пациента в таблице Регистрации
                Database.ExecuteNonQuery(
                    "UPDATE Регистрации SET Фамилия=@last, Имя=@first, Отчество=@middle, IDКойки=@bed, IDПалаты=@room, Диагноз=@diag, НазваниеОтделения=@dept, Год=@year, Месяц=@month " +
                    "WHERE СНИЛС=@snils",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", currentPatient.LastName),
                new SqlParameter("@first", currentPatient.FirstName),
                new SqlParameter("@middle", currentPatient.MiddleName),
                new SqlParameter("@bed", selectedBedId),
                new SqlParameter("@room", roomId),
new SqlParameter("@diag", currentPatient.Diagnosis),
                new SqlParameter("@dept", departmentName),
                new SqlParameter("@year", year),
                new SqlParameter("@month", month),
                new SqlParameter("@snils", currentPatient.SNILS)
                    });

                // 3. Обновляем таблицу ЕжемесячныеОтчёты
                Database.ExecuteNonQuery(
                    "UPDATE ЕжемесячныеОтчёты SET Фамилия=@last, Имя=@first, Отчество=@middle, IDКойки=@bed, IDПалаты=@room, Диагноз=@diag, НазваниеОтделения=@dept, Год=@year, Месяц=@month " +
                    "WHERE СНИЛС=@snils",
                    new SqlParameter[]
                    {
                new SqlParameter("@last", currentPatient.LastName),
                new SqlParameter("@first", currentPatient.FirstName),
                new SqlParameter("@middle", currentPatient.MiddleName),
                new SqlParameter("@bed", selectedBedId),
                new SqlParameter("@room", roomId),
new SqlParameter("@diag", currentPatient.Diagnosis),
                new SqlParameter("@dept", departmentName),
                new SqlParameter("@year", year),
                new SqlParameter("@month", month),
                new SqlParameter("@snils", currentPatient.SNILS)
                    });

                MessageBox.Show("Данные пациента успешно изменены!");
                this.Close();
            }
        }
    }
}
