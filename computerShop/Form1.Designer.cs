using System;

namespace computerShop
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.body = new System.Windows.Forms.TableLayoutPanel();
            this.left_navigation = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.DashboardBTN = new System.Windows.Forms.Button();
            this.membersBTN = new System.Windows.Forms.Button();
            this.main = new System.Windows.Forms.TableLayoutPanel();
            this.ComputersBTN = new System.Windows.Forms.Button();
            this.body.SuspendLayout();
            this.left_navigation.SuspendLayout();
            this.SuspendLayout();
            // 
            // body
            // 
            this.body.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.body.ColumnCount = 2;
            this.body.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 233F));
            this.body.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.body.Controls.Add(this.left_navigation, 0, 0);
            this.body.Controls.Add(this.main, 1, 0);
            this.body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.body.Location = new System.Drawing.Point(0, 0);
            this.body.Margin = new System.Windows.Forms.Padding(0);
            this.body.Name = "body";
            this.body.RowCount = 1;
            this.body.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.body.Size = new System.Drawing.Size(1026, 535);
            this.body.TabIndex = 0;
            this.body.Paint += new System.Windows.Forms.PaintEventHandler(this.body_Paint);
            // 
            // left_navigation
            // 
            this.left_navigation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.left_navigation.ColumnCount = 1;
            this.left_navigation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.left_navigation.Controls.Add(this.label1, 0, 0);
            this.left_navigation.Controls.Add(this.DashboardBTN, 0, 1);
            this.left_navigation.Controls.Add(this.membersBTN, 0, 2);
            this.left_navigation.Controls.Add(this.ComputersBTN, 0, 3);
            this.left_navigation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.left_navigation.Location = new System.Drawing.Point(0, 0);
            this.left_navigation.Margin = new System.Windows.Forms.Padding(0);
            this.left_navigation.Name = "left_navigation";
            this.left_navigation.RowCount = 5;
            this.left_navigation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.left_navigation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.left_navigation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.left_navigation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.left_navigation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.left_navigation.Size = new System.Drawing.Size(233, 535);
            this.left_navigation.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Lucida Sans", 12F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label1.Size = new System.Drawing.Size(227, 70);
            this.label1.TabIndex = 2;
            this.label1.Text = "Computeran ni Mark";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DashboardBTN
            // 
            this.DashboardBTN.BackColor = System.Drawing.Color.Transparent;
            this.DashboardBTN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DashboardBTN.FlatAppearance.BorderSize = 0;
            this.DashboardBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DashboardBTN.Font = new System.Drawing.Font("Lucida Sans", 12F);
            this.DashboardBTN.ForeColor = System.Drawing.Color.White;
            this.DashboardBTN.Image = ((System.Drawing.Image)(resources.GetObject("DashboardBTN.Image")));
            this.DashboardBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DashboardBTN.Location = new System.Drawing.Point(6, 73);
            this.DashboardBTN.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.DashboardBTN.Name = "DashboardBTN";
            this.DashboardBTN.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.DashboardBTN.Size = new System.Drawing.Size(221, 54);
            this.DashboardBTN.TabIndex = 0;
            this.DashboardBTN.Text = "Dashboard";
            this.DashboardBTN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.DashboardBTN.UseVisualStyleBackColor = false;
            this.DashboardBTN.Click += new System.EventHandler(this.DashboardBTN_Click);
            // 
            // membersBTN
            // 
            this.membersBTN.BackColor = System.Drawing.Color.Transparent;
            this.membersBTN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.membersBTN.FlatAppearance.BorderSize = 0;
            this.membersBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.membersBTN.Font = new System.Drawing.Font("Lucida Sans", 12F);
            this.membersBTN.ForeColor = System.Drawing.Color.White;
            this.membersBTN.Image = ((System.Drawing.Image)(resources.GetObject("membersBTN.Image")));
            this.membersBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.membersBTN.Location = new System.Drawing.Point(6, 133);
            this.membersBTN.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.membersBTN.Name = "membersBTN";
            this.membersBTN.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.membersBTN.Size = new System.Drawing.Size(221, 54);
            this.membersBTN.TabIndex = 1;
            this.membersBTN.Text = "Members";
            this.membersBTN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.membersBTN.UseVisualStyleBackColor = false;
            this.membersBTN.Click += new System.EventHandler(this.membersBTN_Click);
            // 
            // main
            // 
            this.main.ColumnCount = 1;
            this.main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.main.Location = new System.Drawing.Point(233, 0);
            this.main.Margin = new System.Windows.Forms.Padding(0);
            this.main.Name = "main";
            this.main.RowCount = 2;
            this.main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.main.Size = new System.Drawing.Size(793, 535);
            this.main.TabIndex = 1;
            // 
            // ComputersBTN
            // 
            this.ComputersBTN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ComputersBTN.Location = new System.Drawing.Point(0, 190);
            this.ComputersBTN.Margin = new System.Windows.Forms.Padding(0);
            this.ComputersBTN.Name = "ComputersBTN";
            this.ComputersBTN.Size = new System.Drawing.Size(233, 60);
            this.ComputersBTN.TabIndex = 3;
            this.ComputersBTN.Text = "button1";
            this.ComputersBTN.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1026, 535);
            this.Controls.Add(this.body);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "Form1";
            this.Text = "Computeran ni Mark";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.body.ResumeLayout(false);
            this.left_navigation.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void membersBTN_ClickAsync(object sender, EventArgs e)
        {

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel body;
        private System.Windows.Forms.TableLayoutPanel left_navigation;
        private System.Windows.Forms.Button DashboardBTN;
        private System.Windows.Forms.Button membersBTN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel main;
        private System.Windows.Forms.Button ComputersBTN;
    }
}