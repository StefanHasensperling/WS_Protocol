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

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            try
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
                await Client.ConnectAsync();
                groupBoxRequest.Enabled = true;

            }
            catch (Exception ex)
            {
                Client = null;
                buttonConnect.Enabled = true;
                MessageBox.Show(ex.Message);
            }
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

        private async void buttonExecute_Click(object sender, EventArgs e)
        {          
            var Command = comboBoxCmd.SelectedItem.ToString();
            var DataType = comboBoxDataType.SelectedItem.ToString();
            var TagID = textBoxTagId.Text;
            var TagValue = textBoxTagValue.Text;

            try
            {
                switch (Command)
                {
                    case "Read Single Value":
                        switch (DataType)
                        {
                            case "Integer":
                                textBoxResponse.Text = (await Client.ReadSingleValueAsIntAsync(uint.Parse(TagID))).ToString();
                                break;
                            case "Floating Point":
                                textBoxResponse.Text = (await Client.ReadSingleValueAsRealAsync(uint.Parse(TagID))).ToString();
                                break;
                            default:
                                throw new ApplicationException("Invalid data type selected");
                        }
                        break;

                    case "Read String Value":
                        textBoxResponse.Text = (await Client.ReadSingleStringAsync(uint.Parse(TagID))).ToString();
                        break;

                    case "Write Single Value":
                        switch (DataType)
                        {
                            case "Integer":
                                await Client.WriteSingleValueAsync(uint.Parse(TagID), int.Parse(TagValue));
                                textBoxResponse.Text = "Value successfully written";
                                break;
                            case "Floating Point":
                                await Client.WriteSingleValueAsync(uint.Parse(TagID), double.Parse(TagValue));
                                textBoxResponse.Text = "Value successfully written";
                                break;
                            default:
                                throw new ApplicationException("Invalid data type selected");
                        }
                        break;
                    case "Write String Value":
                        await Client.WriteSingleStringAsync(uint.Parse(TagID), TagValue);
                        textBoxResponse.Text = "Value successfully written";
                        break;

                    default:
                        throw new ApplicationException("Invalid Command Selected");
                }
            }
            catch (Exception ex)
            {
                textBoxResponse.Text = ex.Message;
            }
        }
    }
}
