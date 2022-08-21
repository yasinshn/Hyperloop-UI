using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using LineSeries = LiveCharts.Wpf.LineSeries;
using LiveCharts;
using LiveCharts.Defaults;
using System.Globalization;
using System.IO;

namespace hyperloop_arayuz_V2._1
{
    public partial class Form1 : Form
    {
        string ip = "192.168.1.7";
        string clientIp = "192.168.9.102";
        int port = 5454;

        string previousStr = "45";

        UDPSocket server;

        string yawStr, pitchStr, rollStr, speedXStr,speedYStr, speedZStr, navXStr, navYStr, navZStr, 
        accelXStr, accelYStr, accelZStr,tempStr, pressureStr, batStr, levStr,currentStr, tmp1, tmp2;
        int indexData = 15;
        long graphCounter = 0, prevCounter = 0;
        Random rnd = new Random();

        // IMU
        List<double> IMUXValues = new List<double>();
        LineSeries seriesIMUX = new LiveCharts.Wpf.LineSeries(){};

        List<double> IMUYValues = new List<double>();
        LineSeries seriesIMUY = new LiveCharts.Wpf.LineSeries() { };

        List<double> IMUZValues = new List<double>();
        LineSeries seriesIMUZ = new LiveCharts.Wpf.LineSeries() { };

        // SPEED
        List<double> SpeedXValues = new List<double>();
        LineSeries seriesSpeedX = new LiveCharts.Wpf.LineSeries() { };

        List<double> SpeedYValues = new List<double>();
        LineSeries seriesSpeedY = new LiveCharts.Wpf.LineSeries() { };

        List<double> SpeedZValues = new List<double>();
        LineSeries seriesSpeedZ = new LiveCharts.Wpf.LineSeries() { };
        
        // ACCEL
        List<double> AccelXValues = new List<double>();
        LineSeries seriesAccelX = new LiveCharts.Wpf.LineSeries() { };

        List<double> AccelYValues = new List<double>();
        LineSeries seriesAccelY = new LiveCharts.Wpf.LineSeries() { };

        List<double> AccelZValues = new List<double>();
        LineSeries seriesAccelZ = new LiveCharts.Wpf.LineSeries() { };

        TextWriter txt = new StreamWriter(@"FileLocation\log.txt");

        private void button_close_Click(object sender, EventArgs e)
        {
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  Kapat Butonuna Tıklandı.\n");
            txt.Close();
            this.Close();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  BAĞLAN Butonuna Tıklandı.\n");
            Thread thr = new Thread(new ThreadStart(serverStart));
            thr.Start();
            timer2.Enabled = true;
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  BAŞLAT Butonuna Tıklandı.\n");
           
                server.Send("A", clientIp, port);
           
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  DURDUR Butonuna Tıklandı.\n");
            try
            {
                server.Send("S", clientIp, port);
            }
            catch(Exception ex)
            {
                txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  Durdur Komutu Hata Mesajı: {0}\n",  ex.ToString());
            }
            timer1.Enabled = false;
        }

        private void button_shutDown_Click(object sender, EventArgs e)
        {
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  GÜCÜ KES Butonuna Tıklandı.\n");
            try
            {
                server.Send("Q", clientIp, port);
            }
            catch(Exception ex)
            {
                txt.WriteLine(DateTime.Now.ToString("HH:mm:ss")+"  Gücü Kes Hata Mesajı: {0}", ex.ToString());
                
            }
            timer1.Enabled = false;
           
        }

        private void button_saveRaspIp_Click(object sender, EventArgs e)
        {
            clientIp = textBox_rasp.Text;
            textBox_rasp.Clear();
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  KAYDET Butonuna Tıklandı.\n");
            ip = textBox1.Text;
            textBox1.Clear();
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        ///
        /// Handling the window messages
        ///
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;
        }

        public Form1()
        {
            InitializeComponent();

            timer1.Interval = 1000;
            timer1.Enabled = false;
            
            timer2.Interval = 500;
            timer2.Enabled = false;
            //timer1.Start();
        }

        private void circularGauge_speed_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  Program Başlatıldı.\n");
            //label1.Visible = false;

            pictureBox_wifi.BackColor = Color.Red;

            /*
            int[] data = { 23, 69, 12, 17, 10 };

            var series1 = new LiveCharts.Wpf.LineSeries()
            {
                Title = "DIZI",
                Values = new LiveCharts.ChartValues<int>(data),
            };
            */
            
            cartesianChart1.Series.Add(seriesIMUX);
            cartesianChart1.Series.Add(seriesIMUY);
            cartesianChart1.Series.Add(seriesIMUZ);

            cartesianChart2.Series.Add(seriesSpeedX);
            cartesianChart2.Series.Add(seriesSpeedY);
            cartesianChart2.Series.Add(seriesSpeedZ);

