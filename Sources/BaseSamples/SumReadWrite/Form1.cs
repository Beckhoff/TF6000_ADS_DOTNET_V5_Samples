#region CODE_SAMPLE


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT.Ads;

namespace _22_SumReadWrite
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
    public partial class Form1 : Form
    {
        
        private AdsClient adsClient;
        private string[] variableNames;
        private int[] variableLengths;
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

        

        

        private void Form1_Load(object sender, System.EventArgs e)
        {
            try
            {
                // Connect to PLC
                adsClient = new AdsClient();
                adsClient.Connect("5.95.143.126.1.1",851);

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
