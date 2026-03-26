using System.Drawing;
using System.Windows.Forms;

namespace LabDatosWF;

public class MainForm : Form
{
    // ── Colores del tema ──────────────────────────────────────
    static readonly Color BgDark = Color.FromArgb(18, 18, 28);
    static readonly Color BgPanel = Color.FromArgb(26, 26, 40);
    static readonly Color BgCard = Color.FromArgb(34, 34, 52);
    static readonly Color Accent1 = Color.FromArgb(99, 102, 241);
    static readonly Color Accent2 = Color.FromArgb(52, 211, 153);
    static readonly Color Accent3 = Color.FromArgb(251, 146, 60);
    static readonly Color TextLight = Color.FromArgb(226, 232, 240);
    static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
    static readonly Color Success = Color.FromArgb(52, 211, 153);
    static readonly Color Error = Color.FromArgb(239, 68, 68);

    private TabControl _tabs = null!;

    // Nivel 1
    private DataGridView _gridNivel1 = null!;
    private RichTextBox _logNivel1 = null!;
    private TextBox _txtId1 = null!, _txtNombre1 = null!, _txtEdad1 = null!, _txtPos1 = null!;

    // Nivel 2
    private DataGridView _gridIndice = null!;
    private RichTextBox _logNivel2 = null!;
    private TextBox _txtBuscarId = null!;

    // Nivel 3
    private DataGridView _gridSQL = null!;
    private RichTextBox _logNivel3 = null!;
    private Label _lblStatus = null!;

    private readonly GestorArchivos _gestor = new();
    private readonly GestorIndice _indice = new();
    private readonly MigradorSql _migrador = new();

    public MainForm()
    {
        InitializeComponent();
        CargarDatosNivel1();
    }

