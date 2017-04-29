using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Text.RegularExpressions;
using MetroFramework;
using MetroFramework.Animation;
using MetroFramework.Components;
using MetroFramework.Controls;
using MetroFramework.Drawing;
using MetroFramework.Drawing.Html;
using MetroFramework.Fonts;
using MetroFramework.Interfaces;
using MetroFramework.Native;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;
using System.Xml;
using System.Net.Sockets;

namespace Otomasyon
{
    public partial class MainForm : MetroForm
    {
        public static string Name_;
        public static string Surname_;
        public static int Account_ID_;
        public static int Selected_AccountID_;

        private static int Player; //1=Player1, 2=Player2;
        private static string Player1Name;
        private static string Player2Name;
        private static string Player1ID;
        private static string Player2ID;

        private static DateTime DateLocal;

        private List<int> HocaList = new List<int>();

        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ted-db;Integrated Security=True;Connect Timeout=2;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        //string connectionString = "Server=tcp:tx7o229p2a.database.windows.net,1433;Database=tedx-db;User ID=eroglu1994@tx7o229p2a;Password=8874788474Aa;Trusted_Connection=False;Encrypt=True;Connection Timeout=3";

        //Formlar arası kontrolu sağlar
        // "0"=Yanlış kullanıcı adı şifre    ------     "1"=Doğru Kullanıcı adı Şifre
        public static int formControl;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DateLocal = GetNistTime();
            GetOnlineUser();
            WorkerLastOnline.RunWorkerAsync();
            WorkerDateTime.RunWorkerAsync();
            //dateTimePicker1.CustomFormat = "dd MMMM yyyy";
            GetControllersData();

        }

        private void metrolinkSignIn_Click(object sender, EventArgs e)
        {

        }

        private void GetOnlineUser()
        {
            lblOnlineAccount.Text = Name_ + " " + Surname_;


        }

        public DateTime GetNistTime()
        {
            int tryCount = 1;
            BaşaDön:
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
            catch (Exception exp)
            {
                MetroMessageBox.Show(this, string.Format("Error: {0}\nİnternet Bağlantınızın Olduğundan Emin Olun.\nTekrar Bağlantı Kurmaya Çalışılacak({1})", exp.Message, tryCount));
                if (tryCount == 2)
                {

                    //return DateTime.Now;
                    return ddd();
                }
                else
                {
                    tryCount++;
                    goto BaşaDön;
                }

            }
            //return ddd();


        }

