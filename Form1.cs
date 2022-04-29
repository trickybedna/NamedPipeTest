using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace NamedPipeTest
{
    public partial class Form1 : Form
    {
        private static string logFilePath = @"C:\Temp\TestPipe.log";

        public Form1()
        {
            InitializeComponent();
            Log("Name Pipe test started");
        }

        private static void Log(string textToLog)
        {
            File.AppendAllText(logFilePath, '\n' + "Sever Pipe: " + textToLog);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Enter pipe name to start server of named pipe");
            Log($"PipeName = {PipeNameTxt.Text}");
            if (PipeNameTxt.Text is null)
            {
                Log($"Given pipe name is null");
            }
            else
            {
                var pipeStream = CreatePipeStream(PipeNameTxt.Text);
                Log(pipeStream is null ? "named pipe was not created" : $"pipeStream created.");
            }
        }

        private static PipeCore<NamedPipeServerStream> CreatePipeStream(string pipeName)

        {

            var ps = new PipeSecurity();

            // Current user must be able to create, read, write &c..
            Log($"WindowsIdentity = {WindowsIdentity.GetCurrent().Name}");

            Log($"Network Sid = {WellKnownSidType.NetworkSid}");

            ps.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().Name, PipeAccessRights.FullControl,
                AccessControlType.Allow));

            // Users coming via the network, except interactive users like Remote Desktop users, are denied.

            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
                PipeAccessRights.FullControl, AccessControlType.Deny));



            // The pipe client is supported in various frameworks.

            // This server currently is only required for the daemon which uses .NET Framework.

            // Implementations for other frameworks are provided here to allow this project to build.
            try
            {
                var pipeStream =

#if NET461_OR_GREATER // .NET Framework, currently used by the daemon

            new NamedPipeServerStream(pipeName, PipeDirection.InOut, 20, PipeTransmissionMode.Message, PipeOptions.None, 4096, 4096, ps);

#elif NET6_0_OR_GREATER // .NET 6

            // To be implemented.

            // See https://docs.microsoft.com/en-us/dotnet/api/system.io.pipes.namedpipeserverstreamacl.create?view=net-6.0

#error .NET 6 is not yet supported

#else // Less secure fallback for .NET Standard, not recommended

                    new NamedPipeServerStream(pipeName, PipeDirection.InOut, 20, PipeTransmissionMode.Message,
                        PipeOptions.None);

#endif
                Log("Waiting for connection..............");
                pipeStream.WaitForConnection();

                Log("Server established connection..............");
                return new PipeCore<NamedPipeServerStream>(pipeStream);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
            }

            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }

    internal class PipeCore<T>
    {
        private NamedPipeServerStream NamedPipeStream;

        public PipeCore(NamedPipeServerStream pipeStream)
        {
            NamedPipeStream = pipeStream;
        }
    }
}

