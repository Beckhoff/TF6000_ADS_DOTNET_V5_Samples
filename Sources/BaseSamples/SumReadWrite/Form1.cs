#region CODE_SAMPLE


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;

namespace Sample22
{
    // Structure declaration for values
    internal struct MyStruct
    {
        public ushort uintValue;
        public int dintValue;
        public bool boolValue;
    }

    // Structure declaration for handles
    internal struct VariableInfo
    {
        public uint indexGroup;
        public uint indexOffset;
        public int length;
    }

    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tbDint;
        private System.Windows.Forms.TextBox tbBool;
        private System.Windows.Forms.TextBox tbUint;
        private System.Windows.Forms.TextBox tbDint2;
        private System.Windows.Forms.TextBox tbBool2;
        private System.Windows.Forms.TextBox tbUint2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private AdsClient adsClient;
        private string[] variableNames;
        private int[] variableLengths;
        private Label label4;
        private Label label5;
        private Label label6;
        VariableInfo[] variables;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    adsClient.Dispose();
                }
                catch
                {
                    // cannot do something
                }
            }
            if (components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.tbDint = new System.Windows.Forms.TextBox();
            this.tbBool = new System.Windows.Forms.TextBox();
            this.tbUint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tbDint2 = new System.Windows.Forms.TextBox();
            this.tbBool2 = new System.Windows.Forms.TextBox();
            this.tbUint2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(104, 100);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(90, 27);
            this.button1.TabIndex = 0;
            this.button1.Text = "Read";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tbDint
            // 
            this.tbDint.Location = new System.Drawing.Point(89, 40);
            this.tbDint.Name = "tbDint";
            this.tbDint.Size = new System.Drawing.Size(120, 22);
            this.tbDint.TabIndex = 2;
            // 
            // tbBool
            // 
            this.tbBool.Location = new System.Drawing.Point(89, 70);
            this.tbBool.Name = "tbBool";
            this.tbBool.Size = new System.Drawing.Size(120, 22);
            this.tbBool.TabIndex = 3;
            // 
            // tbUint
            // 
            this.tbUint.Location = new System.Drawing.Point(89, 10);
            this.tbUint.Name = "tbUint";
            this.tbUint.Size = new System.Drawing.Size(120, 22);
            this.tbUint.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "uintValue:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "dintValue:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 23);
            this.label3.TabIndex = 6;
            this.label3.Text = "boolValue:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(230, 100);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(90, 27);
            this.button2.TabIndex = 7;
            this.button2.Text = "Write";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbDint2
            // 
            this.tbDint2.Location = new System.Drawing.Point(216, 40);
            this.tbDint2.Name = "tbDint2";
            this.tbDint2.Size = new System.Drawing.Size(120, 22);
            this.tbDint2.TabIndex = 12;
            // 
            // tbBool2
            // 
            this.tbBool2.Location = new System.Drawing.Point(216, 70);
            this.tbBool2.Name = "tbBool2";
            this.tbBool2.Size = new System.Drawing.Size(120, 22);
            this.tbBool2.TabIndex = 13;
            // 
            // tbUint2
            // 
            this.tbUint2.Location = new System.Drawing.Point(216, 10);
            this.tbUint2.Name = "tbUint2";
            this.tbUint2.Size = new System.Drawing.Size(120, 22);
            this.tbUint2.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(333, 17);
            this.label4.TabIndex = 14;
            this.label4.Text = "*please note: you have to fill in all of the three fields";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 153);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(279, 17);
            this.label5.TabIndex = 15;
            this.label5.Text = "on the right to write Values to the Variables";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(323, 95);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 17);
            this.label6.TabIndex = 16;
            this.label6.Text = "*";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(350, 179);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbBool2);
            this.Controls.Add(this.tbDint2);
            this.Controls.Add(this.tbUint2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbBool);
            this.Controls.Add(this.tbDint);
            this.Controls.Add(this.tbUint);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "ADS communication of VS .NET";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                // Connect to PLC
                adsClient = new AdsClient();
                adsClient.Connect(851);

                // Fill structures with name and size of PLC variables
                variableNames = new string[] { "MAIN.uintValue", "MAIN.dintValue", "MAIN.boolValue" };
                variableLengths = new int[] { 2, 4, 1 };

                // Write handle parameter into structure
                variables = new VariableInfo[variableNames.Length];
                for (int i = 0; i < variables.Length; i++)
                {
                    variables[i].indexGroup = (int)AdsReservedIndexGroup.SymbolValueByHandle;
                    variables[i].indexOffset = adsClient.CreateVariableHandle(variableNames[i]);
                    variables[i].length = variableLengths[i];
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                adsClient = null;
            }
        }

        // Read variables from the PLC
        // ===========================
        private void button1_Click(object sender, System.EventArgs e)
        {
            if (adsClient == null)
                return;

            try
            {
                // Get the ADS return codes and examine for errors
                byte[] readBuffer = BlockRead(variables);

                BinaryReader reader = new BinaryReader(new MemoryStream(readBuffer));
                for (int i = 0; i < variables.Length; i++)
                {
                    int error = reader.ReadInt32();
                    if (error != (int)AdsErrorCode.NoError)
                        System.Diagnostics.Debug.WriteLine(
                            String.Format("Unable to read variable {0} (Error = {1})", i, error));
                }

                // Read the data from the ADS stream
                MyStruct myStruct;
                myStruct.uintValue = reader.ReadUInt16();
                myStruct.dintValue = reader.ReadInt32();
                myStruct.boolValue = reader.ReadBoolean();

                // Write data from the structure into the text boxes
                tbUint.Text = myStruct.uintValue.ToString();
                tbDint.Text = myStruct.dintValue.ToString();
                tbBool.Text = myStruct.boolValue.ToString();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private byte[] BlockRead(VariableInfo[] variables)
        {
            // Allocate memory
            int rdLength = variables.Length * 4;
            int wrLength = variables.Length * 12;

            // Write data for handles into the ADS Stream
            byte[] writeBuffer = new byte[wrLength];

            BinaryWriter writer = new BinaryWriter(new MemoryStream(writeBuffer));
            for (int i = 0; i < variables.Length; i++)
            {
                writer.Write(variables[i].indexGroup);
                writer.Write(variables[i].indexOffset);
                writer.Write(variables[i].length);
                rdLength += variables[i].length;
            }

            // Sum command to read variables from the PLC
            //AdsStream rdStream = new AdsStream(rdLength);
            byte[] readBuffer = new byte[rdLength];
            adsClient.ReadWrite((uint)0xF080, (uint)variables.Length, readBuffer.AsMemory(), writeBuffer.AsMemory() );

            // Return the ADS error codes
            return readBuffer;
        }

        // Write variables into the PLC
        // ============================
        private void button2_Click(object sender, EventArgs e)
        {
            if (adsClient == null)
                return;

            try
            {
                // Get the ADS return codes and examine for errors
                byte[] readBuffer = BlockRead2(variables);

                BinaryReader reader = new BinaryReader(new MemoryStream(readBuffer));
                for (int i = 0; i < variables.Length; i++)
                {
                    int error = reader.ReadInt32();
                    if (error != (int)AdsErrorCode.NoError)
                        System.Diagnostics.Debug.WriteLine(
                            String.Format("Unable to read variable {0} (Error = {1})", i, error));
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private byte[] BlockRead2(VariableInfo[] variables)
        {
            // Allocate memory
            int rdLength = variables.Length * 4;
            int wrLength = variables.Length * 12 + 7;

            byte[] writeBuffer = new byte[wrLength];

            BinaryWriter writer = new BinaryWriter(new MemoryStream(writeBuffer));
            MyStruct myStruct;
            myStruct.uintValue = ushort.Parse(tbUint2.Text);
            myStruct.dintValue = int.Parse(tbDint2.Text);
            myStruct.boolValue = bool.Parse(tbBool2.Text);
            
            // Write data for handles into the ADS stream
            for (int i = 0; i < variables.Length; i++)
            {
                writer.Write(variables[i].indexGroup);
                writer.Write(variables[i].indexOffset);
                writer.Write(variables[i].length);
            }

            // Write data to send to PLC behind the structure
            writer.Write(myStruct.uintValue);
            writer.Write(myStruct.dintValue);
            writer.Write(myStruct.boolValue);

            // Sum command to write the data into the PLC
            byte[] readBuffer = new byte[rdLength];

            adsClient.ReadWrite((uint)0xF081, (uint)variables.Length, readBuffer.AsMemory(), writeBuffer.AsMemory());

            // Return the ADS error codes
            return readBuffer;
        }
    }
}
#endregion
