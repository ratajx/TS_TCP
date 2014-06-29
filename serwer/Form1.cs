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

namespace TS_42_serwer
{
    public partial class Form1 : Form
    {
        private TcpListener listener = null;
        private TcpClient klient = null;
        private BinaryReader r = null;  
        private BinaryWriter w=null;
        private bool polaczenie=false;
        private bool zmiana_portu = false;
        private bool z = true;
        private int port = 0;
        private int nowy_port = 12345;
        
        
       
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
        }


        private void wlacz(int x)
        {

            listener = new TcpListener(IPAddress.Any, x);
            listener.Start();

            this.Invoke(new Action(() =>
            {
                button1.Enabled = false;
                richTextBox3.AppendText(komunikaty.KomunikatySerwera.On + " port:" + port + "\n");
                richTextBox3.ScrollToCaret();
            }));
            try
            {
                klient = listener.AcceptTcpClient();
            }
            catch (SocketException a)
            {

            }
            if (klient.Connected)
            {
                this.Invoke(new Action(() =>
                {
                    button2.Enabled = true;
                    richTextBox3.ScrollToCaret();
                }));
                polaczenie = true;

            }

            NetworkStream stream = klient.GetStream();
            r = new BinaryReader(stream);
            w = new BinaryWriter(stream);
            this.Invoke(new Action(() =>
               {
                   richTextBox3.AppendText(komunikaty.KomunikatySerwera.OK + " z klientem port: " + x + "\n");
                   Thread.Sleep(1000);
                   napisz(KomunikatySerwera.OK);
                   Thread.Sleep(1000);

               }));


            if ((klient.Connected) && (port != nowy_port))
                odbieranie.RunWorkerAsync();
            else
                odbierz();
            if (z)
                zmiana();

        }
        
        private void zmiana()
        {
            zmiana_portu = true;
            z = false;
            port = nowy_port;
            MessageBox.Show("Nastąpi zmiana portu na port: 12345","Komunikat serwera");
            napisz(KomunikatySerwera.ZmianaPortu);
            napisz(nowy_port.ToString());
            Thread.Sleep(500);

            this.Invoke(new Action(()=> richTextBox3.AppendText("Wysłano żadanie #zmiany portu#\n")));
            rozlacz();
            Thread.Sleep(500);

            wlacz(port);
            
        }

        public void napisz(String a)
            {
                try
                {
                    w.Write(a);
                }
                catch
                {
                    this.Invoke(new Action(() =>
                    {
                        button1.Enabled = false;
                        button2.Enabled = true;
                    }));
                    
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

       
        private void button1_Click(object sender, EventArgs e)
        {
            połączenie.RunWorkerAsync();
        }


        private void połączenie_DoWork(object sender, DoWorkEventArgs e)
        {
            bool port_zmiana = false;
            if (!zmiana_portu)
            {
                bool petla = true;
                
                if (petla)
                {
                    if (textBox1.Text != "")
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
                        if (jest_liczba)
                        {
                            port = Convert.ToInt32(textBox1.Text);
                            wlacz(port);
                            petla = false;
                        }

                    }
                    else
                    {
                        MessageBox.Show("Podaj port");
                    }
                }
            }
            if(port_zmiana)
            wlacz(port);
            port_zmiana = true;
        }

        private void rozlacz()
        {
            
            this.Invoke(new Action(() =>
            {
                button1.Enabled = true;
                button2.Enabled = false;
                richTextBox3.AppendText(KomunikatySerwera.Rozlacz + "\n");
             }));
            listener.Stop();
            listener = null;
            klient.Close();
            klient = null;
            połączenie.CancelAsync();
            odbieranie.CancelAsync();
              
        }

        private void komunikat(String tekst)
        {
            switch (tekst)
            {
                case "#Rozlaczono#":
                    {
                        this.Invoke(new Action(() => richTextBox3.AppendText("Klient rozłączył się\n")));
                        listener.Stop();
                        klient.Close();
                        połączenie.CancelAsync();
                        odbieranie.CancelAsync();
                        this.Invoke(new Action(() =>
                        {
                            button2.Enabled = false;
                            button1.Enabled = true;
                        }));
                        break;
                    }
            }

        }

        private void odbierz()
        {

            string tekst;
            if (polaczenie)
            {
                try
                {
                    while ((tekst = r.ReadString()) != "")
                    {
                        if (tekst[0] != '#')
                        {
                            wyswietl(this.richTextBox1, tekst + '\n');
                        }
                        else
                            komunikat(tekst);
                    }
                }
                catch
                {
                    this.Invoke(new Action(() =>
                    {
                        button1.Enabled = true;
                        button2.Enabled = false;
                    }));
                    

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
                
                listener.Stop();
                if (klient != null) klient.Close();
                polaczenie = false;
            }
           połączenie.CancelAsync();
           odbieranie.CancelAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            napisz(richTextBox2.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void zmiana_poł_DoWork(object sender, DoWorkEventArgs e)
        {
            wlacz(port);
        }
        }
}