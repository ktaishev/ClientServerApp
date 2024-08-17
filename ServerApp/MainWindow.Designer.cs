namespace ServerApp
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            startServer = new Button();
            Clients = new GroupBox();
            tableLayoutPanel = new TableLayoutPanel();
            ServerStatusImg = new PictureBox();
            ServerStatusText = new Label();
            Clients.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ServerStatusImg).BeginInit();
            SuspendLayout();
            // 
            // startServer
            // 
            startServer.Location = new Point(520, 40);
            startServer.Name = "startServer";
            startServer.Size = new Size(150, 40);
            startServer.TabIndex = 3;
            startServer.Text = "Run Server";
            startServer.UseVisualStyleBackColor = true;
            startServer.Click += startServer_Click;
            // 
            // Clients
            // 
            Clients.AutoSize = true;
            Clients.Controls.Add(tableLayoutPanel);
            Clients.Location = new Point(30, 100);
            Clients.Margin = new Padding(0);
            Clients.Name = "Clients";
            Clients.Padding = new Padding(0);
            Clients.Size = new Size(640, 580);
            Clients.TabIndex = 6;
            Clients.TabStop = false;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.AutoScroll = true;
            tableLayoutPanel.ColumnCount = 3;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.88889F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.88889F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22.2222214F));
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 20);
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle());
            tableLayoutPanel.Size = new Size(640, 560);
            tableLayoutPanel.TabIndex = 0;
            // 
            // ServerStatusImg
            // 
            ServerStatusImg.ImageLocation = "C:\\Users\\test\\Desktop\\Solution\\ServerApp\\Res\\offline.ico";
            ServerStatusImg.InitialImage = null;
            ServerStatusImg.Location = new Point(30, 30);
            ServerStatusImg.Name = "ServerStatusImg";
            ServerStatusImg.Size = new Size(60, 60);
            ServerStatusImg.TabIndex = 7;
            ServerStatusImg.TabStop = false;
            ServerStatusImg.WaitOnLoad = true;
            // 
            // ServerStatusText
            // 
            ServerStatusText.Location = new Point(210, 30);
            ServerStatusText.Name = "ServerStatusText";
            ServerStatusText.Size = new Size(190, 60);
            ServerStatusText.TabIndex = 8;
            ServerStatusText.Text = "Server offline";
            ServerStatusText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(682, 653);
            ControlBox = false;
            Controls.Add(ServerStatusText);
            Controls.Add(ServerStatusImg);
            Controls.Add(Clients);
            Controls.Add(startServer);
            Name = "MainWindow";
            Text = "Main";
            Clients.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ServerStatusImg).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button startServer;
        private GroupBox Clients;
        private PictureBox ServerStatusImg;
        private Label ServerStatusText;
        private TableLayoutPanel tableLayoutPanel;
    }
}
