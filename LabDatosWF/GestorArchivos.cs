using System.Runtime.InteropServices;
using System.Text;

namespace LabDatosWF;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public record struct Ciudadano(
    int Id,
    string Nombre,
    int Edad)
{
    public static int Size => 4 + 50 + 4;
}

public class GestorArchivos
{
    public readonly string RutaArchivo = "datos_ciudadanos.dat";

    public void GuardarCiudadano(Ciudadano c, int posicion)
    {
        using var fs = new FileStream(RutaArchivo, FileMode.OpenOrCreate, FileAccess.Write);
        long offset = (long)posicion * Ciudadano.Size;
        fs.Seek(offset, SeekOrigin.Begin);
        using var writer = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);
        writer.Write(c.Id);
        byte[] nombreBytes = new byte[50];
        byte[] src = System.Text.Encoding.ASCII.GetBytes(c.Nombre);
        Array.Copy(src, nombreBytes, Math.Min(src.Length, 50));
        writer.Write(nombreBytes);
        writer.Write(c.Edad);
    }

    public Ciudadano? LeerCiudadano(int posicion)
    {
        if (!File.Exists(RutaArchivo)) return null;
        using var fs = new FileStream(RutaArchivo, FileMode.Open, FileAccess.Read);
        long offset = (long)posicion * Ciudadano.Size;
        if (offset >= fs.Length) return null;
        fs.Seek(offset, SeekOrigin.Begin);
        using var reader = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);
        int id = reader.ReadInt32();
        byte[] nombreBytes = reader.ReadBytes(50);
        string nombre = System.Text.Encoding.ASCII.GetString(nombreBytes).TrimEnd('\0', ' ');
        int edad = reader.ReadInt32();
        return new Ciudadano(id, nombre, edad);
    }

    public List<Ciudadano> LeerTodos()
    {
        var lista = new List<Ciudadano>();
        if (!File.Exists(RutaArchivo)) return lista;
        long totalBytes = new FileInfo(RutaArchivo).Length;
        int totalReg = (int)(totalBytes / Ciudadano.Size);
        for (int i = 0; i < totalReg; i++)
        {
            var c = LeerCiudadano(i);
            if (c.HasValue) lista.Add(c.Value);
        }
        return lista;
    }

    public void EliminarArchivo()
    {
        if (File.Exists(RutaArchivo)) File.Delete(RutaArchivo);
    }
}