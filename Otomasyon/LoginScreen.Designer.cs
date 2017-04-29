namespace Otomasyon
{
    partial class LoginScreen
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginScreen));
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metrotbxID = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroTbxPassword = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.metroBtnLogin = new MetroFramework.Controls.MetroButton();
            this.pictureBoxTrue = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.metroLink1 = new MetroFramework.Controls.MetroLink();
            this.pictureBoxWrong = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWrong)).BeginInit();
            this.SuspendLayout();
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(9, 425);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(111, 19);
            this.metroLabel1.TabIndex = 1;
            this.metroLabel1.Text = "Bağlantı Durumu ";
            // 
            // metrotbxID
            // 
            // 
            // 
            // 
            this.metrotbxID.CustomButton.Image = null;
            this.metrotbxID.CustomButton.Location = new System.Drawing.Point(142, 2);
            this.metrotbxID.CustomButton.Name = "";
            this.metrotbxID.CustomButton.Size = new System.Drawing.Size(25, 25);
            this.metrotbxID.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metrotbxID.CustomButton.TabIndex = 1;
            this.metrotbxID.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metrotbxID.CustomButton.UseSelectable = true;
            this.metrotbxID.CustomButton.Visible = false;
            this.metrotbxID.FontSize = MetroFramework.MetroTextBoxSize.Tall;
            this.metrotbxID.FontWeight = MetroFramework.MetroTextBoxWeight.Light;
            this.metrotbxID.Lines = new string[0];
            this.metrotbxID.Location = new System.Drawing.Point(66, 220);
            this.metrotbxID.MaxLength = 32767;
            this.metrotbxID.Name = "metrotbxID";
            this.metrotbxID.PasswordChar = '\0';
            this.metrotbxID.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metrotbxID.SelectedText = "";
            this.metrotbxID.SelectionLength = 0;
            this.metrotbxID.SelectionStart = 0;
            this.metrotbxID.Size = new System.Drawing.Size(170, 30);
            this.metrotbxID.TabIndex = 3;
            this.metrotbxID.UseSelectable = true;
            this.metrotbxID.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metrotbxID.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.metroLabel2.Location = new System.Drawing.Point(66, 192);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(104, 25);
            this.metroLabel2.TabIndex = 5;
            this.metroLabel2.Text = "Kullanıcı Adı";
            // 
            // metroTbxPassword
            // 
            // 
            // 
            // 
            this.metroTbxPassword.CustomButton.Image = null;
            this.metroTbxPassword.CustomButton.Location = new System.Drawing.Point(142, 2);
            this.metroTbxPassword.CustomButton.Name = "";
            this.metroTbxPassword.CustomButton.Size = new System.Drawing.Size(25, 25);
            this.metroTbxPassword.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTbxPassword.CustomButton.TabIndex = 1;
            this.metroTbxPassword.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTbxPassword.CustomButton.UseSelectable = true;
            this.metroTbxPassword.CustomButton.Visible = false;
            this.metroTbxPassword.FontSize = MetroFramework.MetroTextBoxSize.Tall;
            this.metroTbxPassword.FontWeight = MetroFramework.MetroTextBoxWeight.Light;
            this.metroTbxPassword.Lines = new string[0];
            this.metroTbxPassword.Location = new System.Drawing.Point(66, 290);
            this.metroTbxPassword.MaxLength = 32767;
            this.metroTbxPassword.Name = "metroTbxPassword";
            this.metroTbxPassword.PasswordChar = '•';
            this.metroTbxPassword.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTbxPassword.SelectedText = "";
            this.metroTbxPassword.SelectionLength = 0;
            this.metroTbxPassword.SelectionStart = 0;
            this.metroTbxPassword.Size = new System.Drawing.Size(170, 30);
            this.metroTbxPassword.TabIndex = 6;
            this.metroTbxPassword.UseSelectable = true;
            this.metroTbxPassword.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTbxPassword.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.FontSize = MetroFramework.MetroLabelSize.Tall;
            this.metroLabel3.Location = new System.Drawing.Point(66, 262);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(45, 25);
            this.metroLabel3.TabIndex = 7;
            this.metroLabel3.Text = "Şifre";
            // 
            // metroBtnLogin
            // 
            this.metroBtnLogin.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.metroBtnLogin.FontWeight = MetroFramework.MetroButtonWeight.Regular;
            this.metroBtnLogin.Location = new System.Drawing.Point(101, 337);
            this.metroBtnLogin.Name = "metroBtnLogin";
            this.metroBtnLogin.Size = new System.Drawing.Size(98, 34);
            this.metroBtnLogin.TabIndex = 8;
            this.metroBtnLogin.Text = "Giriş Yap";
            this.metroBtnLogin.UseSelectable = true;
            this.metroBtnLogin.Click += new System.EventHandler(this.metroBtnLogin_Click);
            // 
            // pictureBoxTrue
            // 
            this.pictureBoxTrue.Enabled = false;
            this.pictureBoxTrue.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxTrue.Image")));
            this.pictureBoxTrue.Location = new System.Drawing.Point(120, 424);
            this.pictureBoxTrue.Name = "pictureBoxTrue";
            this.pictureBoxTrue.Size = new System.Drawing.Size(20, 20);
            this.pictureBoxTrue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxTrue.TabIndex = 2;
            this.pictureBoxTrue.TabStop = false;
            this.pictureBoxTrue.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Enabled = false;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(66, 33);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(170, 124);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // metroLink1
            // 
            this.metroLink1.Location = new System.Drawing.Point(227, 425);
            this.metroLink1.Name = "metroLink1";
            this.metroLink1.Size = new System.Drawing.Size(75, 23);
            this.metroLink1.TabIndex = 9;
            this.metroLink1.Text = "Yeni Kayıt";
            this.metroLink1.UseSelectable = true;
            this.metroLink1.Click += new System.EventHandler(this.metroLink1_Click);
            // 
            // pictureBoxWrong
            // 
            this.pictureBoxWrong.Enabled = false;
            this.pictureBoxWrong.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxWrong.Image")));
            this.pictureBoxWrong.Location = new System.Drawing.Point(120, 424);
            this.pictureBoxWrong.Name = "pictureBoxWrong";
            this.pictureBoxWrong.Size = new System.Drawing.Size(20, 20);
            this.pictureBoxWrong.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxWrong.TabIndex = 10;
            this.pictureBoxWrong.TabStop = false;
            this.pictureBoxWrong.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 3000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(170, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Giriş";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(24, 165);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 12;
            this.button2.Text = "Splash";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // LoginScreen
            // 
            this.AcceptButton = this.metroBtnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(308, 453);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBoxWrong);
            this.Controls.Add(this.metroLink1);
            this.Controls.Add(this.metroBtnLogin);
            this.Controls.Add(this.metroLabel3);
            this.Controls.Add(this.metroTbxPassword);
            this.Controls.Add(this.metroLabel2);
            this.Controls.Add(this.metrotbxID);
            this.Controls.Add(this.pictureBoxTrue);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.pictureBox1);
            this.DisplayHeader = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginScreen";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Resizable = false;
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.DropShadow;
            this.Style = MetroFramework.MetroColorStyle.Default;
            this.Text = "TED Oturum Aç";
            this.Theme = MetroFramework.MetroThemeStyle.Default;
            this.Load += new System.EventHandler(this.LoginScreen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxWrong)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private System.Windows.Forms.PictureBox pictureBoxTrue;
        private MetroFramework.Controls.MetroTextBox metrotbxID;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox metroTbxPassword;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroButton metroBtnLogin;
        private MetroFramework.Controls.MetroLink metroLink1;
        private System.Windows.Forms.PictureBox pictureBoxWrong;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}