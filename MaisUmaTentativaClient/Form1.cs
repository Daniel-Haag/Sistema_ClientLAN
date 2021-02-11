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
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.ServiceModel;
using MaisUmaTentativaClient.ServiceReference2;

namespace MaisUmaTentativaClient
{
    public partial class Form1 : Form
    {
        //VERSÃO PARA SUBSEÇÃO: PORTO ALEGRE

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation(); //Tem que mudar isso aqui para fazer um logoff efetivamente e não apenas bloquear a tela.

        string subSecao = "PORTO ALEGRE";

        static TcpClient tcpClient = new TcpClient();

        //Declare and Initialize the IP Adress
        static IPAddress IPServidor;

        //Declarar e inicializar o portnumber
        int PortNumber = 8001;

        public static bool DesligarMaquina = false;
        public static bool EncerrarSessao = false;
        public static bool InserirTempoExtra = false;
        public static int TempoExtra = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;
            EndpointAddress endpoint = new EndpointAddress("https://wsoab.oabrs.org.br/Service.asmx");
            ServiceSoapClient WebService = new ServiceSoapClient(binding, endpoint);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //BasicHttpBinding binding = new BasicHttpBinding();
            //binding.Security.Mode = BasicHttpSecurityMode.None;
            //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            //EndpointAddress endpoint = new EndpointAddress("http://localhost:38115");
            //MaisUmaTentativaClient.ServiceReference1.ServiceSoapClient WebServiceLocal = new MaisUmaTentativaClient.ServiceReference1.ServiceSoapClient(binding, endpoint);
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            IPAddress IPAdressClient = obterIP();
            string IPClient = IPAdressClient.MapToIPv4().ToString();
            string nomeMaquina = Environment.MachineName;

            //O IP que a aplicação cliente deve apontar deve ser descoberto aqui...
            IPServidor = IPAddress.Parse(WebService.wsDescobrirServidorAtualizarDados(subSecao, IPClient, nomeMaquina));            

            if (IPServidor != null)
            {
                //Neste caso o usuário deve ser descoberto pelo próprio software...
                InserirRegistroNoWindows(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", "MaisUmaTentativaClient", "C:\\Users\\dhaag\\Desktop\\MaisUmaTentativaClient\\MaisUmaTentativaClient\\bin\\Debug\\MaisUmaTentativaClient.exe");

                try
                {
                    tcpClient.Connect(IPServidor, PortNumber);
                    //txtStatus.Text += Environment.NewLine + "Conectado!";
                    //txtStatus.Text += Environment.NewLine + "Informe a string que será enviada";
                }
                catch
                {
                    notifyIcon1.Icon = this.Icon;
                    notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    notifyIcon1.BalloonTipText = "Não foi possível conectar-se ao servidor! Esta sessão está sendo encerrada!";
                    notifyIcon1.ShowBalloonTip(10);
                    notifyIcon1.Dispose();

                    await Task.Delay(10000);

                    LockWorkStation();
                    //Contagem.WindowsLogOff();
                    var processes = Process.GetProcessesByName("MaisUmaTentativaClient");
                    foreach (var item in processes)
                    {
                        item.Kill();
                    }
                }

                Thread executarTarefa = new Thread(new ThreadStart(tarefas));
                executarTarefa.Start();

                //txtEnviar.Visible = false;
                //txtEnviar.Text = "Quanto tempo eu tenho?";

                btnEnviar.PerformClick();
            }
            else
            {
                //Servidor não cadastrado para esta subSeção
                notifyIcon1.Icon = this.Icon;
                notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                notifyIcon1.BalloonTipText = "Servidor não encontrado! Esta sessão está sendo encerrada!";
                notifyIcon1.ShowBalloonTip(10);
                
                await Task.Delay(10000);

                LockWorkStation();
                //Contagem.WindowsLogOff();
                var processes = Process.GetProcessesByName("MaisUmaTentativaClient");
                foreach (var item in processes)
                {
                    item.Kill();
                }
            }
        }

