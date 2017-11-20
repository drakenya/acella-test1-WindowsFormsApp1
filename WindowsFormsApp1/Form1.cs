using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class mainPanel : Form
    {
        bool isNetworkOnline = false;

        SmartCab cab1;

        public mainPanel()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.cab1 = new SmartCab(1);
        }

        private void NetworkStatus_Click(object sender, EventArgs e)
        {
            // change state of the network
            isNetworkOnline = !isNetworkOnline;

            // if the network is online, start the timer to poll the serial port
            ctiPoller.Enabled = isNetworkOnline;

            // update label to know the status of the network
            if (isNetworkOnline)
            {
                networkStatus.Text = "Online";
            }
            else
            {
                networkStatus.Text = "Offline";
            }
        }

        private void updateCabStatus(SmartCab cab)
        {
            byte[] command = cab.GenerateCommand();
            StringBuilder hex = new StringBuilder(command.Length * 2);
            foreach (byte b in command)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            networkMessage.Text = hex.ToString();
            networkMessage.Text = command[3].ToString() + '-' + Convert.ToString(command[4], 2).PadLeft(8, '0');
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.cab1.ToggleBrake();
            this.updateCabStatus(this.cab1);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            this.cab1.ToggleDirection();
            this.updateCabStatus(this.cab1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.cab1.SetInertia((byte) trackBar1.Value);
            this.updateCabStatus(this.cab1);
        }
        
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            this.cab1.SetSpeed((byte) trackBar2.Value);
            this.updateCabStatus(this.cab1);
        }
    }

    interface IAcellaCommand
    {
        byte[] GenerateCommand();
    }

    class SmartCab : IAcellaCommand
    {
        enum Directions { Forward, Reverse };
        enum Brakes { Off, On };
        enum IdleVoltages { DoNotApply, Apply };

        byte index;
        byte speed;
        byte direction;
        byte brake;
        byte inertia;
        byte idleVoltage;

        public SmartCab(byte index)
        {
            this.index = index;
            this.speed = 0;
            this.direction = (byte)Directions.Forward;
            this.brake = (byte)Brakes.On;
            this.inertia = 0;
            this.idleVoltage = (byte)IdleVoltages.Apply;
        }

        public byte[] GenerateCommand()
        {
            /*
            byte[] command = new byte[5];
            command[0] = Convert.ToByte("0a", 16); // op code
            command[1] = 0;
            command[2] = this.index;
            command[3] = this.speed;
            byte attributes = (byte)(this.inertia + (this.brake * 8) + (this.direction * 16) + (this.idleVoltage * 32));
            command[4] = attributes;
            */

            byte[] command =
            {
                Convert.ToByte("0a", 16), // op code
                0,
                this.index,
                this.speed,
                (byte)(this.inertia + (this.brake * 8) + (this.direction * 16) + (this.idleVoltage * 32))
            };

            return command;
        }

        public void ToggleBrake()
        {
            this.brake = (this.brake == (byte)Brakes.On) ? (byte)Brakes.Off : (byte)Brakes.On;
        }

        public void ToggleDirection()
        {
            this.direction = (this.direction == (byte)Directions.Forward) ? (byte)Directions.Reverse : (byte)Directions.Forward;
        }

        public void SetInertia(byte inertia)
        {
            this.inertia = inertia;
        }

        public void SetSpeed(byte speed)
        {
            if (speed > 100)
            {
                speed = 100;
            }
            this.speed = speed;
        }
    }
}
