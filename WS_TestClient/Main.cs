using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WS_TestClient
{
    /// <summary>
    /// See backgroundWorkerRunCommand_DoWork for the bulk of the communication with the WS CLient
    /// </summary>
    public partial class Main : Form
    {
        private WS_Protocol.Client.WS_TcpClient Client;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            comboBoxCmd.SelectedIndex = 0;
            comboBoxDataType.SelectedIndex = 0;
            groupBoxRequest.Enabled = false;
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Client = new WS_Protocol.Client.WS_TcpClient(textBoxIP.Text, (int)numericUpDownPort.Value);
            Client.ConnectionBroken += (a, b) => 
            {
                this.BeginInvoke(new Action(() => 
                {
                    MessageBox.Show("Connection was interrupted with Client");
                    Client?.Disconnect();
                    buttonConnect.Enabled = true;
                    groupBoxRequest.Enabled = false;
                })); 
            };

            buttonConnect.Enabled = false;
            backgroundWorkerConnect.RunWorkerAsync();

        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                Client?.Disconnect();
                buttonConnect.Enabled = true;
                groupBoxRequest.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {          
            backgroundWorkerRunCommand.RunWorkerAsync(new Tuple<string, string, string, string>(comboBoxCmd.SelectedItem.ToString(), 
                                                                                                comboBoxDataType.SelectedItem.ToString(),
                                                                                                textBoxTagId.Text,
                                                                                                textBoxTagValue.Text)) ;
        }

        private void backgroundWorkerConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            Client.Connect();
        }

        private void backgroundWorkerConnect_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Client = null;
                buttonConnect.Enabled = true;
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                groupBoxRequest.Enabled = true;
            }
        }

        private void backgroundWorkerRunCommand_DoWork(object sender, DoWorkEventArgs e)
        {
            var T = (Tuple<string, string, string, string>)e.Argument;
            var Command = T.Item1;
            var DataType = T.Item2;
            var TagID = T.Item3;
            var TagValue = T.Item4;

            switch (Command)
            {
               case "Read Single Value":
                   switch (DataType) 
                    {
                        case "Integer":
                            e.Result = Client.ReadSingleValueAsInt(uint.Parse(TagID));
                            break;
                        case "Floating Point":
                            e.Result = Client.ReadSingleValueAsReal(uint.Parse(TagID));
                            break;
                        default:
                            throw new ApplicationException("Invalid data type selected");
                    }
                    break;

                case "Read String Value":
                    e.Result = Client.ReadSingleString(uint.Parse(TagID));
                    break;

                case "Write Single Value":
                    switch (DataType)
                    {
                        case "Integer":
                            Client.WriteSingleValue(uint.Parse(TagID), int.Parse(TagValue));
                            e.Result = "Value successfully written";
                            break;
                        case "Floating Point":
                            Client.WriteSingleValue(uint.Parse(TagID), double.Parse(TagValue));
                            e.Result = "Value successfully written";
                            break;
                        default:
                            throw new ApplicationException("Invalid data type selected");
                    }
                    break;
                case "Write String Value":
                    Client.WriteSingleString(uint.Parse(TagID), TagValue);
                    e.Result = "Value successfully written";
                    break;

                default:
                    throw new ApplicationException("Invalid Command Selected");
            }
        }

        private void backgroundWorkerRunCommand_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error  != null)
            {
                textBoxResponse.Text = e.Error.Message; 
            }
            else
            {
                textBoxResponse.Text = e.Result.ToString();
            }
        }
    }
}