        public void tarefas()
        {
            //while (true)
            //{
            //Aqui eu vou programar as perguntas que o cliente vai fazer para o servidor
            //Quais serão as perguntas ?
            //Será definido por numeração:
            //00 Nada
            //01 Encerrar Sessão
            //02 Desligar Máquina
            //03 Inserir Tempo Extra

            int contador = 0;

            while (true)
            {
                //Fazer consulta no banco para saber se devo adicionar mais tempo na sessão...
                //

                contador++;

                using (Form1 formNotificacao = new Form1())
                {
                    //formNotificacao.notifyIcon1.Icon = formNotificacao.Icon;
                    //formNotificacao.notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    //formNotificacao.notifyIcon1.BalloonTipText = $"Método de rotina sendo executado... Contador:{contador}";
                    //formNotificacao.notifyIcon1.ShowBalloonTip(10);


                    //Aqui o client solicita uma tarefa ao servidor
                    string nomeMaquina = Environment.MachineName;

                    InputOutput($"Tarefa: {nomeMaquina}: {obterIP().ToString()}");

                    if (contador == 4)
                    {
                        //InserirTempoExtra = true;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(10));

                    //formNotificacao.Dispose();
                }
            }
            //}
        }

        /// <summary>
        /// Método invocado programaticamente para iniciar a sessão
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEnviar_Click_1(object sender, EventArgs e)
        {
            string nomeMaquina = Environment.MachineName;

            InputOutput($"Obter tempo sessao: {nomeMaquina}: {obterIP().ToString()}");
        }

        /// <summary>
        /// Quando invocado este método trata de enviar e receber informações com o servidor via protocolo TCP/IP
        /// </summary>
        /// <param name="mensagemParaServidor"></param>
        public static async void InputOutput(string mensagemParaServidor)
        {
            try
            {
                
                String str = mensagemParaServidor + "$";//txtEnviar.Text + "$";
                Stream stm = tcpClient.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(str);

                stm.Write(ba, 0, ba.Length);

                byte[] bb = new byte[100];
                int k = stm.Read(bb, 0, 100);

                string resposta = Encoding.ASCII.GetString(bb);

                Regex obtemMinutos = new Regex(@"\d{1,2}|\d{3}");
                Match obtemMinutosMatch = obtemMinutos.Match(resposta);

                Regex obtemMensagemSemCadastro = new Regex(@"IP nao encontrado na base de dados\.");
                Match obtemMensagemSemCadastroMatch = obtemMensagemSemCadastro.Match(resposta);

                Regex obtemMensagemTempoSessao = new Regex(@"Tempo de sessao nao definido para esta maquina\.");
                Match obtemMensagemTempoSessaoMatch = obtemMensagemTempoSessao.Match(resposta);

                Regex obtemMensagemTarefaTempoExtra = new Regex(@"TempoExtra:\d{1,2}");
                Match obtemMensagemTarefaTempoExtraMatch = obtemMensagemTarefaTempoExtra.Match(resposta);

                Form1 formNotificacao = new Form1();

                if (obtemMensagemTarefaTempoExtraMatch.Success)
                {
                    string[] dados = obtemMensagemTarefaTempoExtraMatch.Value.Split(':');

                    InserirTempoExtra = true;
                    TempoExtra = int.Parse(dados[1].Trim());
                }

                if (!InserirTempoExtra)
                {
                    if (obtemMinutosMatch.Success)
                    {
                        string tempoEmMinutos = obtemMinutosMatch.Value;
                        double tempoEmMinutosDouble = 0;

                        //txtStatus.Text += Environment.NewLine + "Resposta do servidor: " + Response;

                        if (double.TryParse(tempoEmMinutos, out tempoEmMinutosDouble))
                        {
                            DateTime now = DateTime.Now;

                            now = now.AddMinutes(tempoEmMinutosDouble);

                            if (now > DateTime.Now)
                            {
                                Contagem contagemForm = new Contagem(now);
                                contagemForm.ShowDialog();
                            }
                        }
                    }
                }

                if (obtemMensagemSemCadastroMatch.Success)
                {
                    formNotificacao.notifyIcon1.Icon = formNotificacao.Icon;
                    formNotificacao.notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    formNotificacao.notifyIcon1.BalloonTipText = "Máquina não cadastrada, sua sessão está sendo encerrada!";
                    formNotificacao.notifyIcon1.ShowBalloonTip(10);

                    await Task.Delay(10000);

                    LockWorkStation();
                    //Contagem.WindowsLogOff();

                    var processes = Process.GetProcessesByName("MaisUmaTentativaClient");
                    foreach (var item in processes)
                    {
                        item.Kill();
                    }
                }

                if (obtemMensagemTempoSessaoMatch.Success)
                {
                    formNotificacao.notifyIcon1.Icon = formNotificacao.Icon;
                    formNotificacao.notifyIcon1.BalloonTipTitle = "Informações sobre sua sessão";
                    formNotificacao.notifyIcon1.BalloonTipText = "Sessão não configurada para esta máquina, encerrando sessão...";
                    formNotificacao.notifyIcon1.ShowBalloonTip(10);

                    await Task.Delay(1000);

                    LockWorkStation();
                    //Contagem.WindowsLogOff();

                    var processes = Process.GetProcessesByName("MaisUmaTentativaClient");
                    foreach (var item in processes)
                    {
                        item.Kill();
                    }
                }
            }

            catch (Exception ex)
            {
                string erro = ex.Message;
            }
        }

        /// <summary>
        /// Método utilizado para obter o IP da máquina, o IP é usado como identificador único da máquina na rede (LAN)
        /// </summary>
        /// <returns></returns>
        public static IPAddress obterIP()
        {
            IPAddress ipAd;
            string IP = string.Empty;
            Regex regex = new Regex(@"(192\.168\.\d{1,3}\.\d{1,3})|(172\.16\.\d{1,3}\.{1,3})|(172\.31\.\d{1,3}\.{1,3})|(172\.16\.\d{1,3}\.{1,3}|(10\.\d{1,3}\.\d{1,3}\.{1,3}))");

            string strHostName = Dns.GetHostName();
            IPHostEntry iPHostEntry = Dns.GetHostEntry(strHostName);

            foreach (var item in iPHostEntry.AddressList)
            {
                Match match = regex.Match(item.ToString());

                if (match.Success)
                {
                    IP = match.Value;
                }
            }

            ipAd = IPAddress.Parse(IP);

            return ipAd;
        }

        /// <summary>
        /// Este método quando invocado insere o software na listagem de programas que serão inicializados no logon do usuário
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        static void InserirRegistroNoWindows(RegistryKey parentKey, String subKey, String valueName, Object value)
        {
            RegistryKey key;
            try
            {
                key = parentKey.OpenSubKey(subKey, true);
                if (key == null)
                    key = parentKey.CreateSubKey(subKey);


                key.SetValue(valueName, value);
            }
            catch (Exception e)
            {
                string erro = e.Message;
            }
        }
    }
}