        private static DateTime GetFastestNISTDate()
        {
            var result = DateTime.MinValue;
            // Initialize the list of NIST time servers
            // http://tf.nist.gov/tf-cgi/servers.cgi
            string[] servers = new string[] {
"nist1-ny.ustiming.org",
"nist1-nj.ustiming.org",
"nist1-pa.ustiming.org",
"time-a.nist.gov",
"time-b.nist.gov",
"nist1.aol-va.symmetricom.com",
"nist1.columbiacountyga.gov",
"nist1-chi.ustiming.org",
"nist.expertsmi.com",
"nist.netservicesgroup.com"
};

            // Try 5 servers in random order to spread the load
            Random rnd = new Random();
            foreach (string server in servers.OrderBy(s => rnd.NextDouble()).Take(5))
            {
                try
                {
                    // Connect to the server (at port 13) and get the response
                    string serverResponse = string.Empty;
                    using (var reader = new StreamReader(new System.Net.Sockets.TcpClient(server, 13).GetStream()))
                    {
                        serverResponse = reader.ReadToEnd();
                    }

                    // If a response was received
                    if (!string.IsNullOrEmpty(serverResponse))
                    {
                        // Split the response string ("55596 11-02-14 13:54:11 00 0 0 478.1 UTC(NIST) *")
                        string[] tokens = serverResponse.Split(' ');

                        // Check the number of tokens
                        if (tokens.Length >= 6)
                        {
                            // Check the health status
                            string health = tokens[5];
                            if (health == "0")
                            {
                                // Get date and time parts from the server response
                                string[] dateParts = tokens[1].Split('-');
                                string[] timeParts = tokens[2].Split(':');

                                // Create a DateTime instance
                                DateTime utcDateTime = new DateTime(
                                    Convert.ToInt32(dateParts[0]) + 2000,
                                    Convert.ToInt32(dateParts[1]), Convert.ToInt32(dateParts[2]),
                                    Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]),
                                    Convert.ToInt32(timeParts[2]));

                                // Convert received (UTC) DateTime value to the local timezone
                                result = utcDateTime.ToLocalTime();

                                return result;
                                // Response successfully received; exit the loop

                            }
                        }

                    }

                }
                catch
                {
                    // Ignore exception and try the next server
                }
            }
            return result;
        }

        private static DateTime ddd()
        {
            BASADON:;
            var client = new TcpClient("time.nist.gov", 13);
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                if (response.ToString().Length < 7)
                {
                    goto BASADON;
                }
                else
                {
                    var utcDateTimeString = response.Substring(7, 17);
                    var localDateTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    return localDateTime;
                }

            }
        }

        private void WorkerLastOnline_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {

                    SqlConnection conn = new SqlConnection(connectionString);

                    SqlCommand cmd = new SqlCommand("UPDATE Accounts SET LastOnline=@LastOnline WHERE Id=@AccountID", conn);
                    conn.Open();
                    cmd.Parameters.AddWithValue("@LastOnline", GetNistTime());
                    cmd.Parameters.AddWithValue("@AccountID", Account_ID_);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                    System.Threading.Thread.Sleep(60000);

                }
                catch (Exception)
                {

                }
            }
            //e.Cancel = true;
            //Perform a time consuming operation and report progress.
            //e.Cancel = false;
            //worker.ReportProgress((i * 10));

        }



        private void maskedTextBox1_Click(object sender, EventArgs e)
        {
            if (maskedtbxPersonalPhoneNumber.Text == "(    )    -")
            {
                //txtbox.SelectionStart = txtbox.Text.Length - 1 // add some logic if length is 0
                //txtbox.SelectionLength = 0

                maskedtbxPersonalPhoneNumber.SelectionStart = maskedtbxPersonalPhoneNumber.Text.Length - 10;
                maskedtbxPersonalPhoneNumber.SelectionLength = 0;
            }
        }

        private void maskedTextBox2_Click(object sender, EventArgs e)
        {
            if (maskedtbxWorkPhoneNumber.Text == "(    )    -")
            {
                //txtbox.SelectionStart = txtbox.Text.Length - 1 // add some logic if length is 0
                //txtbox.SelectionLength = 0

                maskedtbxWorkPhoneNumber.SelectionStart = maskedtbxWorkPhoneNumber.Text.Length - 10;
                maskedtbxWorkPhoneNumber.SelectionLength = 0;
            }
        }

        private void metroButton1_Click_1(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Haftanın Gününe Göre Veri Çeker
        /// </summary>
        /// <param name="day">DayOfTheWeek</param> <param name="KortName">Kort İsmi</param><param name="MetroGrid">Kullanılacak DataGridView</param>
        private void GuneGoreVeriCek(int day, string KortName, MetroGrid MetroGrid)
        {
            try
            {


                if (KortName == "")
                {
                    MetroMessageBox.Show(this, "\nLütfen Kort Seçiniz!", "Hata", 120);
                    goto EmptyCortName;
                }

                DateTime date = new DateTime();
                date = FindDay(day, DateLocal);


                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT * FROM KortTakip WHERE Date=@Date AND KortName=@KortName ORDER BY Time ASC", conn);
                SqlCommand cmd_2 = new SqlCommand("SELECT * FROM KortTakip WHERE Date=@Date_2 ORDER BY KortName,Time ASC", conn);

                conn.Open();



                if (KortName == "*Hepsi")
                {

                    cmd_2.Parameters.AddWithValue("@Date_2", date.Date);
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd_2);

                    DataTable datatable = new DataTable();
                    dataAdapter.Fill(datatable);
                    MetroGrid.DataSource = datatable;
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    cmd.Parameters.AddWithValue("@KortName", KortName);
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                    DataTable datatable = new DataTable();
                    dataAdapter.Fill(datatable);
                    MetroGrid.DataSource = datatable;
                }
                MetroGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                //MetroGrid.Columns[0].Visible = false;


                MetroGrid.Columns[0].HeaderText = "ID";
                MetroGrid.Columns[1].HeaderText = "Kort İsmi";
                MetroGrid.Columns[2].HeaderText = "Saat";
                MetroGrid.Columns[3].HeaderText = "Tarih";
                MetroGrid.Columns[4].HeaderText = "Oyuncu 1 (ID)";
                MetroGrid.Columns[5].HeaderText = "Oyuncu 2 (ID)";
                MetroGrid.Columns[6].Visible = false;
                MetroGrid.Columns[7].Visible = false;
                MetroGrid.Columns[8].HeaderText = "Işık";
                MetroGrid.Columns[9].HeaderText = "Ekleyen Kişi";
                MetroGrid.Columns[10].HeaderText = "Not";
                MetroGrid.Columns[11].Visible = false;

                MetroGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                MetroGrid.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                MetroGrid.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //MetroGrid.Columns[3].Width = 100;


                //MetroGrid.Columns["Time"].SortMode = DataGridViewColumnSortMode.Automatic;
                //MetroGrid.Sort(MetroGrid.Columns[3], ListSortDirection.Ascending);
                conn.Close();

                try
                {
                    foreach (DataGridViewRow row in MetroGrid.Rows)
                    {
                        for (int i = 0; i < HocaList.Count; i++)
                        {
                            if (row.Cells[6].Value.ToString() == HocaList[i].ToString() || row.Cells[7].Value.ToString() == HocaList[i].ToString())
                            {
                                try
                                {
                                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                                    style.Font = new Font(MetroGrid.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                                    style.ForeColor = Color.IndianRed;

                                    foreach (DataGridViewCell cell in row.Cells)
                                        cell.Style.ApplyStyle(style);


                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {


                }





                //GOTO Statement
                EmptyCortName:;

            }
            catch (Exception exp)
            {
                GetErrorMessage(exp);
            }



        }

        private void GetControllersData()
        {
            try
            {
                //Hoca Listesi
                HocaListesi();

                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("Select KortName From Corts ORDER BY KortName ASC", conn);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    metroComboBoxPazartesi.Items.Add(dr["KortName"]);
                    metroComboBoxSalı.Items.Add(dr["KortName"]);
                    metroComboBoxCarsamba.Items.Add(dr["KortName"]);
                    metroComboBoxPersembe.Items.Add(dr["KortName"]);
                    metroComboBoxCuma.Items.Add(dr["KortName"]);
                    metroComboBoxCumartesi.Items.Add(dr["KortName"]);
                    metroComboBoxPazar.Items.Add(dr["KortName"]);

                    metroComboBoxKortKayıtKort.Items.Add(dr["KortName"]);

                    metroComboBoxKortFilterKort.Items.Add(dr["KortName"]);

                    metroComboBoxFixKayıtKayıtKort.Items.Add(dr["KortName"]);

                    metroComboBoxKortKayıtDüzenleKort.Items.Add(dr["KortName"]);
                }
                conn.Close();

                //Kort Tarih Labelleri Güncelleniyor
                RefreshDateLabels();
                //metrolblTarihPazartesi.Text =   FindDay(1, date).ToString("dd/MM/yyyy");
                //metrolblTarihSalı.Text =        FindDay(2, date).ToString("dd/MM/yyyy");
                //metrolblTarihCarsamba.Text =    FindDay(3, date).ToString("dd/MM/yyyy");
                //metrolblTarihPersembe.Text =    FindDay(4, date).ToString("dd/MM/yyyy");
                //metrolblTarihCuma.Text =        FindDay(5, date).ToString("dd/MM/yyyy");
                //metrolblTarihCumartesi.Text =   FindDay(6, date).ToString("dd/MM/yyyy");
                //metrolblTarihPazar.Text =       FindDay(7, date).ToString("dd/MM/yyyy");

                //Üye Kayıt
                SqlConnection connUyeKayit = new SqlConnection(connectionString);
                SqlCommand cmdUyeKayit = new SqlCommand("SELECT MembershipName From MembershipTypes", connUyeKayit);
                connUyeKayit.Open();
                SqlDataReader DataReaderUyeKayit = cmdUyeKayit.ExecuteReader();
                while (DataReaderUyeKayit.Read())
                {
                    metroComboBoxTypeOfMembership.Items.Add(DataReaderUyeKayit["MembershipName"]);
                    metroComboBoxBilgiTypeOfMembership.Items.Add(DataReaderUyeKayit["MembershipName"]);
                    metroComboBoxMuhasebeÜyelikTipi.Items.Add(DataReaderUyeKayit["MembershipName"]);
                }
                connUyeKayit.Close();

                //Kort Kayit
                KortKayıtGridView();
                Player = 1;

                //Haftanın Kortları
                GuneGoreVeriCek(1, "*Hepsi", metroGridKortPazartesi);
                GuneGoreVeriCek(2, "*Hepsi", metroGridKortSalı);
                GuneGoreVeriCek(3, "*Hepsi", metroGridKortCarsamba);
                GuneGoreVeriCek(4, "*Hepsi", metroGridKortPersembe);
                GuneGoreVeriCek(5, "*Hepsi", metroGridKortCuma);
                GuneGoreVeriCek(6, "*Hepsi", metroGridKortCumartesi);
                GuneGoreVeriCek(0, "*Hepsi", metroGridKortPazar);


                //Kort Search
                KortSearch();


                //Uye Bilgileri
                GetMemberData();

                //Muhasebe Uye Bilgileri
                AidatSonÖdemeGüncelle();
                MuhasebeAidatUyeler();


                //Hoca Takip
                HocaTakipGetData();
                HocaMemberGetData();

                //Fix Kayıt
                FixKayıtKortKayıtları();
                FixKayıtOyuncuList();

                //Kort Kayıt Düzenle
                KortKayıtDüzenle();

                //Döviz Kurları
                try
                {
                    DövizKur doviz = new DövizKur();
                    string EURO_Date = doviz.EURO_Date();
                    string USD_Date = doviz.USD_Date();
                    string POUND_Date = doviz.POUND_Date();


                    metrolblMuhasebeEURO.Text = EURO_Date;
                    metrolblMuhasebeUSD.Text = USD_Date;
                    metrolblMuhasebePOUND.Text = POUND_Date;


                    metrolblHocaTakipEURO.Text = EURO_Date;
                    metrolblHocaTakipUSD.Text = USD_Date;
                    metrolblHocaTakipPOUND.Text = POUND_Date;

                }
                catch (Exception)
                {


                }





            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }


        }

        private void KortKayıtGridView()
        {
            try
            {
                //GuneGoreVeriCek(Convert.ToInt32(GetNistTime().DayOfWeek), "*Hepsi", metroGridKortKayıtları);
                //metroGridKortKayıtları.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT KortName,Time,Date,Player1,Player2,Player1_Id,Player2_Id,Light FROM KortTakip WHERE Date=@Date ORDER BY Date,KortName,Time", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@Date", dateTimePickerKortKayıtTarih.Value.Date);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                metroGridKortKayıtları.DataSource = dt;
                metroGridKortKayıtları.Columns[5].Visible = false;
                metroGridKortKayıtları.Columns[6].Visible = false;

                //metroGridKortKayıtları.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;


                metroGridKortKayıtları.Columns[0].HeaderText = "Kort İsmi";
                metroGridKortKayıtları.Columns[1].HeaderText = "Saat";
                //metroGridKortKayıtları.Columns[1].Width = 50;
                metroGridKortKayıtları.Columns[2].Visible = false;
                metroGridKortKayıtları.Columns[3].HeaderText = "Oyuncu 1";
                metroGridKortKayıtları.Columns[4].HeaderText = "Oyuncu 2";
                metroGridKortKayıtları.Columns[7].HeaderText = "Işık";

                metroGridKortKayıtları.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridKortKayıtları.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridKortKayıtları.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

                conn.Close();

                try
                {
                    foreach (DataGridViewRow row in metroGridKortKayıtları.Rows)
                    {
                        for (int i = 0; i <= HocaList.Count; i++)
                        {
                            if (Convert.ToInt32(row.Cells[5].Value) == HocaList[i] || Convert.ToInt32(row.Cells[6].Value) == HocaList[i])
                            {
                                try
                                {
                                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                                    style.Font = new Font(metroGridKortKayıtları.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                                    style.ForeColor = Color.IndianRed;

                                    foreach (DataGridViewCell cell in row.Cells)
                                        cell.Style.ApplyStyle(style);


                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {


                }
            }
            catch (Exception)
            {


            }





        }

        private static int FindWeek(DateTime date)
        {
            GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private void RefreshDateLabels()
        {
            DateTime date = new DateTime();
            date = DateLocal;
            int day = (int)date.DayOfWeek;

            switch (day)
            {
                case 1://PAZARTESİ
                    metrolblTarihPazartesi.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(2).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(3).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(4).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(5).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(6).ToString("dd/MM/yyyy");
                    break;
                case 2://SALI
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(2).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(3).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(4).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(5).ToString("dd/MM/yyyy");
                    break;
                case 3://ÇARŞAMBA
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-2).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(2).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(3).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(4).ToString("dd/MM/yyyy");
                    break;
                case 4://PERŞEMBE
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-3).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(-2).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(2).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(3).ToString("dd/MM/yyyy");
                    break;
                case 5://CUMA
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-4).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(-3).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(-2).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(2).ToString("dd/MM/yyyy");
                    break;
                case 6://CUMARTESİ
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-5).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(-4).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(-3).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(-2).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.AddDays(1).ToString("dd/MM/yyyy");
                    break;
                case 0://PAZAR
                    metrolblTarihPazartesi.Text = date.Date.AddDays(-6).ToString("dd/MM/yyyy");
                    metrolblTarihSalı.Text = date.Date.AddDays(-5).ToString("dd/MM/yyyy");
                    metrolblTarihCarsamba.Text = date.Date.AddDays(-4).ToString("dd/MM/yyyy");
                    metrolblTarihPersembe.Text = date.Date.AddDays(-3).ToString("dd/MM/yyyy");
                    metrolblTarihCuma.Text = date.Date.AddDays(-2).ToString("dd/MM/yyyy");
                    metrolblTarihCumartesi.Text = date.Date.AddDays(-1).ToString("dd/MM/yyyy");
                    metrolblTarihPazar.Text = date.Date.ToString("dd/MM/yyyy");
                    break;

                default: break;
            }

            //metrolblTarihPazartesi.Text =   FindDay(1, date).ToString("dd/MM/yyyy");
            //metrolblTarihSalı.Text =        FindDay(2, date).ToString("dd/MM/yyyy");
            //metrolblTarihCarsamba.Text =    FindDay(3, date).ToString("dd/MM/yyyy");
            //metrolblTarihPersembe.Text =    FindDay(4, date).ToString("dd/MM/yyyy");
            //metrolblTarihCuma.Text =        FindDay(5, date).ToString("dd/MM/yyyy");
            //metrolblTarihCumartesi.Text =   FindDay(6, date).ToString("dd/MM/yyyy");
            //metrolblTarihPazar.Text =       FindDay(7, date).ToString("dd/MM/yyyy");
        }


        /// <summary>
        /// Verilen değişkene göre o değişkenin hafta içindeki tarihini bulur.
        /// </summary>
        /// <param name="ArananGün">Haftanın günü sayı olarak(1,2,3,4,5,6,7)</param>
        /// <returns></returns>
        private DateTime FindDay(int ArananGün, DateTime DateTime)
        {
            try
            {
                DateTime date = new DateTime();
                int newDayNumber;
                int fark;
                date = DateTime;
                fark = (int)date.DayOfWeek - ArananGün;
                newDayNumber = date.AddDays(-fark).Day;
                DateTime new_date = new DateTime(date.Year, date.Month, newDayNumber);
                return new_date;
            }
            catch (Exception exp)
            {
                GetErrorMessage(exp);
                return new DateTime(0000, 00, 00);
            }


        }


        public void GetErrorMessage(Exception exp)
        {
            MetroMessageBox.Show(this, string.Format("Error: {0}\nDestek: eroglu1994@gmail.com", exp.ToString()), " ", 1000);
        }

        private void metroTile5_Click(object sender, EventArgs e)
        {
            UyeKayit();
        }

        private void UyeKayit()
        {
            try
            {

                //"INSERT INTO kisiler (Ad,Soyad,Yas,Tarih,Onay) VALUES (@ad,@soyad,@yas,@tarih,@onay)"
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("INSERT INTO Members (Name,Surname,DateOfBirth,Gender,TCNo,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate,Notes) VALUES (@Name,@Surname,@DateOfBirth,@Gender,@TCNo,@WorkPhoneNumber,@PersonalPhoneNumber,@Email,@TypeOfMembership,@AddedBy,@AddedDate,@MembershipFinishDate,@Notes)", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@Name", metrotbxName.Text);
                cmd.Parameters.AddWithValue("@Surname", metrotbxSurname.Text);
                cmd.Parameters.AddWithValue("@DateOfBirth", dateTimePickerDateOfBirth.Value.Date);
                cmd.Parameters.AddWithValue("@Gender", metroComboBoxGender.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("TCNo", metrotbxTCNo.Text);
                cmd.Parameters.AddWithValue("@WorkPhoneNumber", maskedtbxWorkPhoneNumber.Text);
                cmd.Parameters.AddWithValue("@PersonalPhoneNumber", maskedtbxPersonalPhoneNumber.Text);
                cmd.Parameters.AddWithValue("@Email", metrotbxEmail.Text);
                cmd.Parameters.AddWithValue("@TypeOfMembership", metroComboBoxTypeOfMembership.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
                cmd.Parameters.AddWithValue("@AddedDate", DateLocal.Date);
                //Membership Finish Date
                DateTime date = new DateTime();

                switch (metroComboBoxTypeOfMembership.SelectedItem.ToString())
                {
                    case "Ömür Boyu": date = DateLocal.AddYears(5000); break;
                    case "Yıllık": date = DateLocal.AddYears(1); break;
                    case "6 Aylık": date = DateLocal.AddMonths(6); break;
                    case "1 Aylık": date = DateLocal.AddMonths(1); break;
                    default: MetroMessageBox.Show(this, "\nÜyelik Tipini Seçerken Sorun Oluştu", "Hata"); break;
                }
                cmd.Parameters.AddWithValue("@MembershipFinishDate", date.Date);
                cmd.Parameters.AddWithValue("@Notes", richtbxNotes.Text);
                //cmd.Parameters.AddWithValue("@Paid", @Paid);
                //cmd.Parameters.AddWithValue("@PaidDate",@PaidDate);

                cmd.ExecuteNonQuery();
                conn.Close();


                MetroMessageBox.Show(this, "Üye Kaydı Başarıyla Alındı.", 120);

            }
            catch (Exception exp)
            {
                GetErrorMessage(exp);

            }

        }

        private void metroTile6_Click(object sender, EventArgs e)
        {

            foreach (Control item in groupBoxUyeKayit.Controls)
            {
                if (item is Label)
                {

                }
                else if (item is MetroTile)
                {

                }
                else if (item is Button)
                {

                }
                else
                {
                    item.ResetText();
                }
            }


            //if (item is MetroTextBox)
            //{
            //    ((MetroTextBox)item).Clear();
            //}
            //if (item is MaskedTextBox)
            //{
            //    ((MaskedTextBox)item).Clear();
            //}
            //if (item is RichTextBox)
            //{
            //    ((RichTextBox)item).Clear();
            //}
            //if (item is MetroComboBox)
            //{
            //    ((MetroComboBox)item).ResetText();
            //}
            ////if (item.Controls.Count > 0)
            ////{
            ////    textclear(item);
            ////}

        }

        private void metroTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void metroTile7_Click(object sender, EventArgs e)
        {
            //Kontrol Kapandı
            metroTileUyeBilgileri.Enabled = false;

            GetMemberData();

            //Kontrol Açıldı
            metroTileUyeBilgileri.Enabled = true;

        }

        private void metroTile4_Click(object sender, EventArgs e)
        {
            groupBoxUyeBilgileri.Visible = false;
            groupBoxUyeBilgileri.Enabled = false;

            //Own Controller
            groupBoxUyeKayit.Visible = true;
            groupBoxUyeKayit.Enabled = true;
        }

        private void metroTile6_Click_1(object sender, EventArgs e)
        {

            groupBoxUyeKayit.Visible = false;
            groupBoxUyeKayit.Enabled = false;
            //Own Controller
            groupBoxUyeBilgileri.Location = new Point(186, 20);
            groupBoxUyeBilgileri.Visible = true;
            groupBoxUyeBilgileri.Enabled = true;
        }

        private void GetMemberData()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT Id,Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate,Status FROM Members ORDER BY Name,Surname", conn);
                conn.Open();
                SqlDataAdapter dataAtapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                dataAtapter.Fill(dt);

                metroGridUyeBilgileri.DataSource = dt;
                conn.Close();

                //HOCA BOLD
                try
                {
                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                    style.Font = new Font(metroGridUyeBilgileri.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                    style.ForeColor = Color.IndianRed;
                    foreach (DataGridViewRow row in metroGridUyeBilgileri.Rows)
                    {
                        if (row.Cells["Status"].Value.ToString() == "Hoca")
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                                cell.Style.ApplyStyle(style);
                        }
                    }
                }
                catch (Exception)
                {
                }



                //MetroGrid.Columns[1].HeaderText = "Kort İsmi";
                metroGridUyeBilgileri.Columns[0].HeaderText = "ID";
                metroGridUyeBilgileri.Columns[1].HeaderText = "Ad";
                metroGridUyeBilgileri.Columns[2].HeaderText = "Soyad";
                metroGridUyeBilgileri.Columns[3].HeaderText = "Doğum";
                metroGridUyeBilgileri.Columns[4].HeaderText = "Cinsiyet";
                metroGridUyeBilgileri.Columns[5].HeaderText = "İş-Tel";
                metroGridUyeBilgileri.Columns[6].HeaderText = "Cep-Tel";
                metroGridUyeBilgileri.Columns[7].HeaderText = "Email";
                metroGridUyeBilgileri.Columns[8].HeaderText = "Üyelik";
                metroGridUyeBilgileri.Columns[9].HeaderText = "Ekleyen";
                metroGridUyeBilgileri.Columns[10].HeaderText = "Tarih";
                metroGridUyeBilgileri.Columns[11].HeaderText = "Bitiş";
                //Status (Hoca, vb...)
                metroGridUyeBilgileri.Columns[12].Visible = false;


                int a = metroGridUyeBilgileri.Columns.Count;

                for (int i = 0; i < a; i++)
                {
                    metroGridUyeBilgileri.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
                metroGridUyeBilgileri.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridUyeBilgileri.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridUyeBilgileri.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;



                ////ROW HEIGHT AYARLAMA
                //foreach (DataGridViewRow row in metroGridUyeBilgileri.Rows)
                //{
                //    row.Height = 28;
                //}


            }
            catch (Exception)
            {


            }





        }

        private void UyeBilgileriSearch(string FilterName)
        {
            try
            {
                string sqlCommandStatement = "";
                //Filters
                switch (FilterName)
                {
                    case "*Filtre Yok": FilterName = "All"; break;
                    case "İsim": FilterName = "Name"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE Name LIKE @Value"; break;
                    case "Soyisim": FilterName = "Surname"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE Surname LIKE @Value"; break;
                    case "Doğum Tarihi": FilterName = "DateOfBirth"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE DateOfBirth LIKE @Value"; break;                 //DATE
                    case "Cinsiyet": FilterName = "Gender"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE Gender LIKE @Value"; break;
                    case "TC No": FilterName = "TCNo"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE TCNo LIKE @Value"; break;
                    case "İş Telefonu": FilterName = "WorkPhoneNumber"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE WorkPhoneNumber LIKE @Value"; break;
                    case "Cep Telefonu": FilterName = "PersonalPhoneNumber"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE PersonalPhoneNumber LIKE @Value"; break;
                    case "Email": FilterName = "Email"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE Email LIKE @Value"; break;
                    case "Üyelik Tipi": FilterName = "TypeOfMembership"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE TypeOfMembership LIKE @Value"; break;
                    case "Üye Yapan": FilterName = "AddedBy"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE AddedBy LIKE @Value"; break;
                    case "Üye Kayıt Tarihi": FilterName = "AddedDate"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE AddedDate LIKE @Value"; break;               //DATE
                    case "Üyelik Bitiş Tarihi": FilterName = "MembershipFinishDate"; sqlCommandStatement = "SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE MembershipFinishDate LIKE @Value"; break; //DATE

                    default: Application.Exit(); break;
                }

                if (FilterName == "All")
                {
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Name,Surname,DateOfBirth,Gender,WorkPhoneNumber,PersonalPhoneNumber,Email,TypeOfMembership,AddedBy,AddedDate,MembershipFinishDate FROM Members WHERE Name LIKE @Search OR Surname LIKE @Search OR DateOfBirth LIKE @Search OR Gender LIKE @Search OR WorkPhoneNumber LIKE @Search OR PersonalPhoneNumber LIKE @Search OR Email LIKE @Search OR TypeOfMembership LIKE @Search OR AddedBy LIKE @Search OR AddedDate LIKE @Search OR MembershipFinishDate LIKE @Search", conn);


                    cmd.Parameters.AddWithValue("@Search", "%" + SplitStringForSearch(metrotbxUyeBilgileriSearch.Text) + "%");

                    SqlDataAdapter dataAtapter = new SqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    dt.Clear();
                    dataAtapter.Fill(dt);

                    metroGridUyeBilgileri.DataSource = dt;
                    conn.Close();

                }
                else
                {
                    int dateControl = 0;
                    int day = 0;
                    int month = 0;
                    int year = 0;
                    string search = metrotbxUyeBilgileriSearch.Text;
                    //Eğer Tarih Giriliyorsa;
                    if ((FilterName == "DateOfBirth" && FilterName == "AddedDate" && FilterName == "MembershipFinishDate") || metrotbxUyeBilgileriSearch.Text.Length == 10)
                    {
                        day = Convert.ToInt32(search.Substring(0, 2));
                        month = Convert.ToInt32(search.Substring(3, 2));
                        year = Convert.ToInt32(search.Substring(6, 4));

                        dateControl = 1;
                    }



                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sqlCommandStatement, conn);

                    //cmd.Parameters.AddWithValue("@Column", FilterName);

                    if (dateControl == 1)
                    {
                        DateTime date = new DateTime(year, month, day);
                        SqlParameter dateparam = new SqlParameter("@Value", SqlDbType.Date);
                        dateparam.Value = date.Date;
                        cmd.Parameters.Add(dateparam);


                        //SqlParameter sinceDateTimeParam = new SqlParameter("@sinceDateTime", SqlDbType.DateTime);
                        //sinceDateTimeParam.Value = since;

                        //SqlCommand command = new SqlCommand(sql);
                        //command.Parameters.AddWithValue("@userid", userId);
                        //command.Parameters.Add(sinceDateTimeParam);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Value", "%" + SplitStringForSearch(metrotbxUyeBilgileriSearch.Text) + "%");
                    }


                    SqlDataAdapter dataAtapter = new SqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    dt.Clear();
                    dataAtapter.Fill(dt);

                    metroGridUyeBilgileri.DataSource = dt;
                    conn.Close();

                }
                //MetroGrid.Columns[1].HeaderText = "Kort İsmi";
                metroGridUyeBilgileri.Columns[0].HeaderText = "Ad";
                metroGridUyeBilgileri.Columns[1].HeaderText = "Soyad";
                metroGridUyeBilgileri.Columns[2].HeaderText = "Doğum";
                metroGridUyeBilgileri.Columns[3].HeaderText = "Cinsiyet";
                metroGridUyeBilgileri.Columns[4].HeaderText = "İş-Tel";
                metroGridUyeBilgileri.Columns[5].HeaderText = "Cep-Tel";
                metroGridUyeBilgileri.Columns[6].HeaderText = "Email";
                metroGridUyeBilgileri.Columns[7].HeaderText = "Üyelik";
                metroGridUyeBilgileri.Columns[8].HeaderText = "Ekleyen";
                metroGridUyeBilgileri.Columns[9].HeaderText = "Tarih";
                metroGridUyeBilgileri.Columns[10].HeaderText = "Bitiş";


                int a = metroGridUyeBilgileri.Columns.Count;

                for (int i = 0; i < a; i++)
                {
                    metroGridUyeBilgileri.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
                metroGridUyeBilgileri.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridUyeBilgileri.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridUyeBilgileri.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                metroGridUyeBilgileri.DefaultCellStyle.Font = new Font("Segoe UI", 8);

                //ROW HEIGHT AYARLAMA
                //foreach (DataGridViewRow row in metroGridUyeBilgileri.Rows)
                //{
                //    row.Height = 28;
                //}

            }
            catch (Exception)
            {


            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            UyeBilgileriSearch("All");
        }

        private void metrotbxUyeBilgileriSearch_TextChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    if (!WorkerMemberSearch.IsBusy)
            //    {
            //        WorkerMemberSearch.RunWorkerAsync();
            //    }

            //}
            //catch (Exception)
            //{

            //}
        }

        private void MetrobtnAra_Click(object sender, EventArgs e)
        {

            if (metroComboBoxSearchFilter.SelectedItem == null)
            {
                UyeBilgileriSearch("*Filtre Yok");
            }
            else
            {
                UyeBilgileriSearch(metroComboBoxSearchFilter.SelectedItem.ToString());
            }
        }

        private void metroTileBilgiEdit_Click(object sender, EventArgs e)
        {
            if (!groupBoxBilgileriDuzenle.Visible && !groupBoxBilgileriDuzenle.Enabled)
            {
                groupBoxBilgileriDuzenle.Visible = true;
                groupBoxBilgileriDuzenle.Enabled = true;
            }
            else
            {
                groupBoxBilgileriDuzenle.Visible = false;
                groupBoxBilgileriDuzenle.Enabled = false;
            }
        }

        private void UyeBilgileriShowOnControls()
        {
            try
            {
                if (metroGridUyeBilgileri.SelectedRows.Count == 1 || metroGridUyeBilgileri.SelectedRows != null)
                {
                    DataGridViewRow row = metroGridUyeBilgileri.SelectedRows[0];
                    Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                    metrotbxBilgiName.Text = row.Cells["Name"].Value.ToString();
                    metrotbxBilgiSurname.Text = row.Cells["Surname"].Value.ToString();
                    dateTimePickerBilgiDateOfBirth.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value).Date;
                    //Gender
                    if (row.Cells["Gender"].Value.ToString() == "Bay")
                    {
                        metroComboBoxBilgiGender.SelectedIndex = 0;
                    }
                    else
                    {
                        metroComboBoxBilgiGender.SelectedIndex = 1;
                    }
                    //metrotbxBilgiTCno.Text = row.Cells["TCNo"].Value.ToString();
                    maskedtbxBilgiWorkPhone.Text = row.Cells["WorkPhoneNumber"].Value.ToString();
                    maskedtbxBilgiPersonalPhone.Text = row.Cells["PersonalPhoneNumber"].Value.ToString();
                    metrotbxBilgiEmail.Text = row.Cells["Email"].Value.ToString();
                    metroComboBoxBilgiTypeOfMembership.SelectedItem = row.Cells["TypeOfMembership"].Value.ToString();
                    dateTimePickerBilgiAddedDate.Value = Convert.ToDateTime(row.Cells["AddedDate"].Value).Date;
                    dateTimePickerBilgiMembershipFinishDate.Value = Convert.ToDateTime(row.Cells["MembershipFinishDate"].Value).Date;
                }
            }
            catch (Exception)
            {


            }



        }

        private void metroGridUyeBilgileri_SelectionChanged(object sender, EventArgs e)
        {
            if (groupBoxBilgileriDuzenle.Visible || groupBoxBilgileriDuzenle.Enabled)
            {
                UyeBilgileriShowOnControls();
            }
        }

        private void UyeBilgileriKaydet()
        {
            try
            {
                int check = 0;

                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("UPDATE Members SET Name=@Name,Surname=@Surname,DateOfBirth=@DateOfBirth,Gender=@Gender,WorkPhoneNumber=@WorkPhoneNumber,PersonalPhoneNumber=@PersonalPhoneNumber,Email=@Email,TypeOfMembership=@TypeOfMembership,AddedBy=@AddedBy,AddedDate=@AddedDate,MembershipFinishDate=@MembershipFinishDate,UpdatedDate=@UpdatedDate WHERE Id=@Selected_AccountID_", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@Name", metrotbxBilgiName.Text);
                cmd.Parameters.AddWithValue("@Surname", metrotbxBilgiSurname.Text);
                cmd.Parameters.AddWithValue("@DateOfBirth", dateTimePickerBilgiDateOfBirth.Value.Date);
                cmd.Parameters.AddWithValue("@Gender", metroComboBoxBilgiGender.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@WorkPhoneNumber", maskedtbxBilgiWorkPhone.Text);
                cmd.Parameters.AddWithValue("@PersonalPhoneNumber", maskedtbxBilgiPersonalPhone.Text);
                cmd.Parameters.AddWithValue("@Email", metrotbxBilgiEmail.Text);
                cmd.Parameters.AddWithValue("@TypeOfMembership", metroComboBoxBilgiTypeOfMembership.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
                cmd.Parameters.AddWithValue("@AddedDate", dateTimePickerBilgiAddedDate.Value.Date);
                cmd.Parameters.AddWithValue("@MembershipFinishDate", dateTimePickerBilgiMembershipFinishDate.Value.Date);
                cmd.Parameters.AddWithValue("@UpdatedDate", DateLocal.Date);
                cmd.Parameters.AddWithValue("@Selected_AccountID_", Selected_AccountID_);
                check = cmd.ExecuteNonQuery();
                conn.Close();

                if (check > 0)
                {
                    MetroMessageBox.Show(this, "\nKayıt Başarıyla Güncellendi.", "Başarı");
                    GetMemberData();
                }
            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }

        }

        private void metroTileBilgiSave_Click(object sender, EventArgs e)
        {
            UyeBilgileriKaydet();
        }

        private void metroTileBilgiCancel_Click(object sender, EventArgs e)
        {
            foreach (Control item in groupBoxBilgileriDuzenle.Controls)
            {
                if (item is Label)
                {

                }
                else if (item is MetroLabel)
                {

                }
                else if (item is MetroTile)
                {

                }
                else if (item is Button)
                {

                }
                else
                {
                    item.ResetText();
                    groupBoxBilgileriDuzenle.Visible = false;
                    groupBoxBilgileriDuzenle.Enabled = false;
                }
            }
        }

        private void KortUyeListe()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT Id,Name,Surname,Gender,MembershipFinishDate,Paid,PaidDate,Status FROM Members ORDER BY Name,Surname", conn);
            conn.Open();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            metroGridKortOyuncu.DataSource = dt;

            metroGridKortOyuncu.DefaultCellStyle.Font = new Font("Segoe UI", 9);

            metroGridKortOyuncu.Columns[0].HeaderText = "ID";
            metroGridKortOyuncu.Columns[1].HeaderText = "Ad";
            metroGridKortOyuncu.Columns[2].HeaderText = "Soyad";
            metroGridKortOyuncu.Columns[3].HeaderText = "Cinsiyet";
            //metroGridOyuncu.Columns[4].HeaderText = "Bitiş";
            metroGridKortOyuncu.Columns[4].Visible = false;
            metroGridKortOyuncu.Columns[5].HeaderText = "Aidat";
            metroGridKortOyuncu.Columns[6].HeaderText = "Ödeme Tarihi";
            //Hoca
            metroGridKortOyuncu.Columns[7].Visible = false;


            //MetroGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //MetroGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //MetroGrid.Columns[3].Width = 100;
            metroGridKortOyuncu.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            conn.Close();
            try
            {
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.Font = new Font(metroGridKortOyuncu.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                style.ForeColor = Color.IndianRed;
                foreach (DataGridViewRow row in metroGridKortOyuncu.Rows)
                {
                    if (row.Cells["Status"].Value.ToString() == "Hoca")
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private void metroLinkOyuncu1_Click(object sender, EventArgs e)
        {
            Player = 1;
            groupBoxOyuncu.Text = "Oyuncu 1'i Seç";
            KortUyeListe();
        }

        private void metroLinkOyuncu2_Click(object sender, EventArgs e)
        {
            Player = 2;
            groupBoxOyuncu.Text = "Oyuncu 2'yi Seç";
            KortUyeListe();
        }

        private void metroTileKortOyuncuSec_Click(object sender, EventArgs e)
        {
            if (Player == 1)
            {
                KortOyuncuSec(metroLinkOyuncu1);
            }
            else
            {
                if (metrotbxMisafirOyuncu2.Visible)
                {

                }
                else
                {
                    KortOyuncuSec(metroLinkOyuncu2);
                }

            }

        }

        private void KortKayit()
        {
            try
            {
                int check = 0;

                //INSERT INTO personel(ID, isim, bolum) VALUES(71, 'Serap Demirci', 'Reklam')
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("INSERT INTO KortTakip(KortName,Time,Date,Player1,Player2,Player1_Id,Player2_Id,Light,AddedBy) VALUES (@KortName,@Time,@Date,@Player1,@Player2,@Player1_Id,@Player2_Id,@Light,@AddedBy)", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@KortName", metroComboBoxKortKayıtKort.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@Time", metroComboBoxKortKayıtSaat.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@Date", dateTimePickerKortKayıtTarih.Value.Date);
                cmd.Parameters.AddWithValue("@Player1", metroLinkOyuncu1.Text);
                cmd.Parameters.AddWithValue("@Player1_Id", Player1ID);
                if (metrotbxMisafirOyuncu2.Visible)
                {
                    cmd.Parameters.AddWithValue("@Player2", metrotbxMisafirOyuncu2.Text + " (Misafir)");
                    cmd.Parameters.AddWithValue("@Player2_Id", "Misafir");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Player2", metroLinkOyuncu2.Text);
                    cmd.Parameters.AddWithValue("@Player2_Id", Player2ID);
                }

                if (metroComboBoxKortLight.SelectedItem.ToString() == "Var")
                {
                    cmd.Parameters.AddWithValue("@Light", "Var");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Light", "Yok");
                }

                cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);

                check = cmd.ExecuteNonQuery();

                if (check > 0)
                {
                    MetroMessageBox.Show(this, "\nKort Kaydı Başarıyla Alındı", "Başarı");
                }
                else
                {
                    MetroMessageBox.Show(this, "\nKayıt Yapılırken Hata Oluştu, Bilgileri tekrar kontrol edin", "Hata");
                }
                conn.Close();
            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }

        }

        private void KortOyuncuSec(MetroLink VmetroLink)
        {
            string PlayerName;
            int PlayerID;

            //DataGridViewddan Üye Seçimi (Mause İle)
            if (metroGridKortOyuncu.SelectedRows.Count == 1 || metroGridKortOyuncu.SelectedRows != null)
            {
                DataGridViewRow row = metroGridKortOyuncu.SelectedRows[0];
                //Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                PlayerName = row.Cells["Name"].Value.ToString() + " " + row.Cells["Surname"].Value.ToString();
                PlayerID = Convert.ToInt32(row.Cells["Id"].Value);
                if (Player == 1)
                {
                    Player1Name = PlayerName;
                    Player1ID = PlayerID.ToString();
                }
                else
                {
                    Player2Name = PlayerName;
                    Player2ID = PlayerID.ToString();
                }

                VmetroLink.Text = PlayerName + " (" + PlayerID + ")";
            }



        }

        private void metroCheckBoxKortMisafirOyuncu2_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBoxKortMisafirOyuncu2.Checked)
            {
                metrotbxMisafirOyuncu2.Location = new Point(87, 320);
                metrotbxMisafirOyuncu2.Visible = true;
                metroLinkOyuncu2.Visible = false;
                Player = 1;
                groupBoxOyuncu.Text = "Oyuncu 1'i Seç";
                metroCheckBoxKortMisafirOyuncu2.Text = "Var";
            }
            else
            {
                metrotbxMisafirOyuncu2.Visible = false;
                metroLinkOyuncu2.Visible = true;
                metroCheckBoxKortMisafirOyuncu2.Text = "Yok";
            }
        }

        private void metroTileKortKayıt_Click(object sender, EventArgs e)
        {
            KortKayit();
            KortKayıtGridView();
        }

        private void metroTileKortKayıtGüncelle_Click(object sender, EventArgs e)
        {
            KortKayıtGridView();
        }

        private void metrobtnKayıtSearch_Click(object sender, EventArgs e)
        {
            if (metrotbxKortSearch.Text == "")
            {
                KortUyeListe();
            }
            else
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT Id,Name,Surname,Gender,MembershipFinishDate,Paid,PaidDate FROM Members WHERE Id LIKE @Id OR Name LIKE @Name OR Surname LIKE @Surname", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@Id", "%" + metrotbxKortSearch.Text + "%");
                cmd.Parameters.AddWithValue("@Name", "%" + metrotbxKortSearch.Text + "%");
                cmd.Parameters.AddWithValue("@Surname", "%" + metrotbxKortSearch.Text + "%");
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                metroGridKortOyuncu.DataSource = dt;

                metroGridKortOyuncu.DefaultCellStyle.Font = new Font("Segoe UI", 9);

                metroGridKortOyuncu.Columns[0].HeaderText = "ID";
                metroGridKortOyuncu.Columns[1].HeaderText = "Ad";
                metroGridKortOyuncu.Columns[2].HeaderText = "Soyad";
                metroGridKortOyuncu.Columns[3].HeaderText = "Cinsiyet";
                //metroGridOyuncu.Columns[4].HeaderText = "Bitiş";
                metroGridKortOyuncu.Columns[4].Visible = false;
                metroGridKortOyuncu.Columns[5].HeaderText = "Aidat";
                metroGridKortOyuncu.Columns[6].HeaderText = "Ödeme Tarihi";

                //MetroGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //MetroGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //MetroGrid.Columns[3].Width = 100;
                metroGridKortOyuncu.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                conn.Close();
            }
        }

        private void metroTile9_Click(object sender, EventArgs e)
        {
            KortUyeListe();
        }

        private void metroTileKortKayıtCancel_Click(object sender, EventArgs e)
        {

        }


        private void KortSearch()
        {
            try
            {
                string command = KortSearchFilterCommandString();

                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(command, conn);
                conn.Open();
                try
                {
                    cmd.Parameters.AddWithValue("@Date", dateTimePickerKortFilterDate.Value.Date);
                }
                catch (Exception)
                {


                }

                try
                {
                    cmd.Parameters.AddWithValue("@KortName", metroComboBoxKortFilterKort.SelectedItem.ToString());
                }
                catch (Exception)
                {


                }

                try
                {
                    cmd.Parameters.AddWithValue("@Time", metroComboBoxKortFilterTime.SelectedItem.ToString());
                }
                catch (Exception)
                {


                }

                try
                {
                    cmd.Parameters.AddWithValue("@Player1", "%" + metrotbxKortFilterPlayer1.Text + "%");
                }
                catch (Exception)
                {


                }

                try
                {
                    cmd.Parameters.AddWithValue("@Player2", "%" + metrotbxKortFilterPlayer2.Text + "%");
                }
                catch (Exception)
                {


                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                metroGridKortSearch.DataSource = dt;

                conn.Close();
                try
                {
                    foreach (DataGridViewRow row in metroGridKortSearch.Rows)
                    {
                        for (int i = 0; i < HocaList.Count; i++)
                        {
                            if (row.Cells[6].Value.ToString() == HocaList[i].ToString() || row.Cells[7].Value.ToString() == HocaList[i].ToString())
                            {
                                try
                                {
                                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                                    style.Font = new Font(metroGridKortSearch.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                                    style.ForeColor = Color.IndianRed;

                                    foreach (DataGridViewCell cell in row.Cells)
                                        cell.Style.ApplyStyle(style);


                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {


                }

                FixUyeRenk(metroGridKortSearch);

                metroGridKortSearch.Columns[0].HeaderText = "ID";
                metroGridKortSearch.Columns[1].HeaderText = "Kort İsmi";
                metroGridKortSearch.Columns[2].HeaderText = "Saat";
                metroGridKortSearch.Columns[3].HeaderText = "Tarih";
                metroGridKortSearch.Columns[4].HeaderText = "Oyuncu 1 (ID)";
                metroGridKortSearch.Columns[5].HeaderText = "Oyuncu 2 (ID)";
                metroGridKortSearch.Columns[6].Visible = false;
                metroGridKortSearch.Columns[7].Visible = false;
                metroGridKortSearch.Columns[8].HeaderText = "Işık";
                metroGridKortSearch.Columns[9].HeaderText = "Ekleyen Kişi";
                metroGridKortSearch.Columns[10].HeaderText = "Not";
                metroGridKortSearch.Columns[11].Visible = false;

                metroGridKortSearch.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridKortSearch.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridKortSearch.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridKortSearch.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;




            }
            catch (Exception)
            {


            }



        }

        private string KortSearchFilterCommandString()
        {
            string command = "SELECT * FROM KortTakip WHERE";
            int AndCheck = 0;
            //KortFilterDate
            //KortFilterKortName
            //KortFilterTime
            //KortFilterPlayer1
            //KortFilterPlayer2

            if (KortFilterDate.Checked)
            {
                command += " Date=@Date";
                AndCheck = 1;
            }
            if (KortFilterKortName.Checked)
            {
                if (AndCheck == 1)
                {
                    command += " AND KortName=@KortName";
                }
                else
                {
                    command += " KortName=@KortName";
                }
                AndCheck = 1;
            }
            if (KortFilterTime.Checked)
            {
                if (AndCheck == 1)
                {
                    command += " AND Time=@Time";
                }
                else
                {
                    command += " Time=@Time";
                }
                AndCheck = 1;
            }
            if (KortFilterPlayer1.Checked)
            {
                if (AndCheck == 1)
                {
                    command += " AND Player1 LIKE @Player1";
                }
                else
                {
                    command += " Player1 LIKE @Player1";
                }
                AndCheck = 1;
            }
            if (KortFilterPlayer2.Checked)
            {
                if (AndCheck == 1)
                {
                    command += " AND Player2 LIKE @Player2";
                }
                else
                {
                    command += " Player2 LIKE @Player2";
                }
                AndCheck = 1;
            }

            if (AndCheck == 0)
            {
                return "SELECT * FROM KortTakip ORDER BY Date,KortName,Time ASC ";
            }
            else
            {

                return command + " ORDER BY Date,KortName,Time ASC";
            }



        }

        private void metroTileKortSearch_Click(object sender, EventArgs e)
        {
            KortSearch();
        }

        private void metroTileKortKayıtSil_Click(object sender, EventArgs e)
        {
            try
            {
                int Kort_ID = 0;
                int check = 0;

                if (metroGridKortSearch.SelectedRows.Count == 1 || metroGridKortSearch.SelectedRows != null)
                {
                    DataGridViewRow row = metroGridKortSearch.SelectedRows[0];
                    //Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);

                    Kort_ID = Convert.ToInt32(row.Cells["Id"].Value);
                    SqlConnection conn = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand("DELETE from KortTakip where Id=@Id", conn);
                    conn.Open();
                    cmd.Parameters.AddWithValue("@Id", Kort_ID);
                    check = cmd.ExecuteNonQuery();
                    conn.Close();
                    if (check > 0)
                    {
                        MetroMessageBox.Show(this, "\nKayıt Başarıyla Silindi", "Başarı");
                        KortSearch();
                    }
                    else
                    {
                        MetroMessageBox.Show(this, "\nKayıt Silerken Hata Oluştu. Sunucu Meşgul olabilir, Biraz Bekledikten Sonra Tekrar Deneyiniz.", "Hata");
                    }
                }
            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }


        }

        private void WorkerDateTime_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (true)
                {
                    DateLocal = GetNistTime();
                    Thread.Sleep(600000);
                }

            }
            catch (Exception)
            {


            }

        }

        private void HocaListesi()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("Select ID,Name,Status From Members", conn);
                MetroGrid dgw = new MetroGrid();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                dgw.DataSource = dt;
                this.Controls.Add(dgw);
                dgw.Visible = false;
                dgw.Enabled = false;


                try
                {
                    foreach (DataGridViewRow row in dgw.Rows)
                    {
                        if (row.Cells[2].Value.ToString() == "Hoca")
                        {
                            HocaList.Add(Convert.ToInt32(row.Cells[0].Value));
                        }

                    }
                }
                catch (Exception)
                {


                }
            }
            catch (Exception)
            {


            }





        }


        /// <summary>
        /// Hocaları Bold ve Kırmızı Yapar
        /// </summary>
        /// <param name="gridview">metrogridview</param>
        /// <param name="cell_id">cell parametresi=row.Cells[1] veya row.Cells["Status"]</param>
        /// <param name="font_size">font büyüklüğü (default 8)</param>
        private void MakeHocaBold(MetroGrid gridview, string cell_id, int font_size)
        {
            try
            {
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.Font = new Font(gridview.Font = new Font("Segoe UI", font_size), FontStyle.Bold);
                style.ForeColor = Color.IndianRed;
                foreach (DataGridViewRow row in gridview.Rows)
                {
                    if (row.Cells[cell_id].Value.ToString() == "Hoca")
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void metroTile7_Click_1(object sender, EventArgs e)
        {

        }

        private void releaseObject(object obj)

        {

            try

            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);

                obj = null;

            }

            catch (Exception ex)

            {

                obj = null;

                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());

            }

            finally

            {

                GC.Collect();

            }

        }

        private void ExportToExcel(MetroGrid DataGrid)
        {
            try
            {


                Excel.Application xlApp;

                Excel.Workbook xlWorkBook;

                Excel.Worksheet xlWorkSheet;

                object misValue = System.Reflection.Missing.Value;



                xlApp = new Excel.Application();

                xlWorkBook = xlApp.Workbooks.Add(misValue);

                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                //Headers
                int control = 0;  //Visible olmayan columnlar kaymasın diye

                for (int z = 0; z <= DataGrid.ColumnCount - 1; z++)
                {
                    if (DataGrid.Columns[z].Visible == true)
                    {
                        DataGridViewCell cell = DataGrid[z, 1];
                        xlWorkSheet.Cells[1, z - control + 1] = DataGrid.Columns[z].HeaderText;
                        xlWorkSheet.Cells[1, z - control + 1].Font.Color = Color.IndianRed;
                    }

                    else
                    {
                        control++;
                    }

                }
                //Excel.Range headerRange = xlWorkSheet.get_Range("A1", "A1");
                //headerRange.HorizontalAlignment =Excel.XlHAlign.xlHAlignCenter;
                //headerRange.Value = "Header text 1";



                //DATA
                int i = 0;

                int j = 0;

                control = 0;

                for (i = 1; i <= DataGrid.RowCount - 1; i++)

                {

                    for (j = 0; j <= DataGrid.ColumnCount - 1; j++)

                    {
                        try
                        {
                            if (DataGrid.Columns[j].Visible == true)
                            {
                                DataGridViewCell cell = DataGrid[j, i];

                                xlWorkSheet.Cells[i + 1, j - control + 1] = cell.Value.ToString();
                            }
                            else
                            {
                                control++;
                            }


                        }
                        catch (Exception)
                        {
                        }


                    }
                    control = 0;
                }
                //workSheet.Cells[10, 1].EntireRow.Font.Bold = true;
                //worksheet.get_Range("A7", "A7").Cells.Font.Size = 20;


                xlWorkSheet.Cells[1, 20].EntireRow.Font.Bold = true;
                xlWorkSheet.get_Range("A1", "Z1").Cells.Font.Size = 12;
                xlWorkSheet.Columns.AutoFit();


                //xlWorkBook.SaveAs("csharp.net-informations.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);


                xlWorkBook.Close(true, misValue, misValue);

                xlApp.Quit();

                releaseObject(xlWorkSheet);

                releaseObject(xlWorkBook);

                releaseObject(xlApp);

            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);

            }

        }

        private void msg(string content, string header, int size)
        {
            MetroMessageBox.Show(this, content, header, size);
        }

        private void excelDosyasıOlarakKaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Control ControlName = metroContextMenuGridExport.SourceControl;
                ExportToExcel((MetroGrid)ControlName);
            }
            catch (Exception)
            {


            }

        }

        private void MuhasebeAidatUyeler()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT * FROM Members ORDER BY PaidFinishDate,Name,Surname ASC", conn);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                metroGridMuhasebeAidatUyeler.DataSource = dt;
                conn.Close();
                //Id,Name,Surname,WorkPhoneNumber,PersonalPhoneNumber,Paid,PaidDate,PaidFinishDate,Status
                metroGridMuhasebeAidatUyeler.DefaultCellStyle.Font = new Font("Segoe UI", 8);
                metroGridMuhasebeAidatUyeler.Columns[0].HeaderText = "ID";
                metroGridMuhasebeAidatUyeler.Columns[1].HeaderText = "İsim";
                metroGridMuhasebeAidatUyeler.Columns[2].HeaderText = "Soyisim";
                metroGridMuhasebeAidatUyeler.Columns[3].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[4].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[5].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[6].HeaderText = "İş-Tel";
                metroGridMuhasebeAidatUyeler.Columns[7].HeaderText = "Cep-Tel";
                metroGridMuhasebeAidatUyeler.Columns[8].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[9].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[10].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[11].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[12].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[13].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[14].HeaderText = "Durum";
                metroGridMuhasebeAidatUyeler.Columns[15].HeaderText = "Son Alınan Ödeme";
                metroGridMuhasebeAidatUyeler.Columns[16].HeaderText = "Sonraki Ödeme Tarihi"; ;
                metroGridMuhasebeAidatUyeler.Columns[17].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[18].Visible = false;

                metroGridMuhasebeAidatUyeler.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridMuhasebeAidatUyeler.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


                //HOCALARI KIRMIZI YAP
                try
                {
                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                    style.Font = new Font(metroGridMuhasebeAidatUyeler.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                    style.ForeColor = Color.IndianRed;
                    foreach (DataGridViewRow row in metroGridMuhasebeAidatUyeler.Rows)
                    {
                        if (row.Cells["Status"].Value.ToString() == "Hoca")
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                                cell.Style.ApplyStyle(style);
                        }
                    }
                }
                catch (Exception)
                {
                }

            }
            catch (Exception)
            {


            }




        }

        private void MuhasebeAidatUyelerShowOnControls(string Control)
        {
            try
            {
                if (Control == "Düzenle")
                {
                    if (metroGridMuhasebeAidatUyeler.SelectedRows.Count == 1 || metroGridMuhasebeAidatUyeler.SelectedRows != null)
                    {
                        DataGridViewRow row = metroGridMuhasebeAidatUyeler.SelectedRows[0];
                        Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                        metrotbxMuhasebeAidatİsim.Text = row.Cells["Name"].Value.ToString();
                        metrotbxMuhasebeAidatSoyisim.Text = row.Cells["Surname"].Value.ToString();
                        dateTimePickerMuhasebeAidatDoğumTarihi.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value).Date;
                        //Ödeme Durumu
                        if (row.Cells["Paid"].Value.ToString() == "Ödendi")
                        {
                            metroComboBoxMuhasebeAidatÖdemeDurumu.SelectedIndex = 0;
                        }
                        else
                        {
                            metroComboBoxMuhasebeAidatÖdemeDurumu.SelectedIndex = 1;
                        }
                        //metrotbxBilgiTCno.Text = row.Cells["TCNo"].Value.ToString();
                        maskedtbxMuhasebeAidatWorkPhone.Text = row.Cells["WorkPhoneNumber"].Value.ToString();
                        maskedtbxMuhasebeAidatPersonalPhone.Text = row.Cells["PersonalPhoneNumber"].Value.ToString();
                        metrotbxMuhasebeAidatEmail.Text = row.Cells["Email"].Value.ToString();
                        metroComboBoxMuhasebeÜyelikTipi.SelectedItem = row.Cells["TypeOfMembership"].Value.ToString();
                        dateTimePickerMuhasebeAidatPaidDate.Value = Convert.ToDateTime(row.Cells["PaidDate"].Value).Date;
                        dateTimePickerMuhasebeAidatPaidFinishDate.Value = Convert.ToDateTime(row.Cells["PaidFinishDate"].Value).Date;
                    }

                }

                else if (Control == "Ödeme")
                {
                    if (metroGridMuhasebeAidatUyeler.SelectedRows.Count == 1 || metroGridMuhasebeAidatUyeler.SelectedRows != null)
                    {
                        DataGridViewRow row = metroGridMuhasebeAidatUyeler.SelectedRows[0];
                        Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                        metrotbxMuhasebeÖdemeName.Text = row.Cells["Name"].Value.ToString();
                        metrotbxMuhasebeÖdemeSurname.Text = row.Cells["Surname"].Value.ToString();
                        //dateTimePickerMuhasebeAidatDoğumTarihi.Value = Convert.ToDateTime(row.Cells["DateOfBirth"].Value).Date;
                        //Ödeme Durumu
                        //if (row.Cells["Paid"].Value.ToString() == "Ödendi")
                        //{
                        //    metroComboBoxMuhasebeAidatÖdemeDurumu.SelectedIndex = 0;
                        //}
                        //else
                        //{
                        //    metroComboBoxMuhasebeAidatÖdemeDurumu.SelectedIndex = 1;
                        //}
                        //metrotbxBilgiTCno.Text = row.Cells["TCNo"].Value.ToString();
                        metrotbxMuhasebeÖdemeMemberID.Text = row.Cells["Id"].Value.ToString();
                        //maskedtbxMuhasebeAidatWorkPhone.Text = row.Cells["WorkPhoneNumber"].Value.ToString();
                        //maskedtbxMuhasebeAidatPersonalPhone.Text = row.Cells["PersonalPhoneNumber"].Value.ToString();
                        //metrotbxMuhasebeAidatEmail.Text = row.Cells["Email"].Value.ToString();
                        //metroComboBoxMuhasebeÜyelikTipi.SelectedItem = row.Cells["TypeOfMembership"].Value.ToString();
                        //dateTimePickerMuhasebeAidatPaidDate.Value = Convert.ToDateTime(row.Cells["PaidDate"].Value).Date;
                        //dateTimePickerMuhasebeAidatPaidFinishDate.Value = Convert.ToDateTime(row.Cells["PaidFinishDate"].Value).Date;
                    }
                }


            }
            catch (Exception)
            {


            }



        }

        private void MuhasebeAidatUyelerSearch(string FilterName)
        {


            try
            {
                string sqlCommandStatement = "";
                //Filters
                switch (FilterName)
                {
                    case "*Filtre Yok": FilterName = "All"; break;
                    case "İsim": FilterName = "Name"; sqlCommandStatement = "SELECT * FROM Members WHERE Name LIKE @Value"; break;
                    case "Soyisim": FilterName = "Surname"; sqlCommandStatement = "SELECT * FROM Members WHERE Surname LIKE @Value"; break;
                    case "Doğum Tarihi": FilterName = "DateOfBirth"; sqlCommandStatement = "SELECT * FROM Members WHERE DateOfBirth LIKE @Value"; break;                 //DATE
                    //case "Cinsiyet": FilterName = "Gender"; break;
                    //case "TC No": FilterName = "TCNo"; break;
                    case "İş Telefonu": FilterName = "WorkPhoneNumber"; sqlCommandStatement = "SELECT * FROM Members WHERE WorkPhoneNumber LIKE @Value"; break;
                    case "Cep Telefonu": FilterName = "PersonalPhoneNumber"; sqlCommandStatement = "SELECT * FROM Members WHERE PersonalPhoneNumber LIKE @Value"; break;
                    case "Email": FilterName = "Email"; sqlCommandStatement = "SELECT * FROM Members WHERE Email LIKE @Value"; break;
                    case "Üyelik Tipi": FilterName = "TypeOfMembership"; sqlCommandStatement = "SELECT * FROM Members WHERE TypeOfMembership LIKE @Value"; break;
                    //case "Üye Yapan": FilterName = "AddedBy"; break;
                    /*case "Üye Kayıt Tarihi": FilterName = "AddedDate"; break;*/               //DATE
                    /*case "Üyelik Bitiş Tarihi": FilterName = "MembershipFinishDate"; break;*/ //DATE
                    case "Ödeme Durumu": FilterName = "Paid"; sqlCommandStatement = "SELECT * FROM Members WHERE Paid LIKE @Value"; break;
                    case "Son Alınan Ödeme": FilterName = "PaidDate"; sqlCommandStatement = "SELECT * FROM Members WHERE PaidDate LIKE @Value"; break;
                    case "Son Ödeme Tarihi": FilterName = "PaidFinishDate"; sqlCommandStatement = "SELECT * FROM Members WHERE PaidFinishDate LIKE @Value"; break;

                    default: break;
                }

                if (FilterName == "All")
                {
                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Members WHERE Name LIKE @Search OR Surname LIKE @Search OR DateOfBirth LIKE @Search OR WorkPhoneNumber LIKE @Search OR PersonalPhoneNumber LIKE @Search OR Email LIKE @Search OR TypeOfMembership LIKE @Search OR Paid LIKE @Search OR PaidDate LIKE @Search OR PaidFinishDate LIKE @Search", conn);


                    cmd.Parameters.AddWithValue("@Search", "%" + SplitStringForSearch(metrotbxMuhasebeAidatSearch.Text) + "%");

                    SqlDataAdapter dataAtapter = new SqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    dt.Clear();
                    dataAtapter.Fill(dt);

                    metroGridMuhasebeAidatUyeler.DataSource = dt;
                    conn.Close();

                }
                else
                {
                    int dateControl = 0;
                    int day = 0;
                    int month = 0;
                    int year = 0;
                    string search = SplitStringForSearch(metrotbxMuhasebeAidatSearch.Text);
                    //Eğer Tarih Giriliyorsa;
                    if ((FilterName == "DateOfBirth" && FilterName == "PaidDate" && FilterName == "PaidFinishDate") || metrotbxMuhasebeAidatSearch.Text.Length == 10)
                    {
                        search = metrotbxMuhasebeAidatSearch.Text;
                        day = Convert.ToInt32(search.Substring(0, 2));
                        month = Convert.ToInt32(search.Substring(3, 2));
                        year = Convert.ToInt32(search.Substring(6, 4));

                        dateControl = 1;
                    }



                    SqlConnection conn = new SqlConnection(connectionString);
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sqlCommandStatement, conn);

                    //cmd.Parameters.AddWithValue("@Column", FilterName);

                    if (dateControl == 1)
                    {
                        DateTime date = new DateTime(year, month, day);
                        SqlParameter dateparam = new SqlParameter("@Value", SqlDbType.Date);
                        dateparam.Value = date.Date;
                        cmd.Parameters.Add(dateparam);


                        //SqlParameter sinceDateTimeParam = new SqlParameter("@sinceDateTime", SqlDbType.DateTime);
                        //sinceDateTimeParam.Value = since;

                        //SqlCommand command = new SqlCommand(sql);
                        //command.Parameters.AddWithValue("@userid", userId);
                        //command.Parameters.Add(sinceDateTimeParam);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Value", "%" + metrotbxMuhasebeAidatSearch.Text + "%");
                    }


                    SqlDataAdapter dataAtapter = new SqlDataAdapter(cmd);

                    DataTable dt = new DataTable();
                    dt.Clear();
                    dataAtapter.Fill(dt);

                    metroGridMuhasebeAidatUyeler.DataSource = dt;
                    conn.Close();

                }

                //HOcaları Kırmızı Yap
                try
                {
                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                    style.Font = new Font(metroGridMuhasebeAidatUyeler.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                    style.ForeColor = Color.IndianRed;
                    foreach (DataGridViewRow row in metroGridMuhasebeAidatUyeler.Rows)
                    {
                        if (row.Cells["Status"].Value.ToString() == "Hoca")
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                                cell.Style.ApplyStyle(style);
                        }
                    }
                }
                catch (Exception)
                {
                }



                metroGridMuhasebeAidatUyeler.DefaultCellStyle.Font = new Font("Segoe UI", 8);
                metroGridMuhasebeAidatUyeler.Columns[0].HeaderText = "ID";
                metroGridMuhasebeAidatUyeler.Columns[1].HeaderText = "İsim";
                metroGridMuhasebeAidatUyeler.Columns[2].HeaderText = "Soyisim";
                metroGridMuhasebeAidatUyeler.Columns[3].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[4].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[5].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[6].HeaderText = "İş-Tel";
                metroGridMuhasebeAidatUyeler.Columns[7].HeaderText = "Cep-Tel";
                metroGridMuhasebeAidatUyeler.Columns[8].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[9].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[10].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[11].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[12].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[13].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[14].HeaderText = "Durum";
                metroGridMuhasebeAidatUyeler.Columns[15].HeaderText = "Son Alınan Ödeme";
                metroGridMuhasebeAidatUyeler.Columns[16].HeaderText = "Sonraki Ödeme Tarihi"; ;
                metroGridMuhasebeAidatUyeler.Columns[17].Visible = false;
                metroGridMuhasebeAidatUyeler.Columns[18].Visible = false;

                metroGridMuhasebeAidatUyeler.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                metroGridMuhasebeAidatUyeler.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;



                //ROW HEIGHT AYARLAMA
                //foreach (DataGridViewRow row in metroGridUyeBilgileri.Rows)
                //{
                //    row.Height = 28;
                //}

            }
            catch (Exception)
            {


            }

        }

        private void MuhasebeAidatUyeBilgileriUpdate()
        {
            try
            {
                int check = 0;

                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("UPDATE Members SET UpdatedDate=@UpdatedDate,Paid=@Paid,PaidDate=@PaidDate,PaidFinishDate=@PaidFinishDate WHERE Id=@Selected_AccountID_", conn);
                conn.Open();
                //cmd.Parameters.AddWithValue("@Name", metrotbxBilgiName.Text);
                //cmd.Parameters.AddWithValue("@Surname", metrotbxBilgiSurname.Text);
                //cmd.Parameters.AddWithValue("@DateOfBirth", dateTimePickerBilgiDateOfBirth.Value.Date);
                //cmd.Parameters.AddWithValue("@Gender", metroComboBoxBilgiGender.SelectedItem.ToString());
                //cmd.Parameters.AddWithValue("@WorkPhoneNumber", maskedtbxBilgiWorkPhone.Text);
                //cmd.Parameters.AddWithValue("@PersonalPhoneNumber", maskedtbxBilgiPersonalPhone.Text);
                //cmd.Parameters.AddWithValue("@Email", metrotbxBilgiEmail.Text);
                //cmd.Parameters.AddWithValue("@TypeOfMembership", metroComboBoxBilgiTypeOfMembership.SelectedItem.ToString());
                //cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
                //cmd.Parameters.AddWithValue("@AddedDate", dateTimePickerBilgiAddedDate.Value.Date);
                //cmd.Parameters.AddWithValue("@MembershipFinishDate", dateTimePickerBilgiMembershipFinishDate.Value.Date);
                cmd.Parameters.AddWithValue("@Paid", metroComboBoxMuhasebeAidatÖdemeDurumu.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@PaidDate", dateTimePickerMuhasebeAidatPaidDate.Value.Date);
                cmd.Parameters.AddWithValue("@PaidFinishDate", dateTimePickerMuhasebeAidatPaidFinishDate.Value.Date);
                cmd.Parameters.AddWithValue("@UpdatedDate", DateLocal.Date);
                cmd.Parameters.AddWithValue("@Selected_AccountID_", Selected_AccountID_);
                check = cmd.ExecuteNonQuery();
                conn.Close();

                if (check > 0)
                {
                    MetroMessageBox.Show(this, "\nKayıt Başarıyla Güncellendi.", "Başarı");
                    GetMemberData();
                }
            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }
        }

        private void metroTileMuhasebeAidatDüzenle_Click(object sender, EventArgs e)
        {
            if (!groupBoxMuhasebeAidatDüzenle.Visible && !groupBoxMuhasebeAidatDüzenle.Enabled)
            {
                groupBoxMuhasebeAidatDüzenle.Visible = true;
                groupBoxMuhasebeAidatDüzenle.Enabled = true;

                //Ödemeyi Kapat
                groupBoxMuhasebeAidatÖdeme.Visible = false;
                groupBoxMuhasebeAidatÖdeme.Enabled = false;
            }

        }

        private void metroGridMuhasebeAidatUyeler_SelectionChanged(object sender, EventArgs e)
        {
            if (groupBoxMuhasebeAidatDüzenle.Visible || groupBoxMuhasebeAidatDüzenle.Enabled)
            {
                MuhasebeAidatUyelerShowOnControls("Düzenle");
            }
            else if (groupBoxMuhasebeAidatÖdeme.Visible || groupBoxMuhasebeAidatÖdeme.Enabled)
            {
                MuhasebeAidatUyelerShowOnControls("Ödeme");
            }
        }

        private void metrobtnMuhasebeAidatAra_Click(object sender, EventArgs e)
        {
            if (metroComboBoxMuhasebeAidatSearchFilter.SelectedItem == null)
            {
                MuhasebeAidatUyelerSearch("*Filtre Yok");
            }
            else
            {
                MuhasebeAidatUyelerSearch(metroComboBoxMuhasebeAidatSearchFilter.SelectedItem.ToString());
            }

        }

        private string SplitStringForSearch(string SearchText)
        {
            string newText = "";
            SearchText = SearchText.Replace(' ', '%');
            for (int i = 0; i < SearchText.Length; i++)
            {
                newText += SearchText.Substring(i, 1);
                if (SearchText.Length == i + 1)
                {

                }
                else
                {
                    newText += "%";
                }
            }
            return newText;
        }


        private void metroTileMuhasebeAidatUyeListesiYenile_Click(object sender, EventArgs e)
        {
            metroTileMuhasebeAidatUyeListesiYenile.Enabled = false;
            MuhasebeAidatUyeler();
            metroTileMuhasebeAidatUyeListesiYenile.Enabled = true;
        }

        private void metroTileMuhasebeAidatDüzenleKaydet_Click(object sender, EventArgs e)
        {
            metroTileMuhasebeAidatDüzenleKaydet.Enabled = false;
            MuhasebeAidatUyeBilgileriUpdate();
            metroTileMuhasebeAidatDüzenleKaydet.Enabled = true;
        }

        private void metrotbxMuhasebeÖdemeÜcret_Click(object sender, EventArgs e)
        {

        }

        private void metrotbxMuhasebeÖdemeÜcret_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (metrotbxMuhasebeÖdemeÜcret.Text == "")
                {
                    metroTileMuhasebeÖdemeÜcret.Text = "Ücret";
                    metroTileMuhasebeÖdemeÜcret.TextAlign = ContentAlignment.BottomLeft;
                }
                else
                {
                    if (metrotbxMuhasebeÖdemeÜcret.TextLength == 1)
                    {
                        metroTileMuhasebeÖdemeÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileMuhasebeÖdemeÜcret.Text = "Ücret:\n" + Convert.ToInt32(metrotbxMuhasebeÖdemeÜcret.Text) + " ₺";
                    }
                    else
                    {
                        metroTileMuhasebeÖdemeÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileMuhasebeÖdemeÜcret.Text = "Ücret:\n" + String.Format("{0:0,0}", Convert.ToInt32(metrotbxMuhasebeÖdemeÜcret.Text)) + " ₺";
                    }

                }
            }
            catch (Exception)
            {


            }



        }

        private void metrotbxMuhasebeÖdemeÜcret_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                MessageBox.Show("Bu öğe için CTRL tuşu iptal edildi!");
            }
        }

        private void metrotbxMuhasebeÖdemeÜcret_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void metroTileMuhasebeAidatÖdeme_Click(object sender, EventArgs e)
        {
            if (!groupBoxMuhasebeAidatÖdeme.Visible && !groupBoxMuhasebeAidatÖdeme.Enabled)
            {
                groupBoxMuhasebeAidatÖdeme.Location = new Point(13, 430);
                groupBoxMuhasebeAidatÖdeme.Visible = true;
                groupBoxMuhasebeAidatÖdeme.Enabled = true;

                //Düzenleyi Kapat
                groupBoxMuhasebeAidatDüzenle.Visible = false;
                groupBoxMuhasebeAidatDüzenle.Enabled = false;
            }

        }

        private void MuhasebeAidatÖdemeyiGerçekleştir()
        {
            //INSERT INTO table_name(column1, column2, column3,...) VALUES(value1, value2, value3,...);
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("INSERT INTO Aidat (UyeID,Name,Surname,Ücret,PaidDate,AddedBy) VALUES (@UyeID,@Name,@Surname,@Ücret,@PaidDate,@AddedBy)", conn);
            conn.Open();
            cmd.Parameters.AddWithValue("@UyeID", Convert.ToInt32(metrotbxMuhasebeÖdemeMemberID.Text));
            cmd.Parameters.AddWithValue("@Name", metrotbxMuhasebeÖdemeName.Text);
            cmd.Parameters.AddWithValue("@Surname", metrotbxMuhasebeÖdemeSurname.Text);
            cmd.Parameters.AddWithValue("@Ücret", Convert.ToDouble(metrotbxMuhasebeÖdemeÜcret.Text));
            cmd.Parameters.AddWithValue("@PaidDate", DateLocal.Date);
            cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
            int check = cmd.ExecuteNonQuery();
            conn.Close();

            //UPDATE Accounts SET LastOnline = @LastOnline WHERE Id = @AccountID
            SqlConnection conn_2 = new SqlConnection(connectionString);
            SqlCommand cmd_2 = new SqlCommand("UPDATE Members SET Paid=@Paid,PaidDate=@PaidDate,PaidFinishDate=@PaidFinishDate WHERE Id=@AccountID", conn_2);
            conn_2.Open();
            cmd_2.Parameters.AddWithValue("@Paid", "Ödendi");
            cmd_2.Parameters.AddWithValue("@PaidDate", DateLocal.Date);
            cmd_2.Parameters.AddWithValue("@PaidFinishDate", dateTimePickerMuhasebeÖdemePaidFinishDate.Value.Date);
            cmd_2.Parameters.AddWithValue("@AccountID", Selected_AccountID_);
            int check_2 = cmd_2.ExecuteNonQuery();
            conn_2.Close();

            if (check > 0 && check_2 > 0)
            {
                msg("\nÖdeme İşlemi Başarıyla Gerçekleşti.", "Başarı", 150);
            }


        }

        private void rbtnMuhasebeAidat3AySonra_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMuhasebeAidat3AySonra.Checked)
            {
                dateTimePickerMuhasebeÖdemePaidFinishDate.Enabled = false;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Value = DateLocal.Date;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Text = dateTimePickerMuhasebeÖdemePaidFinishDate.Value.AddMonths(3).Date.ToString();

            }
        }

        private void rbtnMuhasebeAidat6AySonra_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMuhasebeAidat6AySonra.Checked)
            {
                dateTimePickerMuhasebeÖdemePaidFinishDate.Enabled = false;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Value = DateLocal.Date;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Text = dateTimePickerMuhasebeÖdemePaidFinishDate.Value.AddMonths(6).Date.ToString();
            }
        }

        private void rbtnMuhasebeAidat1YılSonra_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMuhasebeAidat1YılSonra.Checked)
            {
                dateTimePickerMuhasebeÖdemePaidFinishDate.Enabled = false;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Value = DateLocal.Date;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Text = dateTimePickerMuhasebeÖdemePaidFinishDate.Value.AddYears(1).Date.ToString();
            }
        }

        private void rbtnMuhasebeAidatKendinSeç_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnMuhasebeAidatKendinSeç.Checked)
            {
                dateTimePickerMuhasebeÖdemePaidFinishDate.Enabled = true;
                dateTimePickerMuhasebeÖdemePaidFinishDate.Value = DateLocal.Date;
            }
        }

        private void metroTileMuhasebeÖdemeÖdemeyiGerçekleştir_Click(object sender, EventArgs e)
        {
            metroTileMuhasebeÖdemeÖdemeyiGerçekleştir.Enabled = false;
            MuhasebeAidatÖdemeyiGerçekleştir();
            metroTileMuhasebeÖdemeÖdemeyiGerçekleştir.Enabled = true;
        }

        private void metroCheckBoxKortNote_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBoxKortNote.Checked)
            {
                metrotbxKortKortNotu.Enabled = true;
                metrotbxKortKortNotu.Visible = true;
                metroCheckBoxKortNote.Text = "Var";
            }
            else
            {
                metrotbxKortKortNotu.Enabled = false;
                metrotbxKortKortNotu.Visible = false;
                metroCheckBoxKortNote.Text = "Yok";
            }
        }

        private void AidatSonÖdemeGüncelle()
        {
            DateTime GuncelTarih = new DateTime();
            GuncelTarih = GetNistTime();

            DateTime PaidFinishDate = new DateTime();

            int MemberID = 0;

            //Güncelleme
            //UPDATE Accounts SET LastOnline = @LastOnline WHERE Id = @AccountID
            SqlConnection conn_guncel = new SqlConnection(connectionString);



            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT Id,Paid,PaidFinishDate FROM Members", conn);
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {

                if (dr["PaidFinishDate"] == DBNull.Value)
                {
                    PaidFinishDate = new DateTime(2000, 1, 1);
                }
                else
                {
                    PaidFinishDate = Convert.ToDateTime(dr["PaidFinishDate"]);
                }

                MemberID = Convert.ToInt32(dr["Id"]);
                if (GuncelTarih >= PaidFinishDate && dr["Paid"].ToString() == "Ödendi")
                {

                    if (conn_guncel.State == ConnectionState.Closed)
                    {
                        conn_guncel.Open();
                    }
                    SqlCommand cmd_guncel = new SqlCommand("UPDATE Members SET Paid='Ödenmedi' WHERE Id=@MemberID", conn_guncel);
                    cmd_guncel.Parameters.AddWithValue("@MemberID", MemberID);
                    cmd_guncel.ExecuteNonQuery();

                }
            }
            conn.Close();
            conn_guncel.Close();

        }

        private void metrotbxHocaTakipÖdemeÜcret_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                MessageBox.Show("Bu öğe için CTRL tuşu iptal edildi!");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true)
            {
                MessageBox.Show("Bu öğe için CTRL tuşu iptal edildi!");
            }
        }

        private void metrotbxHocaTakipÖdemeÜcret_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void HocaTakipGetData()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM IncomeFromTeachers ORDER BY AddedDate,Name,Surname", conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            metroGridHocaTakip.DataSource = dt;
            conn.Close();
            metroGridHocaTakip.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            metroGridHocaTakip.Columns[0].HeaderText = "ID";
            metroGridHocaTakip.Columns[1].HeaderText = "Hoca ID";
            metroGridHocaTakip.Columns[2].HeaderText = "İsim";
            metroGridHocaTakip.Columns[3].HeaderText = "Soyisim";
            metroGridHocaTakip.Columns[4].HeaderText = "Ücret";
            metroGridHocaTakip.Columns[5].HeaderText = "Eklenme Tarihi";
            metroGridHocaTakip.Columns[6].HeaderText = "Ekleyen Kişi";
            metroGridHocaTakip.Columns[7].HeaderText = "Açıklama";


            metroGridHocaTakip.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridHocaTakip.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridHocaTakip.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridHocaTakip.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridHocaTakip.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridHocaTakip.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;


        }

        private void HocaTakipShowOnControls(string Control)
        {
            try
            {
                if (Control == "Düzenle")
                {
                    if (metroGridHocaTakip.SelectedRows.Count == 1 || metroGridHocaTakip.SelectedRows != null)
                    {
                        DataGridViewRow row = metroGridHocaTakip.SelectedRows[0];
                        Selected_AccountID_ = Convert.ToInt32(row.Cells["HocaID"].Value);
                        metrotbxHocaTakipDüzenleHocaID.Text = Selected_AccountID_.ToString();
                        metrotbxHocaTakipDüzenleName.Text = row.Cells["Name"].Value.ToString();
                        metrotbxHocaTakipDüzenleSurname.Text = row.Cells["Surname"].Value.ToString();
                        metrotbxHocaTakipDüzenleAçıklama.Text = row.Cells["Notes"].Value.ToString();
                        metrotbxHocaTakipDüzenleAddedBy.Text = row.Cells["AddedBy"].Value.ToString();
                        metrotbxHocaTakipDüzenleÜcret.Text = row.Cells["Ücret"].Value.ToString();
                        dateTimePickerHocaTakipDüzenleAddedDate.Value = Convert.ToDateTime(row.Cells["AddedDate"].Value).Date;

                    }

                }

                else if (Control == "Ödeme")
                {
                    if (metroGridHocaTakipHocaMember.SelectedRows.Count == 1 || metroGridHocaTakipHocaMember.SelectedRows != null)
                    {
                        DataGridViewRow row = metroGridHocaTakipHocaMember.SelectedRows[0];
                        Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                        metrotbxHocaTakipÖdemeHocaID.Text = Selected_AccountID_.ToString();
                        metrotbxHocaTakipÖdemeName.Text = row.Cells["Name"].Value.ToString();
                        metrotbxHocaTakipÖdemeSurname.Text = row.Cells["Surname"].Value.ToString();

                    }
                }


            }
            catch (Exception)
            {


            }
        }

        private void metroGridHocaTakip_SelectionChanged(object sender, EventArgs e)
        {
            if (groupBoxHocaTakipDüzenle.Enabled && groupBoxHocaTakipDüzenle.Visible)
            {
                HocaTakipShowOnControls("Düzenle");
            }
            //else if (groupBoxHocaTakipÖdeme.Enabled && groupBoxHocaTakipÖdeme.Visible)
            //{
            //    HocaTakipShowOnControls("Ödeme");
            //}
        }

        private void metroTileHocaTakipDüzenle_Click(object sender, EventArgs e)
        {
            if (!groupBoxHocaTakipDüzenle.Enabled && !groupBoxHocaTakipDüzenle.Visible)
            {
                groupBoxHocaTakipDüzenle.Enabled = true;
                groupBoxHocaTakipDüzenle.Visible = true;

                metroGridHocaTakip.Visible = true;
                metroGridHocaTakip.Enabled = true;


                metroGridHocaTakipHocaMember.Visible = false;
                metroGridHocaTakipHocaMember.Enabled = false;



                groupBoxHocaTakipÖdeme.Enabled = false;
                groupBoxHocaTakipÖdeme.Visible = false;
            }
        }

        private void metroTileHocaTakipÖdeme_Click(object sender, EventArgs e)
        {
            //13; 430

            if (!groupBoxHocaTakipÖdeme.Enabled && !groupBoxHocaTakipÖdeme.Visible)
            {
                groupBoxHocaTakipÖdeme.Location = new Point(13, 430);
                groupBoxHocaTakipÖdeme.Enabled = true;
                groupBoxHocaTakipÖdeme.Visible = true;

                metroGridHocaTakipHocaMember.Visible = true;
                metroGridHocaTakipHocaMember.Enabled = true;
                metroGridHocaTakipHocaMember.Location = new Point(20, 28);

                metroGridHocaTakip.Visible = false;
                metroGridHocaTakip.Enabled = false;


                groupBoxHocaTakipDüzenle.Enabled = false;
                groupBoxHocaTakipDüzenle.Visible = false;
            }
        }

        private void metrotbxHocaTakipÖdemeÜcret_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (metrotbxHocaTakipÖdemeÜcret.Text == "")
                {
                    metroTileHocaTakipÖdemeÜcret.Text = "Ücret";
                    metroTileHocaTakipÖdemeÜcret.TextAlign = ContentAlignment.BottomLeft;
                }
                else
                {
                    if (metrotbxHocaTakipÖdemeÜcret.TextLength == 1)
                    {
                        metroTileHocaTakipÖdemeÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileHocaTakipÖdemeÜcret.Text = "Ücret:\n" + Convert.ToInt32(metrotbxHocaTakipÖdemeÜcret.Text) + " ₺";
                    }
                    else
                    {
                        metroTileHocaTakipÖdemeÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileHocaTakipÖdemeÜcret.Text = "Ücret:\n" + String.Format("{0:0,0}", Convert.ToInt32(metrotbxHocaTakipÖdemeÜcret.Text)) + " ₺";
                    }

                }
            }
            catch (Exception)
            {


            }
        }

        private void metrotbxHocaTakipDüzenleÜcret_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (metrotbxHocaTakipDüzenleÜcret.Text == "")
                {
                    metroTileHocaTakipDüzenleÜcret.Text = "Ücret";
                    metroTileHocaTakipDüzenleÜcret.TextAlign = ContentAlignment.BottomLeft;
                }
                else
                {
                    if (metrotbxHocaTakipDüzenleÜcret.TextLength == 1)
                    {
                        metroTileHocaTakipDüzenleÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileHocaTakipDüzenleÜcret.Text = "Ücret:\n" + Convert.ToInt32(metrotbxHocaTakipDüzenleÜcret.Text) + " ₺";
                    }
                    else
                    {
                        metroTileHocaTakipDüzenleÜcret.TextAlign = ContentAlignment.TopLeft;
                        metroTileHocaTakipDüzenleÜcret.Text = "Ücret:\n" + String.Format("{0:0,0}", Convert.ToInt32(metrotbxHocaTakipDüzenleÜcret.Text)) + " ₺";
                    }

                }
            }
            catch (Exception)
            {


            }
        }

        private void rbtnHocaTakipSeçenek1_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHocaTakipSeçenek1.Checked)
            {
                metrotbxHocaTakipÖdemeAçıklama.Enabled = false;
                metrotbxHocaTakipÖdemeAçıklama.Text = rbtnHocaTakipSeçenek1.Text;
            }
        }

        private void rbtnHocaTakipSeçenek2_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHocaTakipSeçenek2.Checked)
            {
                metrotbxHocaTakipÖdemeAçıklama.Enabled = false;
                metrotbxHocaTakipÖdemeAçıklama.Text = rbtnHocaTakipSeçenek2.Text;
            }
        }

        private void rbtnHocaTakipSeçenek3_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHocaTakipSeçenek3.Checked)
            {
                metrotbxHocaTakipÖdemeAçıklama.Enabled = false;
                metrotbxHocaTakipÖdemeAçıklama.Text = rbtnHocaTakipSeçenek3.Text;
            }
        }

        private void rbtnHocaTakipSeçenekElle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnHocaTakipSeçenekElle.Checked)
            {
                metrotbxHocaTakipÖdemeAçıklama.Text = "";
                metrotbxHocaTakipÖdemeAçıklama.Enabled = true;

            }
        }

        private void metroTileHocaTakipYenile_Click(object sender, EventArgs e)
        {
            metroTileHocaTakipYenile.Enabled = false;


            if (metroGridHocaTakip.Enabled)
            {
                HocaTakipGetData();
            }
            else if (metroGridHocaTakipHocaMember.Enabled)
            {
                HocaMemberGetData();
            }


            metroTileHocaTakipYenile.Enabled = true;
        }

        private void HocaMemberGetData()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT * FROM Members WHERE Status='Hoca' ORDER BY Name,Surname", conn);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                metroGridHocaTakipHocaMember.DataSource = dt;

                metroGridHocaTakipHocaMember.DefaultCellStyle.Font = new Font("Segoe UI", 9);

                metroGridHocaTakipHocaMember.Columns[0].HeaderText = "ID";
                metroGridHocaTakipHocaMember.Columns[1].HeaderText = "Name";
                metroGridHocaTakipHocaMember.Columns[2].HeaderText = "Surname";
                metroGridHocaTakipHocaMember.Columns[3].HeaderText = "DateOfBirth";
                metroGridHocaTakipHocaMember.Columns[4].Visible = false;
                metroGridHocaTakipHocaMember.Columns[5].Visible = false;
                metroGridHocaTakipHocaMember.Columns[6].HeaderText = "İş-Tel";
                metroGridHocaTakipHocaMember.Columns[7].HeaderText = "Cep-Tel";
                metroGridHocaTakipHocaMember.Columns[8].HeaderText = "E-Mail";
                metroGridHocaTakipHocaMember.Columns[9].Visible = false;
                metroGridHocaTakipHocaMember.Columns[10].Visible = false;
                metroGridHocaTakipHocaMember.Columns[11].Visible = false;
                metroGridHocaTakipHocaMember.Columns[12].Visible = false;
                metroGridHocaTakipHocaMember.Columns[13].Visible = false;
                metroGridHocaTakipHocaMember.Columns[14].Visible = false;
                metroGridHocaTakipHocaMember.Columns[15].Visible = false;
                metroGridHocaTakipHocaMember.Columns[16].Visible = false;
                metroGridHocaTakipHocaMember.Columns[17].Visible = false;
                metroGridHocaTakipHocaMember.Columns[18].Visible = false;







                for (int i = 0; i < metroGridHocaTakipHocaMember.Columns.Count; i++)
                {
                    metroGridHocaTakipHocaMember.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                //metroGridHocaTakipHocaMember.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //metroGridHocaTakipHocaMember.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //metroGridHocaTakipHocaMember.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //metroGridHocaTakipHocaMember.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //metroGridHocaTakipHocaMember.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridHocaTakipHocaMember.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                //metroGridHocaTakip.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

                conn.Close();
            }
            catch (Exception exp)
            {

                GetErrorMessage(exp);
            }

        }

        private void metroGridHocaTakipHocaMember_SelectionChanged(object sender, EventArgs e)
        {
            if (groupBoxHocaTakipÖdeme.Enabled && groupBoxHocaTakipÖdeme.Visible)
            {
                HocaTakipShowOnControls("Ödeme");
            }

        }

        private void HocaÖdemeKayıt()
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("INSERT INTO IncomeFromTeachers (HocaID,Name,Surname,Ücret,AddedDate,AddedBy,Notes) Values (@HocaID,@Name,@Surname,@Ücret,@AddedDate,@AddedBy,@Notes)", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@HocaID", Convert.ToInt32(metrotbxHocaTakipÖdemeHocaID.Text));
                cmd.Parameters.AddWithValue("@Name", metrotbxHocaTakipÖdemeName.Text);
                cmd.Parameters.AddWithValue("@Surname", metrotbxHocaTakipÖdemeSurname.Text);
                cmd.Parameters.AddWithValue("@Ücret", Convert.ToInt32(metrotbxHocaTakipÖdemeÜcret.Text));
                cmd.Parameters.AddWithValue("@AddedDate", DateLocal.Date);
                cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
                cmd.Parameters.AddWithValue("Notes", metrotbxHocaTakipÖdemeAçıklama.Text);
                int check = cmd.ExecuteNonQuery();

                if (check > 0)
                {
                    msg("\nÖdeme Kayıdı Başarıyla Yapıldı!", "Başarı", 150);
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                GetErrorMessage(exp);

            }
        }

        private void metroTileHocaTakipÖdemeÖdemeyiGerçekleştir_Click(object sender, EventArgs e)
        {
            metroTileHocaTakipÖdemeÖdemeyiGerçekleştir.Enabled = false;

            HocaÖdemeKayıt();

            metroTileHocaTakipÖdemeÖdemeyiGerçekleştir.Enabled = true;
        }

        private void metroTileHocaTakip_Click(object sender, EventArgs e)
        {
            if (!groupBoxHocaTakip.Visible)
            {
                groupBoxHocaTakip.Location = new Point(158, 14);
                groupBoxHocaTakip.Visible = true;
                groupBoxHocaTakip.Enabled = true;



                groupBoxAidatTakip.Visible = false;
                groupBoxAidatTakip.Enabled = false;
            }
        }

        private void metroTileAidatTakip_Click(object sender, EventArgs e)
        {
            if (!groupBoxAidatTakip.Visible)
            {

                groupBoxHocaTakip.Visible = false;
                groupBoxHocaTakip.Enabled = false;



                groupBoxAidatTakip.Visible = true;
                groupBoxAidatTakip.Enabled = true;
            }
        }

        private void metroTileHocaTakipÖdeme_Click_1(object sender, EventArgs e)
        {
            if (!groupBoxHocaTakipÖdeme.Enabled && !groupBoxHocaTakipÖdeme.Visible)
            {
                groupBoxHocaTakipÖdeme.Location = new Point(13, 430);
                groupBoxHocaTakipÖdeme.Enabled = true;
                groupBoxHocaTakipÖdeme.Visible = true;

                metroGridHocaTakipHocaMember.Visible = true;
                metroGridHocaTakipHocaMember.Enabled = true;
                metroGridHocaTakipHocaMember.Location = new Point(20, 28);

                metroGridHocaTakip.Visible = false;
                metroGridHocaTakip.Enabled = false;


                groupBoxHocaTakipDüzenle.Enabled = false;
                groupBoxHocaTakipDüzenle.Visible = false;
            }
        }

        private void FixKayıtOyuncuList()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT Id,Name,Surname,Gender,MembershipFinishDate,Paid,PaidDate,Status FROM Members ORDER BY Name,Surname", conn);
            conn.Open();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            metroGridFixKayıtKayıtOyuncular.DataSource = dt;

            metroGridFixKayıtKayıtOyuncular.DefaultCellStyle.Font = new Font("Segoe UI", 9);

            metroGridFixKayıtKayıtOyuncular.Columns[0].HeaderText = "ID";
            metroGridFixKayıtKayıtOyuncular.Columns[1].HeaderText = "Ad";
            metroGridFixKayıtKayıtOyuncular.Columns[2].HeaderText = "Soyad";
            metroGridFixKayıtKayıtOyuncular.Columns[3].HeaderText = "Cinsiyet";
            //metroGridOyuncu.Columns[4].HeaderText = "Bitiş";
            metroGridFixKayıtKayıtOyuncular.Columns[4].Visible = false;
            metroGridFixKayıtKayıtOyuncular.Columns[5].HeaderText = "Aidat";
            metroGridFixKayıtKayıtOyuncular.Columns[6].HeaderText = "Ödeme Tarihi";
            //Hoca
            metroGridFixKayıtKayıtOyuncular.Columns[7].Visible = false;


            //MetroGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //MetroGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            //MetroGrid.Columns[3].Width = 100;
            metroGridFixKayıtKayıtOyuncular.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            conn.Close();
            try
            {
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.Font = new Font(metroGridFixKayıtKayıtOyuncular.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                style.ForeColor = Color.IndianRed;
                foreach (DataGridViewRow row in metroGridFixKayıtKayıtOyuncular.Rows)
                {
                    if (row.Cells["Status"].Value.ToString() == "Hoca")
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void FixKayıtKortKayıtları()
        {
            try
            {
                //GuneGoreVeriCek(Convert.ToInt32(GetNistTime().DayOfWeek), "*Hepsi", metroGridFixKayıtKayıtKortKayıtları);
                //metroGridFixKayıtKayıtKortKayıtları.DefaultCellStyle.Font = new Font("Segoe UI", 9);
                SqlConnection conn = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("SELECT KortName,Time,Date,Player1,Player2,Player1_Id,Player2_Id,Light,Status FROM KortTakip WHERE Date BETWEEN @Date1 AND @Date2 ORDER BY Date,KortName,Time", conn);
                conn.Open();
                cmd.Parameters.AddWithValue("@Date1", dateTimePickerFixKayıtKayıtBaşlangıç.Value.Date);
                cmd.Parameters.AddWithValue("@Date2", dateTimePickerFixKayıtKayıtBitiş.Value.Date);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                metroGridFixKayıtKayıtKortKayıtları.DataSource = dt;
                metroGridFixKayıtKayıtKortKayıtları.Columns[5].Visible = false;
                metroGridFixKayıtKayıtKortKayıtları.Columns[6].Visible = false;

                //metroGridFixKayıtKayıtKortKayıtları.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;


                metroGridFixKayıtKayıtKortKayıtları.Columns[0].HeaderText = "Kort İsmi";
                metroGridFixKayıtKayıtKortKayıtları.Columns[1].HeaderText = "Saat";
                //metroGridFixKayıtKayıtKortKayıtları.Columns[1].Width = 50;
                metroGridFixKayıtKayıtKortKayıtları.Columns[2].HeaderText = "Tarih";
                metroGridFixKayıtKayıtKortKayıtları.Columns[3].HeaderText = "Oyuncu 1";
                metroGridFixKayıtKayıtKortKayıtları.Columns[4].HeaderText = "Oyuncu 2";
                metroGridFixKayıtKayıtKortKayıtları.Columns[7].HeaderText = "Işık";
                metroGridFixKayıtKayıtKortKayıtları.Columns[8].Visible = false;

                metroGridFixKayıtKayıtKortKayıtları.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridFixKayıtKayıtKortKayıtları.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                metroGridFixKayıtKayıtKortKayıtları.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

                conn.Close();

                try
                {
                    foreach (DataGridViewRow row in metroGridFixKayıtKayıtKortKayıtları.Rows)
                    {
                        for (int i = 0; i < HocaList.Count; i++)
                        {
                            try
                            {
                                if (Convert.ToInt32(row.Cells[5].Value) == HocaList[i] || Convert.ToInt32(row.Cells[6].Value) == HocaList[i])
                                {
                                    try
                                    {
                                        DataGridViewCellStyle style = new DataGridViewCellStyle();
                                        style.Font = new Font(metroGridFixKayıtKayıtKortKayıtları.Font, FontStyle.Bold);
                                        style.ForeColor = Color.IndianRed;

                                        foreach (DataGridViewCell cell in row.Cells)
                                            cell.Style.ApplyStyle(style);


                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                            catch (Exception)
                            {


                            }

                        }
                    }
                }
                catch (Exception)
                {


                }


            }
            catch (Exception)
            {


            }

            FixUyeRenk(metroGridFixKayıtKayıtKortKayıtları);


        }

        private void metroTileHocaTakipDüzenle_Click_1(object sender, EventArgs e)
        {
            if (!groupBoxHocaTakipDüzenle.Enabled && !groupBoxHocaTakipDüzenle.Visible)
            {
                groupBoxHocaTakipDüzenle.Enabled = true;
                groupBoxHocaTakipDüzenle.Visible = true;

                metroGridHocaTakip.Visible = true;
                metroGridHocaTakip.Enabled = true;


                metroGridHocaTakipHocaMember.Visible = false;
                metroGridHocaTakipHocaMember.Enabled = false;



                groupBoxHocaTakipÖdeme.Enabled = false;
                groupBoxHocaTakipÖdeme.Visible = false;
            }
        }

        private List<DateTime> FindNextDay(DayOfWeek Day, DateTime Başlangıç, DateTime Bitiş)
        {
            //TimeSpan GunFarki = Başlangıç.Subtract(Bitiş);

            List<DateTime> allDates = new List<DateTime>();
            for (DateTime date = Başlangıç; date <= Bitiş; date = date.AddDays(1))
            {
                if (date.DayOfWeek == Day)
                {
                    allDates.Add(date);
                }

            }
            return allDates;
        }

        private void FixKayıtKayıt()
        {
            DayOfWeek DoW = new DayOfWeek();

            switch (metroComboBoxFixKayıtKayıtGün.SelectedItem.ToString())
            {
                case "Pazartesi": DoW = DayOfWeek.Monday; break;
                case "Salı": DoW = DayOfWeek.Tuesday; break;
                case "Çarşamba": DoW = DayOfWeek.Wednesday; break;
                case "Perşembe": DoW = DayOfWeek.Thursday; break;
                case "Cuma": DoW = DayOfWeek.Friday; break;
                case "Cumartesi": DoW = DayOfWeek.Saturday; break;
                case "Pazar": DoW = DayOfWeek.Sunday; break;

                default: break;
            }

            List<DateTime> Dates = FindNextDay(DoW, dateTimePickerFixKayıtKayıtBaşlangıç.Value.Date, dateTimePickerFixKayıtKayıtBitiş.Value.Date);

            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("INSERT INTO KortTakip (KortName,Time,Date,Player1,Player1_Id,Light,AddedBy,Notes,Status) VALUES (@KortName,@Time,@Date,@Player1,@Player1_Id,@Light,@AddedBy,@Notes,@Status) ", conn);

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            cmd.Parameters.AddWithValue("@KortName", metroComboBoxFixKayıtKayıtKort.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@Time", metroComboBoxFixKayıtKayıtSaat.SelectedItem.ToString());
            //DATE---------->
            string playerName = "", playerID = "";
            if (metroComboBoxFixKayıtKayıtKayıtTürü.SelectedItem.ToString() == "Oyuncu")
            {
                playerName = metroLinkFixKayıtKayıtOyuncuSeç.Text;
                playerID = Player1ID;
                cmd.Parameters.AddWithValue("@Status", "Oyuncu");
            }
            else if (metroComboBoxFixKayıtKayıtKayıtTürü.SelectedItem.ToString() == "Spor Okulu")
            {
                playerName = "Spor Okulu";
                playerID = "Spor Okulu";
                cmd.Parameters.AddWithValue("@Status", "Spor Okulu");
            }
            else if (metroComboBoxFixKayıtKayıtKayıtTürü.SelectedItem.ToString() == "Diğer")
            {
                playerName = metrotbxFixKayıtKayıtKayıtTürüDiğer.Text + "(Diğer)";
                playerID = "Diğer";
                cmd.Parameters.AddWithValue("@Status", "Diğer");
            }
            cmd.Parameters.AddWithValue("@Player1", playerName);
            cmd.Parameters.AddWithValue("@Player1_Id", playerID);
            cmd.Parameters.AddWithValue("@Light", metroComboBoxFixKayıtKayıtAydınlatma.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
            if (metrotbxFixKayıtKayıtKortNotu.Text != "")
            {
                cmd.Parameters.AddWithValue("@Notes", metrotbxFixKayıtKayıtKortNotu.Text);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Notes", DBNull.Value);
            }


            cmd.Parameters.AddWithValue("@Date", SqlDbType.Date);

            for (int i = 0; i < Dates.Count; i++)
            {
                cmd.Parameters["@Date"].Value = Dates[i].Date;
                cmd.ExecuteNonQuery();
            }


            conn.Close();

        }

        private void FixKayıtOyuncuSeç()
        {
            string PlayerName;
            int PlayerID;

            //DataGridViewddan Üye Seçimi (Mause İle)
            if (metroGridFixKayıtKayıtOyuncular.SelectedRows.Count == 1 || metroGridFixKayıtKayıtOyuncular.SelectedRows != null)
            {
                DataGridViewRow row = metroGridFixKayıtKayıtOyuncular.SelectedRows[0];
                //Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                PlayerName = row.Cells["Name"].Value.ToString() + " " + row.Cells["Surname"].Value.ToString();
                PlayerID = Convert.ToInt32(row.Cells["Id"].Value);
                if (Player == 1)
                {
                    Player1Name = PlayerName;
                    Player1ID = PlayerID.ToString();
                }
                else
                {
                    Player2Name = PlayerName;
                    Player2ID = PlayerID.ToString();
                }

                metroLinkFixKayıtKayıtOyuncuSeç.Text = PlayerName + " (" + PlayerID + ")";
            }
        }

        private void metroTileFixKayıtKayıtKayıtYap_Click(object sender, EventArgs e)
        {
            FixKayıtKayıt();
        }

        private void FixUyeRenk(MetroGrid MetroGrid)
        {
            //HOCA BOLD
            try
            {


                foreach (DataGridViewRow row in MetroGrid.Rows)
                {
                    if (row.Cells["Status"].Value.ToString() == "Oyuncu")
                    {
                        DataGridViewCellStyle style = new DataGridViewCellStyle();
                        //style.Font = new Font(MetroGrid.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                        style.Font = new Font(MetroGrid.Font, FontStyle.Bold);
                        style.ForeColor = Color.FromArgb(255, 243, 119, 53);

                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                    else if (row.Cells["Status"].Value.ToString() == "Spor Okulu")
                    {
                        DataGridViewCellStyle style = new DataGridViewCellStyle();
                        //style.Font = new Font(MetroGrid.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                        style.Font = new Font(MetroGrid.Font, FontStyle.Bold);
                        style.ForeColor = Color.FromArgb(255, 0, 170, 173);


                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                    else if (row.Cells["Status"].Value.ToString() == "Diğer")
                    {
                        DataGridViewCellStyle style = new DataGridViewCellStyle();
                        //style.Font = new Font(MetroGrid.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                        style.Font = new Font(MetroGrid.Font, FontStyle.Bold);
                        style.ForeColor = Color.FromArgb(255, 165, 81, 0);


                        foreach (DataGridViewCell cell in row.Cells)
                            cell.Style.ApplyStyle(style);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void metroTileFixKayıtKayıtKortGüncelle_Click(object sender, EventArgs e)
        {
            FixKayıtKortKayıtları();
        }

        private void metroCheckBoxFixKayıtKayıtKortNotu_CheckedChanged(object sender, EventArgs e)
        {
            if (!metroCheckBoxFixKayıtKayıtKortNotu.Checked)
            {
                metroCheckBoxFixKayıtKayıtKortNotu.Text = "Yok";
                metrotbxFixKayıtKayıtKortNotu.Visible = false;
                metrotbxFixKayıtKayıtKortNotu.Enabled = false;
            }
            else
            {
                metroCheckBoxFixKayıtKayıtKortNotu.Text = "Var";
                metrotbxFixKayıtKayıtKortNotu.Visible = true;
                metrotbxFixKayıtKayıtKortNotu.Enabled = true;
            }
        }

        private void metroComboBoxFixKayıtKayıtKayıtTürü_SelectedValueChanged(object sender, EventArgs e)
        {
            if (metroComboBoxFixKayıtKayıtKayıtTürü.SelectedItem.ToString() == "Diğer")
            {

                metrotbxFixKayıtKayıtKayıtTürüDiğer.Location = new Point(99, 354);
                metroLinkFixKayıtKayıtOyuncuSeç.Visible = false;
                metroLinkFixKayıtKayıtOyuncuSeç.Enabled = false;
                metroTileFixKayıtKayıtSeç.Enabled = false;
                metrotbxFixKayıtKayıtKayıtTürüDiğer.Visible = true;
                metrotbxFixKayıtKayıtKayıtTürüDiğer.Enabled = true;
            }
            else
            {
                metroLinkFixKayıtKayıtOyuncuSeç.Visible = true;
                metroLinkFixKayıtKayıtOyuncuSeç.Enabled = true;
                metroTileFixKayıtKayıtSeç.Enabled = true;
                metrotbxFixKayıtKayıtKayıtTürüDiğer.Visible = false;
                metrotbxFixKayıtKayıtKayıtTürüDiğer.Enabled = false;
            }
        }

        private void metroTileFixKayıtKayıtSeç_Click_1(object sender, EventArgs e)
        {
            FixKayıtOyuncuSeç();
        }

        private void metroTileFixKayıtKayıtOyuncuListesiYenile_Click(object sender, EventArgs e)
        {
            metroTileFixKayıtKayıtOyuncuListesiYenile.Enabled = false;
            FixKayıtOyuncuList();
            metroTileFixKayıtKayıtOyuncuListesiYenile.Enabled = true;
        }

        private void KortKayıtDüzenle()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM KortTakip", conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            metroGridKortKayıtDüzenle.DataSource = dt;
            conn.Close();

            try
            {
                foreach (DataGridViewRow row in metroGridKortKayıtDüzenle.Rows)
                {
                    for (int i = 0; i < HocaList.Count; i++)
                    {
                        try
                        {
                            if (row.Cells[6].Value.ToString() == HocaList[i].ToString() || row.Cells[7].Value.ToString() == HocaList[i].ToString())
                            {
                                try
                                {
                                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                                    style.Font = new Font(metroGridKortKayıtDüzenle.Font = new Font("Segoe UI", 8), FontStyle.Bold);
                                    style.ForeColor = Color.IndianRed;

                                    foreach (DataGridViewCell cell in row.Cells)
                                        cell.Style.ApplyStyle(style);


                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        catch (Exception)
                        {


                        }
                    }
                }
            }
            catch (Exception)
            {


            }

            FixUyeRenk(metroGridKortKayıtDüzenle);

            metroGridKortKayıtDüzenle.Columns[0].HeaderText = "ID";
            metroGridKortKayıtDüzenle.Columns[1].HeaderText = "Kort İsmi";
            metroGridKortKayıtDüzenle.Columns[2].HeaderText = "Saat";
            metroGridKortKayıtDüzenle.Columns[3].HeaderText = "Tarih";
            metroGridKortKayıtDüzenle.Columns[4].HeaderText = "Oyuncu 1 (ID)";
            metroGridKortKayıtDüzenle.Columns[5].HeaderText = "Oyuncu 2 (ID)";
            metroGridKortKayıtDüzenle.Columns[6].Visible = false;
            metroGridKortKayıtDüzenle.Columns[7].Visible = false;
            metroGridKortKayıtDüzenle.Columns[8].HeaderText = "Işık";
            metroGridKortKayıtDüzenle.Columns[9].HeaderText = "Ekleyen Kişi";
            metroGridKortKayıtDüzenle.Columns[10].HeaderText = "Not";
            metroGridKortKayıtDüzenle.Columns[11].Visible = false;

            metroGridKortKayıtDüzenle.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridKortKayıtDüzenle.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridKortKayıtDüzenle.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            metroGridKortKayıtDüzenle.Columns[8].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;


        }

        private void metroTileKortKayıtDüzenleListeyiYenile_Click(object sender, EventArgs e)
        {
            metroTileKortKayıtDüzenleListeyiYenile.Enabled = false;
            KortKayıtDüzenle();
            metroTileKortKayıtDüzenleListeyiYenile.Enabled = true;
        }

        private void KortKayıtUpdate()
        {
            //UPDATE Accounts SET LastOnline = @LastOnline WHERE Id = @AccountID
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("UPDATE KortTakip SET KortName=@KortName,Time=@Time,Date=@Date,Player1=@Player1,Player2=@Player2,Player1_Id=@Player1_Id,Player2_Id=@Player2_Id,Light=@Light,AddedBy=@AddedBy,Notes=@Notes,Status=@Status WHERE ID=@ID", conn);
            conn.Open();
            cmd.Parameters.AddWithValue("@Id", Selected_AccountID_);
            cmd.Parameters.AddWithValue("@KortName", metroComboBoxKortKayıtDüzenleKort.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@Time", metroComboBoxKortKayıtDüzenleSaat.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@Date", dateTimePickerKortKayıtDüzenleTarih.Value.Date);
            cmd.Parameters.AddWithValue("@Player1", metrotbxKortKayıtDüzenleOyuncu1Name.Text + " (" + metrotbxKortKayıtDüzenleOyuncu1ID.Text + ")");
            cmd.Parameters.AddWithValue("@Player2", metrotbxKortKayıtDüzenleOyuncu2Name.Text + " (" + metrotbxKortKayıtDüzenleOyuncu2ID.Text + ")");
            cmd.Parameters.AddWithValue("@Player1_Id", metrotbxKortKayıtDüzenleOyuncu1ID.Text);
            cmd.Parameters.AddWithValue("@Player2_Id", metrotbxKortKayıtDüzenleOyuncu2ID.Text);
            cmd.Parameters.AddWithValue("@Light", metroComboBoxKortKayıtDüzenleAydınlatma.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@AddedBy", lblOnlineAccount.Text);
            cmd.Parameters.AddWithValue("@Notes", richtbxKortKayıtDüzenleKortNotu.Text);
            cmd.Parameters.AddWithValue("@Status", metroComboBoxKortKayıtDüzenleKayıtTürü.SelectedItem.ToString());
            cmd.ExecuteNonQuery();

            conn.Close();
        }

        private void KortKayıtDüzenleShowOnControls()
        {
            try
            {
                if (metroGridKortKayıtDüzenle.SelectedRows.Count == 1 || metroGridKortKayıtDüzenle.SelectedRows != null)
                {
                    DataGridViewRow row = metroGridKortKayıtDüzenle.SelectedRows[0];
                    Selected_AccountID_ = Convert.ToInt32(row.Cells["Id"].Value);
                    metroComboBoxKortKayıtDüzenleKort.SelectedItem = row.Cells["KortName"].Value.ToString();
                    metroComboBoxKortKayıtDüzenleSaat.SelectedItem = row.Cells["Time"].Value.ToString();
                    dateTimePickerKortKayıtDüzenleTarih.Value = Convert.ToDateTime(row.Cells["Date"].Value);
                    metrotbxKortKayıtDüzenleOyuncu1Name.Text = row.Cells["Player1"].Value.ToString();
                    metrotbxKortKayıtDüzenleOyuncu2Name.Text = row.Cells["Player2"].Value.ToString();
                    metrotbxKortKayıtDüzenleOyuncu1ID.Text = row.Cells["Player1_Id"].Value.ToString();
                    metrotbxKortKayıtDüzenleOyuncu2ID.Text = row.Cells["Player2_Id"].Value.ToString();
                    metroComboBoxKortKayıtDüzenleAydınlatma.SelectedItem = row.Cells["Light"].Value.ToString();
                    if (row.Cells["Status"].Value == DBNull.Value)
                    {
                        metroComboBoxKortKayıtDüzenleKayıtTürü.SelectedItem = null;
                    }
                    else
                    {
                        metroComboBoxKortKayıtDüzenleKayıtTürü.SelectedItem = row.Cells["Status"].Value.ToString();
                    }
                    richtbxKortKayıtDüzenleKortNotu.Text = row.Cells["Notes"].Value.ToString();

                }
            }
            catch (Exception)
            {


            }
        }

        private void metroGridKortKayıtDüzenle_SelectionChanged(object sender, EventArgs e)
        {
            KortKayıtDüzenleShowOnControls();
        }

        private void metroTileKortKayıt_Click_1(object sender, EventArgs e)
        {
            metroTileKortKayıt.Enabled = false;
            FixKayıtKayıt();
            metroTileKortKayıt.Enabled = true;
        }

        private void metroTileKortKayıtDüzenleKaydet_Click(object sender, EventArgs e)
        {
            KortKayıtUpdate();
        }

        private void metroTileFixKayıtKayıtKayıtYap_Click_1(object sender, EventArgs e)
        {

        }
    }


}