    // ==========================================================
    //  CONSTRUCCIÓN DE LA UI
    // ==========================================================
    private void InitializeComponent()
    {
        Text = "El Arquitecto de Datos — Del Archivo a la Nube";
        Size = new Size(1100, 720);
        MinimumSize = new Size(1000, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgDark;
        ForeColor = TextLight;
        Font = new Font("Segoe UI", 9.5f);

        var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = BgPanel };
        header.Controls.Add(new Label
        {
            Text = "⚡  El Arquitecto de Datos",
            Font = new Font("Segoe UI Semibold", 16f),
            ForeColor = TextLight,
            AutoSize = true,
            Location = new Point(20, 10)
        });
        header.Controls.Add(new Label
        {
            Text = "Administración y Organización de Datos  •  C# 14 / .NET 10 / SQL Server",
            Font = new Font("Segoe UI", 9f),
            ForeColor = TextMuted,
            AutoSize = true,
            Location = new Point(22, 37)
        });

        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10f),
            Padding = new Point(16, 8),
            DrawMode = TabDrawMode.OwnerDrawFixed,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(240, 38),
        };
        _tabs.DrawItem += Tabs_DrawItem;
        _tabs.BackColor = BgDark;

        _tabs.TabPages.Add(BuildTab1());
        _tabs.TabPages.Add(BuildTab2());
        _tabs.TabPages.Add(BuildTab3());

        Controls.Add(_tabs);
        Controls.Add(header);
    }

    private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
    {
        var tab = _tabs.TabPages[e.Index];
        bool selected = (e.State & DrawItemState.Selected) != 0;
        using var brush = new SolidBrush(selected ? Accent1 : BgPanel);
        e.Graphics.FillRectangle(brush, e.Bounds);
        using var tb = new SolidBrush(selected ? Color.White : TextMuted);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        e.Graphics.DrawString(tab.Text, new Font("Segoe UI Semibold", 9.5f), tb, e.Bounds, sf);
    }

    // ==========================================================
    //  PESTAÑA 1
    // ==========================================================
    private TabPage BuildTab1()
    {
        var tab = new TabPage("📁  Nivel 1 — El Escriba") { BackColor = BgDark, Padding = new Padding(12) };

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BackColor = BgDark,
            SplitterWidth = 4,
            SplitterDistance = 340,
            Panel1MinSize = 280,
            Panel2MinSize = 100
        };

        var pnlTop = new Panel { Dock = DockStyle.Fill, BackColor = BgDark };

        // Tarjeta izquierda — altura suficiente para todos los controles
        var card = MakeCard(new Rectangle(0, 0, 340, 310));

        var lblCard = MakeLabel("Tamaño de registro: 58 bytes  (int 4 + char[50] + int 4)", 10, 10, Accent2);
        lblCard.Font = new Font("Segoe UI", 8.5f);

        // ID y Edad en la misma fila para aprovechar espacio
        _txtId1 = MakeTextBox("ID", 10, 35, 140);
        _txtEdad1 = MakeTextBox("Edad", 160, 35, 170);
        _txtNombre1 = MakeTextBox("Nombre completo", 10, 70, 320);
        _txtPos1 = MakeTextBox("Posición (slot 0, 1, 2...)", 10, 105, 200);

        // Botones con ancho completo para que no se corten
        var btnGuardar = MakeButton("💾  Guardar en posición", 10, 140, Accent1, width: 320);
        var btnLeer = MakeButton("📖  Leer de posición", 10, 178, BgCard, width: 320);
        var btnCargar5 = MakeButton("⚡  Cargar 5 de ejemplo", 10, 216, Accent2, width: 320, textColor: BgDark);
        var btnLimpiar = MakeButton("🗑  Limpiar archivo .dat", 10, 254, BgCard, width: 320);

        btnGuardar.Click += BtnGuardar1_Click;
        btnLeer.Click += BtnLeer1_Click;
        btnCargar5.Click += BtnCargar5_Click;
        btnLimpiar.Click += (_, _) =>
        {
            _gestor.EliminarArchivo();
            _gridNivel1.Rows.Clear();
            AppendLog(_logNivel1, "🗑 Archivo .dat eliminado.", TextMuted);
        };

        card.Controls.AddRange(new Control[]
            { lblCard, _txtId1, _txtEdad1, _txtNombre1, _txtPos1,
              btnGuardar, btnLeer, btnCargar5, btnLimpiar });

        // Grid: empieza después de la tarjeta y se estira con la ventana
        _gridNivel1 = MakeGrid();
        _gridNivel1.Location = new Point(350, 0);
        _gridNivel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _gridNivel1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Posición", Width = 72 });
        _gridNivel1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Offset (bytes)", Width = 105 });
        _gridNivel1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", Width = 52 });
        _gridNivel1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _gridNivel1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Edad", Width = 58 });

        pnlTop.Controls.Add(_gridNivel1);
        pnlTop.Controls.Add(card);
        // Ajustar alto del grid cuando el panel cambia de tamaño
        pnlTop.Resize += (_, _) =>
        {
            _gridNivel1.Width = pnlTop.Width - _gridNivel1.Left - 8;
            _gridNivel1.Height = pnlTop.Height - 8;
        };

        _logNivel1 = MakeLog();
        AppendLog(_logNivel1, "▶ Listo. Guarda ciudadanos o carga los 5 de ejemplo.", TextMuted);

        split.Panel1.Controls.Add(pnlTop);
        split.Panel2.Controls.Add(_logNivel1);
        tab.Controls.Add(split);
        return tab;
    }

    // ==========================================================
    //  PESTAÑA 2
    // ==========================================================
    private TabPage BuildTab2()
    {
        var tab = new TabPage("🔍  Nivel 2 — El Indexador") { BackColor = BgDark, Padding = new Padding(12) };

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BackColor = BgDark,
            SplitterWidth = 4,
            SplitterDistance = 340,
            Panel1MinSize = 280,
            Panel2MinSize = 100
        };

        var pnlTop = new Panel { Dock = DockStyle.Fill, BackColor = BgDark };

        var card = MakeCard(new Rectangle(0, 0, 340, 270));

        var lblInfo = MakeLabel("Archivo .idx: pares  Id (4 bytes) + Posición (4 bytes)", 10, 10, Accent2);
        lblInfo.Font = new Font("Segoe UI", 8.5f);

        var btnConstruir = MakeButton("🗂  Construir índice .idx", 10, 35, Accent1, width: 320);
        var lblBuscar = MakeLabel("ID a buscar:", 10, 80, TextLight);
        _txtBuscarId = MakeTextBox("Número de ID", 10, 100, 160);
        var btnSecuencial = MakeButton("🔎  Buscar secuencial", 10, 135, BgCard, width: 320);
        var btnIndexado = MakeButton("⚡  Buscar indexado", 10, 173, Accent2, width: 320, textColor: BgDark);
        var lblCompar = MakeLabel("Compara los µs en el log ↓", 10, 216, TextMuted);
        lblCompar.Font = new Font("Segoe UI", 8f);

        btnConstruir.Click += BtnConstruirIndice_Click;
        btnSecuencial.Click += BtnBuscarSecuencial_Click;
        btnIndexado.Click += BtnBuscarIndexado_Click;

        card.Controls.AddRange(new Control[]
            { lblInfo, btnConstruir, lblBuscar, _txtBuscarId,
              btnSecuencial, btnIndexado, lblCompar });

        _gridIndice = MakeGrid();
        _gridIndice.Location = new Point(350, 0);
        _gridIndice.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _gridIndice.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Entrada .idx", Width = 92 });
        _gridIndice.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", Width = 62 });
        _gridIndice.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "→ Posición", Width = 92 });
        _gridIndice.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Offset bytes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        pnlTop.Controls.Add(_gridIndice);
        pnlTop.Controls.Add(card);
        pnlTop.Resize += (_, _) =>
        {
            _gridIndice.Width = pnlTop.Width - _gridIndice.Left - 8;
            _gridIndice.Height = pnlTop.Height - 8;
        };

        _logNivel2 = MakeLog();
        AppendLog(_logNivel2, "▶ Primero construye el índice (necesitas haber ejecutado el Nivel 1).", TextMuted);

        split.Panel1.Controls.Add(pnlTop);
        split.Panel2.Controls.Add(_logNivel2);
        tab.Controls.Add(split);
        return tab;
    }

    // ==========================================================
    //  PESTAÑA 3
    // ==========================================================
    private TabPage BuildTab3()
    {
        var tab = new TabPage("☁  Nivel 3 — SQL Server") { BackColor = BgDark, Padding = new Padding(12) };

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BackColor = BgDark,
            SplitterWidth = 4,
            SplitterDistance = 340,
            Panel1MinSize = 280,
            Panel2MinSize = 100
        };

        var pnlTop = new Panel { Dock = DockStyle.Fill, BackColor = BgDark };

        var card = MakeCard(new Rectangle(0, 0, 340, 270));

        var lblConn = MakeLabel("Servidor: SQLEXPRESS", 10, 10, TextMuted);
        lblConn.Font = new Font("Segoe UI", 8f);
        var lblDb = MakeLabel("Base de datos: LabDatos  |  Usuario: sa", 10, 28, TextMuted);
        lblDb.Font = new Font("Segoe UI", 8f);
        _lblStatus = MakeLabel("● Sin verificar", 10, 48, TextMuted);
        _lblStatus.Font = new Font("Segoe UI Semibold", 9f);

        var btnVerify = MakeButton("🔌  Verificar conexión", 10, 72, BgCard, width: 320);
        var btnMigrar = MakeButton("📤  Migrar .dat → SQL Server", 10, 110, Accent1, width: 320);
        var btnConsult = MakeButton("📋  Consultar todos (SELECT)", 10, 148, Accent2, width: 320, textColor: BgDark);
        var btnLimpiar = MakeButton("🗑  Limpiar log", 10, 186, BgCard, width: 320);

        btnVerify.Click += async (_, _) => await BtnVerificar_Click();
        btnMigrar.Click += async (_, _) => await BtnMigrar_Click();
        btnConsult.Click += async (_, _) => await BtnConsultar_Click();
        btnLimpiar.Click += (_, _) => _logNivel3.Clear();

        card.Controls.AddRange(new Control[]
            { lblConn, lblDb, _lblStatus, btnVerify, btnMigrar, btnConsult, btnLimpiar });

        _gridSQL = MakeGrid();
        _gridSQL.Location = new Point(350, 0);
        _gridSQL.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        _gridSQL.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", Width = 62 });
        _gridSQL.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _gridSQL.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Edad", Width = 62 });

        pnlTop.Controls.Add(_gridSQL);
        pnlTop.Controls.Add(card);
        pnlTop.Resize += (_, _) =>
        {
            _gridSQL.Width = pnlTop.Width - _gridSQL.Left - 8;
            _gridSQL.Height = pnlTop.Height - 8;
        };

        _logNivel3 = MakeLog();
        AppendLog(_logNivel3, "▶ Verifica la conexión antes de migrar o consultar.", TextMuted);

        split.Panel1.Controls.Add(pnlTop);
        split.Panel2.Controls.Add(_logNivel3);
        tab.Controls.Add(split);
        return tab;
    }

    // ==========================================================
    //  EVENTOS - NIVEL 1
    // ==========================================================
    private void BtnGuardar1_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(_txtId1.Text, out int id) ||
            !int.TryParse(_txtEdad1.Text, out int edad) ||
            !int.TryParse(_txtPos1.Text, out int pos) ||
            string.IsNullOrWhiteSpace(_txtNombre1.Text))
        {
            AppendLog(_logNivel1, "⚠ Rellena todos los campos correctamente.", Error);
            return;
        }
        // Verificar que el ID no exista ya en otro slot
        var todos = _gestor.LeerTodos();
        bool idDuplicado = todos.Any(x => x.Id == id);

        if (idDuplicado)
        {
            AppendLog(_logNivel1, $"⚠ Ya existe un ciudadano con Id:{id} en el archivo.", Error);
            return;
        }

        var c = new Ciudadano(id, _txtNombre1.Text.Trim(), edad);
        _gestor.GuardarCiudadano(c, pos);
        long offset = (long)pos * Ciudadano.Size;
        AppendLog(_logNivel1, $"✔ Guardado Id:{id} | {c.Nombre} | {edad} años  →  posición {pos} (offset {offset} bytes)", Success);
        CargarDatosNivel1();
    }

    private void BtnLeer1_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(_txtPos1.Text, out int pos))
        { AppendLog(_logNivel1, "⚠ Ingresa una posición válida.", Error); return; }

        var c = _gestor.LeerCiudadano(pos);
        if (c.HasValue)
        {
            _txtId1.Text = c.Value.Id.ToString(); _txtNombre1.Text = c.Value.Nombre; _txtEdad1.Text = c.Value.Edad.ToString();
            AppendLog(_logNivel1, $"📖 Leído en posición {pos}: Id:{c.Value.Id} | {c.Value.Nombre} | {c.Value.Edad} años", Accent2);
        }
        else AppendLog(_logNivel1, $"⚠ No hay registro en la posición {pos}.", TextMuted);
    }

    private void BtnCargar5_Click(object? sender, EventArgs e)
    {
        _gestor.EliminarArchivo();
        var ciudadanos = new[]
        {
            new Ciudadano(1, "Abdiel Herrera",       28),
            new Ciudadano(2, "Oscar Ibarra",   35),
            new Ciudadano(3, "Oscar Almanza",  22),
            new Ciudadano(4, "Braulio Davila",   45),
            new Ciudadano(5, "Fernando Tamez",   31),
        };
        for (int i = 0; i < ciudadanos.Length; i++)
            _gestor.GuardarCiudadano(ciudadanos[i], i);
        AppendLog(_logNivel1, "⚡ 5 ciudadanos de ejemplo guardados en el archivo binario.", Accent1);
        CargarDatosNivel1();
    }

    private void CargarDatosNivel1()
    {
        _gridNivel1.Rows.Clear();
        var todos = _gestor.LeerTodos();
        for (int i = 0; i < todos.Count; i++)
            _gridNivel1.Rows.Add(i, (long)i * Ciudadano.Size, todos[i].Id, todos[i].Nombre, todos[i].Edad);
    }

    // ==========================================================
    //  EVENTOS - NIVEL 2
    // ==========================================================
    private void BtnConstruirIndice_Click(object? sender, EventArgs e)
    {
        if (!File.Exists(_gestor.RutaArchivo))
        { AppendLog(_logNivel2, "⚠ Ejecuta el Nivel 1 primero para generar el archivo .dat", Error); return; }

        _indice.EliminarIndice();
        var todos = _gestor.LeerTodos();
        for (int i = 0; i < todos.Count; i++) _indice.GuardarIndice(todos[i].Id, i);
        AppendLog(_logNivel2, $"🗂 Índice construido: {todos.Count} entrada(s) en datos_ciudadanos.idx", Accent1);

        _gridIndice.Rows.Clear();
        var entradas = _indice.ObtenerTodoElIndice();
        for (int i = 0; i < entradas.Count; i++)
            _gridIndice.Rows.Add(i, entradas[i].Id, entradas[i].Posicion, (long)entradas[i].Posicion * Ciudadano.Size);
    }

    private void BtnBuscarSecuencial_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(_txtBuscarId.Text, out int id)) { AppendLog(_logNivel2, "⚠ ID inválido.", Error); return; }
        var (resultado, us) = _indice.BuscarSecuencial(id);
        string res = resultado.HasValue ? $"{resultado.Value.Nombre} ({resultado.Value.Edad} años)" : "no encontrado";
        AppendLog(_logNivel2, $"🔎 Secuencial  →  {us,6} µs  |  Id:{id} = {res}", TextLight);
    }

    private void BtnBuscarIndexado_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(_txtBuscarId.Text, out int id)) { AppendLog(_logNivel2, "⚠ ID inválido.", Error); return; }
        var (resultado, us) = _indice.BuscarIndexado(id);
        string res = resultado.HasValue ? $"{resultado.Value.Nombre} ({resultado.Value.Edad} años)" : "no encontrado";
        AppendLog(_logNivel2, $"⚡ Indexado    →  {us,6} µs  |  Id:{id} = {res}", Accent2);
    }

    // ==========================================================
    //  EVENTOS - NIVEL 3
    // ==========================================================
    private async Task BtnVerificar_Click()
    {
        AppendLog(_logNivel3, "Verificando conexión...", TextMuted);
        var (ok, msg) = await _migrador.VerificarConexion();
        _lblStatus.Text = ok ? "● Conectado" : "● Sin conexión";
        _lblStatus.ForeColor = ok ? Success : Error;
        AppendLog(_logNivel3, ok ? $"✔ {msg}" : $"✘ {msg}", ok ? Success : Error);
    }

    private async Task BtnMigrar_Click()
    {
        if (!File.Exists(_gestor.RutaArchivo))
        { AppendLog(_logNivel3, "⚠ Ejecuta el Nivel 1 primero.", Error); return; }
        AppendLog(_logNivel3, "Iniciando migración...", TextMuted);
        try
        {
            var (ins, omit, log) = await _migrador.MigrarDesdeArchivo(_gestor.RutaArchivo);
            foreach (var l in log)
                AppendLog(_logNivel3, l, l.StartsWith("[OK]") ? Success : l.StartsWith("[OMITIDO]") ? Accent3 : TextLight);
        }
        catch (Exception ex) { AppendLog(_logNivel3, $"✘ Error: {ex.Message}", Error); }
    }

    private async Task BtnConsultar_Click()
    {
        AppendLog(_logNivel3, "Ejecutando SELECT...", TextMuted);
        try
        {
            var lista = await _migrador.ConsultarTodos();
            _gridSQL.Rows.Clear();
            foreach (var c in lista) _gridSQL.Rows.Add(c.Id, c.Nombre, c.Edad);
            AppendLog(_logNivel3, $"✔ {lista.Count} registro(s) obtenido(s) de SQL Server.", Success);
        }
        catch (Exception ex) { AppendLog(_logNivel3, $"✘ Error: {ex.Message}", Error); }
    }

    // ==========================================================
    //  HELPERS DE UI  (sin cambios en lógica, solo ajustes de tamaño)
    // ==========================================================
    private static Panel MakeCard(Rectangle bounds)
    {
        var p = new Panel { Bounds = bounds, BackColor = BgCard, Padding = new Padding(10) };
        p.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(99, 102, 241), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };
        return p;
    }

    private static TextBox MakeTextBox(string placeholder, int x, int y, int width) => new()
    {
        Location = new Point(x, y),
        Width = width,
        BackColor = Color.FromArgb(18, 18, 28),
        ForeColor = Color.FromArgb(226, 232, 240),
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Segoe UI", 9.5f),
        PlaceholderText = placeholder
    };

    // width ahora es parámetro con nombre para no romper las llamadas existentes
    private static Button MakeButton(string text, int x, int y, Color bg,
        int width = 320, Color? textColor = null)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Width = width,
            Height = 32,
            BackColor = bg,
            ForeColor = textColor ?? Color.FromArgb(226, 232, 240),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9f),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private static Label MakeLabel(string text, int x, int y, Color color) => new()
    {
        Text = text,
        Location = new Point(x, y),
        ForeColor = color,
        AutoSize = true,
        Font = new Font("Segoe UI", 9f)
    };

    private static DataGridView MakeGrid() => new()
    {
        // Tamaño inicial pequeño; el Resize del panel lo ajusta al tamaño real
        Width = 200,
        Height = 200,
        BackgroundColor = Color.FromArgb(26, 26, 40),
        GridColor = Color.FromArgb(50, 50, 70),
        BorderStyle = BorderStyle.None,
        RowHeadersVisible = false,
        AllowUserToAddRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(26, 26, 40),
            ForeColor = Color.FromArgb(226, 232, 240),
            SelectionBackColor = Color.FromArgb(99, 102, 241),
            SelectionForeColor = Color.White,
            Font = new Font("Consolas", 9f)
        },
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(34, 34, 52),
            ForeColor = Color.FromArgb(99, 102, 241),
            Font = new Font("Segoe UI Semibold", 9f),
            SelectionBackColor = Color.FromArgb(34, 34, 52),
        },
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
        ColumnHeadersHeight = 32,
        RowTemplate = { Height = 26 }
    };

    private static RichTextBox MakeLog() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(12, 12, 20),
        ForeColor = Color.FromArgb(226, 232, 240),
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 9f),
        ReadOnly = true,
        ScrollBars = RichTextBoxScrollBars.Vertical
    };

    private static void AppendLog(RichTextBox box, string text, Color color)
    {
        box.SelectionStart = box.TextLength; box.SelectionLength = 0;
        box.SelectionColor = color; box.AppendText(text + "\n"); box.ScrollToCaret();
    }
}