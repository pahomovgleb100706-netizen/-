namespace Очень_надо
{
    partial class FormPatients
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnMarkBroken = new System.Windows.Forms.Button();
            this.btnDischarge = new System.Windows.Forms.Button();
            this.btnRepairBed = new System.Windows.Forms.Button();
            this.tabControlDepartments = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // btnMarkBroken
            // 
            this.btnMarkBroken.Location = new System.Drawing.Point(227, 407);
            this.btnMarkBroken.Name = "btnMarkBroken";
            this.btnMarkBroken.Size = new System.Drawing.Size(185, 23);
            this.btnMarkBroken.TabIndex = 9;
            this.btnMarkBroken.Text = "Поврежденная койка";
            this.btnMarkBroken.UseVisualStyleBackColor = true;
            this.btnMarkBroken.Click += new System.EventHandler(this.btnMarkBroken_Click);
            // 
            // btnDischarge
            // 
            this.btnDischarge.Location = new System.Drawing.Point(443, 407);
            this.btnDischarge.Name = "btnDischarge";
            this.btnDischarge.Size = new System.Drawing.Size(170, 23);
            this.btnDischarge.TabIndex = 10;
            this.btnDischarge.Text = "Выписать пациента";
            this.btnDischarge.UseVisualStyleBackColor = true;
            this.btnDischarge.Click += new System.EventHandler(this.btnDischarge_Click);
            // 
            // btnRepairBed
            // 
            this.btnRepairBed.Location = new System.Drawing.Point(25, 407);
            this.btnRepairBed.Name = "btnRepairBed";
            this.btnRepairBed.Size = new System.Drawing.Size(172, 23);
            this.btnRepairBed.TabIndex = 11;
            this.btnRepairBed.Text = "Койка после ремонта";
            this.btnRepairBed.UseVisualStyleBackColor = true;
            this.btnRepairBed.Click += new System.EventHandler(this.btnRepairBed_Click);
            // 
            // tabControlDepartments
            // 
            this.tabControlDepartments.Location = new System.Drawing.Point(12, 34);
            this.tabControlDepartments.Name = "tabControlDepartments";
            this.tabControlDepartments.SelectedIndex = 0;
            this.tabControlDepartments.Size = new System.Drawing.Size(666, 355);
            this.tabControlDepartments.TabIndex = 12;
            // 
            // FormPatients
            // 
            this.ClientSize = new System.Drawing.Size(781, 482);
            this.Controls.Add(this.tabControlDepartments);
            this.Controls.Add(this.btnRepairBed);
            this.Controls.Add(this.btnDischarge);
            this.Controls.Add(this.btnMarkBroken);
            this.Name = "FormPatients";
            this.Load += new System.EventHandler(this.FormPatients_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMarkBroken;
        private System.Windows.Forms.Button btnDischarge;
        private System.Windows.Forms.Button btnRepairBed;
        private System.Windows.Forms.TabControl tabControlDepartments;
    }
}