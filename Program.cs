using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilosofosGUI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private const int NumFilosofos = 5;
        private readonly string[] Nombres = { "Aristóteles", "Platón", "Sócrates", "Confucio", "Descartes" };
        private readonly Color[] ColorEstado = { Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15), Color.FromArgb(46, 204, 113) };
        private readonly string[] TextoEstado = { "Pensando", "Hambriento", "Comiendo" };

        private int[] estados = new int[NumFilosofos];
        private int[] comidasContador = new int[NumFilosofos];
        private double factorVelocidad = 1.0;

        private CancellationTokenSource cts;
        private Task tareaSimulacion;
        private System.Windows.Forms.Timer timerRender;
        private DateTime ultimaActualizacionDeadlock = DateTime.MinValue;

        private Panel panelControles;
        private PanelDibujo panelMesa;

        // Panel con doble buffer para eliminar el parpadeo al dibujar la mesa
        private class PanelDibujo : Panel
        {
            public PanelDibujo()
            {
                this.SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.CacheText,
                    true);
                this.UpdateStyles();
                this.DoubleBuffered = true;
                this.BackColor = Color.White;
            }
        }
        private Panel panelEstadisticas;
        private Label lblEstadoGeneral;
        private Label lblVelocidad;
        private TrackBar trkVelocidad;
        private Button btnIniciar;
        private Button btnDetener;
        private Button btnReiniciar;
        private Button btnSalir;
        private ComboBox cmbMetodos;
        private FlowLayoutPanel flpEstadisticas;
        private Label[] lblEstadoFilo;
        private Label[] lblComidasFilo;

        public MainForm()
        {
            this.Text = "Simulador Visual - Problema de los Filósofos Cenando";
            this.Size = new Size(1250, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(1000, 700);
            this.BackColor = Color.FromArgb(236, 240, 245);
            this.DoubleBuffered = true;

            InicializarComponentesUI();

            timerRender = new System.Windows.Forms.Timer();
            timerRender.Interval = 50;
            timerRender.Tick += (s, e) =>
            {
                panelMesa.Invalidate();
                ActualizarEstadisticasUI();
                DetectarDeadlock();
            };
        }

        private void InicializarComponentesUI()
        {
            panelControles = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.FromArgb(41, 128, 185),
                Padding = new Padding(10)
            };

            Label lblTitulo = new Label()
            {
                Text = "PROBLEMA DE LOS FILÓSOFOS CENANDO",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            panelControles.Controls.Add(lblTitulo);
            Label lblMetodo = new Label()
            {
                Text = "Método de solución:",
                Location = new Point(18, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            panelControles.Controls.Add(lblMetodo);

            cmbMetodos = new ComboBox()
            {
                Location = new Point(140, 46),
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbMetodos.Items.Add("1. Jerarquía de Recursos (Sin Deadlock)");
            cmbMetodos.Items.Add("2. Solución por Estados (Monitor)");
            cmbMetodos.Items.Add("3. Límite de Filósofos (N-1)");
            cmbMetodos.Items.Add("4. Deadlock Intencional (Bloqueo)");
            cmbMetodos.SelectedIndex = 0;
            panelControles.Controls.Add(cmbMetodos);

            Label lblVelTexto = new Label()
            {
                Text = "Velocidad simulación:",
                Location = new Point(505, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            panelControles.Controls.Add(lblVelTexto);

            trkVelocidad = new TrackBar()
            {
                Location = new Point(625, 46),
                Width = 200,
                Minimum = 1,
                Maximum = 20,
                Value = 10,
                TickFrequency = 5
            };
            trkVelocidad.Scroll += (s, e) =>
            {
                factorVelocidad = trkVelocidad.Value / 10.0;
                lblVelocidad.Text = $"x{factorVelocidad:F1}";
            };
            panelControles.Controls.Add(trkVelocidad);

            lblVelocidad = new Label()
            {
                Text = "x1.0",
                Location = new Point(835, 46),
                Width = 45,
                AutoSize = false,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            panelControles.Controls.Add(lblVelocidad);
            btnIniciar = new Button()
            {
                Text = "Iniciar",
                Location = new Point(18, 96),
                Width = 110,
                Height = 40,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.Click += BtnIniciar_Click;

            btnDetener = new Button()
            {
                Text = "Detener",
                Location = new Point(140, 96),
                Width = 110,
                Height = 40,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnDetener.FlatAppearance.BorderSize = 0;
            btnDetener.Click += BtnDetener_Click;

            btnReiniciar = new Button()
            {
                Text = "Reiniciar",
                Location = new Point(262, 96),
                Width = 110,
                Height = 40,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnReiniciar.FlatAppearance.BorderSize = 0;
            btnReiniciar.Click += (s, e) =>
            {
                DetenerSimulacion();
                Array.Clear(estados, 0, estados.Length);
                Array.Clear(comidasContador, 0, comidasContador.Length);
                panelMesa.Invalidate();
                ActualizarEstadisticasUI();
                lblEstadoGeneral.Text = "Simulación reiniciada. Selecciona un método e inicia.";
            };

            btnSalir = new Button()
            {
                Text = "Salir",
                Location = new Point(930, 96),
                Width = 90,
                Height = 40,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.Click += (s, e) => Application.Exit();

            panelControles.Controls.Add(btnIniciar);
            panelControles.Controls.Add(btnDetener);
            panelControles.Controls.Add(btnReiniciar);
            panelControles.Controls.Add(btnSalir);

            this.Controls.Add(panelControles);
            panelMesa = new PanelDibujo()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            panelMesa.Paint += PanelMesa_Paint;
            panelEstadisticas = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 115,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(10)
            };

            Label lblLeyenda = new Label()
            {
                Text = "Leyenda:    ",
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            panelEstadisticas.Controls.Add(lblLeyenda);

            int leyX = 130;
            foreach (var kv in new (int estado, Color color, string texto)[] {
                (0, ColorEstado[0], "Pensando"),
                (1, ColorEstado[1], "Hambriento"),
                (2, ColorEstado[2], "Comiendo") })
            {
                Panel p = new Panel() { Location = new Point(leyX, 8), Size = new Size(110, 22) };
                Label colorBox = new Label() { Location = new Point(2, 4), Size = new Size(16, 16), BackColor = kv.color, BorderStyle = BorderStyle.FixedSingle };
                Label txt = new Label() { Location = new Point(20, 3), AutoSize = true, Text = kv.texto, Font = new Font("Segoe UI", 9) };
                p.Controls.Add(colorBox);
                p.Controls.Add(txt);
                panelEstadisticas.Controls.Add(p);
                leyX += 130;
            }

            lblEstadoGeneral = new Label()
            {
                Location = new Point(15, 38),
                Size = new Size(1000, 20),
                Text = "Selecciona un método y presiona 'Iniciar' para comenzar la simulación.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 80, 80)
            };
            panelEstadisticas.Controls.Add(lblEstadoGeneral);

            flpEstadisticas = new FlowLayoutPanel()
            {
                Location = new Point(15, 66),
                Size = new Size(1000, 38),
                FlowDirection = FlowDirection.LeftToRight
            };
            lblEstadoFilo = new Label[NumFilosofos];
            lblComidasFilo = new Label[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                Panel p = new Panel() { Size = new Size(190, 30) };
                Label nombre = new Label() { Location = new Point(5, 4), AutoSize = true, Text = Nombres[i], Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                Label est = new Label() { Location = new Point(5, 16), AutoSize = true, Text = TextoEstado[0], ForeColor = ColorEstado[0] };
                Label comidas = new Label() { Location = new Point(115, 16), AutoSize = true, Text = "Comidas: 0", Font = new Font("Segoe UI", 8) };
                p.Controls.Add(nombre);
                p.Controls.Add(est);
                p.Controls.Add(comidas);
                flpEstadisticas.Controls.Add(p);
                lblEstadoFilo[i] = est;
                lblComidasFilo[i] = comidas;
            }
            panelEstadisticas.Controls.Add(flpEstadisticas);

            this.Controls.Add(panelEstadisticas);
            this.Controls.Add(panelMesa);
        }

        private async void BtnIniciar_Click(object sender, EventArgs e)
        {
            btnIniciar.Enabled = false;
            btnDetener.Enabled = true;
            btnReiniciar.Enabled = false;
            cmbMetodos.Enabled = false;
            // El trackbar se deja habilitado para ajustar la velocidad en tiempo real

            cts = new CancellationTokenSource();
            Array.Clear(estados, 0, estados.Length);
            Array.Clear(comidasContador, 0, comidasContador.Length);
            ultimaActualizacionDeadlock = DateTime.MinValue;

            int seleccion = cmbMetodos.SelectedIndex;
            lblEstadoGeneral.Text = $"Ejecutando: {cmbMetodos.SelectedItem}";
            timerRender.Start();

            try
            {
                switch (seleccion)
                {
                    case 0: tareaSimulacion = Task.Run(() => MetodoJerarquiaRecursos(cts.Token)); break;
                    case 1: tareaSimulacion = Task.Run(() => MetodoPorEstados(cts.Token)); break;
                    case 2: tareaSimulacion = Task.Run(() => MetodoLimiteFilosofos(cts.Token)); break;
                    case 3: tareaSimulacion = Task.Run(() => MetodoConDeadlock(cts.Token)); break;
                }
                await tareaSimulacion;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                lblEstadoGeneral.Text = $"Error: {ex.Message}";
            }
            finally
            {
                if (!cts.Token.IsCancellationRequested)
                {
                    btnIniciar.Enabled = true;
                    btnDetener.Enabled = false;
                    btnReiniciar.Enabled = true;
                    cmbMetodos.Enabled = true;
                    trkVelocidad.Enabled = true;
                    lblEstadoGeneral.Text = "Simulación finalizada.";
                    timerRender.Stop();
                }
            }
        }

        private void BtnDetener_Click(object sender, EventArgs e)
        {
            DetenerSimulacion();
        }

        private void DetenerSimulacion()
        {
            cts?.Cancel();
            timerRender.Stop();
            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;
            btnReiniciar.Enabled = true;
            cmbMetodos.Enabled = true;
            trkVelocidad.Enabled = true;
            lblEstadoGeneral.Text = "Simulación detenida.";
            panelMesa.Invalidate();
        }

        private void ActualizarEstadisticasUI()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            try
            {
                this.Invoke((Action)(() =>
                {
                    for (int i = 0; i < NumFilosofos; i++)
                    {
                        lblEstadoFilo[i].Text = TextoEstado[estados[i]];
                        lblEstadoFilo[i].ForeColor = ColorEstado[estados[i]];
                        lblComidasFilo[i].Text = $"Comidas: {comidasContador[i]}";
                    }
                }));
            }
            catch { }
        }

        private void DetectarDeadlock()
        {
            bool todosHambrientos = true;
            for (int i = 0; i < NumFilosofos; i++)
                if (estados[i] != 1) { todosHambrientos = false; break; }

            if (todosHambrientos)
            {
                if (ultimaActualizacionDeadlock == DateTime.MinValue)
                    ultimaActualizacionDeadlock = DateTime.Now;
                else if ((DateTime.Now - ultimaActualizacionDeadlock).TotalSeconds > 3)
                {
                    lblEstadoGeneral.ForeColor = Color.Red;
                    lblEstadoGeneral.Text = "⚠️ DEADLOCK DETECTADO: todos están hambrientos y ninguno puede comer.";
                }
            }
            else
            {
                ultimaActualizacionDeadlock = DateTime.MinValue;
                lblEstadoGeneral.ForeColor = Color.FromArgb(80, 80, 80);
            }
        }

        private void PanelMesa_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(panelMesa.BackColor);

            int centroX = panelMesa.Width / 2;
            int centroY = panelMesa.Height / 2 - 10;
            int radioMesa = 150;
            int radioFilosofo = 40;

            for (int i = 0; i < NumFilosofos; i++)
            {
                double ang = i * (2 * Math.PI / NumFilosofos) - Math.PI / 2;
                int x1 = centroX + (int)(Math.Cos(ang) * (radioMesa - 10));
                int y1 = centroY + (int)(Math.Sin(ang) * (radioMesa - 10));
                int x2 = centroX + (int)(Math.Cos(ang) * (radioMesa + 80));
                int y2 = centroY + (int)(Math.Sin(ang) * (radioMesa + 80));
                using (Pen p = new Pen(Color.FromArgb(200, 200, 200), 2))
                    g.DrawLine(p, x1, y1, x2, y2);
            }

            g.FillEllipse(Brushes.LightGray, centroX - 50, centroY - 50, 100, 100);
            g.DrawEllipse(Pens.Gray, centroX - 50, centroY - 50, 100, 100);
            using (Font fCentro = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("MESA", fCentro, Brushes.DarkGray, new RectangleF(centroX - 50, centroY - 50, 100, 100), sf);
            }

            g.FillEllipse(Brushes.SaddleBrown, centroX - radioMesa, centroY - radioMesa, radioMesa * 2, radioMesa * 2);
            g.DrawEllipse(new Pen(Color.SaddleBrown), centroX - radioMesa, centroY - radioMesa, radioMesa * 2, radioMesa * 2);

            PointF[] posiciones = new PointF[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                double ang = i * (2 * Math.PI / NumFilosofos) - Math.PI / 2;
                posiciones[i] = new PointF(
                    centroX + (float)(Math.Cos(ang) * (radioMesa + 75)),
                    centroY + (float)(Math.Sin(ang) * (radioMesa + 75)));
            }

            for (int i = 0; i < NumFilosofos; i++)
            {
                PointF p1 = posiciones[i];
                PointF p2 = posiciones[(i + 1) % NumFilosofos];
                float cx = (p1.X + p2.X) / 2;
                float cy = (p1.Y + p2.Y) / 2;

                bool usado = estados[i] == 2 || estados[(i + 1) % NumFilosofos] == 2;
                Color cubiertoColor = usado ? Color.LimeGreen : Color.Gold;
                Pen cubiertoPen = usado ? Pens.Green : Pens.DarkGoldenrod;

                g.FillEllipse(new SolidBrush(cubiertoColor), cx - 13, cy - 13, 26, 26);
                g.DrawEllipse(cubiertoPen, cx - 13, cy - 13, 26, 26);
            }

            for (int i = 0; i < NumFilosofos; i++)
            {
                PointF p = posiciones[i];
                Color estadoColor = ColorEstado[estados[i]];
                bool comiendo = estados[i] == 2;

                g.FillEllipse(Brushes.Gray, p.X + 3 - radioFilosofo, p.Y + 3 - radioFilosofo, radioFilosofo * 2, radioFilosofo * 2);

                using (Brush brocha = new SolidBrush(estadoColor))
                    g.FillEllipse(brocha, p.X - radioFilosofo, p.Y - radioFilosofo, radioFilosofo * 2, radioFilosofo * 2);

                using (Pen borde = new Pen(comiendo ? Color.Red : Color.Black, comiendo ? 4f : 2f))
                    g.DrawEllipse(borde, p.X - radioFilosofo, p.Y - radioFilosofo, radioFilosofo * 2, radioFilosofo * 2);

                using (Font fNom = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    SizeF tamNom = g.MeasureString(Nombres[i], fNom);
                    g.DrawString(Nombres[i], fNom, Brushes.Black, p.X - tamNom.Width / 2, p.Y - 10);
                }

                using (Font fEst = new Font("Segoe UI", 8, FontStyle.Regular))
                {
                    SizeF tamEst = g.MeasureString(TextoEstado[estados[i]], fEst);
                    g.DrawString(TextoEstado[estados[i]], fEst, new SolidBrush(estadoColor), p.X - tamEst.Width / 2, p.Y + radioFilosofo + 5);
                }

                using (Font fCom = new Font("Segoe UI", 8, FontStyle.Italic))
                {
                    string txtCom = $"x{comidasContador[i]}";
                    SizeF tamCom = g.MeasureString(txtCom, fCom);
                    g.DrawString(txtCom, fCom, Brushes.DarkGray, p.X - tamCom.Width / 2, p.Y + radioFilosofo + 21);
                }
            }
        }

        private Task DelayAleatorio(Random rnd, int min, int max, CancellationToken token)
        {
            int ms = rnd.Next(min, max);
            int msAjustado = (int)(ms / factorVelocidad);
            return Task.Delay(msAjustado, token);
        }

        private async Task MetodoJerarquiaRecursos(CancellationToken token)
        {
            SemaphoreSlim[] cubiertos = new SemaphoreSlim[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++) cubiertos[i] = new SemaphoreSlim(1, 1);

            Task[] tareas = new Task[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                int id = i;
                int izq = id;
                int der = (id + 1) % NumFilosofos;
                int primero = Math.Min(izq, der);
                int segundo = Math.Max(izq, der);

                tareas[id] = Task.Run(async () =>
                {
                    Random rnd = new Random(id * 100);
                    while (!token.IsCancellationRequested)
                    {
                        estados[id] = 0;
                        await DelayAleatorio(rnd, 2000, 5000, token);

                        estados[id] = 1;
                        await cubiertos[primero].WaitAsync(token);
                        await cubiertos[segundo].WaitAsync(token);

                        estados[id] = 2;
                        Interlocked.Increment(ref comidasContador[id]);
                        await DelayAleatorio(rnd, 3000, 6000, token);

                        cubiertos[segundo].Release();
                        cubiertos[primero].Release();
                        estados[id] = 0;
                    }
                }, token);
            }
            try { await Task.WhenAll(tareas); } catch (OperationCanceledException) { }
        }

        private async Task MetodoPorEstados(CancellationToken token)
        {
            SemaphoreSlim mutexMesa = new SemaphoreSlim(1, 1);
            SemaphoreSlim[] semsFilosofos = new SemaphoreSlim[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++) semsFilosofos[i] = new SemaphoreSlim(0, 1);

            // Test asume que YA se posee el mutex de la mesa (no lo adquiere ni libera)
            void Test(int i)
            {
                if (estados[i] == 1 && estados[(i + 4) % NumFilosofos] != 2 && estados[(i + 1) % NumFilosofos] != 2)
                {
                    estados[i] = 2;
                    Interlocked.Increment(ref comidasContador[i]);
                    semsFilosofos[i].Release(); // despierta al filósofo i para que coma
                }
            }

            Task[] tareas = new Task[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                int id = i;
                tareas[id] = Task.Run(async () =>
                {
                    Random rnd = new Random(id * 200);
                    while (!token.IsCancellationRequested)
                    {
                        // --- PENSANDO ---
                        estados[id] = 0;
                        await DelayAleatorio(rnd, 2000, 5000, token);

                        // --- HAMBRIENTO: intenta comer ---
                        await mutexMesa.WaitAsync(token);
                        estados[id] = 1;
                        Test(id);
                        mutexMesa.Release();

                        // Si aún está hambriento, sus vecinos lo impiden: espera a que lo despierten
                        if (estados[id] == 1)
                        {
                            await semsFilosofos[id].WaitAsync(token);
                        }

                        // --- COMIENDO ---
                        if (estados[id] == 2)
                        {
                            await DelayAleatorio(rnd, 3000, 6000, token);

                            // Termina de comer: avisa a los vecinos
                            await mutexMesa.WaitAsync(token);
                            estados[id] = 0;
                            Test((id + 4) % NumFilosofos); // vecino izquierdo
                            Test((id + 1) % NumFilosofos); // vecino derecho
                            mutexMesa.Release();
                        }
                    }
                }, token);
            }
            try { await Task.WhenAll(tareas); } catch (OperationCanceledException) { }
        }

        private async Task MetodoLimiteFilosofos(CancellationToken token)
        {
            SemaphoreSlim[] cubiertos = new SemaphoreSlim[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++) cubiertos[i] = new SemaphoreSlim(1, 1);
            SemaphoreSlim limitadorMesa = new SemaphoreSlim(NumFilosofos - 1, NumFilosofos - 1);

            Task[] tareas = new Task[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                int id = i;
                int izq = id;
                int der = (id + 1) % NumFilosofos;

                tareas[id] = Task.Run(async () =>
                {
                    Random rnd = new Random(id * 300);
                    while (!token.IsCancellationRequested)
                    {
                        estados[id] = 0;
                        await DelayAleatorio(rnd, 2000, 5000, token);

                        estados[id] = 1;
                        await limitadorMesa.WaitAsync(token);
                        await cubiertos[izq].WaitAsync(token);
                        await cubiertos[der].WaitAsync(token);

                        estados[id] = 2;
                        Interlocked.Increment(ref comidasContador[id]);
                        await DelayAleatorio(rnd, 3000, 6000, token);

                        cubiertos[der].Release();
                        cubiertos[izq].Release();
                        limitadorMesa.Release();
                        estados[id] = 0;
                    }
                }, token);
            }
            try { await Task.WhenAll(tareas); } catch (OperationCanceledException) { }
        }

        private async Task MetodoConDeadlock(CancellationToken token)
        {
            SemaphoreSlim[] cubiertos = new SemaphoreSlim[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++) cubiertos[i] = new SemaphoreSlim(1, 1);

            Task[] tareas = new Task[NumFilosofos];
            for (int i = 0; i < NumFilosofos; i++)
            {
                int id = i;
                int izq = id;
                int der = (id + 1) % NumFilosofos;

                tareas[id] = Task.Run(async () =>
                {
                    Random rnd = new Random(id * 400);
                    while (!token.IsCancellationRequested)
                    {
                        estados[id] = 0;
                        await DelayAleatorio(rnd, 2000, 4000, token);

                        estados[id] = 1;
                        await cubiertos[izq].WaitAsync(token);
                        await DelayAleatorio(rnd, 500, 1000, token);
                        await cubiertos[der].WaitAsync(token);

                        estados[id] = 2;
                        Interlocked.Increment(ref comidasContador[id]);
                        await DelayAleatorio(rnd, 3000, 6000, token);

                        cubiertos[der].Release();
                        cubiertos[izq].Release();
                        estados[id] = 0;
                    }
                }, token);
            }
            try { await Task.WhenAll(tareas); } catch (OperationCanceledException) { }
        }
    }
}