using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Text;

public class ScriptRunner : MonoBehaviour
{
    private string shellScriptPath;

    public void RunScript(string configFileName, string runId, bool resume)
    {
        StringBuilder outputBuilder = new StringBuilder();
        Process process = new Process();

        string resumeFlag = resume ? "true" : "false";

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            shellScriptPath = "\"C:\\Users\\lipb1\\Documents\\UnityProjects\\3D\\Kite\\Assets\\ML_PPO\\start_training_win.bat\"";
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {shellScriptPath} {configFileName} {runId} {resumeFlag}";
        }
        else if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            shellScriptPath = "./Assets/ML_PPO/start_training_linux.sh";
            process.StartInfo.FileName = "/usr/bin/gnome-terminal";
            process.StartInfo.Arguments = $"--tab -- /bin/bash -c \"{shellScriptPath} {configFileName} {runId} {resumeFlag}\"";
        }
        else
        {
            shellScriptPath = "./Assets/ML_PPO/start_training.sh";
            process.StartInfo.FileName = "/bin/zsh";
            process.StartInfo.Arguments = $"{shellScriptPath} {configFileName} {runId} {resumeFlag}";
        }

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = false;

        process.OutputDataReceived += (sender, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data)) 
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        
        process.ErrorDataReceived += (sender, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data)) 
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer) {
            process.WaitForExit();
        }

        UnityEngine.Debug.Log("Script output: " + outputBuilder.ToString());
    }
}