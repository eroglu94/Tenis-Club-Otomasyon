using MetroFramework;
using MetroFramework.Forms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;

namespace Otomasyon
{
    public partial class LoginScreen : MetroForm
    {
        //string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ted-db;Integrated Security=True;Connect Timeout=2;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        string connectionString = "Server=tcp:tx7o229p2a.database.windows.net,1433;Database=tedx-db;User ID=eroglu1994@tx7o229p2a;Password=8874788474Aa;Trusted_Connection=False;Encrypt=True;Connection Timeout=3";


        public LoginScreen()
        {
            InitializeComponent();
        }

        private void LoginScreen_Load(object sender, EventArgs e)
        {
            try
            {
                metrotbxID.Enabled = false;
                metroTbxPassword.Enabled = false;
                metroBtnLogin.Enabled = false;


                if (SqlConnectionCheck() == true)
                {
                    pictureBoxTrue.Visible = true;
                    pictureBoxWrong.Visible = false;
                }
                else
                {
                    pictureBoxTrue.Visible = false;
                    pictureBoxWrong.Visible = true;
                }


                metrotbxID.Enabled = true;
                metroTbxPassword.Enabled = true;
                metroBtnLogin.Enabled = true;





            }
            catch (Exception)
            {


            }


        }

        private void metroLink1_Click(object sender, EventArgs e)
        {

        }

        private void metroBtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (LogIn() == true)
                {
                    //Form Kapatılıp MainForm Açılıyor
                    SplashScreen();
                }

            }
            catch (Exception exp)
            {
                GetErrorMessage(exp);

            }

        }

        private bool SqlConnectionCheck()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;

            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SqlConnectionCheck() == true)
            {
                pictureBoxTrue.Visible = true;
                pictureBoxWrong.Visible = false;
            }
            else
            {
                pictureBoxTrue.Visible = false;
                pictureBoxWrong.Visible = true;
            }
        }

        private bool LogIn()
        {
            try
            {
                // Veri Değişmesin Diye Kontolleri Kapattık
                ControlsOFF();


                SqlConnection conn = new SqlConnection(connectionString);
                //SqlCommand cmd = new SqlCommand("SELECT * FROM Accounts WHERE Username ='" + metrotbxID.Text + "'AND Password = '" + metroTbxPassword.Text + "'", conn);
                SqlCommand cmd = new SqlCommand("SELECT * FROM Accounts WHERE Username =@Username AND Password=@Password", conn);
                cmd.Parameters.AddWithValue("@Username", metrotbxID.Text);
                cmd.Parameters.AddWithValue("@Password", metroTbxPassword.Text);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {

                    if (metrotbxID.Text == dr["Username"].ToString() && metroTbxPassword.Text == dr["Password"].ToString())
                    {

                        //LastLogin kısmı güncelleniyor
                        LogInUserUpdate();

                        MainForm.Name_ = dr["Name"].ToString();
                        MainForm.Surname_ = dr["Surname"].ToString();
                        MainForm.Account_ID_ = Convert.ToInt32(dr["Id"]);

                        MetroMessageBox.Show(this, "Başarıyla Giriş Yapıldı.", " ", 120);
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        conn.Close();
                        return false;
                    }

                }
                else
                {
                    MetroMessageBox.Show(this, "\nKullanıcı Adı veya Şifre Hatalı", "Hata", 120);
                }

                conn.Close();


                //// İşlemler Bitince Kontrolleri Açtık.
                ControlsON();
                return false;


            }
            catch (SqlException )
            {

                MetroMessageBox.Show(this, string.Format("Sunucuyla Bağlantı Kurulamadı. İnternet Bağlantınızı Kontol Ediniz. \nDestek: eroglu1994@gmail.com"), " ");
                ControlsON();
                return false;
            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(this, string.Format("Error: {0}\nDestek: eroglu1994@gmail.com", exp.Message), " ");
                ControlsON();
                return false;
            }

        }

        private void LogInUserUpdate()
        {
            try
            {
                SqlConnection conn_Update = new SqlConnection(connectionString);
                conn_Update.Open();
                string kayit = "UPDATE Accounts SET LastLogin=@LastLogin WHERE Username=@Username AND Password=@Password";
                SqlCommand komut = new SqlCommand(kayit, conn_Update);
                komut.Parameters.AddWithValue("@LastLogin", GetNistTime());
                komut.Parameters.AddWithValue("@Username", metrotbxID.Text);
                komut.Parameters.AddWithValue("@Password", metroTbxPassword.Text);
                komut.ExecuteNonQuery();
                conn_Update.Close();

            }
            catch (Exception exp)
            {
                MetroMessageBox.Show(this, string.Format("Error: {0}\nDestek: eroglu1994@gmail.com", exp.Message), " ");

            }

        }

        private void ControlsOFF()
        {
            metrotbxID.Enabled = false;
            metroTbxPassword.Enabled = false;
        }

        private void ControlsON()
        {
            metrotbxID.Enabled = true;
            metroTbxPassword.Enabled = true;
        }

        public static DateTime GetNistTime()
        {
            try
            {
                DateTime dateTime = DateTime.MinValue;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader stream = new StreamReader(response.GetResponseStream());
                    string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                    string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                    double milliseconds = Convert.ToInt64(time) / 1000.0;
                    dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
                }

                return dateTime;
            }
            catch (Exception)
            {

                return DateTime.Now;
            }
            
        }

        public void GetErrorMessage(Exception exp)
        {
            MetroMessageBox.Show(this, string.Format("Error: {0}\nDestek: eroglu1994@gmail.com", exp.Message), " ");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SplashScreen();






            //SplashForm.ShowSplashScreen();
            //MainForm mainForm = new MainForm(); //this takes ages
            //SplashForm.CloseForm();
            //Application.Run(mainForm);




        }

        private void button2_Click(object sender, EventArgs e)
        {
            SplashForm form = new SplashForm();
            form.Show();
        }

        private void SplashScreen()
        {
            this.Hide();
            SplashForm.ShowSplashScreen();
            //Thread.Sleep(5000);
            var MainForm = new MainForm();
           
            MainForm.Closed += (s, args) => this.Close();
            
            MainForm.Show();
            SplashForm.CloseForm();
            timer1.Dispose();
            MainForm.TopMost = true;
            MainForm.TopMost = false;

        }
    }
}
