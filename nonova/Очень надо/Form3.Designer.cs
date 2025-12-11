namespace Очень_надо
{
    partial class Form3
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
            this.btnConfirm = new System.Windows.Forms.Button();
            this.tabControlDepartments = new System.Windows.Forms.TabControl();
            this.BtnEdit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(727, 401);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(90, 23);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "Сохранить";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click_1);
            // 
            // tabControlDepartments
            // 
            this.tabControlDepartments.Location = new System.Drawing.Point(23, 23);
            this.tabControlDepartments.Name = "tabControlDepartments";
            this.tabControlDepartments.SelectedIndex = 0;
            this.tabControlDepartments.Size = new System.Drawing.Size(701, 415);
            this.tabControlDepartments.TabIndex = 0;
            // 
            // BtnEdit
            // 
            this.BtnEdit.Location = new System.Drawing.Point(726, 350);
            this.BtnEdit.Name = "BtnEdit";
            this.BtnEdit.Size = new System.Drawing.Size(90, 23);
            this.BtnEdit.TabIndex = 2;
            this.BtnEdit.Text = "Редактировать";
            this.BtnEdit.UseVisualStyleBackColor = true;
            this.BtnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 450);
            this.Controls.Add(this.BtnEdit);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.tabControlDepartments);
            this.Name = "Form3";
            this.Text = "Form3";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.TabControl tabControlDepartments;
        private System.Windows.Forms.Button BtnEdit;
    }
}