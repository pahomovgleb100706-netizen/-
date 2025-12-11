using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
namespace Очень_надо
{
    public partial class Form2: Form
    {
        public Form2()
        {
            InitializeComponent();
            InitializeMaskedTextBox();
        }
        private void InitializeMaskedTextBox()
        {
            // Устанавливаем маску для ввода 11 цифр
            txtSNILS.Mask = "000-000-000 00";  // 11 цифр
            txtSNILS.ValidatingType = typeof(int); // Устанавливаем тип для валидации
        }
        private void btnChooseBed_Click(object sender, EventArgs e)
        {
            // === Проверка обязательных полей ===
            if (string.IsNullOrWhiteSpace(txtSNILS.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text) ||
                string.IsNullOrWhiteSpace(txtDiagnosis.Text) ||
                (!rbtnMale.Checked && !rbtnFemale.Checked))
            {
                MessageBox.Show(
                    "Пожалуйста, заполните все обязательные поля!\n(Отчество можно оставить пустым)",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Если всё ОК, создаём пациента
            Patient patient = new Patient
            {
                SNILS = txtSNILS.Text,
                FirstName = txtFirstName.Text,
                LastName = txtLastName.Text,
                MiddleName = txtMiddleName.Text, // отчество может быть пустым
                BirthDate = dtpBirth.Value,
                Address = txtAddress.Text,
                Gender = rbtnMale.Checked ? "Мужской" : "Женский",
                Diagnosis = txtDiagnosis.Text
            };

            // Добавление в таблицу Пациенты
            string query = "INSERT INTO Пациенты (СНИЛС, Имя, Фамилия, Отчество, ДатаРождения, Адрес, Пол) " +
                           "VALUES (@SNILS, @FirstName, @LastName, @MiddleName, @BirthDate, @Address, @Gender)";
            SqlParameter[] parameters = {
        new SqlParameter("@SNILS", patient.SNILS),
        new SqlParameter("@FirstName", patient.FirstName),
        new SqlParameter("@LastName", patient.LastName),
           new SqlParameter("@MiddleName", patient.MiddleName),
        new SqlParameter("@BirthDate", patient.BirthDate),
        new SqlParameter("@Address", patient.Address),
        new SqlParameter("@Gender", patient.Gender)
    };
            Database.ExecuteNonQuery(query, parameters);

            Form3 f3 = new Form3(patient);
            f3.ShowDialog();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // Месяцы
            cmbMonth.Items.AddRange(new object[]
            {
        "1","2","3","4","5","6","7","8","9","10","11","12"
            });
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;

            // Года
            for (int y = 2020; y <= DateTime.Now.Year; y++)
            {
                cmbYear.Items.Add(y);
            }

            cmbYear.SelectedItem = DateTime.Now.Year;
        }

        private void btnExportMonthlyReport_Click(object sender, EventArgs e)
        {
            int year = Convert.ToInt32(cmbYear.SelectedItem);
            int month = Convert.ToInt32(cmbMonth.SelectedItem);

            // Загружаем данные, включая ДатаПоступления и ДатаВыписки
            DataTable dt = Database.ExecuteQuery(
                "SELECT Фамилия, Имя, Отчество, СНИЛС, IDКойки, IDПалаты, Диагноз, НазваниеОтделения, ДатаПоступления, ДатаВыписки " +
                "FROM ЕжемесячныеОтчёты WHERE Год=@year AND Месяц=@month",
                new SqlParameter[] {
            new SqlParameter("@year", year),
            new SqlParameter("@month", month)
                });

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для отчёта.");
                return;
            }

            // Теперь добавляем две дополнительные колонки (даты) в PNG‑отчёт
            // Настроим ширины колонок, включая новые
            int[] colWidths = { 200, 150, 150, 200, 100, 100, 450, 250, 200, 200 }; // + два столбца для дат
            int rowHeight = 50;

            int totalHeight = (dt.Rows.Count + 3) * rowHeight;
            int totalWidth = colWidths.Sum() + 20;

            Bitmap bmp = new Bitmap(totalWidth, totalHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            Font headerFont = new Font("Arial", 20, FontStyle.Bold);
            Font font = new Font("Arial", 12, FontStyle.Regular);
            Pen pen = new Pen(Color.Black, 2);

            g.DrawString($"Ежемесячный отчёт — {month:D2}.{year}", headerFont, Brushes.Black, 20, 10);

            int y = 60;
            int x = 10;

            string[] headers = {
        "Фамилия", "Имя", "Отчество", "СНИЛС",
        "Койка", "Палата", "Диагноз", "Отделение",
        "Дата поступления", "Дата выписки"
    };

            // Рисуем заголовки
            x = 10;
            for (int i = 0; i < headers.Length; i++)
            {
                g.DrawRectangle(pen, x, y, colWidths[i], rowHeight);
                g.DrawString(headers[i], font, Brushes.Black, new RectangleF(x + 5, y + 5, colWidths[i], rowHeight));
                x += colWidths[i];
            }

            y += rowHeight;

            // Рисуем строки
            foreach (DataRow dr in dt.Rows)
            {
                x = 10;

                string dateIn = "";
                string dateOut = "";
                if (dr["ДатаПоступления"] != DBNull.Value)
                    dateIn = Convert.ToDateTime(dr["ДатаПоступления"]).ToString("dd.MM.yyyy HH:mm");
                if (dr["ДатаВыписки"] != DBNull.Value)
                    dateOut = Convert.ToDateTime(dr["ДатаВыписки"]).ToString("dd.MM.yyyy HH:mm");

                string[] row = {
            dr["Фамилия"].ToString(),
            dr["Имя"].ToString(),
            dr["Отчество"].ToString(),
            dr["СНИЛС"].ToString(),
            dr["IDКойки"].ToString(),
            dr["IDПалаты"].ToString(),
            dr["Диагноз"].ToString(),
            dr["НазваниеОтделения"].ToString(),
            dateIn,
            dateOut
        };

                for (int i = 0; i < row.Length; i++)
                {
                    g.DrawRectangle(pen, x, y, colWidths[i], rowHeight);
                    g.DrawString(row[i], font, Brushes.Black,
                        new RectangleF(x + 5, y + 5, colWidths[i] - 10, rowHeight - 10));
                    x += colWidths[i];
                }

                y += rowHeight;
            }

            string downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";
            string filePath = downloads + $"Отчёт_{month:D2}_{year}.png";

            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            MessageBox.Show($"PNG-отчёт сохранён в:\n{filePath}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF files (.pdf)|.pdf";
            saveFileDialog.DefaultExt = "pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                GeneratePDF(saveFileDialog.FileName);
            }
        }
        private void GeneratePDF(string filePath)
        {
            int year = Convert.ToInt32(cmbYear.SelectedItem);
            int month = Convert.ToInt32(cmbMonth.SelectedItem);

            // Загружаем данные
            DataTable dt = Database.ExecuteQuery(
                "SELECT Фамилия, Имя, Отчество, СНИЛС, IDКойки, IDПалаты, Диагноз, НазваниеОтделения " +
                "FROM ЕжемесячныеОтчёты WHERE Год=@year AND Месяц=@month",
                new SqlParameter[]
                {
            new SqlParameter("@year", year),
            new SqlParameter("@month", month)
                });

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для PDF!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdfDocument = new PdfDocument(writer))
            using (Document document = new Document(pdfDocument, PageSize.A4.Rotate()))
            {
                string fontFilePath = @"C:\Windows\Fonts\arial.ttf";
                PdfFont font = PdfFontFactory.CreateFont(fontFilePath, PdfEncodings.IDENTITY_H);

                Paragraph title = new Paragraph($"Ежемесячный отчёт — {month:D2}.{year}")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFont(font)
                    .SetFontSize(20);

                document.Add(title);
                document.Add(new Paragraph("\n"));

                // создаём таблицу
                Table table = new Table(dt.Columns.Count, true);

                // Заголовки
                foreach (DataColumn column in dt.Columns)
                {
                    table.AddHeaderCell(
                        new Cell().Add(
                            new Paragraph(column.ColumnName)
                            .SetFont(font)
                        ).SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                    );
                }

                // Строки
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var cell in row.ItemArray)
                    {
                        table.AddCell(new Cell().Add(
                            new Paragraph(cell?.ToString() ?? "")
                            .SetFont(font)
                        ));
                    }
                }

                document.Add(table);
            }

            MessageBox.Show("PDF файл успешно создан!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtSNILS_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            string patientID = txtSNILS.Text;

            // Убираем все тире и пробелы
            patientID = patientID.Replace("-", "").Replace(" ", "");

            // Теперь в patientID хранится строка из 11 цифр без тире
            MessageBox.Show("СНИЛС: " + txtSNILS);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FormEditPatient form = new FormEditPatient();
            form.Show();
        }

        private void txtDiagnosis_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Show();   // показываем Form1
            this.Close(); // закрываем текущую форму (Form2)
        }

        private void txtSNILS_MaskInputRejected_1(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void txtMiddleName_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbYear_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
    
}
