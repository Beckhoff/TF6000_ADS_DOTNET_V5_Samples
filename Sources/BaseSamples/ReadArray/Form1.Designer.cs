namespace S12_ReadArray
{
    public partial class Form1
{
    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.lbArray = new System.Windows.Forms.ListBox();
        this.btnRead = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // lbArray
        // 
        this.lbArray.ItemHeight = 16;
        this.lbArray.Location = new System.Drawing.Point(19, 9);
        this.lbArray.Name = "lbArray";
        this.lbArray.Size = new System.Drawing.Size(173, 244);
        this.lbArray.TabIndex = 0;
        // 
        // btnRead
        // 
        this.btnRead.Location = new System.Drawing.Point(19, 277);
        this.btnRead.Name = "btnRead";
        this.btnRead.Size = new System.Drawing.Size(173, 26);
        this.btnRead.TabIndex = 1;
        this.btnRead.Text = "Read";
        this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
        // 
        // Form1
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
        this.ClientSize = new System.Drawing.Size(212, 322);
        this.Controls.Add(this.btnRead);
        this.Controls.Add(this.lbArray);
        this.Name = "Form1";
        this.Text = "Sample12";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.ResumeLayout(false);

    }
    #endregion

    private System.Windows.Forms.Button btnRead;
    private System.ComponentModel.Container components = null;
    private System.Windows.Forms.ListBox lbArray;
}
}
