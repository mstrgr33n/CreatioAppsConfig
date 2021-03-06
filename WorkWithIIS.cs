using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows.Forms;

namespace CreatioManagmentTools
{
    internal class WorkWithIIS
    {
        public void CreateSite(string projectName, string port, string path, string sitename = "")
        {
            string ProjectName = projectName, Port = port, Path = path;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Import-Module WebAdministration");
            sb.AppendLine($"New-WebAppPool -Name \"{ProjectName}\" -Force");
            var websiteString = $"New-Website -Name \"{ProjectName}\" -Port {port} -ApplicationPool \"{ProjectName}\" -PhysicalPath \"{Path}\" -Force";
            if (!string.IsNullOrEmpty(sitename))
            {
                websiteString += $" -HostHeader \"{sitename}\"";
            }
            sb.AppendLine(websiteString);
            sb.AppendLine($"New-WebApplication -Site \"{ProjectName}\" -Name \"0\" -PhysicalPath \"{Path}\\Terrasoft.WebApp\" -ApplicationPool \"{ProjectName}\" -Force");
            var script = sb.ToString();

            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var pipeline = runspace.CreatePipeline();

            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                var initial = InitialSessionState.CreateDefault();
                initial.ImportPSModule(new[] { "ServerManager" });
                PowerShellInst.Runspace = runspace;
                PowerShellInst.AddScript("Set-ExecutionPolicy RemoteSigned");
                PowerShellInst.AddScript("Get-ExecutionPolicy");
                PowerShellInst.Invoke();

                pipeline.Commands.AddScript(script);
                pipeline.Invoke();
                pipeline.Dispose();
                runspace.Dispose();
            }
        }
    }
}
