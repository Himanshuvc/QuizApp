using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Qapp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int seconds = 30;
        DataTable dt = new DataTable();
        DispatcherTimer timer = new DispatcherTimer();
        int totalQue, startque = 0;
        SerialPort _serialPort1, _serialPort2, _serialPort3, _serialPort4, _serialPort5;
        string connectionString = "server=localhost;database=dbquiz;uid=root;pwd=;";
        int deskid = 0;
        int queid, ansid = 0;
        int batchid = 0;
        int totportsopen = 0;

        private delegate void SetTextDeleg(string text);

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            connectms();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            TimeSpan span = new TimeSpan(0, 0, 0, 5, 0);
            //  TypewriteTextblock("Hello World how are t kjfsfjsadfjsljfdslkjfgslgkj", tnn, span);
            if (dt != null && dt.Rows.Count > 0)
            {
                totalQue = dt.Rows.Count;
                txtQue.Text = dt.Rows[startque]["Que"].ToString();
                txtans1.Text = dt.Rows[startque]["ans1"].ToString();
                txtans2.Text = dt.Rows[startque]["ans2"].ToString();
                queid = Convert.ToInt32(dt.Rows[startque]["id"]);
            }
            batchid = getbatch();
            if (batchid == 0)
                batchid++;
        }

        private int getbatch()
        {
            using (MySqlConnection cn = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT max(batchid) as Batchid FROM usersanswer;";
                    cn.Open();
                    DataTable dtbatch = new DataTable();
                    using (MySqlCommand command = new MySqlCommand(query, cn))
                    {
                        //MySqlDataReader Reader = command.ExecuteReader();

                        MySqlDataAdapter returnVal = new MySqlDataAdapter(query, cn);
                        //DataTable dt = new DataTable();
                        returnVal.Fill(dtbatch);
                        cn.Close();
                    }
                    if (dtbatch != null && dtbatch.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(dtbatch.Rows[0][0].ToString()))
                        {
                            return 0;
                        }

                        return (Convert.ToInt32(dtbatch.Rows[0][0])+1);
                    }
                    else
                        return 0;


                }
                catch (MySqlException ex)
                {

                    MessageBox.Show("Error in adding mysql row. Error: " + ex.Message);
                    return 0;
                }
            }
        }

        //private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    Thread.Sleep(500);
        //    string data = _serialPort.ReadLine();


        //    txtQue.Text += data.ToString();
        //    //SetTextDeleg setTextDeleg=new SetTextDeleg(data);
        //    //this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        //    //BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        //    //SetTextDeleg myLogger = new SetTextDeleg(si_DataReceived);
        //}

        private void si_DataReceived(string data)
        {
            if (data == "1")
                imgbench1.Source = new BitmapImage(new Uri("right.png", UriKind.Relative));
            else
                imgbench1.Source = new BitmapImage(new Uri("wrong.png", UriKind.Relative));
            // if (data.Contains(":"))
            //      textBox2.Text += data.ToString();
            //else
            // textBox1.Text = data.Trim();
        }

        private void connectms()
        {

            using (MySqlConnection cn = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT ID,Que,Ans1,Ans2 FROM tblqueans ORDER BY RAND() LIMIT 10;";
                    cn.Open();

                    using (MySqlCommand command = new MySqlCommand(query, cn))
                    {
                        //MySqlDataReader Reader = command.ExecuteReader();

                        MySqlDataAdapter returnVal = new MySqlDataAdapter(query, cn);
                        //DataTable dt = new DataTable();
                        returnVal.Fill(dt);
                        cn.Close();
                    }


                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error in adding mysql row. Error: " + ex.Message);
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            tbSec.Text = (seconds--).ToString();
            if (seconds < 0)
            {
                timer.Stop();
                nextQuestion();

                seconds = 30;

                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    txtQue.Text = dt.Rows[0]["Que"].ToString();
                //    txtans1.Text = dt.Rows[0]["ans1"].ToString();
                //    txtans2.Text = dt.Rows[0]["ans2"].ToString();
                //}

            }
        }

        private void nextQuestion()
        {
            startque++;
            if (totalQue == startque)
            {
                //New Batch
                timer.Stop();
                batchid++;
            }
            else
            {
                TimeSpan span = new TimeSpan(0, 0, 0, 2, 0);
                queid = Convert.ToInt32(dt.Rows[startque]["id"]);
                TypewriteTextblock(dt.Rows[startque]["Que"].ToString(), txtQue, span);
                System.Threading.Thread.Sleep(5000);
                imgbench1.Source = new BitmapImage(new Uri("default.png", UriKind.Relative));
                imgbench2.Source = new BitmapImage(new Uri("default.png", UriKind.Relative));
                imgbench3.Source = new BitmapImage(new Uri("default.png", UriKind.Relative));
                imgbench4.Source = new BitmapImage(new Uri("default.png", UriKind.Relative));

                //System.Threading.Thread.Sleep(5000);
                resetallport();


                timer.Start();
            }


        }

        private void resetallport()
        {
            switch (totportsopen)
            {
                case 2:
                    _serialPort1.Write("RESET\n");
                    break;
                case 3:
                    _serialPort1.Write("RESET\n");
                    _serialPort2.Write("RESET\n");
                    break;
                case 4:
                    _serialPort1.Write("RESET\n");
                    _serialPort2.Write("RESET\n");
                    _serialPort3.Write("RESET\n");
                    break;
                case 5:
                    _serialPort1.Write("RESET\n");
                    _serialPort2.Write("RESET\n");
                    _serialPort3.Write("RESET\n");
                    _serialPort4.Write("RESET\n");
                    break;
            }
            //_serialPort1.Write("RESET\n");
            //_serialPort2.Write("RESET\n");
            //_serialPort3.Write("RESET\n");
            //_serialPort4.Write("RESET\n");
        }

        private void TypewriteTextblock(string textToAnimate, TextBlock txt, TimeSpan timeSpan)
        {
            Storyboard story = new Storyboard();
            story.FillBehavior = FillBehavior.HoldEnd;
            //story.RepeatBehavior = RepeatBehavior.Forever;

            DiscreteStringKeyFrame discreteStringKeyFrame;
            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames();
            stringAnimationUsingKeyFrames.Duration = new Duration(timeSpan);

            string tmp = string.Empty;
            foreach (char c in textToAnimate)
            {
                discreteStringKeyFrame = new DiscreteStringKeyFrame();
                discreteStringKeyFrame.KeyTime = KeyTime.Paced;
                tmp += c;
                discreteStringKeyFrame.Value = tmp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }
            Storyboard.SetTargetName(stringAnimationUsingKeyFrames, txt.Name);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            story.Children.Add(stringAnimationUsingKeyFrames);

            story.Begin(txt);
            //story.Stop(txt);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = 1;

                string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                totportsopen = ports.Length;
                if (ports.Contains("COM1"))
                {
                    switch (ports.Length)
                    {
                        case 2:
                            _serialPort1 = new SerialPort(ports[1].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort1.Handshake = Handshake.None;
                            _serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived1);
                            _serialPort1.ReadTimeout = 500;
                            _serialPort1.WriteTimeout = 500;
                            _serialPort1.Open();

                            break;
                        case 3:
                            _serialPort1 = new SerialPort(ports[1].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort2 = new SerialPort(ports[2].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort1.Handshake = Handshake.None;
                            _serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived1);
                            _serialPort1.ReadTimeout = 500;
                            _serialPort1.WriteTimeout = 500;
                            _serialPort1.Open();

                            _serialPort2.Handshake = Handshake.None;
                            _serialPort2.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived2);
                            _serialPort2.ReadTimeout = 500;
                            _serialPort2.WriteTimeout = 500;
                            _serialPort2.Open();
                            break;
                        case 4:
                            _serialPort1 = new SerialPort(ports[1].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort2 = new SerialPort(ports[2].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort3 = new SerialPort(ports[3].ToString(), 9600, Parity.None, 8, StopBits.One);

                            _serialPort1.Handshake = Handshake.None;
                            _serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived1);
                            _serialPort1.ReadTimeout = 500;
                            _serialPort1.WriteTimeout = 500;
                            _serialPort1.Open();

                            _serialPort2.Handshake = Handshake.None;
                            _serialPort2.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived2);
                            _serialPort2.ReadTimeout = 500;
                            _serialPort2.WriteTimeout = 500;
                            _serialPort2.Open();

                            _serialPort3.Handshake = Handshake.None;
                            _serialPort3.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived3);
                            _serialPort3.ReadTimeout = 500;
                            _serialPort3.WriteTimeout = 500;
                            _serialPort3.Open();
                            break;
                        case 5:
                            _serialPort1 = new SerialPort(ports[1].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort2 = new SerialPort(ports[2].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort3 = new SerialPort(ports[3].ToString(), 9600, Parity.None, 8, StopBits.One);
                            _serialPort4 = new SerialPort(ports[4].ToString(), 9600, Parity.None, 8, StopBits.One);

                            _serialPort1.Handshake = Handshake.None;
                            _serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived1);
                            _serialPort1.ReadTimeout = 500;
                            _serialPort1.WriteTimeout = 500;
                            _serialPort1.Open();

                            _serialPort2.Handshake = Handshake.None;
                            _serialPort2.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived2);
                            _serialPort2.ReadTimeout = 500;
                            _serialPort2.WriteTimeout = 500;
                            _serialPort2.Open();

                            _serialPort3.Handshake = Handshake.None;
                            _serialPort3.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived3);
                            _serialPort3.ReadTimeout = 500;
                            _serialPort3.WriteTimeout = 500;
                            _serialPort3.Open();

                            _serialPort4.Handshake = Handshake.None;
                            _serialPort4.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived4);
                            _serialPort4.ReadTimeout = 500;
                            _serialPort4.WriteTimeout = 500;
                            _serialPort4.Open();
                            break;

                    }


                }




            }
            catch (Exception ex)
            {

            }
        }

        private void sp_DataReceived4(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort4.ReadLine();
            string[] ans = data.Split(':');
            if (ans[0] == "RESET ALL\r")
            {
                return;
            }
            if (ans[1] == "TRUE\r")
            {
                ansid = 1;
            }
            else
                ansid = 0;
            int iscorrect = iscorrectans(queid, ansid);

            Entryindb(data, 4, queid, ansid, iscorrect, batchid, ans[0]);
        }

        private void Entryindb(string data, int deskid, int queid, int ansid, int iscorrect, int batchid, string devicekey)
        {
            using (MySqlConnection cn = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO usersanswer(Deskid,queid,ansid,iscorrect,batchid,deviceid) VALUES (?Deskid,?queid,?ansid,?iscorrect,?batchid,?deviceid);";
                cn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, cn))
                {
                    cmd.Parameters.Add("?Deskid", MySqlDbType.Int32).Value = deskid;
                    cmd.Parameters.Add("?queid", MySqlDbType.Int32).Value = queid;
                    cmd.Parameters.Add("?ansid", MySqlDbType.Int32).Value = ansid;
                    cmd.Parameters.Add("?iscorrect", MySqlDbType.Int32).Value = iscorrect;
                    cmd.Parameters.Add("?batchid", MySqlDbType.Int32).Value = batchid;
                    cmd.Parameters.Add("?deviceid", MySqlDbType.Text).Value = devicekey;
                    //cmd.Parameters.Add("?SignatureImg", MySqlDbType.Blob).Value = btnImage;
                    cmd.ExecuteNonQuery();
                    // recordID = cmd.LastInsertedId;
                }
            }
        }

        private int iscorrectans(int queid, int ansid)
        {
            using (MySqlConnection cn = new MySqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT correctans FROM tblqueans where id=" + queid + ";";
                    cn.Open();
                    DataTable dtcorrect = new DataTable();
                    using (MySqlCommand command = new MySqlCommand(query, cn))
                    {
                        //MySqlDataReader Reader = command.ExecuteReader();

                        MySqlDataAdapter returnVal = new MySqlDataAdapter(query, cn);
                        //DataTable dt = new DataTable();
                        returnVal.Fill(dtcorrect);
                        cn.Close();
                    }
                    if (dtcorrect != null && dtcorrect.Rows.Count > 0)
                    {
                        if (ansid == Convert.ToInt32(dtcorrect.Rows[0]["correctans"]))
                        {
                            return 1;
                        }
                        else
                            return 0;
                    }

                }
                catch (MySqlException ex)
                {

                    MessageBox.Show("Error in adding mysql row. Error: " + ex.Message);
                    return -1;
                }
                return -1;
            }
        }

        private void sp_DataReceived3(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort3.ReadLine();
            string[] ans = data.Split(':');
            if (ans[0] == "RESET ALL\r")
            {
                return;
            }
            if (ans[1] == "TRUE\r")
            {
                ansid = 1;
            }
            else
                ansid = 0;
            int iscorrect = iscorrectans(queid, ansid);

            Entryindb(data, 3, queid, ansid, iscorrect, batchid, ans[0]);


        }

        private void sp_DataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort2.ReadLine();
            string[] ans = data.Split(':');
            if (ans[0] == "RESET ALL\r")
            {
                return;
            }
            if (ans[1] == "TRUE\r")
            {
                ansid = 1;
            }
            else
                ansid = 0;
            int iscorrect = iscorrectans(queid, ansid);

            Entryindb(data, 2, queid, ansid, iscorrect, batchid, ans[0]);


        }

        private void sp_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);
            string data = _serialPort1.ReadLine();
            string[] ans = data.Split(':');

            if (ans[0] == "RESET ALL\r")
            {
                return;
            }
            if (ans[1] == "TRUE\r")
            {
                ansid = 1;
            }
            else
                ansid = 0;
            int iscorrect = iscorrectans(queid, ansid);

            Entryindb(data, 1, queid, ansid, iscorrect, batchid, ans[0]);

            if (iscorrect == 1)
            {

                this.Dispatcher.Invoke(new SetTextDeleg(si_DataReceived), new object[] { "1" });

            }
            else
                this.Dispatcher.Invoke(new SetTextDeleg(si_DataReceived), new object[] { "0" });


        }



        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
