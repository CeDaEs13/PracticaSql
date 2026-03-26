using System.Diagnostics;

namespace LabDatosWF;

public class GestorIndice
{
    private readonly string _rutaIndice = "datos_ciudadanos.idx";
    private readonly GestorArchivos _gestor = new();

    public void GuardarIndice(int id, int posicion)
    {
        int entrada = ObtenerPosicionEnIndice(id);
        using var fs = new FileStream(_rutaIndice, FileMode.OpenOrCreate, FileAccess.Write);
        long offsetIndice = entrada >= 0 ? (long)entrada * 8 : fs.Length;
        fs.Seek(offsetIndice, SeekOrigin.Begin);
        using var writer = new BinaryWriter(fs, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(id);
        writer.Write(posicion);
    }

    public int BuscarPorId(int id)
    {
        if (!File.Exists(_rutaIndice)) return -1;
        using var fs = new FileStream(_rutaIndice, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);
        while (fs.Position < fs.Length)
        {
            int idGuardado = reader.ReadInt32();
            int posGuardada = reader.ReadInt32();
            if (idGuardado == id) return posGuardada;
        }
        return -1;
    }

    public (Ciudadano? resultado, long microsegundos) BuscarSecuencial(int id)
    {
        BuscarPorId(id);
        var sw = Stopwatch.StartNew();
        var todos = _gestor.LeerTodos();
        Ciudadano? encontrado = null;
        foreach (var c in todos)
            if (c.Id == id) { encontrado = c; break; }
        sw.Stop();
        long us = sw.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
        return (encontrado, us);
    }

    public (Ciudadano? resultado, long microsegundos) BuscarIndexado(int id)
    {
        var sw = Stopwatch.StartNew();
        int posicion = BuscarPorId(id);
        Ciudadano? c = posicion >= 0 ? _gestor.LeerCiudadano(posicion) : null;
        sw.Stop();
        long us = sw.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
        return (c, us);
    }

    public List<(int Id, int Posicion)> ObtenerTodoElIndice()
    {
        var lista = new List<(int, int)>();
        if (!File.Exists(_rutaIndice)) return lista;
        using var fs = new FileStream(_rutaIndice, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);
        while (fs.Position < fs.Length)
            lista.Add((reader.ReadInt32(), reader.ReadInt32()));
        return lista;
    }

    public void EliminarIndice()
    {
        if (File.Exists(_rutaIndice)) File.Delete(_rutaIndice);
    }

    private int ObtenerPosicionEnIndice(int id)
    {
        if (!File.Exists(_rutaIndice)) return -1;
        using var fs = new FileStream(_rutaIndice, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);
        int entrada = 0;
        while (fs.Position < fs.Length)
        {
            int idGuardado = reader.ReadInt32();
            reader.ReadInt32();
            if (idGuardado == id) return entrada;
            entrada++;
        }
        return -1;
    }
}