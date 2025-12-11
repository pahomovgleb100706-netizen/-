using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Очень_надо
{
    public partial class FormPatients: Form
    {
        private int selectedBedId = 0;
        private Timer timer = new Timer();

        public FormPatients()
        {
            InitializeComponent();
            LoadDepartments();
        }

        private void FormPatients_Load(object sender, EventArgs e)
        {
            timer.Interval = 3000; // 3 сек
            timer.Tick += Timer_Tick;
            timer.Start();
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
                "SELECT Койки.IDКойки, Койки.Статус, Пациенты.Фамилия, Пациенты.Имя, Пациенты.Отчество " +
                "FROM Койки " +
                "LEFT JOIN Регистрации ON Койки.IDКойки = Регистрации.IDКойки AND Регистрации.ДатаВыписки IS NULL " +
                "LEFT JOIN Пациенты ON Регистрации.СНИЛС = Пациенты.СНИЛС " +
                "WHERE Койки.IDПалаты=@roomId",
                new SqlParameter[] { new SqlParameter("@roomId", roomId) }
            );

            foreach (DataRow dr in dt.Rows)
            {
                string status = dr["Статус"].ToString().Trim().ToLower();
                string text = status == "занята"
                    ? $"{dr["Фамилия"]} {dr["Имя"]} {dr["Отчество"]}"
                    : $"Койка {dr["IDКойки"]}";

                Button btnBed = new Button
                {
                    Text = text,
                    Tag = dr["IDКойки"],
                    Width = 120,
                    Height = 60,
                    Margin = new Padding(10),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = status == "свободна" ? Color.LightGreen :
                                status == "занята" ? Color.Red : Color.Gray,
                    ForeColor = gender.Trim().ToLower() == "мужской" ? Color.Blue : Color.Magenta
                };

                btnBed.Click += Bed_Click;
                flp.Controls.Add(btnBed);
            }
        }
        private void Bed_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            selectedBedId = Convert.ToInt32(btn.Tag);
        }
        private void btnMarkBroken_Click(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку для пометки как поломанную!");
                return;
            }

            Database.ExecuteNonQuery(
                "UPDATE Койки SET Статус='Поломана' WHERE IDКойки=@bed",
                new SqlParameter[] { new SqlParameter("@bed", selectedBedId) }
            );

            MessageBox.Show("Койка помечена как поломанная.");
            Timer_Tick(null, null);
        }

        private void btnRepairBed_Click(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку для восстановления!");
                return;
            }

            Database.ExecuteNonQuery(
                "UPDATE Койки SET Статус='Свободна' WHERE IDКойки=@bed",
                new SqlParameter[] { new SqlParameter("@bed", selectedBedId) }
            );

            MessageBox.Show("Койка восстановлена и доступна для пациентов.");
            Timer_Tick(null, null);
        }
        private void btnDischarge_Click(object sender, EventArgs e)
        {
            if (selectedBedId == 0)
            {
                MessageBox.Show("Выберите койку для выписки пациента!");
                return;
            }

            if (MessageBox.Show("Выписать пациента?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Database.ExecuteNonQuery(
                    "UPDATE Регистрации SET ДатаВыписки=@dateOut WHERE IDКойки=@bed AND ДатаВыписки IS NULL",
                    new SqlParameter[] { new SqlParameter("@dateOut", DateTime.Now), new SqlParameter("@bed", selectedBedId) }
                );

                Database.ExecuteNonQuery(
                    "UPDATE Койки SET Статус='Свободна' WHERE IDКойки=@bed",
                    new SqlParameter[] { new SqlParameter("@bed", selectedBedId) }
                );

                MessageBox.Show("Пациент выписан, койка освобождена.");
                selectedBedId = 0;
                Timer_Tick(null, null);
            }
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
    }
}
