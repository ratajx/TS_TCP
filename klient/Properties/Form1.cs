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
using System.IO;
using komunikaty;

namespace zad_42_klient
{
    public partial class Form1 : Form
    {
        private TcpClient klient = null;
        private BinaryReader r = null;
        private BinaryWriter w = null;
        private IPEndPoint ServerAdres = null;
        private bool polaczenie = false;
        private int port;
        private String IP;
        
       

        private void wlacz(String x, int y)
        {
            IP = x;
            klient = new TcpClient();
            klient.NoDelay = true;
           
            try
            {
                try
                {
                    ServerAdres = new IPEndPoint(IPAddress.Parse(x), y);
                }
                catch
                {
                    MessageBox.Show("Adres IP zawiera niepoprawne znaki");
                    
                }

                try
                {
                    klient.Connect(ServerAdres);
                }
                catch (SocketException e)
                {
                    MessageBox.Show(e.Message);
                }


                if (klient.Connected)
                {
                    
                    polaczenie = true;

                    this.Invoke(new Action(() =>
                        {
                            button2.Enabled = true;
                            button1.Enabled = false;
                        }));
                }

                NetworkStream stream = klient.GetStream();
                r = new BinaryReader(stream);
                w = new BinaryWriter(stream);

                if (klient.Connected)
                {
                    if (port != 12345)
                        odbieranie.RunWorkerAsync();
                    else
                        odbierz();
                }

                if (port == 12345)
                {
                    MessageBox.Show("Klient wysyła prośbę rozłącznia się", "Komunikat klienta");
                    Thread.Sleep(1000);

                 napisz(KomunikatySerwera.Rozlacz);
                 rozlacz();
                }
            }
            catch
            {
            }
            
        }
        
        private void wyswietl(RichTextBox o, string tekst)
        {
            this.Invoke(new Action(() =>
            {
                o.Focus();
                o.AppendText(tekst);
                o.ScrollToCaret();
            }));
        }

        private void rozlacz()
        {
            this.Invoke(new Action(() =>
            {
                button1.Enabled = true;
                button2.Enabled = false;
                richTextBox3.AppendText(KomunikatySerwera.Rozlacz + "\n");
            }));
            klient.Close();
            klient = null;

            połączenie.CancelAsync();
            odbieranie.CancelAsync();

        }


    public void napisz(String a)
            {
                BinaryWriter writer = new BinaryWriter(klient.GetStream());
                writer.Write(a);
            }


        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Thread.Sleep(1000);
            połączenie.RunWorkerAsync();
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            napisz(richTextBox1.Text);
        }

        private void połączenie_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                bool jest_liczba = true;
                try
                {
                    Convert.ToInt16(textBox1.Text);
                }
                catch
                {

                    MessageBox.Show("Numer portu zawiera niepoprawne znaki, wprowadź liczbę");
                    jest_liczba = false;
                }
                try
                {
                    textBox1.Text.ToString();
                }
                catch
                {

                    MessageBox.Show("Adres IP niepoprawny");
                    jest_liczba = false;
                }
                if (jest_liczba)
                {
                    port = Convert.ToInt32(textBox1.Text);
                    wlacz(textBox2.Text, Convert.ToInt32(textBox1.Text));
                }
             }
            else
                MessageBox.Show("Podaj dane do połączenia z serwerem");
                
        }

        private void komunikat(String tekst)
        {
            switch (tekst)
            {
                case "#Polaczono#":
                    {
                        this.Invoke(new Action(() => richTextBox3.AppendText("#Połączono# z serwerem port: "+port+"\n")));
                        break;
                    }
                case "#Port#":
                    {
                        this.Invoke(new Action(() => richTextBox3.AppendText("Serwer żąda #zmiany portu# na: ")));
                        break;
                    }
                case "#Rozlaczono#":
                    {
                        this.Invoke(new Action(() => richTextBox3.AppendText("#Rozłączono#\n")));
                        break;
                    }
            }

        }

        private void odbierz()
        {
            string tekst;
            bool pr = true;
            if (polaczenie)
            {
                try
                {
                    while (((tekst = r.ReadString()) != ""))
                    {
                        
                        if (tekst[0] != '#') 
                        {
                            if (pr)
                                wyswietl(this.richTextBox2, tekst + "\n");
                            else
                            {
                                port = Convert.ToInt32(tekst);
                                this.Invoke(new Action(() => richTextBox3.AppendText(tekst + "\n")));
                                pr = true;
                            }
                        }
                        if ((tekst == KomunikatySerwera.OK) || (tekst == KomunikatySerwera.ZmianaPortu)) //|| (r.ReadString() == KomunikatySerwera.OK) || (r.ReadString() == KomunikatySerwera.Rozlacz) || (r.ReadString() == KomunikatySerwera.ZmianaPortu))
                        {
                            komunikat(tekst);
                            if (port == 12345)
                                break;
                            if (tekst == KomunikatySerwera.ZmianaPortu)
                                pr = false;
                        }
                    }
                }
                catch
                {
                    this.Invoke(new Action(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = false;
                        richTextBox3.AppendText("#Rozłączono#\n");
                    }));
                    klient.Close();
                    wlacz(IP, port);

                }

            }
        }
        private void odbieranie_DoWork(object sender, DoWorkEventArgs e)
        {
            odbierz();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (polaczenie)
            {
                if (klient != null) klient.Close();
                polaczenie = false;
            }
            połączenie.CancelAsync();
            odbieranie.CancelAsync();
        }
    }
}
