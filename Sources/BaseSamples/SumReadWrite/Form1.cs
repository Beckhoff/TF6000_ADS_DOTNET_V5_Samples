#region CODE_SAMPLE


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.Ads.SumCommand;
using TwinCAT.TypeSystem;
using TwinCAT.ValueAccess;


namespace _22_SumReadWrite
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public partial class Form1 : Form
    {
        
        private AdsClient adsClient;
        private ISymbolLoader loader;
        SymbolCollection symbols = new SymbolCollection();
        private string[] variablePaths;

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
                variablePaths = new string[] { "MAIN.uintValue", "MAIN.dintValue", "MAIN.boolValue" };

                SymbolLoaderSettings settings = new SymbolLoaderSettings(SymbolsLoadMode.DynamicTree);
                loader = SymbolLoaderFactory.Create(adsClient, settings);

                foreach (string str in variablePaths)
                {
                    symbols.Add((DynamicSymbol)loader.Symbols[str]);
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
                SumSymbolRead readCommand = new SumSymbolRead(adsClient, symbols);
                
                var resultSumRead = readCommand.Read();

                tbUint.Text = resultSumRead[0].ToString();
                tbDint.Text = resultSumRead[1].ToString();
                tbBool.Text = resultSumRead[2].ToString();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        // Write variables into the PLC
        // ============================
        private void button2_Click(object sender, EventArgs e)
        {
            if (adsClient == null)
                return;

            try
            {
                SumSymbolWrite writeCommand = new SumSymbolWrite(adsClient,symbols);

                object[] writeValues = new object[]{Convert.ToInt16(tbUint2.Text),
                                                    Convert.ToInt16(tbDint2.Text),
                                                    Convert.ToBoolean(tbBool2.Text) };
                
                writeCommand.Write(writeValues);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
#endregion
