using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwinCAT.Ads;

namespace _30_ADS.NET_MethodCall
{

    #region CODE_SAMPLE
    public partial class Form1 : Form
    {
        AdsClient tcClient = null;

        //Update the AMSNetId for your target
        AmsNetId targetAmsNetId = AmsNetId.Local;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //Create a new instance of class AdsClient
            tcClient = new AdsClient();

            //Connect to target PLC - Port 851
            tcClient.Connect(targetAmsNetId, 851);
        }

        private void btnMethodCall_Click(object sender, EventArgs e)
        {
            try
            {
                //Get the values entered into the form
                short first = Convert.ToInt16(tbValueA.Text);
                short second = Convert.ToInt16(tbValueB.Text);

                //Add a function block FB_Math in TwinCAT
                //Create a method called mAdd on FB_Math, with the following signature

                /*  {attribute 'TcRpcEnable'}
                METHOD PUBLIC mAdd : INT
                VAR_INPUT
                    i1 : INT := 0;
                    i2 : INT := 0;
                END_VAR 
                */

                //Instance the function block in main call it fbMath

                //Call the method mAdd of fbMath in MAIN
                short result = (short)tcClient.InvokeRpcMethod("MAIN.fbMath", "mAdd", new object[] { first, second });

                //Display the result
                tbSumAB.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose the Client during Form Cleanup
                tcClient.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
    #endregion
}
