using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Windows.Management.Deployment;
using Windows.Media.Capture.Frames;
using Windows.Media.Protection.PlayReady;
using Windows.Networking.Sockets;
using Windows.Storage.Pickers;

namespace ServerApp
{
    public partial class MainWindow : Form
    {
        const string DEFAULT_IP = "127.0.0.1";
        const int DEFAULT_PORT = 4747;
        int servStatus = 0;
        Socket serv;
        int ClientCount = 0;
        int nextID = 0;

        List<Socket> ClientList = new List<Socket>();
        List<string> ClientName = new List<string>();
        List<string> ClientAddr = new List<string>();
        List<string> ClientLastSeen = new List<string>();
        List<Label> ClientLastSeenLabel = new List<Label>();
        List<string> ClientScreenPath = new List<string>();
        List<PictureBox> ClientScreenshot = new List<PictureBox>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendCmd(Socket conn, string Msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Msg.ToString());
            conn.Send(buffer);
        }

        private int RecvBytes(Socket conn, byte[] buffer)
        {
            int receivedBytes = -1;
            try
            {
                receivedBytes = conn.Receive(buffer);
            }
            catch (SocketException sEx)
            {
                Debug.WriteLine(sEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
            return receivedBytes;
        }

        private void SaveScreenshot(Socket conn, string Path)
        {
            byte[] buffer = new byte[10000000];
            int szBuffer = RecvBytes(conn, buffer);
            if (File.Exists(Path))
                File.Delete(Path);
            File.WriteAllBytes(Path, buffer.SkipLast(10000000 - szBuffer).ToArray());
        }

        private string RecvMsg(Socket conn)
        {
            byte[] buffer = new byte[1024];
            RecvBytes(conn, buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        public Label CreateLabel(string Name, string Text = "", int X = 0, int Y = 0, int Width = 120, int Height = 30)
        {
            Label Label = new Label();
            Label.Name = Name;
            Label.Text = Text;
            Label.Left = X;
            Label.Top = Y;
            Label.Size = new Size(Width, Height);
            Label.TextAlign = ContentAlignment.MiddleCenter;
            //Label.Dock = DockStyle.Fill;
            return Label;
        }

        public Button CreateButton(string Name, string Text = "", int X = 0, int Y = 0, int Width = 0, int Height = 0)
        {
            Button Button = new Button();
            Button.Name = Name;
            Button.Text = Text;
            Button.Tag = Tag;
            Button.Left = X;
            Button.Top = Y;
            Button.Size = new Size(Width, Height);
            Button.TextAlign = ContentAlignment.MiddleCenter;
            //Button.Dock = DockStyle.Fill;
            return Button;
        }

        private void AddClientTable(int ID)
        {
            tableLayoutPanel.RowCount = ClientCount;

            Label ClientNameLabel = CreateLabel("ClientNameLabel_" + ID.ToString(), ClientName[ID], 50, 120 + 40 * ClientCount, 210, 30);
            tableLayoutPanel.Controls.Add(ClientNameLabel);

            Label ClientAddrLabel = CreateLabel("ClientAddrLabel_" + ID.ToString(), ClientAddr[ID], 260, 120 + 40 * ClientCount, 210, 30);
            tableLayoutPanel.Controls.Add(ClientAddrLabel);

            Button GetDetails = CreateButton("GetDetail_" + ID.ToString(), "Details", 540, 120 + 40 * ClientCount, 100, 30);
            GetDetails.Tag = ID;
            GetDetails.Click += GetDetails_Click;
            tableLayoutPanel.Controls.Add(GetDetails);
        }

        private void handleClient(Socket conn)
        {
            ClientCount++;
            ClientList.Add(conn);

            SendCmd(conn, "2");
            string Name = RecvMsg(conn);

            string Addr = conn.RemoteEndPoint.ToString();

            SendCmd(conn, "1");
            string LastSeen = RecvMsg(conn);

            SendCmd(conn, "3");
            string ScreenshotPath = "screen_" + nextID + ".bmp";
            SaveScreenshot(conn, ScreenshotPath);

            ClientName.Add(Name);
            ClientAddr.Add(Addr);
            ClientLastSeen.Add(LastSeen);
            ClientScreenPath.Add(ScreenshotPath);

            AddClientTable(nextID++);
        }

        private void GetDetails_Click(object sender, EventArgs e)
        {
            Button self = (Button)sender;
            int ID = Int32.Parse(self.Tag.ToString());
            Form ClientInfo = new Form();

            ClientInfo.Show();
            ClientInfo.Size = new Size(480, 520);
            ClientInfo.Text = "Current Client #" + ID;

            PictureBox Screenshot = new PictureBox();
            Screenshot.Size = new Size(384, 204);
            Screenshot.Location = new Point(48, 60);
            Screenshot.SizeMode = PictureBoxSizeMode.Zoom;
            Screenshot.ImageLocation = ClientScreenPath[ID];
            ClientScreenshot.Add(Screenshot);
            ClientInfo.Controls.Add(Screenshot);

            Label ClientNameLabel = CreateLabel("clientNameLabel_" + ID.ToString(), ClientName[ID], 160, 10, 160, 30);
            ClientInfo.Controls.Add(ClientNameLabel);


            Label ClientAddrLabel = CreateLabel("clientAddrLabel_" + ID.ToString(), "Client: " + ClientAddr[ID], 30, 360, 300, 30);

            ClientAddrLabel.TextAlign = ContentAlignment.MiddleLeft;
            ClientInfo.Controls.Add(ClientAddrLabel);

            Label LastSeen = CreateLabel("clientLastSeenLabel_" + ID.ToString(), "Last activity (seconds ago): " + ClientLastSeen[ID], 30, 400, 300, 60);

            LastSeen.TextAlign = ContentAlignment.MiddleLeft;
            ClientInfo.Controls.Add(LastSeen);
            ClientLastSeenLabel.Add(LastSeen);

            Button RefreshScreen = CreateButton("RefreshScreen", "Refresh", 280, 300, 120, 30);
            ClientInfo.Controls.Add(RefreshScreen);
            RefreshScreen.Click += (sender, EventArgs) => { Refresh_Click(sender, EventArgs, ID); };

            Button SaveScreenshot = CreateButton("OpenScreenshot", "Open", 80, 300, 120, 30);
            ClientInfo.Controls.Add(SaveScreenshot);
            SaveScreenshot.Click += (sender, EventArgs) => { OpenScreenshot_Click(sender, EventArgs, ID); };
        }

        private void Refresh_Click(object sender, EventArgs e, int ID)
        {
            SendCmd(ClientList[ID], "1");
            string LastSeen = RecvMsg(ClientList[ID]);
            string ScreenshotPath = ClientScreenPath[ID];
            SendCmd(ClientList[ID], "3");
            SaveScreenshot(ClientList[ID], ScreenshotPath);
            ClientScreenshot[ID].ImageLocation = ScreenshotPath;

            ClientLastSeen[ID] = LastSeen;
            ClientLastSeenLabel[ID].Text = "Last activity (seconds ago): " + LastSeen;
        }

        private void OpenScreenshot_Click(object sender, EventArgs e, int ID)
        {
            Form ClientScreen = new Form();
            ClientScreen.Show();
            ClientScreen.Size = new Size(980, 590);
            ClientScreen.Text = "Current Client Screen #" + ID + " (semi-live)";
            PictureBox Screenshot = new PictureBox();
            Screenshot.Size = new Size(960, 540);
            Screenshot.Location = new Point(0, 0);
            Screenshot.SizeMode = PictureBoxSizeMode.Zoom;
            Screenshot.ImageLocation = ClientScreenPath[ID];
            ClientScreen.Controls.Add(Screenshot);
        }

        private async void startServer_Click(object sender, EventArgs e)
        {
            if (servStatus == 0)
            {
                servStatus = 1;
                IPEndPoint hostIPEndPoint = new IPEndPoint(IPAddress.Any, DEFAULT_PORT);
                serv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                serv.Bind(hostIPEndPoint);
                ServerStatusImg.ImageLocation = "C:\\Users\\test\\Desktop\\Solution\\ServerApp\\Res\\online.ico";
                ServerStatusText.Text = "Server online since: " + DateTime.Now.ToString();
                serv.Listen(4096);

                do
                {
                    Socket Client = await serv.AcceptAsync();
                    handleClient(Client);
                } while (true);
            }
        }

    }
}
