using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaisUmaTentativaClient
{
    public partial class Contagem : Form
    {
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private DateTime dtFimEvento;
        private DateTime dataConstrutor;
        private DateTime dataAviso5Minutos;
        private DateTime dataAviso3Minutos;
        private DateTime dataAviso1Minuto;
        private bool notificado5Minutos = false;
        private bool notificado3Minutos = false;
        private bool notificado1Minuto = false;

        public Contagem(DateTime data)
        {
            InitializeComponent();
            dtFimEvento = data;
            this.dataConstrutor = data;
            //tempoExtra = data.AddMinutes(3);

            dataAviso5Minutos = dtFimEvento.AddMinutes(-5);
            dataAviso3Minutos = dtFimEvento.AddMinutes(-3);
            dataAviso1Minuto = dtFimEvento.AddMinutes(-1);

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width, Screen.PrimaryScreen.WorkingArea.Height - this.Height);
        }

        
        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (!notificado5Minutos)
            {
                if (DateTime.Now >= dataAviso5Minutos)
                {
                    notifyIcon1.Icon = this.Icon;
                    notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    notifyIcon1.BalloonTipText = "Atenção, sua sessão será encerrada em menos de 5 minutos!";
                    notifyIcon1.ShowBalloonTip(20);

                    notificado5Minutos = true;
                }
            }

            if (!notificado3Minutos)
            {
                if (DateTime.Now >= dataAviso3Minutos)
                {
                    notifyIcon1.Icon = this.Icon;
                    notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    notifyIcon1.BalloonTipText = "Atenção, sua sessão será encerrada em menos de 3 minutos!";
                    notifyIcon1.ShowBalloonTip(20);

                    notificado3Minutos = true;
                }
            }

            if (!notificado1Minuto)
            {
                if (DateTime.Now >= dataAviso1Minuto)
                {
                    notifyIcon1.Icon = this.Icon;
                    notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    notifyIcon1.BalloonTipText = "Salve seus arquivos, sua sessão será encerrada em menos de 1 minuto!";
                    notifyIcon1.ShowBalloonTip(20);

                    notificado1Minuto = true;
                }
            }

            //Se alcançou a data selecionada
            if (dtFimEvento <= DateTime.Now)
            {
                timer1.Enabled = false;
                notifyIcon1.Icon = this.Icon;
                notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                notifyIcon1.BalloonTipText = "Sua sessão está sendo encerrada!";
                notifyIcon1.ShowBalloonTip(10);

                await Task.Delay(10000);

                Form1.InputOutput($"Contagem finalizada: {Form1.obterIP().ToString()}");

                //WindowsLogOff();
                LockWorkStation();

                var processes = Process.GetProcessesByName("MaisUmaTentativaClient");
                foreach (var item in processes)
                {
                    item.Kill();
                }

            }
            //Se não alcançou, atualiza os labels
            else
            {
               
                TimeSpan tsTempoRestante = dtFimEvento.Subtract(DateTime.Now);

                if (Form1.InserirTempoExtra)
                {
                    DateTime novoDtFimEvento = dtFimEvento.AddMinutes(Form1.TempoExtra);

                    dtFimEvento = new DateTime();
                    dtFimEvento = novoDtFimEvento;

                    dataAviso5Minutos = dtFimEvento.AddMinutes(-5);
                    dataAviso3Minutos = dtFimEvento.AddMinutes(-3);
                    dataAviso1Minuto = dtFimEvento.AddMinutes(-1);

                    notificado5Minutos = false;
                    notificado3Minutos = false;
                    notificado1Minuto = false;

                    Form1.InserirTempoExtra = false;
                }

                lblDias.Text = tsTempoRestante.Days.ToString();
                lblHoras.Text = tsTempoRestante.Hours.ToString();
                lblMinutos.Text = tsTempoRestante.Minutes.ToString();
                lblSegundos.Text = tsTempoRestante.Seconds.ToString();
            }
        }

        public static bool WindowsLogOff()
        {
            return ExitWindowsEx(0 | 0x00000004, 0);
        }

        private void Contagem_Load(object sender, EventArgs e)
        {

        }



    }
}
