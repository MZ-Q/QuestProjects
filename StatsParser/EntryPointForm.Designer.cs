namespace StatsParser
{
    partial class EntryPointForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntryPointForm));
            this.ParseBtn = new System.Windows.Forms.Button();
            this.GameIDTextBox = new System.Windows.Forms.TextBox();
            this.StatsPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.StatsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ParseBtn
            // 
            this.ParseBtn.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ParseBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ParseBtn.Font = new System.Drawing.Font("Arial Black", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ParseBtn.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.ParseBtn.Location = new System.Drawing.Point(227, 13);
            this.ParseBtn.Name = "ParseBtn";
            this.ParseBtn.Size = new System.Drawing.Size(57, 46);
            this.ParseBtn.TabIndex = 0;
            this.ParseBtn.Text = "Parse";
            this.ParseBtn.UseVisualStyleBackColor = false;
            this.ParseBtn.Click += new System.EventHandler(this.ParseBtn_Click);
            // 
            // GameIDTextBox
            // 
            this.GameIDTextBox.BackColor = System.Drawing.SystemColors.MenuText;
            this.GameIDTextBox.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.GameIDTextBox.Location = new System.Drawing.Point(104, 13);
            this.GameIDTextBox.Name = "GameIDTextBox";
            this.GameIDTextBox.Size = new System.Drawing.Size(80, 20);
            this.GameIDTextBox.TabIndex = 1;
            // 
            // StatsPanel
            // 
            this.StatsPanel.BackColor = System.Drawing.SystemColors.Control;
            this.StatsPanel.BackgroundImage = global::StatsParser.Properties.Resources.QuestUA;
            this.StatsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.StatsPanel.Controls.Add(this.label1);
            this.StatsPanel.Controls.Add(this.ParseBtn);
            this.StatsPanel.Controls.Add(this.GameIDTextBox);
            this.StatsPanel.ForeColor = System.Drawing.SystemColors.Control;
            this.StatsPanel.Location = new System.Drawing.Point(12, 12);
            this.StatsPanel.Name = "StatsPanel";
            this.StatsPanel.Size = new System.Drawing.Size(324, 125);
            this.StatsPanel.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(101, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Game URL:";
            // 
            // EntryPointForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 152);
            this.Controls.Add(this.StatsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EntryPointForm";
            this.Text = "StatsParser";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.EntryPointForm_Paint);
            this.StatsPanel.ResumeLayout(false);
            this.StatsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ParseBtn;
        private System.Windows.Forms.TextBox GameIDTextBox;
        private System.Windows.Forms.Panel StatsPanel;
        private System.Windows.Forms.Label label1;
    }
}

