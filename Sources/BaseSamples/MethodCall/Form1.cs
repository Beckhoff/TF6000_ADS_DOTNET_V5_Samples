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
        AdsClient tcClient;

        //Update the AMSNetId for your target
        string targetAmsNetId = "5.95.143.126.1.1";

        public Form1()
        {
            //Create a new instance of class AdsClient
            tcClient = new AdsClient();

            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Connect to target PLC - Port 851
                tcClient.Connect(targetAmsNetId,851);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                tcClient.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    #endregion
}
