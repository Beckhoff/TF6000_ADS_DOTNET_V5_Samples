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


namespace S22_SumReadWrite
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
                if (adsClient != null)
                    adsClient.Dispose();
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
                adsClient.Connect(AmsNetId.Local,851);

                variablePaths = new string[] { "MAIN.uintValue", "MAIN.dintValue", "MAIN.boolValue" };

                // 1. Alternative Create a symbols list via SymbolLoader
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

        // Read variables from the PLC via the Symbol Loader and sophisticated SumSymbolRead class
        // =======================================================================================
        private void readSymbolic_Click(object sender, System.EventArgs e)
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

        // Write variables into the PLC via Symbol Loader and sophisticated SumSymbolWrite class
        // =====================================================================================
        private void writeSymbolic_Click(object sender, EventArgs e)
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
