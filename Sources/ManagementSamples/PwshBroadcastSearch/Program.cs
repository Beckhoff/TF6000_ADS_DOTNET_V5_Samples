using System.Linq;
using System.Management.Automation;


// TcXaeMgmt must be installed from powershell Gallery
string name = "TcXaeMgmt";

// Alternatively the TcXaeMgmt Module can be loaded from an XCopy Deployed folder
// string path = c:\tmp\TcXaeMgmt\6.0.68\TcXaeMgmt.psd1
PowerShell sh = PowerShell.Create();

//Import Module
sh.AddStatement()
    .AddCommand("Import-Module")
    .AddParameter("Name", name).Invoke();

//Broadcast Search
Console.WriteLine("Starting BroadCast Search ...");

var result = sh.AddStatement()
   .AddCommand("Get-AdsRoute")
   .AddParameter("all", true)
   .Invoke();

foreach(var r in result)
{
    Console.WriteLine(r.BaseObject.ToString());
}

Console.WriteLine("");
Console.WriteLine("Completed");
Console.WriteLine("");
Console.WriteLine("Press Enter");
Console.ReadLine();
