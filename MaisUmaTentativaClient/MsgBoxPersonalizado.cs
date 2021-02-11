using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaisUmaTentativaClient
{
    public partial class MsgBoxPersonalizado : Form
    {
        public MsgBoxPersonalizado()
        {
            InitializeComponent();
        }

        public DialogResult Resultado { get; private set; }

        public static DialogResult Mostrar(string mensagem, string textoSim, string textoNao)
        {
            var msgBox = new MsgBoxPersonalizado();
            msgBox.lblMensagem.Text = mensagem;
            //msgBox.btnSim.Text = textoSim;
            //msgBox.btnNao.Text = textoNao;
            //msgBox.ShowDialog();
            return msgBox.Resultado;
        }


        private void MsgBoxPersonalizado_Load(object sender, EventArgs e)
        {

        }
    }
}
