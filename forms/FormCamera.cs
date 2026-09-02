using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace CrudApp.Forms
{
    public class FormCamera : Form
    {
        private ComboBox cbCameras;
        private PictureBox picPreview;
        private Button btnIniciar, btnCapturar, btnCancelar;
        private TrackBar tbZoom;

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        private double zoomLevel = 1.0;

        public byte[]? FotoCapturada { get; private set; }

        public FormCamera()
        {
            InitializeComponent();
            CarregarCameras();
        }

        private void InitializeComponent()
        {
            this.Text = "Câmera";
            this.Icon = new Icon("app.ico");
            this.ClientSize = new Size(340, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            cbCameras = new ComboBox
            {
                Left = 20,
                Top = 20,
                Width = 300
            };

            btnIniciar = new Button
            {
                Left = 20,
                Top = 60,
                Width = 100,
                Text = "Iniciar"
            };

            picPreview = new PictureBox
            {
                Left = 50,
                Top = 90,
                Width = 240,
                Height = 320,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            btnCapturar = new Button
            {
                Left = 50,
                Top = 460,
                Width = 100,
                Text = "Capturar"
            };

            btnCancelar = new Button
            {
                Left = 200,
                Top = 460,
                Width = 100,
                Text = "Cancelar"
            };

            tbZoom = new TrackBar
            {
                Left = 50,
                Top = 420,
                Width = 230,
                Minimum = 10,
                Maximum = 40,
                Value = 10,
                TickFrequency = 5,
                LargeChange = 5,
                SmallChange = 1
            };

            tbZoom.Scroll += (s, e) =>
            {
                zoomLevel = tbZoom.Value / 10.0;
            };

            btnIniciar.Click += BtnIniciar_Click;
            btnCapturar.Click += BtnCapturar_Click;
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.Add(cbCameras);
            this.Controls.Add(btnIniciar);
            this.Controls.Add(picPreview);
            this.Controls.Add(btnCapturar);
            this.Controls.Add(btnCancelar);
            this.Controls.Add(tbZoom);
        }

        private void CarregarCameras()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo device in videoDevices)
                cbCameras.Items.Add(device.Name);

            if (cbCameras.Items.Count > 0)
                cbCameras.SelectedIndex = 0;
        }

        private void BtnIniciar_Click(object? sender, EventArgs e)
        {
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Nenhuma câmera encontrada.");
                return;
            }

            if (videoSource != null && videoSource.IsRunning)
                videoSource.SignalToStop();

            videoSource = new VideoCaptureDevice(videoDevices[cbCameras.SelectedIndex].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();

                int w = frame.Width;
                int h = frame.Height;

                // -------------------------------
                // 1) CALCULAR CORTE 3x4
                // -------------------------------
                double targetRatio = 3.0 / 4.0; // 3x4 (LxA)

                double frameRatio = (double)w / h;

                int cropW, cropH;

                if (frameRatio > targetRatio)
                {
                    // Muito largo → cortar laterais
                    cropH = h;
                    cropW = (int)(h * targetRatio);
                }
                else
                {
                    // Muito alto → cortar topo/base
                    cropW = w;
                    cropH = (int)(w / targetRatio);
                }

                int cropX = (w - cropW) / 2;
                int cropY = (h - cropH) / 2;

                Rectangle cropRect = new Rectangle(cropX, cropY, cropW, cropH);
                Bitmap cropped = frame.Clone(cropRect, frame.PixelFormat);

                // -------------------------------
                // 2) APLICAR ZOOM DIGITAL
                // -------------------------------
                if (zoomLevel > 1.0)
                {
                    int zw = (int)(cropW / zoomLevel);
                    int zh = (int)(cropH / zoomLevel);

                    int zx = (cropW - zw) / 2;
                    int zy = (cropH - zh) / 2;

                    Rectangle zoomCrop = new Rectangle(zx, zy, zw, zh);

                    Bitmap croppedZoom = cropped.Clone(zoomCrop, cropped.PixelFormat);

                    cropped.Dispose();
                    cropped = new Bitmap(croppedZoom, cropW, cropH);
                    croppedZoom.Dispose();
                }

                // -------------------------------
                // 3) MOSTRAR NO PICTUREBOX
                // -------------------------------
                picPreview.Invoke(new Action(() =>
                {
                    picPreview.Image?.Dispose();
                    picPreview.Image = cropped;
                }));

                frame.Dispose();
            }
            catch
            {
                // Ignorar erros do AForge
            }
        }


        private void BtnCapturar_Click(object? sender, EventArgs e)
        {
            if (picPreview.Image == null)
            {
                MessageBox.Show("Nada para capturar.");
                return;
            }

            using var ms = new MemoryStream();
            picPreview.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            FotoCapturada = ms.ToArray();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
                videoSource.SignalToStop();

            base.OnFormClosing(e);
        }
    }
}