            cartesianChart3.Series.Add(seriesAccelX);
            cartesianChart3.Series.Add(seriesAccelY);
            cartesianChart3.Series.Add(seriesAccelZ);

            //cartesianChart1.Series.Clear();
            //cartesianChart1.Series.Add(series1);
        }

       
        private void timer1_Tick(object sender, EventArgs e)
        {
           
            graphCounter++;
            label1.Text = Convert.ToString(graphCounter);
            
            IMUXValues.Add(convertData(yawStr));
            IMUYValues.Add(convertData(rollStr));
            IMUZValues.Add(convertData(pitchStr));

            SpeedXValues.Add(convertData(speedXStr));
            SpeedYValues.Add(convertData(speedYStr));
            SpeedZValues.Add(convertData(speedZStr));

            AccelXValues.Add(convertData(accelXStr));
            AccelYValues.Add(convertData(accelYStr));
            AccelZValues.Add(convertData(accelZStr));

            if (graphCounter % 3 == 0)
            {
                seriesIMUX.Values = new LiveCharts.ChartValues<double>(IMUXValues);
                seriesIMUY.Values = new LiveCharts.ChartValues<double>(IMUYValues);
                seriesIMUZ.Values = new LiveCharts.ChartValues<double>(IMUZValues);

                seriesSpeedX.Values = new LiveCharts.ChartValues<double>(SpeedXValues);
                seriesSpeedY.Values = new LiveCharts.ChartValues<double>(SpeedYValues);
                seriesSpeedZ.Values = new LiveCharts.ChartValues<double>(SpeedZValues);

                seriesAccelX.Values = new LiveCharts.ChartValues<double>(AccelXValues);
                seriesAccelY.Values = new LiveCharts.ChartValues<double>(AccelYValues);
                seriesAccelZ.Values = new LiveCharts.ChartValues<double>(AccelZValues);
            }

            //cartesianChart1.Refresh();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                server.Send(".",clientIp,port);
            }
            catch(Exception ex)
            {
                txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  Timer2 Baglanti Hata Mesaji: {0}", ex.ToString());
            }
        }

        public float maxSpeed(float oldSpeed, float newSpeed)
        {
            return (oldSpeed < newSpeed) ? newSpeed : oldSpeed;
        }

        public void serverStart()
        {
            server = new UDPSocket(ip, port);
            int speed = 0;
            pictureBox_wifi.BackColor = Color.Green;
            button_connect.BeginInvoke(new Action(delegate ()
            {
                button_connect.Enabled = false;
            }));

            while (true)
            {
                string receivedMessage = server.Listen();
                if (receivedMessage.Contains('#')){
                    parseData(receivedMessage);
                }
                
                //label1.Text = receivedMessage;
                
                label1.BeginInvoke(new Action(delegate ()
                {
                    label18.Text = receivedMessage;
                }));
                txt.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  Gelen Veri: \n" + receivedMessage);


                //speed = int.Parse(receivedMessage);
                //circularGauge_yaw.Value = speed;

            }

        }

        public void parseData(string data)
        {
            string[] splitData = data.Split('#');    
            for (int i = 0; i < splitData.Length; i++)
            {
                foreach (char c in splitData[i])
                {     if(c != '\0')
                        chooseDataIndex(c);
                }

                switch (indexData)
                {
                    case 0:
                        rollStr = splitData[i];
                        circularGauge_roll.Value = convertData(rollStr);
                        break;

                    case 1:
                        pitchStr = splitData[i];
                        circularGauge_pitch.Value = convertData(pitchStr);
                        break;

                    case 2:
                        yawStr = splitData[i];
                        circularGauge_yaw.Value = convertData(yawStr);
                        break;

                    case 3:
                        speedXStr = splitData[i];
                        circularGauge_speed.Value = convertData(speedXStr);
                        label_speedX.BeginInvoke(new Action(delegate ()
                        {
                            label_speedX.Text = speedXStr.Substring(0, speedXStr.Length - 1);
                        }));
                        break;

                    case 4:
                        speedYStr = splitData[i];
                        label_speedY.BeginInvoke(new Action(delegate ()
                        {
                            label_speedY.Text = speedYStr.Substring(0, speedYStr.Length - 1);
                        }));
                        break;

                    
                    case 5:
                        speedZStr = splitData[i];
                        label_speedZ.BeginInvoke(new Action(delegate ()
                        {
                            label_speedZ.Text = speedZStr.Substring(0, speedZStr.Length - 1);
                        }));
                        break;

                    case 6:
                        navXStr = splitData[i];
                        linearGauge_nav.Value = convertData(navXStr);
                        label_navX.BeginInvoke(new Action(delegate ()
                        {
                            label_navX.Text = navXStr.Substring(0, navXStr.Length - 1);
                        }));
                        break;

                    case 7:
                        navYStr = splitData[i];
                        label_navY.BeginInvoke(new Action(delegate ()
                        {
                            label_navY.Text = navYStr.Substring(0, navYStr.Length - 1);
                        }));
                        break;

                    case 8:
                        navZStr = splitData[i];
                        label_navZ.BeginInvoke(new Action(delegate ()
                        {
                            label_navZ.Text = navZStr.Substring(0, navZStr.Length - 1);
                        }));

                        break;

                    case 9:
                        accelXStr = splitData[i];
                       label_accelX.BeginInvoke(new Action(delegate ()
                        {
                            label_accelX.Text = accelXStr.Substring(0, accelXStr.Length - 1);
                        }));
                        break;

                    case 10:
                        accelYStr = splitData[i];
                        label_accelY.BeginInvoke(new Action(delegate ()
                        {
                            label_accelY.Text = accelYStr.Substring(0, accelYStr.Length - 1);
                        }));
                        break;

                    case 11:
                        accelZStr = splitData[i];
                        label_accelZ.BeginInvoke(new Action(delegate ()
                        {
                            label_accelZ.Text = accelZStr.Substring(0, accelZStr.Length - 1);
                        }));
                        break;

                    case 12:
                        tempStr = splitData[i];
                        linearGauge_Temp.Value = convertData(tempStr);
                        break;

                    case 13:
                        pressureStr = splitData[i];
                        linearGauge_pressure.Value = convertData(pressureStr);
                        break;
                    case 14:
                        levStr = splitData[i];
                        linearGauge_lev.Value = convertData(levStr);
                        break;
                    case 15:
                        currentStr = splitData[i];
                        linearGauge_bat.Value = convertData(currentStr);
                        break;
                    case 16:
                        tmp1 = splitData[i];
                        linearGauge_temp1.Value = convertData(tmp1);
                        break;
                    case 17:
                        tmp2 = splitData[i];
                        linearGauge_temp2.Value = convertData(tmp2);
                        break;
                    default:
                        break;
                }
            }


        }

        public void chooseDataIndex(char c)
        {
            switch (c)
            {
                case 'r':
                    indexData = 0;
                    break;
                case 'p':
                    indexData = 1;
                    break;
                case 'y':
                    indexData = 2;
                    break;
                case 'X':
                    indexData = 3;
                    break;
                case 'Y':
                    indexData = 4;
                    break;
                case 'Z':
                    indexData = 5;
                    break;
                case 'u':
                    indexData = 6;
                    break;
                case 'v':
                    indexData = 7;
                    break;
                case 'w':
                    indexData = 8;
                    break;
                case 'i':
                    indexData = 9;
                    break;
                case 'j':
                    indexData = 10;
                    break;
                case 'k':
                    indexData = 11;
                    break;
                case 'T':
                    indexData = 12;
                    break;
                case 'P':
                    indexData = 13;
                    break;
                case 'd':
                    indexData = 14;
                    break;
                case 'C':
                    indexData = 15;
                    break;
                case 'g':
                    indexData = 16;
                    break;
                case 'G':
                    indexData = 17;
                    break;
                default:
                    indexData = 25;
                    break;
            }
        }

        public double convertData(string data)
        {
            string subStr;
            if (data != null)
            {
               subStr  = data.Substring(0, data.Length - 1);
               previousStr = subStr;
            }
            else
            {
                subStr = previousStr;
            }
           
           decimal d = decimal.Parse(subStr, CultureInfo.InvariantCulture);
           double num = (double)d;
           return num;

        }


        //Soket Class
        public class UDPSocket
        {

            Socket _socket;
            const int BufferSize = 1024;

            public const int SIO_UDP_CONNRESET = -1744830452;

            public UDPSocket(string ipAddress, int portNum)
            {
                _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                _socket.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null);
                IPAddress parsedIpAddress = IPAddress.Parse(ipAddress);
                IPEndPoint localEndPoint = new IPEndPoint(parsedIpAddress, portNum);
                _socket.Bind(localEndPoint);
            }

            public string Listen()
            {
                byte[] receivedBytes = new byte[BufferSize];
                _socket.Receive(receivedBytes);

                // Log received message to user
                string receivedMessage = Encoding.ASCII.GetString(receivedBytes);
                return receivedMessage;
            }

            public void Send(string messageToSend, string ipAddress, int portNum)
            {
                IPAddress serverIPAddress = IPAddress.Parse(ipAddress);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, portNum);
                byte[] bytesToSend = Encoding.ASCII.GetBytes(messageToSend);
                _socket.SendTo(bytesToSend, serverEndPoint);
            }


            public string Echo()
            {
                byte[] receivedBytes = new byte[BufferSize];
                EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                _socket.ReceiveFrom(receivedBytes, ref clientEndPoint);

                // Log received message to the user
                string receivedMessage = Encoding.ASCII.GetString(receivedBytes);

                // Echo received message
                _socket.SendTo(receivedBytes, clientEndPoint);

                return receivedMessage;
            }
        }
    }
   
}

